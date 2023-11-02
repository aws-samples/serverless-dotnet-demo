using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Microsoft.Extensions.Logging;

namespace GetProducts;
public static class CloudWatchQueryExecution
{
    const int DELAY_SEC=1;
    public static async Task<List<List<ResultField>>> RunQuery(AmazonCloudWatchLogsClient cloudWatchLogsClient,Microsoft.Extensions.Logging.ILogger logger)
    {
        string cwQuery="filter @type=\"REPORT\" | fields greatest(@initDuration, 0) + @duration as duration, ispresent(@initDuration) as coldstart | stats count(*) as count, pct(duration, 50) as p50, pct(duration, 90) as p90, pct(duration, 99) as p99, max(duration) as max by coldstart";
        long cwStartTime=((DateTimeOffset)DateTime.Now.AddMinutes(-20)).ToUnixTimeSeconds();
        long cwEndTime=((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();

        logger.LogInformation($"Starting Query: {cwQuery} - StartTime:{cwStartTime} - EndTime:{cwEndTime}");
        
        var queryRes = await cloudWatchLogsClient.StartQueryAsync(new StartQueryRequest()
        {
            LogGroupNames = {Environment.GetEnvironmentVariable("AWS_LAMBDA_LOG_GROUP_NAME")},
            QueryString = cwQuery,
            StartTime = cwStartTime,
            EndTime = cwEndTime,
        });
        int totalWaitSec=0;
        while(true) //why not having a limit?
        {
            var queryResults = await cloudWatchLogsClient.GetQueryResultsAsync(new GetQueryResultsRequest()
            {
                QueryId = queryRes.QueryId
            });
            
            logger.LogInformation($"Query id:{queryRes.QueryId} status: {queryResults.Status}");

            bool keepWaiting=queryResults.Status == QueryStatus.Running || queryResults.Status == QueryStatus.Scheduled;
            if(keepWaiting)
            {
                totalWaitSec+=DELAY_SEC;
                logger.LogInformation($"Wait {DELAY_SEC} sec (waited {totalWaitSec}) status: {queryResults.Status}");
                await Task.Delay(TimeSpan.FromSeconds(DELAY_SEC));
            }   
            else
                return queryResults.Results;
        }
    }
}