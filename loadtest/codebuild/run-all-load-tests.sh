TEST_DURATIOMN_SEC=60
LOG_INTERVALL_MIN=20
LOG_DELETE=1

if [ x"${LT_TEST_DURATIOMN_SEC}" != "x" ];  
then
  TEST_DURATIOMN_SEC=$LT_TEST_DURATIOMN_SEC
fi

if [ x"${LT_LOG_INTERVALL_MIN}" != "x" ];  
then
  LOG_INTERVALL_MIN=$LT_LOG_INTERVALL_MIN
fi

if [ x"${LT_LOG_DELETE}" != "x" ];  
then
  LOG_DELETE=$LT_LOG_DELETE
fi

if [ x"${LT_NET6_MINIMAL_API}" == "x" ];  
then
  echo SKIPPING net6 minimal api :$LT_NET6_MINIMAL_API
else
  echo "RUNNING load test for net6 minimal api"
  cd ../../src/NET6MinimalAPI/
  source ./deploy.sh
  source ./run-loadtest.sh $TEST_DURATIOMN_SEC $LOG_INTERVALL_MIN $LOG_DELETE
fi

#export LT_NET6_MINIMAL_API_WEB_ADAPTER=1
if [ x"${LT_NET6_MINIMAL_API_WEB_ADAPTER}" == "x" ];  
then
  echo SKIPPING net6 minimal api web adapter :$LT_NET6_MINIMAL_API_WEB_ADAPTER
else
  echo "RUNNING load test for net6 minimal api web adapter"
  cd ../../src/NET6MinimalAPIWebAdapter/
  source ./deploy.sh
  source ./run-loadtest.sh $TEST_DURATIOMN_SEC $LOG_INTERVALL_MIN $LOG_DELETE
fi

#export LT_NET6_MINIMAL_API_WEB_ADAPTER=1
if [ x"${LT_NET8_MINIMAL_API}" == "x" ];  
then
  echo SKIPPING net8 minimal api :$LT_NET8_MINIMAL_API
else
  echo "RUNNING load test for net8 minimal api"
  cd ../../src/NET8MinimalAPI/
  source ./deploy.sh
  source ./run-loadtest.sh $TEST_DURATIOMN_SEC $LOG_INTERVALL_MIN $LOG_DELETE
fi