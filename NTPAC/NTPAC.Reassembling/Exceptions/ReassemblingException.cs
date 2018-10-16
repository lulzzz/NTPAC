using System;
using NTPAC.ConversatinTracking.Interfaces.Models;

namespace NTPAC.Reassembling.Exceptions
{
  public class ReassemblingException : Exception
  {
    public ReassemblingException(Frame frame, String message) : base(message) => this.Frame = frame;

    public ReassemblingException(String message, Exception innerException) : base(message, innerException) =>
      this.Frame = this.Frame;

    public Frame Frame { get; }
  }
}
