FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-image

ARG FUNCTION_DIR="/build"
ARG SAM_BUILD_MODE="run"
ARG ROOT_PATH="src/NET7Containers"
ARG FUNCTION_PATH="PutProduct"
ENV PATH="/root/.dotnet/tools:${PATH}"

RUN apt-get update && apt-get -y install zip
RUN dotnet tool install -g Amazon.Lambda.Tools

RUN mkdir $FUNCTION_DIR
WORKDIR $FUNCTION_DIR
COPY Directory.Build.props .
COPY $ROOT_PATH/$FUNCTION_PATH/Function.cs $ROOT_PATH/$FUNCTION_PATH/$FUNCTION_PATH.csproj $FUNCTION_DIR/$FUNCTION_PATH/
COPY $ROOT_PATH/Shared/DataAccess/* $ROOT_PATH/Shared/Models/* $ROOT_PATH/Shared/Shared.csproj $FUNCTION_DIR/Shared/

# Build and Copy artifacts depending on build mode.
RUN mkdir -p build_artifacts
RUN if [ "$SAM_BUILD_MODE" = "debug" ]; then dotnet lambda package -pl $FUNCTION_PATH --configuration Debug; else dotnet lambda package -pl $FUNCTION_PATH --configuration Release; fi
RUN if [ "$SAM_BUILD_MODE" = "debug" ]; then cp -r /build/$FUNCTION_PATH/bin/Debug/net6.0/publish/* /build/build_artifacts; else cp -r /build/$FUNCTION_PATH/bin/Release/net6.0/publish/* /build/build_artifacts; fi

FROM public.ecr.aws/lambda/dotnet:7

COPY --from=build-image /build/build_artifacts/ /var/task/
# Command can be overwritten by providing a different command in the template directly.
CMD ["PutProduct::PutProduct.Function::FunctionHandler"]
