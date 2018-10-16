using System;
using System.Collections.Generic;
using NTPAC.ConversatinTracking.Interfaces.Models;

namespace NTPAC.Reassembling.IP
{
  public class IPFragmentComparer : IComparer<Frame>
  {
    public Int32 Compare(Frame x, Frame y) => x.Offset - y.Offset;
  }
}
