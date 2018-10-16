FROM microsoft/dotnet:2.1-runtime AS base
RUN apt-get update && apt-get install -y \
	net-tools iputils-ping procps netcat nano

FROM microsoft/dotnet:2.1-sdk AS publish
WORKDIR /src
COPY nuget.config ./
COPY Lighthouse.NetCoreApp/Lighthouse.NetCoreApp.csproj Lighthouse.NetCoreApp/
RUN dotnet restore Lighthouse.NetCoreApp 
COPY . .
WORKDIR /src/Lighthouse.NetCoreApp
RUN dotnet publish -c Release --no-restore -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
COPY Lighthouse.NetCoreApp/docker-entrypoint.sh .
ENTRYPOINT ["sh", "docker-entrypoint.sh"]
