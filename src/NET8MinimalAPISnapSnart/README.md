# Comparing performance with SnapStart enabled or disabled

1. Navigate to code directory
```
C:\projects\aws-samples\serverless-dotnet-demo\src\NET8MinimalAPISnapSnart>
```

2. Deploy this project using the SAM Cli:

```
sam build && sam deploy
```

3. SAM will output the root API Gateway endpoint URL:

```
---------------------------------------------------------------------------------------------------------------------
Outputs
---------------------------------------------------------------------------------------------------------------------
Key                 ApiUrl
Description         API Gateway endpoint URL
Value               https://123example.execute-api.us-west-2.amazonaws.com/

```
4. Initialize Lambdas:
```
$baseUrl = "https://123example.execute-api.us-west-2.amazonaws.com/"

Invoke-WebRequest $baseUrl/regular/test
Invoke-WebRequest $baseUrl/snapstart/test

Sleep -Seconds 1

Invoke-WebRequest $baseUrl/regular/test
Invoke-WebRequest $baseUrl/snapstart/test
```

5. Run Artillery

```
cd ..\..\loadtest\

artillery run load-test.yml --target "$baseUrl/snapstart"

artillery run load-test.yml --target "$baseUrl/regular"
```

6. Analyze the Results

In the AWS Console, navigate to Cloud Watch Log Insights.

Select the Log Groups for the `ApiWithSnapstart` and `ApiWithoutSnapstart` lambda functions.

Run the following Query:

```
filter @type = "REPORT"
  | parse @log /\d+:\/aws\/lambda\/(?<function>.*)/
  | parse @message /Restore Duration: (?<restoreDuration>.*?) ms/
  | stats
count(*) as invocations,
pct(coalesce(@initDuration,0)+coalesce(restoreDuration,0), 50) as p50,
pct(coalesce(@initDuration,0)+coalesce(restoreDuration,0), 90) as p90,
pct(coalesce(@initDuration,0)+coalesce(restoreDuration,0), 99) as p99,
pct(coalesce(@initDuration,0)+coalesce(restoreDuration,0), 99.9) as p99.9,
pct(@duration, 50) as durP50,
pct(@duration, 90) as durP90,
pct(@duration, 99) as durP99,
pct(@duration, 99.9) as durP99.9
group by function, ispresent(restoreDuration) as restore, (ispresent(@initDuration) or ispresent(restoreDuration)) as coldstart
  | sort by coldstart desc
```

The results table will show the difference in Duration and Initialization for coldstarts when Snapstart is disabled or enabled.  It also shows warm start time for reference.
