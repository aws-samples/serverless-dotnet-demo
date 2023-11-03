#Ensure to update the stack name
STACK_NAME=dotnet6-minimal-api-web-adapter

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

C='\033[0;33m'
NC='\033[0m' # No Color

echo "${C}--------------------------------------------"
echo RUNNING X86 LOAD TEST $LAMBDA_X86: $API_URL_X86
echo "--------------------------------------------${NC}"
artillery run ../../loadtest/load-test.yml --target "$API_URL_X86"

echo "${C}--------------------------------------------"
echo RUNNING ARM LOAD TEST $LAMBDA_ARM64: $API_URL_ARM
echo "--------------------------------------------${NC}"
artillery run ../../loadtest/load-test.yml --target "$API_URL_ARM"

echo "${C}--------------------------------------------"
echo "Waiting 10 sec. for logs to consolidate" && sleep 10 # give it some time to query


QUERY_X86_ID=$(aws logs start-query \
 --log-group-name /aws/lambda/$LAMBDA_X86 \
 --start-time `date -v-20M "+%s"` \
 --end-time `date "+%s"` \
 --query-string 'filter @type="REPORT" | fields greatest(@initDuration, 0) + @duration as duration, ispresent(@initDuration) as coldstart | stats count(*) as count, pct(duration, 50) as p50, pct(duration, 90) as p90, pct(duration, 99) as p99, max(duration) as max by coldstart' \
 | jq -r '.queryId')

echo "Query started for x86 lambda $LAMBDA_X86 id: $QUERY_X86_ID" 

QUERY_ARM64_ID=$(aws logs start-query \
 --log-group-name /aws/lambda/$LAMBDA_ARM64 \
 --start-time `date -v-20M "+%s"` \
 --end-time `date "+%s"` \
 --query-string 'filter @type="REPORT" | fields greatest(@initDuration, 0) + @duration as duration, ispresent(@initDuration) as coldstart | stats count(*) as count, pct(duration, 50) as p50, pct(duration, 90) as p90, pct(duration, 99) as p99, max(duration) as max by coldstart' \
 | jq -r '.queryId')

echo "Query started for Arm64 $LAMBDA_ARM64 id: $QUERY_ARM64_ID" 
echo "Waiting 10 sec. for queries to complete" && sleep 10

echo "${C}--------------------------------------------"
echo X86 RESULTS $LAMBDA_X86 id: $QUERY_X86_ID
echo "--------------------------------------------${NC}"
date > ./Report/load-test-report-x86.txt
aws logs get-query-results --query-id $QUERY_X86_ID --output text >> ./Report/load-test-report-x86.txt
cat ./Report/load-test-report-x86.txt

echo "${C}--------------------------------------------"
echo X64 RESULTS $LAMBDA_ARM64 id: $QUERY_ARM64_ID 
echo "--------------------------------------------${NC}"
date > ./Report/load-test-report-arm64.txt
aws logs get-query-results --query-id $QUERY_ARM64_ID --output text >> ./Report/load-test-report-arm64.txt
cat ./Report/load-test-report-arm64.txt