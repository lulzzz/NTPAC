using NTPAC.ConversatinTracking.Interfaces;

namespace NTPAC.Messages.L3ConversationTracking
{
  public class L3ConversationTrackingCompleted
  {
    public L3ConversationTrackingCompleted(IL3ConversationKey l3Key) => this.L3ConversationKey = l3Key;
    public IL3ConversationKey L3ConversationKey { get; }
  }
}
