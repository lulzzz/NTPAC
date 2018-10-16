FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ../NTPAC.ApiApp/NTPAC.ApiApp.csproj ../NTPAC.ApiApp/
RUN dotnet restore ../NTPAC.ApiApp/NTPAC.ApiApp.csproj
COPY . .
WORKDIR /src/../NTPAC.ApiApp
RUN dotnet build NTPAC.ApiApp.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish NTPAC.ApiApp.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "NTPAC.ApiApp.dll"]
