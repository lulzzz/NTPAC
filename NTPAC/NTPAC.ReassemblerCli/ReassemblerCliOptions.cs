using System;
using CommandLine;

namespace NTPAC.ReassemblerCli
{
  public class ReassemblerCliOptions
  {
    [Option("cassandra-contactpoint", Default = "localhost", HelpText = "Cassandra contact node's hostname")]
    public String CassandraContactPoint { get; set; }

    [Option("cassandra-keyspace", Default = "ntpac", HelpText = "Cassandra keyspace")]
    public String CassandraKeyspace { get; set; }

    [Option("cassandra-repository", Default = false, HelpText = "Use Cassandra repository")]
    public Boolean CassandraRepository { get; set; }

    [Option('h', "hostname", Default = "127.0.0.1", HelpText = "IP address to listen on to and be *reachable* at")]
    public String ClusterNodeHostname { get; set; }

    [Option('p', "port", Default = 0, HelpText = "Port (0 - random)")]
    public Int32 ClusterNodePort { get; set; }

    [Option('s', "seednode", Default = "127.0.0.1:7070", HelpText = "Hostname and port of the cluster seed node")]
    public String ClusterSeedNodeHostname { get; set; }

    [Option("devnull-repository", Default = false, HelpText = "Dont save any persisted objects")]
    public Boolean DevnullRepository { get; set; }

    [Option("inmemory-repository", Default = false, HelpText = "Dont save any persisted objects")]
    public Boolean InMemoryRepository { get; set; }
  }
}
