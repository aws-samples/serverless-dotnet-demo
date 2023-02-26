build-GetProductFunction:
	dotnet clean
	dotnet publish GetProduct/GetProduct.csproj -c Release -r linux-x64 --self-contained -o $(ARTIFACTS_DIR)

build-GetProductsFunction:
	dotnet clean
	dotnet publish GetProducts/GetProducts.csproj -c Release -r linux-x64 --self-contained -o $(ARTIFACTS_DIR)

build-PutProductFunction:
	dotnet clean
	dotnet publish PutProduct/PutProduct.csproj -c Release -r linux-x64 --self-contained -o $(ARTIFACTS_DIR)

build-DeleteProductFunction:
	dotnet clean
	dotnet publish DeleteProduct/DeleteProduct.csproj -c Release -r linux-x64 --self-contained -o $(ARTIFACTS_DIR)