using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NTPAC.Common.Interfaces;

namespace NTPAC.Messages.RawPacket
{
  [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
  public class ProcessRawPacketFragmentsRequest : IMultipleValues<RawPacket>
  {
    public static readonly ProcessRawPacketFragmentsRequest EmptyInstance = new ProcessRawPacketFragmentsRequest(new Messages.RawPacket.RawPacket[0]);
    public ProcessRawPacketFragmentsRequest(IEnumerable<Messages.RawPacket.RawPacket> fragmentRequests) => this.Values = fragmentRequests;
    public IEnumerable<Messages.RawPacket.RawPacket> Values { get; set; }
  }
}
