build-GetProductFunction:
	dotnet publish GetProduct/GetProduct.csproj -c Release -r linux-x64 --self-contained
	cp -r ./GetProduct/bin/Release/net7.0/linux-x64/publish/bootstrap $(ARTIFACTS_DIR)

build-GetProductsFunction:
	dotnet publish GetProducts/GetProducts.csproj -c Release -r linux-x64 --self-contained
	cp -r ./GetProducts/bin/Release/net7.0/linux-x64/publish/bootstrap $(ARTIFACTS_DIR)

build-PutProductFunction:
	dotnet publish PutProduct/PutProduct.csproj -c Release -r linux-x64 --self-contained
	cp -r ./PutProduct/bin/Release/net7.0/linux-x64/publish/bootstrap $(ARTIFACTS_DIR)

build-DeleteProductFunction:
	dotnet publish DeleteProduct/DeleteProduct.csproj -c Release -r linux-x64 --self-contained
	cp -r ./DeleteProduct/bin/Release/net7.0/linux-x64/publish/bootstrap $(ARTIFACTS_DIR)