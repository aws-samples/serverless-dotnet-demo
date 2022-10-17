FROM public.ecr.aws/sam/build-provided.al2:latest-x86_64 AS build-image

ARG FUNCTION_DIR="/build"
ARG SAM_BUILD_MODE="run"
ENV PATH="/root/.dotnet/tools:${PATH}"

RUN yum update -y && yum install -y clang krb5-devel openssl-devel libicu llvm
RUN yum install -y tar gzip
RUN curl -LO https://dot.net/v1/dotnet-install.sh
RUN chmod +x dotnet-install.sh
RUN ./dotnet-install.sh -Channel 7.0 -Version latest

ENV PATH=/root/.dotnet:$PATH

RUN mkdir $FUNCTION_DIR
WORKDIR $FUNCTION_DIR
COPY ./DeleteProduct/* $FUNCTION_DIR/DeleteProduct/
COPY ./Shared/* $FUNCTION_DIR/Shared/

# Build and Copy artifacts depending on build mode.
RUN mkdir -p build_artifacts
RUN dotnet publish $FUNCTION_DIR/DeleteProduct/DeleteProduct.csproj -r linux-x64 -c Release 
RUN cp -r $FUNCTION_DIR/DeleteProduct/bin/Release/net7.0/linux-x64/publish/* $FUNCTION_DIR/build_artifacts

FROM public.ecr.aws/lambda/provided:al2

COPY --from=build-image /build/build_artifacts/ /var/task/
# Command can be overwritten by providing a different command in the template directly.
ENTRYPOINT [ "/var/task/bootstrap"]
