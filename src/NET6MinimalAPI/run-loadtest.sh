STACK_NAME=dotnet6-minimal-api
TEST_DURATIOMN_SEC=60
LOG_INTERVALL_MIN=20

C='\033[0;33m'
NC='\033[0m' # No Color

if [ x"${LT_TEST_DURATIOMN_SEC}" != "x" ];  
then
  TEST_DURATIOMN_SEC=$LT_TEST_DURATIOMN_SEC
fi

if [ x"${LT_LOG_INTERVALL_MIN}" != "x" ];  
then
  LOG_INTERVALL_MIN=$LT_LOG_INTERVALL_MIN
fi

#get test params:
API_URL_X86=$(aws cloudformation describe-stacks --stack-name $STACK_NAME \
  --query 'Stacks[0].Outputs[?OutputKey==`ApiUrlX86`].OutputValue' \
  --output text)

API_URL_ARM=$(aws cloudformation describe-stacks --stack-name $STACK_NAME \
  --query 'Stacks[0].Outputs[?OutputKey==`ApiUrlArm64`].OutputValue' \
  --output text)

LAMBDA_X86=$(aws cloudformation describe-stacks --stack-name $STACK_NAME \
  --query 'Stacks[0].Outputs[?OutputKey==`LambdaX86Name`].OutputValue' \
  --output text)

LAMBDA_ARM64=$(aws cloudformation describe-stacks --stack-name $STACK_NAME \
  --query 'Stacks[0].Outputs[?OutputKey==`LambdaArm64Name`].OutputValue' \
  --output text)

#running load tests

echo "${C}--------------------------------------------"
echo RUNNING X86 LOAD TEST $TEST_DURATIOMN_SEC sec $LAMBDA_X86: $API_URL_X86
echo "--------------------------------------------${NC}"
artillery run \
  --target "$API_URL_X86" \
  --overrides '{"config": { "phases": [{ "duration": '$TEST_DURATIOMN_SEC', "arrivalRate": 100 }] } }'  \
  --quiet \
  ../../loadtest/load-test.yml 

echo "${C}--------------------------------------------"
echo RUNNING ARM LOAD TEST $TEST_DURATIOMN_SEC sec $LAMBDA_ARM64: $API_URL_ARM
echo "--------------------------------------------${NC}"

artillery run \
  --target "$API_URL_ARM" \
  --overrides '{"config": { "phases": [{ "duration": '$TEST_DURATIOMN_SEC', "arrivalRate": 100 }] } }'  \
  --quiet \
  ../../loadtest/load-test.yml

echo "${C}--------------------------------------------"
echo Waiting 10 sec. for logs to consolidate
echo "--------------------------------------------${NC}"
sleep 10

#get stats from cloudwatch

enddate=$(date "+%s")
startdate=$(($enddate-($LOG_INTERVALL_MIN*60)))
echo "${C}--------------------------------------------"
echo start:$startdate end:$enddate
echo "--------------------------------------------${NC}"

QUERY_X86_ID=$(aws logs start-query \
 --log-group-name /aws/lambda/$LAMBDA_X86 \
 --start-time $startdate \
 --end-time $enddate \
 --query-string 'filter @type="REPORT" | fields greatest(@initDuration, 0) + @duration as duration, ispresent(@initDuration) as coldstart | stats count(*) as count, pct(duration, 50) as p50, pct(duration, 90) as p90, pct(duration, 99) as p99, max(duration) as max by coldstart' \
 | jq -r '.queryId')

echo "${C}--------------------------------------------"
echo Query started for x86 lambda $LAMBDA_X86 id: $QUERY_X86_ID
echo "--------------------------------------------${NC}"

QUERY_ARM64_ID=$(aws logs start-query \
 --log-group-name /aws/lambda/$LAMBDA_ARM64 \
 --start-time $startdate \
 --end-time $enddate \
 --query-string 'filter @type="REPORT" | fields greatest(@initDuration, 0) + @duration as duration, ispresent(@initDuration) as coldstart | stats count(*) as count, pct(duration, 50) as p50, pct(duration, 90) as p90, pct(duration, 99) as p99, max(duration) as max by coldstart' \
 | jq -r '.queryId')

echo "${C}--------------------------------------------"
echo Query started for Arm64 $LAMBDA_ARM64 id: $QUERY_ARM64_ID
echo "--------------------------------------------${NC}"

echo "${C}--------------------------------------------"
echo Waiting 10 sec. for queries to complete
echo "--------------------------------------------${NC}"
sleep 10

#saving stats to files
echo "${C}--------------------------------------------"
echo X86 RESULTS $LAMBDA_X86 id: $QUERY_X86_ID
echo "--------------------------------------------${NC}"
date > ./Report/load-test-report-x86.txt
echo X86 RESULTS lambda: $LAMBDA_X86 >> ./Report/load-test-report-x86.txt
echo Test duration sec: $TEST_DURATIOMN_SEC >> ./Report/load-test-report-x86.txt
echo Log intervall min: $LOG_INTERVALL_MIN >> ./Report/load-test-report-x86.txt
aws logs get-query-results --query-id $QUERY_X86_ID --output text >> ./Report/load-test-report-x86.txt
cat ./Report/load-test-report-x86.txt


echo "${C}--------------------------------------------"
echo X64 RESULTS $LAMBDA_ARM64 id: $QUERY_ARM64_ID 
echo "--------------------------------------------${NC}"
date > ./Report/load-test-report-arm64.txt
echo X86 RESULTS lambda: $LAMBDA_X86 >> ./Report/load-test-report-arm64.txt
echo Test duration sec: $TEST_DURATIOMN_SEC >> ./Report/load-test-report-arm64.txt
echo Log intervall min:: $LOG_INTERVALL_MIN >> ./Report/load-test-report-arm64.txt
aws logs get-query-results --query-id $QUERY_ARM64_ID --output text >> ./Report/load-test-report-arm64.txt
cat ./Report/load-test-report-arm64.txt
