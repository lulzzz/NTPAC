using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.Reassembling.Tests.Comparers;
using PacketDotNet;
using Xunit;
using Xunit.Abstractions;

namespace NTPAC.Reassembling.Tests
{
  public class ReassembledL7ConversationTests
  {
    private readonly IPEndPoint _destinationEndPoint;
    private readonly L7Flow _downL7Flow;
    private readonly IPProtocolType _ipProtocolType;
    private readonly L7PDUComparer _l7PDUComparer;
    private readonly ITestOutputHelper _output;
    private readonly IPEndPoint _sourceEndPoint;
    private readonly L7Flow _upL7Flow;
    private L7Conversation _l7Conversation;

    public ReassembledL7ConversationTests(ITestOutputHelper output)
    {
      this._output              = output;
      this._sourceEndPoint      = new IPEndPoint(1, 1);
      this._destinationEndPoint = new IPEndPoint(2, 2);
      this._ipProtocolType      = IPProtocolType.TCP;
      this._upL7Flow            = new L7Flow();
      this._downL7Flow          = new L7Flow();
      this._l7PDUComparer       = new L7PDUComparer();
    }

    //[Fact]
    [Fact(Skip = "Benchmark")]
    public void Benchmark_MergeUpDownPdus()
    {
      //Arrange
      var l7Pdu1 = CreateL7Pdu(FlowDirection.Down, new DateTime(2018, 04, 03, 0, 0, 1), new DateTime(2018, 04, 04));

      for (var i = 0; i < 5000000; i++)
      {
        this._upL7Flow.AddPdu(l7Pdu1);
        this._downL7Flow.AddPdu(l7Pdu1);
      }

      this.CreateReassembledL7Conversation();

      //Act
      var stopWatch = new Stopwatch();
      stopWatch.Start();
      var reassembledL7Pdus = this._l7Conversation.Pdus.ToArray();
      this._output.WriteLine($"Elapsed: {stopWatch.Elapsed}");

      //Assert
    }

    [Fact]
    public void DownFlowPdus_MergeUpDownPdus_PduExists()
    {
      //Arrange
      var l7Pdu = CreateL7Pdu(FlowDirection.Down, new DateTime(2018, 04, 03), new DateTime(2018, 04, 04));
      this._downL7Flow.AddPdu(l7Pdu);
      this.CreateReassembledL7Conversation();

      //Act
      var reassembledL7Pdus = this._l7Conversation.Pdus;

      //Assert
      Assert.Equal(l7Pdu, reassembledL7Pdus.First(), this._l7PDUComparer);
    }

    [Fact]
    public void DownFlowPdusTwoInOrder_MergeUpDownPdus_PdusExists()
    {
      //Arrange
      var l7Pdu1 = CreateL7Pdu(FlowDirection.Down, new DateTime(2018, 04, 03, 0, 0, 1), new DateTime(2018, 04, 04));
      var l7Pdu2 = CreateL7Pdu(FlowDirection.Down, new DateTime(2018, 04, 03, 0, 0, 2), new DateTime(2018, 04, 04));

      this._downL7Flow.AddPdu(l7Pdu1);
      this._downL7Flow.AddPdu(l7Pdu2);

      this.CreateReassembledL7Conversation();

      //Act
      var reassembledL7Pdus = this._l7Conversation.Pdus.ToArray();

      //Assert
      Assert.Equal(l7Pdu1, reassembledL7Pdus[0], this._l7PDUComparer);
      Assert.Equal(l7Pdu2, reassembledL7Pdus[1], this._l7PDUComparer);
    }

    [Fact]
    public void DownFlowPdusTwoOutOrder_MergeUpDownPdus_PdusExists()
    {
      //Arrange
      var l7Pdu1 = CreateL7Pdu(FlowDirection.Down, new DateTime(2018, 04, 03, 0, 0, 1), new DateTime(2018, 04, 04));
      var l7Pdu2 = CreateL7Pdu(FlowDirection.Down, new DateTime(2018, 04, 03, 0, 0, 2), new DateTime(2018, 04, 04));

      this._downL7Flow.AddPdu(l7Pdu2);
      this._downL7Flow.AddPdu(l7Pdu1);

      this.CreateReassembledL7Conversation();

      //Act
      var reassembledL7Pdus = this._l7Conversation.Pdus.ToArray();

      //Assert
      Assert.Equal(l7Pdu1, reassembledL7Pdus[1], this._l7PDUComparer);
      Assert.Equal(l7Pdu2, reassembledL7Pdus[0], this._l7PDUComparer);
    }

    [Fact]
    public void UpDownFlowPdusTwoInOrder_MergeUpDownPdus_PdusExists()
    {
      //Arrange
      var l7Pdu1 = CreateL7Pdu(FlowDirection.Down, new DateTime(2018, 04, 03, 0, 0, 1), new DateTime(2018, 04, 04));
      var l7Pdu2 = CreateL7Pdu(FlowDirection.Down, new DateTime(2018, 04, 03, 0, 0, 2), new DateTime(2018, 04, 04));

      this._upL7Flow.AddPdu(l7Pdu1);
      this._downL7Flow.AddPdu(l7Pdu2);

      this.CreateReassembledL7Conversation();

      //Act
      var reassembledL7Pdus = this._l7Conversation.Pdus.ToArray();

      //Assert
      Assert.Equal(l7Pdu1, reassembledL7Pdus[0], this._l7PDUComparer);
      Assert.Equal(l7Pdu2, reassembledL7Pdus[1], this._l7PDUComparer);
    }

    [Fact]
    public void UpDownFlowPdusTwoOutOrder_MergeUpDownPdus_PdusExists()
    {
      //Arrange
      var l7Pdu1 = CreateL7Pdu(FlowDirection.Down, new DateTime(2018, 04, 03, 0, 0, 1), new DateTime(2018, 04, 04));
      var l7Pdu2 = CreateL7Pdu(FlowDirection.Down, new DateTime(2018, 04, 03, 0, 0, 2), new DateTime(2018, 04, 04));

      this._upL7Flow.AddPdu(l7Pdu2);
      this._downL7Flow.AddPdu(l7Pdu1);

      this.CreateReassembledL7Conversation();

      //Act
      var reassembledL7Pdus = this._l7Conversation.Pdus.ToArray();

      //Assert
      Assert.Equal(l7Pdu1, reassembledL7Pdus[0], this._l7PDUComparer);
      Assert.Equal(l7Pdu2, reassembledL7Pdus[1], this._l7PDUComparer);
    }

    [Fact]
    public void UpFlowPdus_MergeUpDownPdus_PduExists()
    {
      //Arrange
      var l7Pdu = CreateL7Pdu(FlowDirection.Up, new DateTime(2018, 04, 03), new DateTime(2018, 04, 04));
      this._upL7Flow.AddPdu(l7Pdu);
      this.CreateReassembledL7Conversation();

      //Act
      var reassembledL7Pdus = this._l7Conversation.Pdus;

      //Assert
      Assert.Equal(l7Pdu, reassembledL7Pdus.First(), this._l7PDUComparer);
    }

    [Fact]
    public void UpFlowPdusTwoInOrder_MergeUpDownPdus_PdusExists()
    {
      //Arrange
      var l7Pdu1 = CreateL7Pdu(FlowDirection.Up, new DateTime(2018, 04, 03, 0, 0, 1), new DateTime(2018, 04, 04));
      var l7Pdu2 = CreateL7Pdu(FlowDirection.Up, new DateTime(2018, 04, 03, 0, 0, 2), new DateTime(2018, 04, 04));

      this._upL7Flow.AddPdu(l7Pdu1);
      this._upL7Flow.AddPdu(l7Pdu2);

      this.CreateReassembledL7Conversation();

      //Act
      var reassembledL7Pdus = this._l7Conversation.Pdus.ToArray();

      //Assert
      Assert.Equal(l7Pdu1, reassembledL7Pdus[0], this._l7PDUComparer);
      Assert.Equal(l7Pdu2, reassembledL7Pdus[1], this._l7PDUComparer);
    }

    [Fact]
    public void UpFlowPdusTwoOutOrder_MergeUpDownPdus_PdusExists()
    {
      //Arrange
      var l7Pdu1 = CreateL7Pdu(FlowDirection.Up, new DateTime(2018, 04, 03, 0, 0, 1), new DateTime(2018, 04, 04));
      var l7Pdu2 = CreateL7Pdu(FlowDirection.Up, new DateTime(2018, 04, 03, 0, 0, 2), new DateTime(2018, 04, 04));

      this._upL7Flow.AddPdu(l7Pdu2);
      this._upL7Flow.AddPdu(l7Pdu1);

      this.CreateReassembledL7Conversation();

      //Act
      var reassembledL7Pdus = this._l7Conversation.Pdus.ToArray();

      //Assert
      Assert.Equal(l7Pdu1, reassembledL7Pdus[1], this._l7PDUComparer);
      Assert.Equal(l7Pdu2, reassembledL7Pdus[0], this._l7PDUComparer);
    }

    private static L7Pdu CreateL7Pdu(FlowDirection flowDirection, DateTime firstSeen, DateTime lastSeen)
    {
      Frame CreateFrame(DateTime dateTime) => new Frame {TimestampTicks = dateTime.Ticks, L7Payload = new Byte[0]};

      if (flowDirection == FlowDirection.None)
      {
        throw new Exception("Invalid FlowDirection");
      }

      var reassembledL7Pdu = new L7Pdu(CreateFrame(firstSeen), flowDirection);
      reassembledL7Pdu.AddFrame(CreateFrame(lastSeen));
      return reassembledL7Pdu;
    }

    private void CreateReassembledL7Conversation()
    {
      this._l7Conversation = new L7Conversation(this._sourceEndPoint, this._destinationEndPoint, this._ipProtocolType,
                                                this._upL7Flow, this._downL7Flow);
    }
  }
}
