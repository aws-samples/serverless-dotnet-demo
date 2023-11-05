#Arguments:
#$1 - load test duration in seconds
#$2 - log interval to be used in the cloudwatch query in minutes
#$3 - when equal to 1 cloudwatch log group will be deleted to ensure that only logs of the load test will be evaluated for stat
STACK_NAME=dotnet6-minimal-api-web-adapter
TEST_DURATIOMN_SEC=60
LOG_INTERVAL_MIN=20
LOG_DELETE=1

COLOR='\033[0;33m'
NO_COLOR='\033[0m' # No Color

if [ "x$1" != x ];  
then
  TEST_DURATIOMN_SEC=$1
fi

if [ "x$2" != x ];  
then
  LOG_INTERVAL_MIN=$2
fi

if [ "x$3" != x ];  
then
  LOG_DELETE=$3
fi

echo "${COLOR}"
echo --------------------------------------------
echo DURATION:$TEST_DURATIOMN_SEC
echo LOG INTERVAL:$LOG_INTERVAL_MIN
echo LOG_DELETE: $LOG_DELETE
echo --------------------------------------------
echo "${NO_COLOR}"

mkdir Report -p

function RunLoadTest()
{
  #Params:
  #$1 - Architecture (x86 or arm64).Used for logging and naming report file
  #$2 - Stack output name to get API Url
  #$3 - Stack output name to get lambda name

  #get test params from cloud formation output
  echo "${COLOR}"
  API_URL=$(aws cloudformation describe-stacks --stack-name $STACK_NAME \
    --query "Stacks[0].Outputs[?OutputKey=='$2'].OutputValue" \
    --output text)
  echo API URL: $API_URL

  LAMBDA=$(aws cloudformation describe-stacks --stack-name $STACK_NAME \
  --query "Stacks[0].Outputs[?OutputKey=='$3'].OutputValue" \
  --output text)
  echo LAMBDA: $LAMBDA

  if [ $LOG_DELETE == "1" ];  
  then
    echo --------------------------------------------
    echo DELETING CLOUDWATCH LOG GROUP /aws/lambda/$LAMBDA
    echo --------------------------------------------
    aws logs delete-log-group --log-group-name /aws/lambda/$LAMBDA
    echo ---------------------------------------------
    echo Waiting 10 sec. for deletion to complete
    echo --------------------------------------------
    sleep 10
  fi
  
  #run load test with artillery
  echo --------------------------------------------
  echo $1 RUNNING LOAD TEST $TEST_DURATIOMN_SEC sec $LAMBDA: $API_URL
  echo --------------------------------------------
  echo "${NO_COLOR}"
  artillery run \
    --target "$API_URL" \
    --overrides '{"config": { "phases": [{ "duration": '$TEST_DURATIOMN_SEC', "arrivalRate": 100 }] } }'  \
    --quiet \
    ../../loadtest/load-test.yml 

  echo "${COLOR}"
  echo --------------------------------------------
  echo Waiting 10 sec. for logs to consolidate
  echo --------------------------------------------
  sleep 10

  #get stats from cloudwatch
  enddate=$(date "+%s")
  startdate=$(($enddate-($LOG_INTERVAL_MIN*60)))
  echo --------------------------------------------
  echo Log start:$startdate end:$enddate
  echo --------------------------------------------

  QUERY_ID=$(aws logs start-query \
    --log-group-name /aws/lambda/$LAMBDA \
    --start-time $startdate \
    --end-time $enddate \
    --query-string 'filter @type="REPORT" | fields greatest(@initDuration, 0) + @duration as duration, ispresent(@initDuration) as coldstart | stats count(*) as count, pct(duration, 50) as p50, pct(duration, 90) as p90, pct(duration, 99) as p99, max(duration) as max by coldstart' \
    | jq -r '.queryId')
  
  echo --------------------------------------------
  echo Query started, id: $QUERY_ID
  echo --------------------------------------------

  echo ---------------------------------------------
  echo Waiting 10 sec. for cloudwatch query to complete
  echo --------------------------------------------
  sleep 10

  echo --------------------------------------------
  echo RESULTS $LAMBDA
  echo --------------------------------------------
  echo "${NO_COLOR}"
  date > ./Report/load-test-report-$1.txt
  echo $1 RESULTS lambda: $LAMBDA >> ./Report/load-test-report-$1.txt
  echo Test duration sec: $TEST_DURATIOMN_SEC >> ./Report/load-test-report-$1.txt
  echo Log interval min: $LOG_INTERVAL_MIN >> ./Report/load-test-report-$1.txt
  aws logs get-query-results --query-id $QUERY_ID --output text >> ./Report/load-test-report-$1.txt
  cat ./Report/load-test-report-$1.txt
  aws logs get-query-results --query-id $QUERY_ID --output json >> ./Report/load-test-report-$1.json
}

RunLoadTest x86 ApiUrlX86 LambdaX86Name
RunLoadTest arm64 ApiUrlArm64 LambdaArm64Name