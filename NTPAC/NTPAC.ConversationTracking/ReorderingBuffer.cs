using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NTPAC.Messages.RawPacket;

namespace NTPAC.ConversationTracking
{
  public class ReorderingBuffer : IEnumerable<RawPacketBatchRequest>
  {
    private readonly SortedList<Int64, RawPacketBatchRequest> _reorderingBuffer = new SortedList<Int64, RawPacketBatchRequest>();

    public void Store(RawPacketBatchRequest request) { this._reorderingBuffer.Add(request.SeqId, request); }

    public Boolean IsEmpty => !this._reorderingBuffer.Any();

    public IEnumerator<RawPacketBatchRequest> GetEnumerator() => new ReorderingBufferEnumerator(this._reorderingBuffer);

    public class ReorderingBufferEnumerator : IEnumerator<RawPacketBatchRequest>
    {
      private readonly SortedList<Int64, RawPacketBatchRequest> _reorderingBuffer;
      private Boolean _isInitiated;
      public ReorderingBufferEnumerator(SortedList<Int64, RawPacketBatchRequest> reorderingBuffer) => this._reorderingBuffer = reorderingBuffer;
      public Boolean MoveNext()
      {
        if (!this._reorderingBuffer.Any())
          return false;

        if (!this._isInitiated && this._reorderingBuffer.Any())
        {
          this._isInitiated = true;
          return true;
        }

        this._reorderingBuffer.RemoveAt(0);
        
        return this._reorderingBuffer.Any();
      }

      public void Reset() { }

      public RawPacketBatchRequest Current => this._reorderingBuffer.Values.FirstOrDefault();

      Object IEnumerator.Current => this.Current;

      public void Dispose() { }
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
  }
}