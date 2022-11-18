FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build-image

ARG FUNCTION_DIR="/build"
ARG SAM_BUILD_MODE="run"
ARG FUNCTION_PATH="GetProducts"
ENV PATH="/root/.dotnet/tools:${PATH}"

RUN apk add clang build-base zlib-dev

RUN mkdir $FUNCTION_DIR
WORKDIR $FUNCTION_DIR
COPY $FUNCTION_PATH/Function.cs $FUNCTION_PATH/rd.xml $FUNCTION_PATH/$FUNCTION_PATH.csproj $FUNCTION_DIR/$FUNCTION_PATH/
COPY Shared/DataAccess/* Shared/Models/* Shared/JsonSerializerContext.cs Shared/Shared.csproj $FUNCTION_DIR/Shared/

# Build and Copy artifacts depending on build mode.
RUN mkdir -p build_artifacts
RUN dotnet publish -c Release $FUNCTION_PATH/$FUNCTION_PATH.csproj --runtime alpine-x64 --self-contained true \
    /p:PublishTrimmed=true \
    /p:TrimMode=Link
RUN cp -r /build/$FUNCTION_PATH/bin/Release/net7.0/alpine-x64/publish/GetProducts /build/build_artifacts

FROM alpine
RUN apk add --no-cache libstdc++
COPY --from=build-image /build/build_artifacts/ /var/task/
# Command can be overwritten by providing a different command in the template directly.
ENTRYPOINT ["var/task/GetProducts"]
