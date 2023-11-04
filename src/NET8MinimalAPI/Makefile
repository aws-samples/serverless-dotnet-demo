build-MinimalApiX86:
	dotnet clean
	dotnet publish ApiBootstrap/ApiBootstrap.csproj -c Release -r linux-x64 --self-contained -o $(ARTIFACTS_DIR)
build-MinimalApiArm64:
	dotnet clean
	dotnet publish ApiBootstrap/ApiBootstrap.csproj -c Release -r linux-arm64 --self-contained -o $(ARTIFACTS_DIR)