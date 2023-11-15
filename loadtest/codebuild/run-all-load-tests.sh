TEST_DURATIOMN_SEC=60
LOG_INTERVAL_MIN=20
LOG_DELETE=yes
DELETE_STACK=yes
ECR_URI=NotSet

if [ "x${LT_TEST_DURATIOMN_SEC}" != x ];  
then
  TEST_DURATIOMN_SEC=$LT_TEST_DURATIOMN_SEC
fi

if [ "x${LT_LOG_INTERVAL_MIN}" != x ];  
then
  LOG_INTERVAL_MIN=$LT_LOG_INTERVAL_MIN
fi

if [ "x${LT_LOG_DELETE}" != x ];  
then
  LOG_DELETE=$LT_LOG_DELETE
fi

if [ "x${LT_DELETE_STACK}" != x ];  
then
  DELETE_STACK=$LT_DELETE_STACK
fi

if [ "x${LT_ECR_URI}" != x ];  
then
  ECR_URI=$LT_ECR_URI
fi


echo --------------------------------------------
echo TEST_DURATIOMN_SEC: $TEST_DURATIOMN_SEC
echo LOG_INTERVAL_MIN: $LOG_INTERVAL_MIN
echo LOG_DELETE: $LOG_DELETE
echo DELETE_STACK: $DELETE_STACK
echo ECR_URI: $ECR_URI
echo LT_SNS_TOPIC_ARN: $LT_SNS_TOPIC_ARN
echo --------------------------------------------

if [ "$LT_NET6" != yes ];  
then
  echo SKIPPING net6 - LT_NET6=$LT_NET6
else
  echo "RUNNING load test for net6"
  cd ../../src/NET6/
  source ./deploy.sh $DELETE_STACK
  source ./run-loadtest.sh $TEST_DURATIOMN_SEC $LOG_INTERVAL_MIN $LOG_DELETE $LT_SNS_TOPIC_ARN
fi

if [ "$LT_NET6_CONTAINERS" != yes ];  
then
  echo SKIPPING net6 containers - LT_NET6_CONTAINERS=$LT_NET6_CONTAINERS
else
  echo "RUNNING load test for net6 containers"
  cd ../../src/NET6Containers/
  source ./deploy.sh $ECR_URI $DELETE_STACK
  source ./run-loadtest.sh $TEST_DURATIOMN_SEC $LOG_INTERVAL_MIN $LOG_DELETE $LT_SNS_TOPIC_ARN
fi

if [ "$LT_NET6_CUSTOM" != yes ];  
then
  echo SKIPPING net6 custom runtime - LT_NET6_CUSTOM=$LT_NET6_CUSTOM
else
  echo "RUNNING load test for net6 custom runtime"
  cd ../../src/NET6CustomRuntime/
  source ./deploy.sh $DELETE_STACK
  source ./run-loadtest.sh $TEST_DURATIOMN_SEC $LOG_INTERVAL_MIN $LOG_DELETE $LT_SNS_TOPIC_ARN
fi

if [ "$LT_NET6_MINIMAL_API" != yes ];  
then
  echo SKIPPING net6 minimal api - LT_NET6_MINIMAL_API=$LT_NET6_MINIMAL_API
else
  echo "RUNNING load test for net6 minimal api"
  cd ../../src/NET6MinimalAPI/
  source ./deploy.sh $DELETE_STACK
  source ./run-loadtest.sh $TEST_DURATIOMN_SEC $LOG_INTERVAL_MIN $LOG_DELETE $LT_SNS_TOPIC_ARN
fi

if [ "$LT_NET6_MINIMAL_API_WEB_ADAPTER" != yes ];  
then
  echo SKIPPING net6 minimal api web adapter - LT_NET6_MINIMAL_API_WEB_ADAPTER = $LT_NET6_MINIMAL_API_WEB_ADAPTER
else
  echo "RUNNING load test for net6 minimal api web adapter"
  cd ../../src/NET6MinimalAPIWebAdapter/
  source ./deploy.sh $DELETE_STACK
  source ./run-loadtest.sh $TEST_DURATIOMN_SEC $LOG_INTERVAL_MIN $LOG_DELETE $LT_SNS_TOPIC_ARN
fi

if [ "$LT_NET6_TOPLEVEL" != yes ];  
then
  echo SKIPPING net6 top level - LT_NET6_TOPLEVEL = $LT_NET6_TOPLEVEL
else
  echo "RUNNING load test for net6 top level"
  cd ../../src/NET6TopLevelStatements/
  source ./deploy.sh $DELETE_STACK
  source ./run-loadtest.sh $TEST_DURATIOMN_SEC $LOG_INTERVAL_MIN $LOG_DELETE $LT_SNS_TOPIC_ARN
fi

if [ "$LT_NET6_POWERTOOLS" != yes ];  
then
  echo SKIPPING net6 power tools - LT_NET6_POWERTOOLS = $LT_NET6_POWERTOOLS
else
  echo "RUNNING load test for net6 power tools"
  cd ../../src/NET6WithPowerTools/
  source ./deploy.sh $DELETE_STACK
  source ./run-loadtest.sh $TEST_DURATIOMN_SEC $LOG_INTERVAL_MIN $LOG_DELETE $LT_SNS_TOPIC_ARN
fi

if [ "$LT_NET8" != yes ];
then
  echo SKIPPING net8 - LT_NET8=$LT_NET8
else
  echo "RUNNING load test for net8"
  cd ../../src/NET8/
  source ./deploy.sh $DELETE_STACK
  source ./run-loadtest.sh $TEST_DURATIOMN_SEC $LOG_INTERVAL_MIN $LOG_DELETE $LT_SNS_TOPIC_ARN
fi

if [ "$LT_NET8_MINIMAL_API" != yes ];
then
  echo SKIPPING net8 minimal api - LT_NET8_MINIMAL_API=$LT_NET8_MINIMAL_API
else
  echo "RUNNING load test for net8 minimal api"
  cd ../../src/NET8MinimalAPI/
  source ./deploy.sh $DELETE_STACK
  source ./run-loadtest.sh $TEST_DURATIOMN_SEC $LOG_INTERVAL_MIN $LOG_DELETE $LT_SNS_TOPIC_ARN
fi

if [ "$LT_NET8_NATIVE" != yes ];
then
  echo SKIPPING net8 native - LT_NET8_NATIVE=$LT_NET8_NATIVE
else
  echo "RUNNING load test for net8 native"
  cd ../../src/NET8Native/
  source ./deploy.sh $DELETE_STACK
  source ./run-loadtest.sh $TEST_DURATIOMN_SEC $LOG_INTERVAL_MIN $LOG_DELETE $LT_SNS_TOPIC_ARN
fi

if [ "$LT_NET8_NATIVE_MINIMAL_API" != yes ];
then
  echo SKIPPING net8 native minimal api LT_NET8_NATIVE_MINIMAL_API=$LT_NET8_NATIVE_MINIMAL_API
else
  echo "RUNNING load test for net8 native minimal api"
  cd ../../src/NET8NativeMinimalAPI/
  source ./deploy.sh $DELETE_STACK
  source ./run-loadtest.sh $TEST_DURATIOMN_SEC $LOG_INTERVAL_MIN $LOG_DELETE $LT_SNS_TOPIC_ARN
fi

echo --------------------------------------------
echo GENERATING FULL REPORT
echo --------------------------------------------
cd ../../loadtestcli
dotnet run ../src $LT_SNS_TOPIC_ARN


