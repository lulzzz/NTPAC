using System;
using System.Collections.Generic;
using NTPAC.Common.Helpers;
using NTPAC.ConversatinTracking.Interfaces.Models;

namespace NTPAC.Reassembling.Tests.Comparers
{
  public class L7PDUComparer : IEqualityComparer<L7Pdu>
  {
    public Boolean Equals(L7Pdu x, L7Pdu y) => MemberCompare.Equal(x, y);

    public Int32 GetHashCode(L7Pdu obj) => obj.GetHashCode();
  }
}
