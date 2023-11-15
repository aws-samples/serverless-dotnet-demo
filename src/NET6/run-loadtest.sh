#Arguments:
#$1 - load test duration in seconds
#$2 - log interval to be used in the cloudwatch query in minutes
#$3 - when equal to yes cloudwatch log group will be deleted to ensure that only logs of the load test will be evaluated for stat
#$4 - ARN of sns topic to notify test results

STACK_NAME=dotnet6
TEST_DURATIOMN_SEC=60
LOG_INTERVAL_MIN=20
LOG_DELETE=yes

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

if [ "x$4" != x ];  
then
  SNS_TOPIC_ARN=$4
fi

echo "${COLOR}"
echo --------------------------------------------
echo DURATION:$TEST_DURATIOMN_SEC
echo LOG INTERVAL:$LOG_INTERVAL_MIN
echo LOG_DELETE: $LOG_DELETE
echo SNS_TOPIC_ARN: $SNS_TOPIC_ARN
echo --------------------------------------------
echo "${NO_COLOR}"

mkdir -p Report 

function RunLoadTest()
{
  #Params:
  #$1 - Architecture (x86 or arm64).Used for logging and naming report file
  #$2 - Stack output name to get API Url
  #$3 - Stack output name to get lambda name GetProducts
  #$4 - Stack output name to get lambda name GetProduct
  #$5 - Stack output name to get lambda name DeleteProduct
  #$6 - Stack output name to get lambda name PutProduct

  #get test params from cloud formation output
  echo "${COLOR}"
  export API_URL=$(aws cloudformation describe-stacks --stack-name $STACK_NAME \
    --query "Stacks[0].Outputs[?OutputKey=='$2'].OutputValue" \
    --output text)
  echo API URL: $API_URL

  LAMBDA_GETPRODUCTS=$(aws cloudformation describe-stacks --stack-name $STACK_NAME \
  --query "Stacks[0].Outputs[?OutputKey=='$3'].OutputValue" \
  --output text)
  echo LAMBDA: $LAMBDA_GETPRODUCTS

  LAMBDA_GETPRODUCT=$(aws cloudformation describe-stacks --stack-name $STACK_NAME \
  --query "Stacks[0].Outputs[?OutputKey=='$4'].OutputValue" \
  --output text)
  echo LAMBDA: $LAMBDA_GETPRODUCT

  LAMBDA_DELETEPRODUCT=$(aws cloudformation describe-stacks --stack-name $STACK_NAME \
  --query "Stacks[0].Outputs[?OutputKey=='$5'].OutputValue" \
  --output text)
  echo LAMBDA: $LAMBDA_DELETEPRODUCT

  LAMBDA_PUTPRODUCT=$(aws cloudformation describe-stacks --stack-name $STACK_NAME \
  --query "Stacks[0].Outputs[?OutputKey=='$6'].OutputValue" \
  --output text)
  echo LAMBDA: $LAMBDA_PUTPRODUCT

  if [ $LOG_DELETE == "yes" ];  
  then
    echo --------------------------------------------
    echo DELETING CLOUDWATCH LOG GROUP /aws/lambda/$LAMBDA_GETPRODUCTS
    echo --------------------------------------------
    aws logs delete-log-group --log-group-name /aws/lambda/$LAMBDA_GETPRODUCTS
    echo --------------------------------------------
    echo DELETING CLOUDWATCH LOG GROUP /aws/lambda/$LAMBDA_GETPRODUCT
    echo --------------------------------------------
    aws logs delete-log-group --log-group-name /aws/lambda/$LAMBDA_GETPRODUCT
    echo --------------------------------------------
    echo DELETING CLOUDWATCH LOG GROUP /aws/lambda/$LAMBDA_DELETEPRODUCT
    echo --------------------------------------------
    aws logs delete-log-group --log-group-name /aws/lambda/$LAMBDA_DELETEPRODUCT
    echo --------------------------------------------
    echo DELETING CLOUDWATCH LOG GROUP /aws/lambda/$LAMBDA_PUTPRODUCT
    echo --------------------------------------------
    aws logs delete-log-group --log-group-name /aws/lambda/$LAMBDA_PUTPRODUCT
    echo ---------------------------------------------
    echo Waiting 10 sec. for deletion to complete
    echo --------------------------------------------
    sleep 10
  fi
  
  #run load test with artillery
  echo --------------------------------------------
  echo $1 RUNNING LOAD TEST $TEST_DURATIOMN_SEC sec $API_URL
  echo --------------------------------------------
  echo "${NO_COLOR}"
  artillery run \
    --overrides '{"config": { "phases": [{ "duration": '$TEST_DURATIOMN_SEC', "arrivalRate": 100 }] } }'  \
    --quiet \
    ../../loadtest/codebuild/load-test.yml 
  
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

  echo --------------------------------------------
  echo GET ERROR METRICS $1
  echo --------------------------------------------
  aws cloudwatch get-metric-statistics \
    --namespace AWS/Lambda \
    --metric-name Errors \
    --dimensions Name=FunctionName,Value=$LAMBDA_GETPRODUCTS Name=FunctionName,Value=$LAMBDA_GETPRODUCT Name=FunctionName,Value=$LAMBDA_DELETEPRODUCT Name=FunctionName,Value=$LAMBDA_PUTPRODUCT \
    --statistics Sum --period 43200 \
    --start-time $startdate --end-time $enddate > ./Report/load-test-errors-$1.json
  
  cat ./Report/load-test-errors-$1.json

  QUERY_ID=$(aws logs start-query \
    --log-group-names "/aws/lambda/$LAMBDA_GETPRODUCTS" "/aws/lambda/$LAMBDA_GETPRODUCT" "/aws/lambda/$LAMBDA_DELETEPRODUCT" "/aws/lambda/$LAMBDA_PUTPRODUCT"  \
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
  echo RESULTS $1
  echo --------------------------------------------
  echo "${NO_COLOR}"
  date > ./Report/load-test-report-$1.txt
  echo $1 RESULTS >> ./Report/load-test-report-$1.txt
  echo Test duration sec: $TEST_DURATIOMN_SEC >> ./Report/load-test-report-$1.txt
  echo Log interval min: $LOG_INTERVAL_MIN >> ./Report/load-test-report-$1.txt
  aws logs get-query-results --query-id $QUERY_ID --output text >> ./Report/load-test-report-$1.txt
  cat ./Report/load-test-report-$1.txt
  aws logs get-query-results --query-id $QUERY_ID --output json >> ./Report/load-test-report-$1.json

  if [ "x$SNS_TOPIC_ARN" != x ];  
  then
    echo --------------------------------------------
    echo Sending message to sns topic: $SNS_TOPIC_ARN
    echo --------------------------------------------
    msg=$(<./Report/load-test-report-$1.txt)\n\n$(<./Report/load-test-errors-$1.json)
    subject="serverless dotnet demo load test result for $LAMBDA_GETPRODUCTS"
    aws sns publish --topic-arn $SNS_TOPIC_ARN --subject "$subject" --message "$msg"
  fi
}

RunLoadTest x86 ApiUrlX86 LambdaX86NameGetProducts LambdaX86NameGetProduct LambdaX86NameDeleteProduct LambdaX86NamePutProduct
RunLoadTest arm64 ApiUrlArm64 LambdaArm64NameGetProducts LambdaArm64NameGetProduct LambdaArm64NameDeleteProduct LambdaArm64NamePutProduct