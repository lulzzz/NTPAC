using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.Reassembling.Exceptions;
using PacketDotNet;

namespace NTPAC.Reassembling.TCP
{
  public class TcpConversationTracker : L7ConversationTrackerBase
  {
    private static readonly L7Flow[] EmptyCompletedFlowsCollection = new L7Flow[0];

    //internal static readonly Int32 MaximumLostBytes = 100000; // TODO
    internal static readonly Int64 FlowsTimeWindowIntersectionToleranceTicks = TimeSpan.FromSeconds(60).Ticks;
    internal static readonly Int64 TcpSessionAliveTimeoutTicks = TimeSpan.FromMinutes(10).Ticks;

    private TcpFlowReassembler _downFlowReassembler;
    private Int64 _lastFrameTimestampTicks;
    private TcpFlowReassembler _upFlowReassembler;

    public TcpConversationTracker(IPEndPoint sourceEndPoint, IPEndPoint destinationEndPoint) : base(
      sourceEndPoint, destinationEndPoint)
    {
    }

    public override IPProtocolType ProtocolType => IPProtocolType.TCP;

    public override IEnumerable<L7Conversation> Complete()
    {
      this._upFlowReassembler?.Complete();
      this._downFlowReassembler?.Complete();
      var bidirectionalL7Conversations  = this.PairUpDownCompletedFlows();
      var unidirectionalL7Conversations = this.CreateUnidirectionalL7Conversations(true);

      IEnumerable<L7Conversation> l7Conversations = null;
      if (bidirectionalL7Conversations != null && unidirectionalL7Conversations != null)
      {
        l7Conversations = bidirectionalL7Conversations.Union(unidirectionalL7Conversations).ToList();
      }
      else if (bidirectionalL7Conversations != null)
      {
        l7Conversations = bidirectionalL7Conversations;
      }
      else if (unidirectionalL7Conversations != null)
      {
        l7Conversations = unidirectionalL7Conversations;
      }

      return l7Conversations;
    }

    public override L7Conversation ProcessFrame(Frame frame)
    {
      var flowReassembler = this.GetOrCreateTcpFlowReassemblerForFrame(frame);
      flowReassembler.ProcessFrame(frame);

      this._lastFrameTimestampTicks = frame.TimestampTicks;

      return null;
    }

    private IEnumerable<L7Flow> AvailableUnidirectionL7Conversations(Boolean forceCreate, TcpFlowReassembler reassembler) =>
      reassembler.CompletedFlows.Where(flow => !flow.Paired)
                 .Where(flow => forceCreate ||
                                this._lastFrameTimestampTicks - flow.LastSeenTicks >= TcpSessionAliveTimeoutTicks);

    private void CreateL7ConversationAndAdd(L7Flow upFlow, L7Flow downFlow, ref List<L7Conversation> l7Conversations)
    {
      if (l7Conversations == null)
      {
        l7Conversations = new List<L7Conversation>(1);
      }

      l7Conversations.Add(this.CreateL7Conversation(upFlow, downFlow));

      upFlow?.SetPaired();
      downFlow?.SetPaired();
    }

    private List<L7Conversation> CreateUnidirectionalL7Conversations(Boolean forceCreate)
    {
      List<L7Conversation> l7Conversations = null;

      var upFlows = this._upFlowReassembler != null ?
                      this.AvailableUnidirectionL7Conversations(forceCreate, this._upFlowReassembler) :
                      EmptyCompletedFlowsCollection;
      foreach (var upFlow in upFlows)
      {
        this.CreateL7ConversationAndAdd(upFlow, null, ref l7Conversations);
      }

      var downFlows = this._downFlowReassembler != null ?
                        this.AvailableUnidirectionL7Conversations(forceCreate, this._downFlowReassembler) :
                        EmptyCompletedFlowsCollection;
      foreach (var downFlow in downFlows)
      {
        this.CreateL7ConversationAndAdd(null, downFlow, ref l7Conversations);
      }

      return l7Conversations;
    }

    private TcpFlowReassembler GetOrCreateTcpFlowReassemblerForFrame(Frame frame)
    {
      var direction = this.GetFlowDirectionForFrame(frame);
      switch (direction)
      {
        case FlowDirection.Up:
          return GetOrCreateTcpFlowReassemblerForFrame(ref this._upFlowReassembler, direction);
        case FlowDirection.Down:
          return GetOrCreateTcpFlowReassemblerForFrame(ref this._downFlowReassembler, direction);
        default:
          throw new ReassemblingException(frame, $"Invalid {nameof(direction)} value: {direction}");
      }
    }

    private static TcpFlowReassembler
      GetOrCreateTcpFlowReassemblerForFrame(ref TcpFlowReassembler flow, FlowDirection direction) =>
      flow ?? (flow = new TcpFlowReassembler(direction));

    private List<L7Conversation> PairUpDownCompletedFlows()
    {
      List<L7Conversation> l7Conversations = null;

      var upFlows = (this._upFlowReassembler != null ?
                       this._upFlowReassembler.CompletedFlows.Where(flow => !flow.Paired) :
                       EmptyCompletedFlowsCollection);
      var upFlowsArray = upFlows as L7Flow[] ?? upFlows.ToArray();
      var downFlows = (this._downFlowReassembler != null ?
                         this._downFlowReassembler.CompletedFlows.Where(flow => !flow.Paired) :
                         EmptyCompletedFlowsCollection);
      var downFlowsArray = downFlows as L7Flow[] ?? downFlows.ToArray();

      // Iterate over the pairs of unpaired down and up flows
      foreach (var upFlow in upFlowsArray)
      foreach (var downFlow in downFlowsArray)
      {
        // Both flows have identifier set and they are equal
        if (upFlow.FlowIdentifier.HasValue && downFlow.FlowIdentifier.HasValue)
        {
          if (upFlow.FlowIdentifier.Value == downFlow.FlowIdentifier.Value)
          {
            goto flowsMatched;
          }

          // If the flow identifiers are not matching, dont attempt to pair by time window intersection
          continue;
        }

        // Flows intersects in time (flow opening is missing, so no flow identifier was set)
        if ((upFlow.LastSeenTicks  + FlowsTimeWindowIntersectionToleranceTicks) >= downFlow.FirstSeenTicks &&
            (upFlow.FirstSeenTicks - FlowsTimeWindowIntersectionToleranceTicks) <= downFlow.LastSeenTicks)
        {
          goto flowsMatched;
        }

        continue;
        flowsMatched:
        this.CreateL7ConversationAndAdd(upFlow, downFlow, ref l7Conversations);
        break;
      }

      this.RemovePairedFlows();

      return l7Conversations;
    }

    private void RemovePairedFlows()
    {
      this._upFlowReassembler?.RemovePairedFlows();
      this._downFlowReassembler?.RemovePairedFlows();
    }
  }
}
