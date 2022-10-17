FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-image

ARG FUNCTION_DIR="/build"
ARG SAM_BUILD_MODE="run"
ENV PATH="/root/.dotnet/tools:${PATH}"

RUN apt-get update && apt-get -y install zip
RUN dotnet tool install -g Amazon.Lambda.Tools

RUN mkdir $FUNCTION_DIR
WORKDIR $FUNCTION_DIR
COPY ./GetProduct/* $FUNCTION_DIR/GetProduct/
COPY ./Shared/* $FUNCTION_DIR/Shared/

# Build and Copy artifacts depending on build mode.
RUN mkdir -p build_artifacts
RUN dotnet lambda package --configuration Release -pl $FUNCTION_DIR/GetProduct/ -farch arm64
RUN cp -r /build/GetProduct/bin/Release/net6.0/publish/* /build/build_artifacts

FROM public.ecr.aws/lambda/dotnet:6

COPY --from=build-image /build/build_artifacts/ /var/task/
# Command can be overwritten by providing a different command in the template directly.
CMD ["GetProduct::GetProduct.Function::FunctionHandler"]
