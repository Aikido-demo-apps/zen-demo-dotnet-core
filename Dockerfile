# Adjust DOTNET_OS_VERSION as desired
ARG DOTNET_OS_VERSION="-alpine"
ARG DOTNET_SDK_VERSION=9.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_VERSION}${DOTNET_OS_VERSION} AS build
WORKDIR /src

# copy everything
COPY . ./
# restore as distinct layers
RUN dotnet restore
# build and publish a release
RUN dotnet publish -c Release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_SDK_VERSION}

# vsdbg for ssh debugging
RUN apt-get update && apt-get install -y curl
RUN curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg

# tini
RUN apt-get install -y tini

ENV ASPNETCORE_URLS http://+:8080
ENV ASPNETCORE_ENVIRONMENT Production
ENV AIKIDO_BLOCK true
EXPOSE 8080
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["/usr/bin/tini", "--", "dotnet zen-demo-dotnet.dll"]
