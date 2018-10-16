FROM microsoft/dotnet:2.1-runtime AS base
RUN apt-get update && apt-get install -y \
	net-tools iputils-ping procps netcat nano

FROM microsoft/dotnet:2.1-sdk AS publish
WORKDIR /src
COPY nuget.config ./
COPY NTPAC.AkkaSupport/NTPAC.AkkaSupport.csproj  NTPAC.AkkaSupport/
COPY NTPAC.AkkaSupport.Interfaces/NTPAC.AkkaSupport.Interfaces.csproj NTPAC.AkkaSupport.Interfaces/
COPY NTPAC.Common/NTPAC.Common.csproj NTPAC.Common/
COPY NTPAC.ConversatinTracking.Interfaces/NTPAC.ConversatinTracking.Interfaces.csproj NTPAC.ConversatinTracking.Interfaces/
COPY NTPAC.ConversationTracking.Actors/NTPAC.ConversationTracking.Actors.csproj NTPAC.ConversationTracking.Actors/
COPY NTPAC.ConversationTracking/NTPAC.ConversationTracking.csproj NTPAC.ConversationTracking/
COPY NTPAC.ConversationTracking/NTPAC.ConversationTracking.csproj NTPAC.ConversationTracking/NTPAC.ConversationTracking/
COPY NTPAC.DTO/NTPAC.DTO.csproj NTPAC.DTO/
COPY NTPAC.LoadBalancer.Interfaces/NTPAC.LoadBalancer.Interfaces.csproj NTPAC.LoadBalancer.Interfaces/
COPY NTPAC.LoadBalancer.Messages/NTPAC.LoadBalancer.Messages.csproj NTPAC.LoadBalancer.Messages/
COPY NTPAC.Messages/NTPAC.ConversationTracking.Messages.csproj NTPAC.Common/
COPY NTPAC.Messages/NTPAC.ConversationTracking.Messages.csproj NTPAC.ConversationTracking.Messages/
COPY NTPAC.Messages/NTPAC.ConversationTracking.Messages.csproj NTPAC.Messages/
COPY NTPAC.Messages/NTPAC.ConversationTracking.Messages.csproj NTPAC.Messages/NTPAC.ConversationTracking.Messages/
COPY NTPAC.PcapLoader/NTPAC.PcapLoader.csproj NTPAC.PcapLoader/
COPY NTPAC.Persistence.Cassandra.Facades/NTPAC.Persistence.Cassandra.Facades.csproj NTPAC.Persistence.Cassandra.Facades/
COPY NTPAC.Persistence.DevNull.Facades/NTPAC.Persistence.DevNull.Facades.csproj NTPAC.Persistence.DevNull.Facades/
COPY NTPAC.Persistence.Entities/NTPAC.Persistence.Entities.csproj NTPAC.Persistence.Entities/
COPY NTPAC.Persistence.InMemory.Facades/NTPAC.Persistence.InMemory.Facades.csproj NTPAC.Persistence.InMemory.Facades/
COPY NTPAC.Persistence.Interfaces/NTPAC.Persistence.Interfaces.csproj NTPAC.Persistence.Interfaces/
COPY NTPAC.ReassemblerCli/NTPAC.ReassemblerCli.csproj NTPAC.ReassemblerCli/
COPY NTPAC.Reassembling/NTPAC.Reassembling.csproj NTPAC.Reassembling/
RUN dotnet restore NTPAC.ReassemblerCli/
COPY . .
WORKDIR /src/NTPAC.ReassemblerCli
RUN dotnet publish -c Release --no-restore -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
COPY NTPAC.ReassemblerCli//docker-entrypoint.sh .
ENTRYPOINT ["sh", "docker-entrypoint.sh"]
