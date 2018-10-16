using System;
using UnitOfWork.BaseDataEntity;

namespace NTPAC.ConversatinTracking.Interfaces.Models
{
  public class Capture : IDataEntity
  {
    public Capture(CaptureInfo info, String reassemblerAddress) : this()
    {
      this.Info               = info;
      this.ReassemblerAddress = reassemblerAddress;
    }

    public Capture()
    {
      this.Processed           = DateTime.Now;
      this.Id                  = Guid.NewGuid();
      this.L7ConversationCount = 0;
      this.FirstSeen           = DateTime.MinValue;
      this.LastSeen            = DateTime.MinValue;
    }

    public DateTime FirstSeen { get; set; }

    public CaptureInfo Info { get; set; }
    public Int32 L7ConversationCount { get; set; }
    public DateTime LastSeen { get; set; }
    public DateTime Processed { get; set; }
    public String ReassemblerAddress { get; set; }
    public Guid Id { get; set; }

    public void UpdateForL7Conversation(L7Conversation l7Conversation)
    {
      this.L7ConversationCount++;
      this.UpdateTimestampsForL7Conversation(l7Conversation);
    }

    private void UpdateTimestampsForL7Conversation(L7Conversation l7Conversation)
    {
      if (l7Conversation.FirstSeen == DateTime.MinValue || l7Conversation.LastSeen == DateTime.MinValue)
      {
        return;
      }

      if (this.FirstSeen == DateTime.MinValue || this.FirstSeen > l7Conversation.FirstSeen)
      {
        this.FirstSeen = l7Conversation.FirstSeen;
      }

      if (this.LastSeen == DateTime.MinValue || this.LastSeen < l7Conversation.LastSeen)
      {
        this.LastSeen = l7Conversation.LastSeen;
      }
    }
  }
}
