using System;
using System.Linq;
using System.Net;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.Reassembling.UDP;
using Xunit;
using Xunit.Abstractions;

namespace NTPAC.Reassembling.Tests
{
  public class UdpConversationTrackerTests : ReassemblingTestBase
  {
    public UdpConversationTrackerTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void DNS_1_ProcessFrame_AllFramesProcessed()
    {
      // Simple DNS request - response
      var l7Conversations = this.ReassembleL7Conversations("dns_1.pcapng");

      DNS_1_ProcessFrame(l7Conversations);
    }

    [Fact(Skip = "Fails on Windows")]
    public void DNS_1_ProcessFrame_AllFramesProcessed_WindowsFails()
    {
      // Simple DNS request - response
      var l7Conversations = this.ReassembleL7Conversations("dns_1.pcapng");

      DNS_1_ProcessFrame(l7Conversations);
    }

    private static void DNS_1_ProcessFrame(L7Conversation[] l7Conversations)
    {
      Assert.NotNull(l7Conversations);
      Assert.Single(l7Conversations);
      var l7Conversation = l7Conversations.First();

      Assert.Equal(l7Conversation.SourceEndPoint, new IPEndPoint(IPAddress.Parse("147.229.176.17"), 60416));
      Assert.Equal(l7Conversation.DestinationEndPoint, new IPEndPoint(IPAddress.Parse("147.229.8.12"), 53));

      var pdus = l7Conversation.Pdus.ToArray();
      Assert.Equal(FlowDirection.Up, pdus[0].Direction);
      Assert.Equal(46, pdus[0].PayloadLen);
      Assert.Equal(FlowDirection.Down, pdus[1].Direction);
      Assert.Equal(146, pdus[1].PayloadLen);
    }

    private L7Conversation[] ReassembleL7Conversations(String pcapFileName)
    {
      var frames = this.GetFramesFromPcap(pcapFileName);

      var enumerable      = frames as Frame[] ?? frames.ToArray();
      var originatorFrame = enumerable.First();
      var udpReassembler  = new UdpConversationTracker(originatorFrame.SourceEndPoint, originatorFrame.DestinationEndPoint);
      foreach (var frame in enumerable)
      {
        udpReassembler.ProcessFrame(frame);
      }

      var l7Conversations = udpReassembler.Complete().ToArray();
      return l7Conversations;
    }
  }
}
