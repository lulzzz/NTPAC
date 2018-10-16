using System;
using System.Diagnostics.CodeAnalysis;
using MessagePack;

namespace NTPAC.Messages.RawPacket
{
  [MessagePackObject]
  [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
  public class ProcessRawPacketRequestError : RawPacket
  {
    public ProcessRawPacketRequestError(String description) => this.Description = description;

    public String Description { get; set; }
  }
}
