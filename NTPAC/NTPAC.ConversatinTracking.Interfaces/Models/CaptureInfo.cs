using System;

namespace NTPAC.ConversatinTracking.Interfaces.Models
{
  public class CaptureInfo
  {
    public CaptureInfo() { }
    public CaptureInfo(Uri uri) => this.UriString = uri.AbsoluteUri;
    public Uri Uri => new Uri(this.UriString);
    public String UriString { get; set; }
  }
}
