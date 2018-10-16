using System;
using System.IO;

namespace NTPAC.Common.Extensions
{
  public static class UriExtension
  {
    public static FileInfo ToFileInfo(this Uri uri) => new FileInfo(uri.AbsolutePath);
  }
}
