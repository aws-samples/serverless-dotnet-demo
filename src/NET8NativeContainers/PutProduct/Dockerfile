FROM plantpowerjames/dotnet-8-lambda-build:rc2-arm64 AS build-image
 
ARG FUNCTION_DIR="/build"
ARG SAM_BUILD_MODE="run"
ARG ROOT_PATH="src/NET8NativeContainers"
ARG FUNCTION_PATH="PutProduct"
ENV PATH="/root/.dotnet/tools:${PATH}"

RUN mkdir $FUNCTION_DIR
WORKDIR $FUNCTION_DIR
COPY Directory.Build.props .
COPY $ROOT_PATH/$FUNCTION_PATH/Function.cs $ROOT_PATH/$FUNCTION_PATH/$FUNCTION_PATH.csproj $FUNCTION_DIR/$FUNCTION_PATH/
COPY $ROOT_PATH/Shared/DataAccess/* $ROOT_PATH/Shared/Models/* $ROOT_PATH/Shared/JsonSerializerContext.cs $ROOT_PATH/Shared/Shared.csproj $FUNCTION_DIR/Shared/

# Build and Copy artifacts depending on build mode.
RUN mkdir -p build_artifacts
RUN dotnet publish -c Release $FUNCTION_PATH/$FUNCTION_PATH.csproj --runtime linux-arm64
RUN cp -r /build/$FUNCTION_PATH/bin/Release/net8.0/linux-arm64/publish/ /build/build_artifacts

FROM public.ecr.aws/lambda/provided:al2-arm64
COPY --from=build-image /build/build_artifacts/ /var/task/
# Command can be overwritten by providing a different command in the template directly.
ENTRYPOINT ["/var/task/publish/PutProduct"]