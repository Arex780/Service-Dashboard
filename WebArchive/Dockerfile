FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Restore .NET dependencies
COPY *.csproj ./
RUN dotnet restore

COPY . ./

RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .
COPY --from=build-env /app/PKCS12.pfx .

# Install SSL certificates
RUN apt-get update; \
apt-get -y install unzip; \
apt-get -y install wget; \
wget http://certificates.intel.com/repository/certificates/IntelSHA2RootChain-Base64.zip; \ 
unzip IntelSHA2RootChain-Base64.zip -d /etc/ssl/certs/; \
update-ca-certificates --fresh --verbose

ENTRYPOINT ["dotnet", "WebArchive.dll"]