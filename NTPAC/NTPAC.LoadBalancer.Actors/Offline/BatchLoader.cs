using System;
using System.Threading.Tasks;
using NTPAC.Common.Interfaces;
using NTPAC.LoadBalancer.Interfaces;
using NTPAC.LoadBalancer.Messages;
using NTPAC.Messages.RawPacket;

namespace NTPAC.LoadBalancer.Actors.Offline
{
  public class BatchLoader : IBatchLoader
  {
    private readonly IPcapLoader _pcapLoader;
    public BatchLoader(IPcapLoader pcapLoader) => this._pcapLoader = pcapLoader;

    public Task<PacketBatch> LoadBatch(Int32 batchSize)
    {
      return Task.Run(() =>
      {
        var packetBatch = new PacketBatch(batchSize);
        while (true)
        {
          var rawCapture = this._pcapLoader.GetNextPacket();
          if (rawCapture == null)
          {
            break;
          }

          packetBatch.Add(new RawPacket(rawCapture));
          if (packetBatch.Count >= packetBatch.Capacity)
          {
            break;
          }
        }

        return packetBatch;
      });
    }

    public void Open(Uri uri) { this._pcapLoader.Open(uri); }

    public Int64? CaptureSize => this._pcapLoader.CaptureSize;
  }
}
