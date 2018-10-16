using System;
using System.Threading.Tasks;
using NTPAC.LoadBalancer.Messages;

namespace NTPAC.LoadBalancer.Interfaces
{
  public interface IBatchLoader
  {
    Task<PacketBatch> LoadBatch(Int32 batchSize);
    void Open(Uri uri);
    Int64? CaptureSize { get; }
  }
}
