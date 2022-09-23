FROM public.ecr.aws/sam/build-provided.al2:latest AS base
WORKDIR /source
# Install dependencies for compiling naively
RUN yum update -y && yum install -y clang krb5-devel openssl-devel libicu llvm
# need to look into llvm, which is only needed for stripSymbols https://github.com/dotnet/runtime/issues/72155 

# Install .NET 7 preview
RUN yum install -y tar gzip
RUN curl -LO https://dot.net/v1/dotnet-install.sh
RUN chmod +x dotnet-install.sh
RUN ./dotnet-install.sh -Channel 7.0 -Version latest
ENV PATH=/root/.dotnet:$PATH