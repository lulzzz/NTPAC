using System;
using System.Linq;
using System.Net;
using System.Text;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.Reassembling.TCP;
using Xunit;
using Xunit.Abstractions;

namespace NTPAC.Reassembling.Tests
{
  public class TcpConversationTrackerTests : ReassemblingTestBase
  {
    public TcpConversationTrackerTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void Tcp_http_test_AllFramesProcessed()
    {
      var l7Conversations = this.ReassembleL7Conversations("httpTest.pcapng");

      Assert.NotNull(l7Conversations);
      Assert.Single(l7Conversations);
      L7Conversation l7Conversation = l7Conversations.First();

      Assert.Equal(l7Conversation.SourceEndPoint, new IPEndPoint(IPAddress.Parse("147.229.176.17"), 1103));
      Assert.Equal(l7Conversation.DestinationEndPoint, new IPEndPoint(IPAddress.Parse("87.106.5.34"), 80));

      var pdus = l7Conversation.Pdus.ToArray();
      Assert.NotNull(pdus);
      Assert.Equal(2, pdus.Length);

      Assert.Equal(FlowDirection.Up, pdus[0].Direction);
      Assert.Equal(128, pdus[0].PayloadLen);
      Assert.StartsWith("GET /rules///sc17.bin.incr.2013.11.15.02.01.01 HTTP/1.1", DecodeBytes(pdus[0].Payload));
      Assert.Equal(FlowDirection.Down, pdus[1].Direction);
      Assert.Equal(1427, pdus[1].PayloadLen);
      Assert.StartsWith("HTTP/1.1 200 OK", DecodeBytes(pdus[1].Payload));
    }

    [Fact]
    public void Tcp_more_frames_AllFramesProcessed()
    {
      var l7Conversations = this.ReassembleL7Conversations("tcp_more_frames.pcapng");

      Assert.NotNull(l7Conversations);
      Assert.Single(l7Conversations);
      var l7Conversation = l7Conversations.First();

      Assert.Equal(l7Conversation.SourceEndPoint, new IPEndPoint(IPAddress.Parse("192.168.1.118"), 6018));
      Assert.Equal(l7Conversation.DestinationEndPoint, new IPEndPoint(IPAddress.Parse("31.22.4.96"), 80));

      var pdus = l7Conversation.Pdus.ToArray();
      Assert.NotNull(pdus);
      Assert.Equal(5, pdus.Length);

      Assert.Equal(FlowDirection.Up, pdus[0].Direction);
      Assert.Equal(322, pdus[0].PayloadLen);
      Assert.StartsWith("GET /wp-content/uploads/2013/01/Reborn-Vs-Naruto-v-7.1.jpg HTTP/1.1", DecodeBytes(pdus[0].Payload));
      Assert.Equal(FlowDirection.Down, pdus[1].Direction);
      Assert.Equal(4110, pdus[1].PayloadLen);
      Assert.Equal(FlowDirection.Down, pdus[2].Direction);
      Assert.Equal(4096, pdus[2].PayloadLen);
      Assert.Equal(FlowDirection.Down, pdus[3].Direction);
      Assert.Equal(4096, pdus[3].PayloadLen);
      Assert.Equal(FlowDirection.Down, pdus[4].Direction);
      Assert.Equal(778, pdus[4].PayloadLen);
    }

    [Fact]
    public void Tcp_more_frames_uniA_AllFramesProcessed()
    {
      var l7Conversations = this.ReassembleL7Conversations("tcp_more_frames_uniA.pcapng");

      Assert.NotNull(l7Conversations);
      Assert.Single(l7Conversations);
      var l7Conversation = l7Conversations.First();

      Assert.Equal(l7Conversation.SourceEndPoint, new IPEndPoint(IPAddress.Parse("192.168.1.118"), 6018));
      Assert.Equal(l7Conversation.DestinationEndPoint, new IPEndPoint(IPAddress.Parse("31.22.4.96"), 80));

      var pdus = l7Conversation.Pdus.ToArray();
      Assert.NotNull(pdus);
      Assert.Single(pdus);

      Assert.Equal(FlowDirection.Up, pdus[0].Direction);
      Assert.Equal(322, pdus[0].PayloadLen);
      Assert.StartsWith("GET /wp-content/uploads/2013/01/Reborn-Vs-Naruto-v-7.1.jpg HTTP/1.1", DecodeBytes(pdus[0].Payload));
    }

    [Fact]
    public void Tcp_more_frames_uniB_AllFramesProcessed()
    {
      var l7Conversations = this.ReassembleL7Conversations("tcp_more_frames_uniB.pcapng");

      Assert.NotNull(l7Conversations);
      Assert.Single(l7Conversations);
      var l7Conversation = l7Conversations.First();

      Assert.Equal(l7Conversation.SourceEndPoint, new IPEndPoint(IPAddress.Parse("31.22.4.96"), 80));
      Assert.Equal(l7Conversation.DestinationEndPoint, new IPEndPoint(IPAddress.Parse("192.168.1.118"), 6018));

      var pdus = l7Conversation.Pdus.ToArray();
      Assert.NotNull(pdus);
      Assert.Equal(4, pdus.Length);

      Assert.Equal(FlowDirection.Up, pdus[0].Direction);
      Assert.Equal(4110, pdus[0].PayloadLen);
      Assert.Equal(FlowDirection.Up, pdus[1].Direction);
      Assert.Equal(4096, pdus[1].PayloadLen);
      Assert.Equal(FlowDirection.Up, pdus[2].Direction);
      Assert.Equal(4096, pdus[2].PayloadLen);
      Assert.Equal(FlowDirection.Up, pdus[3].Direction);
      Assert.Equal(778, pdus[3].PayloadLen);
    }

    [Fact]
    public void Tcp_retransmission()
    {
      var l7Conversations = this.ReassembleL7Conversations("isa-http_retransmission.pcapng");
      Assert.NotNull(l7Conversations);
      var l7Conversation = l7Conversations.First();
      Assert.Single(l7Conversation.UpPdus);
      Assert.Single(l7Conversation.DownPdus);
    }

    [Fact]
    public void Tcp_reusedPortsAndKeepAlive_AllFramesProcessed()
    {
      var l7Conversations = this.ReassembleL7Conversations("isa-http_reusedPortsAndKeepalives.pcapng");
      Assert.NotNull(l7Conversations);
      Assert.Equal(8, l7Conversations.Length);

      Assert.Single(l7Conversations[1].UpPdus);
      Assert.Empty(l7Conversations[0].DownPdus);

      Assert.Single(l7Conversations[1].UpPdus);
      Assert.Single(l7Conversations[1].DownPdus);

      Assert.Single(l7Conversations[2].UpPdus);
      Assert.Single(l7Conversations[2].DownPdus);

      Assert.Single(l7Conversations[3].UpPdus);
      Assert.Single(l7Conversations[3].DownPdus);

      Assert.Single(l7Conversations[4].UpPdus);
      Assert.Single(l7Conversations[4].DownPdus);

      Assert.Single(l7Conversations[5].UpPdus);
      Assert.Single(l7Conversations[5].DownPdus);

      Assert.Single(l7Conversations[6].UpPdus);
      Assert.Single(l7Conversations[6].DownPdus);

      Assert.Single(l7Conversations[7].UpPdus);
      Assert.Single(l7Conversations[7].DownPdus);
    }

    [Fact]
    public void Tcp_keepAlive_AllFramesProcessed()
    {
      var l7Conversations = this.ReassembleL7Conversations("tcp_keepalive.pcap");
      Assert.NotNull(l7Conversations);
      Assert.Single(l7Conversations);

      var l7Conversation = l7Conversations.First();
      Assert.Equal(138, l7Conversation.UpPdus.Count());
      Assert.Equal(139, l7Conversation.DownPdus.Count());
    }
    
    [Fact]
    public void Tcp_two_frames_AllFramesProcessed()
    {
      var l7Conversations = this.ReassembleL7Conversations("tcp_two_frames.pcapng");

      Assert.NotNull(l7Conversations);
      Assert.Single(l7Conversations);
      L7Conversation l7Conversation = l7Conversations.First();

      Assert.Equal(l7Conversation.SourceEndPoint, new IPEndPoint(IPAddress.Parse("192.168.1.118"), 2948));
      Assert.Equal(l7Conversation.DestinationEndPoint, new IPEndPoint(IPAddress.Parse("188.138.118.86"), 80));

      var pdus = l7Conversation.Pdus.ToArray();
      Assert.NotNull(pdus);
      Assert.Equal(2, pdus.Length);

      Assert.Equal(FlowDirection.Up, pdus[0].Direction);
      Assert.Equal(306, pdus[0].PayloadLen);
      Assert.StartsWith("GET /CONTENT/user-pics/0/naruto-uzumaki.jpg", DecodeBytes(pdus[0].Payload));
      Assert.Equal(FlowDirection.Down, pdus[1].Direction);
      Assert.Equal(1786, pdus[1].PayloadLen);
      Assert.StartsWith("HTTP/1.1 200 OK", DecodeBytes(pdus[1].Payload));
    }

    private static String DecodeBytes(Byte[] data) => Encoding.Default.GetString(data);

    private L7Conversation[] ReassembleL7Conversations(String pcapFileName)
    {
      var frames = this.GetFramesFromPcap(pcapFileName);

      var enumerable      = frames as Frame[] ?? frames.ToArray();
      var originatorFrame = enumerable.First();
      var tcpReassembler  = new TcpConversationTracker(originatorFrame.SourceEndPoint, originatorFrame.DestinationEndPoint);
      foreach (var frame in enumerable)
      {
        tcpReassembler.ProcessFrame(frame);
      }

      var l7Conversations = tcpReassembler.Complete().ToArray();
      return l7Conversations;
    }
  }
}
