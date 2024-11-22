using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.Lambda.Core;

namespace GetProducts;

public static class CloudWatchQueryExecution
{
    public static async Task<List<List<ResultField>>> RunQuery(AmazonCloudWatchLogsClient cloudWatchLogsClient)
    {
        var logGroupNamePrefix =
            $"{Environment.GetEnvironmentVariable("LOG_GROUP_PREFIX")}{Environment.GetEnvironmentVariable("LAMBDA_ARCHITECTURE")}"
                .Replace("_", "-");

        var logGroupList = await cloudWatchLogsClient.DescribeLogGroupsAsync(new DescribeLogGroupsRequest()
        {
            LogGroupNamePrefix = logGroupNamePrefix,
        });

        var queryRes = await cloudWatchLogsClient.StartQueryAsync(new StartQueryRequest()
        {
            LogGroupNames = logGroupList.LogGroups.Select(p => p.LogGroupName).ToList(),
            QueryString =
                "filter @type=\"REPORT\" | fields greatest(@initDuration, 0) + @duration as duration, ispresent(@initDuration) as coldstart | stats count(*) as count, pct(duration, 50) as p50, pct(duration, 90) as p90, pct(duration, 99) as p99, max(duration) as max by coldstart",
            StartTime = DateTime.Now.AddMinutes(-20).AsUnixTimestamp(),
            EndTime = DateTime.Now.AsUnixTimestamp(),
        });

        QueryStatus currentQueryStatus = QueryStatus.Running;
        List<List<ResultField>> finalResults = new List<List<ResultField>>();

        while (currentQueryStatus == QueryStatus.Running || currentQueryStatus == QueryStatus.Scheduled)
        {
            var queryResults = await cloudWatchLogsClient.GetQueryResultsAsync(new GetQueryResultsRequest()
            {
                QueryId = queryRes.QueryId
            });

            currentQueryStatus = queryResults.Status;
            finalResults = queryResults.Results;

            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        return finalResults;
    }
}