using System.Collections.Generic;
using System.Text.Json;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace logsToMetrics
{
    public class Function
    {
        private readonly AmazonCloudWatchLogsClient _cloudWatchLogsClient;
        private readonly AmazonCloudWatchClient _cloudWatchClient;
        private const string ColdStartColumnName = "coldstart";

        private readonly List<string> _logGroupPrefixes = new()
        {
            "/aws/lambda/net-6-base-arm64",
            "/aws/lambda/net-6-base-x86-64",
            "/aws/lambda/net-6-customRuntime-x86-64",
            "/aws/lambda/net-7-base-x86-64",
            "/aws/lambda/net-7-native-x86-64",
            "/aws/lambda/net-8-base-x86-64",
            "/aws/lambda/net-8-base-arm64",
            "/aws/lambda/net-8-native-x86-64",
            "/aws/lambda/net-8-native-arm64"
        };

        public Function()
        {
            this._cloudWatchLogsClient = new AmazonCloudWatchLogsClient();
            this._cloudWatchClient = new AmazonCloudWatchClient();
        }

        public async Task<string> FunctionHandler(object input, ILambdaContext context)
        {
            bool backfill = false;
            try
            {
                if (((JsonElement)input).ToString().Contains("backfill"))
                {
                    context.Logger.LogLine("Backfilling...");
                    backfill = true;
                }
            }
           catch (Exception e)
            {
                context.Logger.LogLine("Caught Exception when checking for backfill...");
                context.Logger.LogLine(e.ToString());                
            }

            // process each log group in parallel
            List<Task<ProcessLogGroupResult>> tasks = new List<Task<ProcessLogGroupResult>>();

            if (backfill)
            {
                var utcNow = DateTime.UtcNow;
                for (int i = 1; i <= 14; i++)
                {
                    tasks.AddRange(
                        _logGroupPrefixes
                            .Select(l => TryProcessLogGroup(context, l, utcNow.AddDays(-1 * i), utcNow.AddDays((-1 * 1) + 1))));
                }
            }
            else
            {
                tasks.AddRange(
                    _logGroupPrefixes
                        .Select(l => TryProcessLogGroup(context, l, DateTime.Now.AddDays(-1), DateTime.Now)));
            }

            var results = (await Task.WhenAll(tasks)).Where(x => x != null);

            var resultsJson = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });

            context.Logger.LogLine(resultsJson);

            return resultsJson;
        }

        private async Task<ProcessLogGroupResult> TryProcessLogGroup(ILambdaContext context, string logGroupPrefix, DateTime queryStartTime, DateTime queryEndTime)
        {
            var result = new ProcessLogGroupResult
            {
                LogGroupPrefix = logGroupPrefix
            };

            try
            {
                var resultRows = 0;
                var queryCount = 0;

                var finalResults = new List<List<ResultField>>();

                while (resultRows < 2)
                {
                    finalResults = await RunQuery(context, logGroupPrefix, queryStartTime, queryEndTime);

                    resultRows = finalResults.Count;
                    queryCount++;

                    if (queryCount > 3)
                    {
                        context.Logger.LogError($"For logGroupPrefix {logGroupPrefix}, queried 3 times, but didn't get 2 rows of results (cold and warm). Saw {resultRows} rows. queryStartTime:{queryStartTime}. queryEndTime:{queryEndTime}");
                        return null;
                    }
                }

                // If we see "0" in the cold start column in row 0, then the first row is representing warm starts and the second
                // row is representing cold start. Else vice versa.
                // Example output
                /*
                    coldstart	count	p50	    p90	    p99	    max
                    0	        10297	5.2	    8.0	    32.2	74.9
                    1	        478	    1393.2	1444.2	1497.1	1528.8
                */
                var warmStartIndex = finalResults[0].Single(x => x.Field == ColdStartColumnName).Value == "0" ? 0 : 1;
                var coldStartIndex = 1 - warmStartIndex;

                var wrapper = new QueryResultWrapper
                {
                    LoadTestType = logGroupPrefix,
                    WarmStart = new QueryResult
                    {
                        Count = finalResults[warmStartIndex][1].Value,
                        P50 = finalResults[warmStartIndex][2].Value,
                        P90 = finalResults[warmStartIndex][3].Value,
                        P99 = finalResults[warmStartIndex][4].Value,
                        Max = finalResults[warmStartIndex][5].Value,
                    },
                    ColdStart = new QueryResult
                    {
                        Count = finalResults[coldStartIndex][1].Value,
                        P50 = finalResults[coldStartIndex][2].Value,
                        P90 = finalResults[coldStartIndex][3].Value,
                        P99 = finalResults[coldStartIndex][4].Value,
                        Max = finalResults[coldStartIndex][5].Value,
                    }
                };

                result.WarmCount = wrapper.WarmStart.Count;
                result.ColdCount = wrapper.ColdStart.Count;

                await SendLogsAsMetrics(wrapper);

                result.Success = true;
                return result;
            }
            catch (Exception e)
            {
                result.Exception = $"{e.Message}{e.StackTrace}";
                return result;
            }
        }

        private async Task<List<List<ResultField>>> RunQuery(ILambdaContext context, string logGroupNamePrefix, DateTime queryStartTime, DateTime queryEndTime)
        {
            context.Logger.LogLine($"Retrieving log groups with prefix {logGroupNamePrefix}");

            DescribeLogGroupsResponse logGroupList = await _cloudWatchLogsClient.DescribeLogGroupsAsync(new DescribeLogGroupsRequest
            {
                LogGroupNamePrefix = logGroupNamePrefix,
            });

            // Make sure not to include the GenerateLoadTestResults function or any other helper functions
            var filteredLogGroups = logGroupList.LogGroups.Where(x =>
                x.LogGroupName.Contains("DeleteProductFunction")
                || x.LogGroupName.Contains("GetProduct")
                || x.LogGroupName.Contains("GetProducts")
                || x.LogGroupName.Contains("PutProductFunction")
                ).ToList();

            context.Logger.LogLine($"Found {filteredLogGroups.Count} log group(s) for logGroupNamePrefix {logGroupNamePrefix}");

            if (filteredLogGroups.Count < 4)
            {
                context.Logger.LogError($"Expecting 4 or more log groups (Delete, Get, GetMultiple, Put), but found {filteredLogGroups.Count} for logGroupNamePrefix {logGroupNamePrefix}. Skipping this log group prefix.");
                return new List<List<ResultField>>();
            }

            var queryRes = await _cloudWatchLogsClient.StartQueryAsync(new StartQueryRequest
            {
                LogGroupNames = filteredLogGroups.Select(p => p.LogGroupName).ToList(),
                QueryString =
                    "filter @type=\"REPORT\" " +
                    $"| fields greatest(@initDuration, 0) + @duration as duration, ispresent(@initDuration) as {ColdStartColumnName} " +
                    $"| stats count(*) as count, pct(duration, 50) as p50, pct(duration, 90) as p90, pct(duration, 99) as p99, max(duration) as max by {ColdStartColumnName}",
                StartTime = queryStartTime.AsUnixTimestamp(),
                EndTime = queryEndTime.AsUnixTimestamp(),
            });

            context.Logger.LogLine($"Running query, query id is {queryRes.QueryId}, logGroupNamePrefix is {logGroupNamePrefix}");

            QueryStatus currentQueryStatus = QueryStatus.Running;
            List<List<ResultField>> finalResults = new List<List<ResultField>>();

            while (currentQueryStatus == QueryStatus.Running || currentQueryStatus == QueryStatus.Scheduled)
            {
                context.Logger.LogLine($"Retrieving query results, logGroupNamePrefix is {logGroupNamePrefix}");

                var queryResults = await _cloudWatchLogsClient.GetQueryResultsAsync(new GetQueryResultsRequest
                {
                    QueryId = queryRes.QueryId
                });

                context.Logger.LogLine($"Query result status is {queryResults.Status}, logGroupNamePrefix is {logGroupNamePrefix}");

                currentQueryStatus = queryResults.Status;
                finalResults = queryResults.Results;

                await Task.Delay(TimeSpan.FromSeconds(5));
            }

            context.Logger.LogLine($"Final results: {finalResults.Count} row(s), logGroupNamePrefix is {logGroupNamePrefix}");

            return finalResults;
        }

        private async Task SendLogsAsMetrics(QueryResultWrapper wrapper)
        {
            var RuntimeTypeDimension = new Dimension
            {
                Name = "RuntimeType",
                Value = wrapper.LoadTestType
            };

            var cold50 = new MetricDatum
            {
                Dimensions = new List<Dimension> {
                    RuntimeTypeDimension,
                    new Dimension
                    {
                        Name = "Measurement",
                        Value = "P50"
                    }
                },
                MetricName = "Cold Start",
                Unit = Amazon.CloudWatch.StandardUnit.Milliseconds,
                Value = double.Parse(wrapper.ColdStart.P50)
            };

            var cold90 = new MetricDatum
            {
                Dimensions = new List<Dimension> {
                    RuntimeTypeDimension,
                    new Dimension
                    {
                        Name = "Measurement",
                        Value = "P90"
                    }
                },
                MetricName = "Cold Start",
                Unit = Amazon.CloudWatch.StandardUnit.Milliseconds,
                Value = double.Parse(wrapper.ColdStart.P90)
            };

            var cold99 = new MetricDatum
            {
                Dimensions = new List<Dimension> {
                    RuntimeTypeDimension,
                    new Dimension
                    {
                        Name = "Measurement",
                        Value = "P99"
                    }
                },
                MetricName = "Cold Start",
                Unit = Amazon.CloudWatch.StandardUnit.Milliseconds,
                Value = double.Parse(wrapper.ColdStart.P99)
            };

            var coldMax = new MetricDatum
            {
                Dimensions = new List<Dimension> {
                    RuntimeTypeDimension,
                    new Dimension
                    {
                        Name = "Measurement",
                        Value = "Max"
                    }
                },
                MetricName = "Cold Start",
                Unit = Amazon.CloudWatch.StandardUnit.Milliseconds,
                Value = double.Parse(wrapper.ColdStart.Max)
            };

            var coldCount = new MetricDatum
            {
                Dimensions = new List<Dimension> {
                    RuntimeTypeDimension,
                    new Dimension
                    {
                        Name = "Measurement",
                        Value = "Count"
                    }
                },
                MetricName = "Cold Start",
                Unit = Amazon.CloudWatch.StandardUnit.Count,
                Value = double.Parse(wrapper.ColdStart.Count)
            };

            var warm50 = new MetricDatum
            {
                Dimensions = new List<Dimension> {
                    RuntimeTypeDimension,
                    new Dimension
                    {
                        Name = "Measurement",
                        Value = "P50"
                    }
                },
                MetricName = "Warm Start",
                Unit = Amazon.CloudWatch.StandardUnit.Milliseconds,
                Value = double.Parse(wrapper.WarmStart.P50)
            };

            var warm90 = new MetricDatum
            {
                Dimensions = new List<Dimension> {
                    RuntimeTypeDimension,
                    new Dimension
                    {
                        Name = "Measurement",
                        Value = "P90"
                    }
                },
                MetricName = "Warm Start",
                Unit = Amazon.CloudWatch.StandardUnit.Milliseconds,
                Value = double.Parse(wrapper.WarmStart.P90)
            };

            var warm99 = new MetricDatum
            {
                Dimensions = new List<Dimension> {
                    RuntimeTypeDimension,
                    new Dimension
                    {
                        Name = "Measurement",
                        Value = "P99"
                    }
                },
                MetricName = "Warm Start",
                Unit = Amazon.CloudWatch.StandardUnit.Milliseconds,
                Value = double.Parse(wrapper.WarmStart.P99)
            };

            var warmMax = new MetricDatum
            {
                Dimensions = new List<Dimension> {
                    RuntimeTypeDimension,
                    new Dimension
                    {
                        Name = "Measurement",
                        Value = "Max"
                    }
                },
                MetricName = "Warm Start",
                Unit = Amazon.CloudWatch.StandardUnit.Milliseconds,
                Value = double.Parse(wrapper.WarmStart.Max)
            };

            var warmCount = new MetricDatum
            {
                Dimensions = new List<Dimension> {
                    RuntimeTypeDimension,
                    new Dimension
                    {
                        Name = "Measurement",
                        Value = "Count"
                    }
                },
                MetricName = "Warm Start",
                Unit = Amazon.CloudWatch.StandardUnit.Count,
                Value = double.Parse(wrapper.WarmStart.Count)
            };

            var request = new PutMetricDataRequest
            {
                MetricData = new List<MetricDatum> { cold50, cold90, cold99, coldMax, coldCount, warm50, warm90, warm99, warmMax, warmCount },
                Namespace = "DotnetPerformanceMetrics"
            };

            await _cloudWatchClient.PutMetricDataAsync(request);
        }

        private class ProcessLogGroupResult
        {
            public string LogGroupPrefix { get; set; }
            public bool Success { get; set; }
            public string Exception { get; set; }
            public string WarmCount { get; set; }
            public string ColdCount { get; set; }
        }
    }
}
