#!/bin/sh
#
# This script builds binaries for all 4 Lambdas
#
# Run this script without arguments to build
# Use 'sam deploy --guided' to deploy after build is successful
#

check_status()
{
  if [ $? -ne 0 ]; then
    echo "**ERROR**: Build failed"
    exit 1
  fi
}

docker build . -t net-7-native-builder
check_status

docker run --volume /serverless-dotnet-demo/src/NET7Native/:/workingdir --name net7-native-build-container -i net-7-native-builder dotnet publish /workingdir/NET7Native.sln -r linux-x64 -c Release
# check_status TODO: Fix this later

rm -f GetProducts.zip
rm -f DeleteProduct.zip
rm -f GetProduct.zip
rm -f PutProduct.zip

zip  -r -j GetProducts.zip ./GetProducts/bin/Release/net7.0/linux-x64/publish/*
check_status

zip  -r -j DeleteProduct.zip ./DeleteProduct/bin/Release/net7.0/linux-x64/publish/*
check_status

zip  -r -j GetProduct.zip ./GetProduct/bin/Release/net7.0/linux-x64/publish/*
check_status

zip  -r -j PutProduct.zip ./PutProduct/bin/Release/net7.0/linux-x64/publish/*
check_status

docker rm net7-native-build-container

echo "====Build successful===="
