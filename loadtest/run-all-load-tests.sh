#!/bin/bash
#export LT_NET6_MINIMAL_API=1
if [ x"${LT_NET6_MINIMAL_API}" == "x" ];  
then
  echo SKIPPING net6 minimal api :$LT_NET6_MINIMAL_API
else
  echo "RUNNING load test for net6 minimal api"
  cd src/NET6MinimalAPI/
  source ./deploy.sh
  source ./run-load-test.sh
fi

#export LT_NET6_MINIMAL_API_WEB_ADAPTER=1
if [ x"${LT_NET6_MINIMAL_API_WEB_ADAPTER}" == "x" ];  
then
  echo SKIPPING net6 minimal api web adapter :$LT_NET6_MINIMAL_API_WEB_ADAPTER
else
  echo "RUNNING load test for net6 minimal api web adapter"
  cd src/NET6MinimalAPIWebAdapter/
  source ./deploy.sh
  source ./run-load-test.sh
fi
