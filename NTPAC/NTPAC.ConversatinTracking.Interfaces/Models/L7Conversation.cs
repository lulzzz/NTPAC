using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using PacketDotNet;

namespace NTPAC.ConversatinTracking.Interfaces.Models
{
  public class L7Conversation
  {
    private static readonly L7Pdu[] NoPdusPlaceholder = new L7Pdu[0];

    public L7Conversation(IPEndPoint sourceEndPoint,
                          IPEndPoint destinationEndPoint,
                          IPProtocolType protocolType,
                          L7Flow upL7Flow,
                          L7Flow downL7Flow,
                          Int32 conversationSegmentNum = 0,
                          Boolean lastConversationSegment = true)
    {
      this.SourceEndPoint      = sourceEndPoint;
      this.DestinationEndPoint = destinationEndPoint;
      this.ProtocolType        = protocolType;

      this.LastConversationSegment = lastConversationSegment;
      this.ConversationSegmentNum  = conversationSegmentNum;

      this.UpPdus   = upL7Flow?.L7Pdus   ?? NoPdusPlaceholder;
      this.DownPdus = downL7Flow?.L7Pdus ?? NoPdusPlaceholder;

      this.FirstSeenTicks = Math.Min(upL7Flow?.FirstSeenTicks ?? Int64.MaxValue, downL7Flow?.FirstSeenTicks ?? Int64.MaxValue);
      // Handle case where both timestamps are missing
      if (this.FirstSeenTicks == Int64.MaxValue)
      {
        this.FirstSeenTicks = 0;
      }

      this.LastSeenTicks = Math.Max(upL7Flow?.LastSeenTicks ?? 0, downL7Flow?.LastSeenTicks ?? 0);
    }

    public Guid CaptureId { get; set; }
    public Int32 ConversationSegmentNum { get; set; }
    public IPEndPoint DestinationEndPoint { get; set; }

    public IEnumerable<L7Pdu> DownPdus { get; }
    public DateTime FirstSeen => this.FirstSeenTicks > 0 ? new DateTime(this.FirstSeenTicks).ToLocalTime() : DateTime.MinValue;

    public Int64 FirstSeenTicks { get; set; }

    public Guid Id { get; set; } = Guid.NewGuid();
    public Boolean LastConversationSegment { get; set; }
    public DateTime LastSeen => this.LastSeenTicks > 0 ? new DateTime(this.LastSeenTicks).ToLocalTime() : DateTime.MinValue;

    public Int64 LastSeenTicks { get; set; }

    public IEnumerable<L7Pdu> Pdus => this.MergeUpDownPdusYield();
    public IPProtocolType ProtocolType { get; set; }
    public IPEndPoint SourceEndPoint { get; set; }

    public IEnumerable<L7Pdu> UpPdus { get; }

    public override String ToString() =>
      $"{this.FirstSeen} > {this.ProtocolType} {this.SourceEndPoint}-{this.DestinationEndPoint} > UpPDUs:{this.UpPdus.Count()} DownPDUs:{this.DownPdus.Count()}";

    private IEnumerable<L7Pdu> MergeUpDownPdusYield()
    {
      Boolean InitializeEnumeratorIfAny(IEnumerator enumerator) => enumerator.MoveNext();
      var upPdusEnumerator   = this.UpPdus.GetEnumerator();
      var downPdusEnumerator = this.DownPdus.GetEnumerator();

      var isUpFlowAny   = InitializeEnumeratorIfAny(upPdusEnumerator);
      var isDownFlowAny = InitializeEnumeratorIfAny(downPdusEnumerator);

      if (!isUpFlowAny && !isDownFlowAny)
      {
        yield break;
      }

      if (!isUpFlowAny)
      {
        do
        {
          yield return downPdusEnumerator.Current;
        } while (downPdusEnumerator.MoveNext());

        yield break;
      }

      if (!isDownFlowAny)
      {
        do
        {
          yield return upPdusEnumerator.Current;
        } while (upPdusEnumerator.MoveNext());

        yield break;
      }

      while (true)
      {
        IEnumerator<L7Pdu> currentEnumerator, otherEnumerator;
        if (upPdusEnumerator.Current.FirstSeenTicks < downPdusEnumerator.Current.FirstSeenTicks)
        {
          currentEnumerator = upPdusEnumerator;
          otherEnumerator   = downPdusEnumerator;
        }
        else
        {
          currentEnumerator = downPdusEnumerator;
          otherEnumerator   = upPdusEnumerator;
        }

        yield return currentEnumerator.Current;
        if (!currentEnumerator.MoveNext())
        {
          do
          {
            yield return otherEnumerator.Current;
          } while (otherEnumerator.MoveNext());

          yield break;
        }
      }
    }

    private IEnumerable<L7Pdu> PdusInDirection(FlowDirection direction) => this.Pdus.Where(pdu => pdu.Direction == direction);
  }
}
