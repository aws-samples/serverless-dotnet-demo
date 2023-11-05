TEST_DURATIOMN_SEC=60
LOG_INTERVAL_MIN=20
LOG_DELETE=yes
DELETE_STACK=yes

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

echo --------------------------------------------
echo TEST_DURATIOMN_SEC: $TEST_DURATIOMN_SEC
echo LOG_INTERVAL_MIN: $LOG_INTERVAL_MIN
echo LOG_DELETE: $LOG_DELETE
echo DELETE_STACK: $DELETE_STACK
echo --------------------------------------------

if [ "$LT_NET6_MINIMAL_API" != yes ];  
then
  echo SKIPPING net6 minimal api :$LT_NET6_MINIMAL_API
else
  echo "RUNNING load test for net6 minimal api"
  cd ../../src/NET6MinimalAPI/
  source ./deploy.sh $DELETE_STACK
  source ./run-loadtest.sh $TEST_DURATIOMN_SEC $LOG_INTERVAL_MIN $LOG_DELETE
fi

if [ "$LT_NET6_MINIMAL_API_WEB_ADAPTER" != yes ];  
then
  echo SKIPPING net6 minimal api web adapter :$LT_NET6_MINIMAL_API_WEB_ADAPTER
else
  echo "RUNNING load test for net6 minimal api web adapter"
  cd ../../src/NET6MinimalAPIWebAdapter/
  source ./deploy.sh $DELETE_STACK
  source ./run-loadtest.sh $TEST_DURATIOMN_SEC $LOG_INTERVAL_MIN $LOG_DELETE
fi

#export LT_NET8_MINIMAL_API=1
if [ "$LT_NET8_MINIMAL_API" != yes ];
then
  echo SKIPPING net8 minimal api :$LT_NET8_MINIMAL_API
else
  echo "RUNNING load test for net8 minimal api"
  cd ../../src/NET8MinimalAPI/
  source ./deploy.sh
  source ./run-loadtest.sh $TEST_DURATIOMN_SEC $LOG_INTERVAL_MIN $LOG_DELETE
fi
