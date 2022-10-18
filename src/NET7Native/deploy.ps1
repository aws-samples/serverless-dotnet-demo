docker build . -t net-7-native-builder
docker run --volume ${pwd}:/workingdir --name net7-native-build-container -i net-7-native-builder dotnet publish /workingdir/NET7Native.sln -r linux-x64 -c Release
rm GetProducts.zip
rm DeleteProduct.zip
rm GetProduct.zip
rm PutProduct.zip
Compress-Archive .\GetProducts\bin\Release\net7.0\linux-x64\publish\* GetProducts.zip
Compress-Archive .\DeleteProduct\bin\Release\net7.0\linux-x64\publish\* DeleteProduct.zip
Compress-Archive .\GetProduct\bin\Release\net7.0\linux-x64\publish\* GetProduct.zip
Compress-Archive .\PutProduct\bin\Release\net7.0\linux-x64\publish\* PutProduct.zip
docker rm net7-native-build-container