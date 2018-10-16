using NTPAC.ConversatinTracking.Interfaces;

namespace NTPAC.Messages.L4ConversationTracking
{
  public class L4ConversationTrackingCompleted
  {
    public L4ConversationTrackingCompleted(IL4ConversationKey l4ConversationKey) => this.L4ConversationKey = l4ConversationKey;
    public IL4ConversationKey L4ConversationKey { get; }
  }
}
