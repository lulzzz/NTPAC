using System;
using System.Net.Sockets;

namespace NTPAC.DTO.ConversationTracking
{
  public class IPEndPointDTO
  {
    public String Address { get; set; }
    public AddressFamily AddressFamily { get; set; }
    public Int32 Port { get; set; }
  }
}
