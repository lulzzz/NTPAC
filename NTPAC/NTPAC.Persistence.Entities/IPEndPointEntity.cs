using System;
using System.Net;

namespace NTPAC.Persistence.Entities
{
  public class IPEndPointEntity : IPEndPoint
  {
    public IPEndPointEntity() : this(0, 0) { }

    public IPEndPointEntity(Int64 address, Int32 port) : base(address, port) { }

    public IPEndPointEntity(IPAddress address, Int32 port) : base(address, port) { }

    public IPEndPointEntity(IPEndPoint endPoint) : base(endPoint.Address, endPoint.Port) { }
  }
}
