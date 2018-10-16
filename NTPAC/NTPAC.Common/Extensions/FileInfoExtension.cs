using System;
using System.IO;

namespace NTPAC.Common.Extensions
{
  public static class FileInfoExtension
  {
    public static Uri ToUri(this FileInfo fileInfo) =>
      new UriBuilder {Scheme = Uri.UriSchemeFile, Host = "", Path = fileInfo.FullName}.Uri;
  }
}
