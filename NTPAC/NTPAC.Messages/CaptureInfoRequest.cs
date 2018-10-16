using MessagePack;

namespace NTPAC.Messages
{
  [MessagePackObject]
  public class CaptureInfoRequest
  {
    public static readonly CaptureInfoRequest Instance = new CaptureInfoRequest();

    public CaptureInfoRequest() { }
  }
}
