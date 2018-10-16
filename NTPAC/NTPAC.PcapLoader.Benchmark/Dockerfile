# docker run --mount type=bind,source=/root/ntpac/pcap,target=/src/TestingData docker.nesad.fit.vutbr.cz/pcaploaderbenchmark

FROM microsoft/dotnet:2.1-sdk AS base
RUN apt-get update && apt-get install -y \
	net-tools iputils-ping procps netcat nano libpcap0.8 
RUN ln -s /usr/lib/x86_64-linux-gnu/libpcap.so.0.8 /usr/lib/x86_64-linux-gnu/libwpcap.so

FROM base AS build
WORKDIR /src
COPY nuget.config ./
COPY NTPAC.PcapLoader.Benchmark/NTPAC.PcapLoader.Benchmark.csproj NTPAC.PcapLoader.Benchmark/
COPY . .
WORKDIR /src/NTPAC.PcapLoader.Benchmark
RUN dotnet build -c Release

FROM build AS final
ENTRYPOINT ["dotnet", "bin/Release/netcoreapp2.1/NTPAC.PcapLoader.Benchmark.dll"]