using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;
using NTPAC.ConversatinTracking.Interfaces.Models;
using PacketDotNet;
using PacketDotNet.Utils;

namespace NTPAC.Reassembling.IP
{
  public class Ipv4DefragmentationEngine
  {
    private readonly ConcurrentDictionary<IPFragmentKey, ICollection<Frame>> _currentIPv4FragmentedFrames =
      new ConcurrentDictionary<IPFragmentKey, ICollection<Frame>>();

    private readonly ILogger<Ipv4DefragmentationEngine> _logger;

    public Ipv4DefragmentationEngine(ILogger<Ipv4DefragmentationEngine> logger) => this._logger = logger;

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
    public (Boolean isDefragmentationSuccessful, Int64 firstTimestamp, IPv4Packet defragmentedIpv4Packet)
      TryDefragmentFragments(Frame fragment)
    {
      var fragments =
        this._currentIPv4FragmentedFrames.GetOrAdd(fragment.Key, _ => new SortedSet<Frame>(new IPFragmentComparer()));

      ICollection<Frame> sortedFragments;
      var                defragmentedPayloadLength = 0;

      // Gods of Akka, please dont damm me for using this lock
      lock (fragments)
      {
        if (!this.CheckIfFragmentsAreStillTheSame(fragment, fragments))
        {
          return this.TryDefragmentFragments(fragment);
        }

        fragments.Add(fragment);

        if (IsTheLastFragmentPresent(fragments))
        {
          return (false, 0, null);
        }

        sortedFragments = fragments;

        var checkAllFragmentsPresentResult = CheckIfAllFragmetsArePresent(sortedFragments);
        if (!checkAllFragmentsPresentResult.areAllFragmentsresent)
        {
          return (false, 0, null);
        }

        defragmentedPayloadLength = checkAllFragmentsPresentResult.defragmentedPayloadLength;

        if (!this.TryRemoveFragments(fragment))
        {
          this._logger.LogError($"Fragment {fragment} cannot be removed from buffered IPv4 fragments.");
        }
      }

      var mergeFragmentedPayloads = MergeFragmentedPayloads(defragmentedPayloadLength, sortedFragments);

      var defragmentedIpv4Packet = new IPv4Packet(fragment.Key.SourceAddress, fragment.Key.DestinationAddress);
      // TODO tunneling support
      switch (fragment.IpProtocol)
      {
        // TCP and UDP packet constructors will setup references from their parents (defragmentedIPv4Packet) to themselves
        case IPProtocolType.TCP:
          defragmentedIpv4Packet.PayloadPacket =
            new TcpPacket(new ByteArraySegment(mergeFragmentedPayloads.defragmentedPayloadBuffer), defragmentedIpv4Packet);
          break;
        case IPProtocolType.UDP:
          defragmentedIpv4Packet.PayloadPacket =
            new UdpPacket(new ByteArraySegment(mergeFragmentedPayloads.defragmentedPayloadBuffer), defragmentedIpv4Packet);
          break;
        case IPProtocolType.IP:
          defragmentedIpv4Packet.PayloadData = mergeFragmentedPayloads.defragmentedPayloadBuffer;
          break;
      }

      return (true, mergeFragmentedPayloads.firstTimestamp, defragmentedIpv4Packet);
    }

    private static (Boolean areAllFragmentsresent, Int32 defragmentedPayloadLength) CheckIfAllFragmetsArePresent(
      ICollection<Frame> sortedFragments)
    {
      // Check whether we have all fragments
      var expectedOffset = 0;
      foreach (var storedFragment in sortedFragments)
      {
        if (storedFragment.Offset != expectedOffset)
        {
          return (false, 0);
        }

        expectedOffset += storedFragment.L7PayloadLength;
      }

      if (sortedFragments.Last().MoreFragments)
      {
        return (false, 0);
      }

      return (true, expectedOffset);
    }

    private Boolean CheckIfFragmentsAreStillTheSame(Frame fragment, ICollection<Frame> fragments)
    {
      this._currentIPv4FragmentedFrames.TryGetValue(fragment.Key, out var currentFragments);
      return ReferenceEquals(fragments, currentFragments);
    }

    private static Boolean IsTheLastFragmentPresent(IEnumerable<Frame> fragments) => fragments.Last().MoreFragments;

    private static (Int64 firstTimestamp, Byte[] defragmentedPayloadBuffer) MergeFragmentedPayloads(
      Int32 defragmentedPayloadLen,
      IEnumerable<Frame> sortedFragments)
    {
      var defragmentedPayloadBuffer = new Byte[defragmentedPayloadLen];

      var storedFragments = sortedFragments as Frame[] ?? sortedFragments.ToArray();

      foreach (var storedFragment in storedFragments)
      {
        storedFragment.L7Payload.CopyTo(defragmentedPayloadBuffer, storedFragment.Offset);
      }

      var firstTimestamp = storedFragments[0].TimestampTicks;

      return (firstTimestamp, defragmentedPayloadBuffer);
    }

    private Boolean TryRemoveFragments(Frame fragment) => this._currentIPv4FragmentedFrames.TryRemove(fragment.Key, out _);
  }
}
