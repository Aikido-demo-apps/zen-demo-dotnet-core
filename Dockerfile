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
RUN apt-get update && apt-get install -y curl tini procps && \
    curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS http://+:8080
ENV ASPNETCORE_ENVIRONMENT Production
ENV AIKIDO_BLOCK true
EXPOSE 8080
WORKDIR /app
COPY --from=build /app .
COPY docker-entrypoint.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/docker-entrypoint.sh
ENTRYPOINT [ "/usr/bin/tini", "--", "/usr/local/bin/docker-entrypoint.sh" ]
