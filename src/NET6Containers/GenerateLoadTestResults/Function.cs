using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GenerateLoadTestResults
{
    public class Function
    {
        private AmazonCloudWatchLogsClient _cloudWatchLogsClient;

        public Function()
        {
            this._cloudWatchLogsClient = new AmazonCloudWatchLogsClient();
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(
            APIGatewayHttpApiV2ProxyRequest apigProxyEvent,
            ILambdaContext context)
        {
            if (!apigProxyEvent.RequestContext.Http.Method.Equals(HttpMethod.Get.Method))
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Only GET allowed",
                    StatusCode = (int) HttpStatusCode.MethodNotAllowed,
                };
            }

            try
            {
                var resultRows = 0;
                var queryCount = 0;

                List<List<ResultField>> finalResults = new List<List<ResultField>>();

                while (resultRows < 2 || queryCount >= 3)
                {
                    finalResults = await runQuery(context);

                    resultRows = finalResults.Count;
                    queryCount++;
                }

                var wrapper = new QueryResultWrapper()
                {
                    LoadTestType =
                        $"{Environment.GetEnvironmentVariable("LOAD_TEST_TYPE")} ({Environment.GetEnvironmentVariable("LAMBDA_ARCHITECTURE")})",
                    WarmStart = new QueryResult()
                    {
                        Count = finalResults[0][1].Value,
                        P50 = finalResults[0][2].Value,
                        P90 = finalResults[0][3].Value,
                        P99 = finalResults[0][4].Value,
                        Max = finalResults[0][5].Value,
                    },
                    ColdStart = new QueryResult()
                    {
                        Count = finalResults[1][1].Value,
                        P50 = finalResults[1][2].Value,
                        P90 = finalResults[1][3].Value,
                        P99 = finalResults[1][4].Value,
                        Max = finalResults[1][5].Value,
                    }
                };

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = (int) HttpStatusCode.OK,
                    Body = wrapper.AsMarkdownTableRow(),
                    Headers = new Dictionary<string, string> {{"Content-Type", "text/html"}}
                };
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"Error retrieving results {e.Message} {e.StackTrace}");

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Not Found",
                    StatusCode = (int) HttpStatusCode.InternalServerError,
                };
            }
        }

        private async Task<List<List<ResultField>>> runQuery(ILambdaContext context)
        {
            var logGroupNamePrefix =
                $"{Environment.GetEnvironmentVariable("LOG_GROUP_PREFIX")}{Environment.GetEnvironmentVariable("LAMBDA_ARCHITECTURE")}"
                    .Replace("_", "-");

            context.Logger.LogLine($"Retrieving log groups with prefix {logGroupNamePrefix}");

            var logGroupList = await _cloudWatchLogsClient.DescribeLogGroupsAsync(new DescribeLogGroupsRequest()
            {
                LogGroupNamePrefix = logGroupNamePrefix,
            });

            context.Logger.LogLine($"Found {logGroupList.LogGroups.Count} log group(s)");

            var queryRes = await _cloudWatchLogsClient.StartQueryAsync(new StartQueryRequest()
            {
                LogGroupNames = logGroupList.LogGroups.Select(p => p.LogGroupName).ToList(),
                QueryString =
                    "filter @type=\"REPORT\" | fields greatest(@initDuration, 0) + @duration as duration, ispresent(@initDuration) as coldstart | stats count(*) as count, pct(duration, 50) as p50, pct(duration, 90) as p90, pct(duration, 99) as p99, max(duration) as max by coldstart",
                StartTime = DateTime.Now.AddMinutes(-20).AsUnixTimestamp(),
                EndTime = DateTime.Now.AsUnixTimestamp(),
            });

            context.Logger.LogLine($"Running query, query id is {queryRes.QueryId}");

            QueryStatus currentQueryStatus = QueryStatus.Running;
            List<List<ResultField>> finalResults = new List<List<ResultField>>();

            while (currentQueryStatus == QueryStatus.Running || currentQueryStatus == QueryStatus.Scheduled)
            {
                context.Logger.LogLine("Retrieving query results");

                var queryResults = await _cloudWatchLogsClient.GetQueryResultsAsync(new GetQueryResultsRequest()
                {
                    QueryId = queryRes.QueryId
                });

                context.Logger.LogLine($"Query result status is {queryResults.Status}");

                currentQueryStatus = queryResults.Status;
                finalResults = queryResults.Results;

                await Task.Delay(TimeSpan.FromSeconds(5));
            }

            context.Logger.LogLine($"Final results: {finalResults.Count} row(s)");

            return finalResults;
        }
    }
}