using System;
using NTPAC.Messages.RawPacket;
using Xunit;

namespace NTPAC.ConversationTracking.Tests
{
  public class ReorderingBufferTests
  {
    private readonly ReorderingBuffer _reorderingBufferSUT;
    
    private static readonly Int64 MessageId = 10;

    public ReorderingBufferTests()
    {
      this._reorderingBufferSUT = new ReorderingBuffer();
    }

    [Fact]
    public void Empty_MoveNext_False()
    {
      //Arrange
      //Act
      //Assert
      using (var enumerator = this._reorderingBufferSUT.GetEnumerator())
      {
        Assert.False(enumerator.MoveNext());
        Assert.False(enumerator.MoveNext());
      }
    }

    [Fact]
    public void OneItem_MoveNext_True()
    {
      //Arrange
      this._reorderingBufferSUT.Store(new RawPacketBatchRequest() { SeqId = 0});
      //Act
      //Assert
      using (var enumerator = this._reorderingBufferSUT.GetEnumerator())
      {
        Assert.True(enumerator.MoveNext());
        Assert.False(enumerator.MoveNext());
      }
    }

    [Fact]
    public void TwoItem_MoveNext_True_True()
    {
      //Arrange
      this._reorderingBufferSUT.Store(new RawPacketBatchRequest() { SeqId = 0 });
      this._reorderingBufferSUT.Store(new RawPacketBatchRequest() { SeqId = 1 });
      //Act
      //Assert
      using (var enumerator = this._reorderingBufferSUT.GetEnumerator())
      {
        Assert.True(enumerator.MoveNext());
        Assert.True(enumerator.MoveNext());
        Assert.False(enumerator.MoveNext());
      }
    }

    [Fact]
    public void Empty_IsEmpty_True()
    {
      //Arrange
      //Act
      //Assert
      Assert.True(this._reorderingBufferSUT.IsEmpty);
    }

    [Fact]
    public void OneItem_IsEmpty_Flase()
    {
      //Arrange
      this._reorderingBufferSUT.Store(new RawPacketBatchRequest() { SeqId = 0 });
      //Act
      //Assert
      Assert.False(this._reorderingBufferSUT.IsEmpty);
    }

    [Fact]
    public void Store_Sequence_With_Unique_SeqId()
    {
      //Act
      this._reorderingBufferSUT.Store(new RawPacketBatchRequest { SeqId = 0 });
      this._reorderingBufferSUT.Store(new RawPacketBatchRequest { SeqId = 2 });
      this._reorderingBufferSUT.Store(new RawPacketBatchRequest { SeqId = 1 });
    }

    [Fact]
    public void Store_Sequence_With_Duplicite_SeqId()
    {
      //Arrange
      this._reorderingBufferSUT.Store(new RawPacketBatchRequest() { SeqId = 0 });
      //Assert
      Assert.Throws<System.ArgumentException>(() => this._reorderingBufferSUT.Store(new RawPacketBatchRequest() { SeqId = 0 }));
    }

    [Fact]
    public void Enumerate_Empty()
    {
      //Arrange
      //Act & Assert
      foreach (var item in this._reorderingBufferSUT)
      {
        Assert.True(false, "Cannot be iterated over empty set");
      }
    }

    [Fact]
    public void Enumerate_OneItem()
    {
      //Arrange
      var rawPacketBatchRequest = new RawPacketBatchRequest() { SeqId = 0 };
      this._reorderingBufferSUT.Store(rawPacketBatchRequest);
      var isFirstIteration = true;
      //Act & Assert
      foreach (var item in this._reorderingBufferSUT)
      {
        Assert.Equal(rawPacketBatchRequest, item);

        Assert.True(isFirstIteration);
        isFirstIteration = false;
      }

      Assert.True(this._reorderingBufferSUT.IsEmpty);
    }


    [Fact]
    public void Enumerate_OneItem_And_Store_Back()
    {
      //Arrange
      var rawPacketBatchRequest = new RawPacketBatchRequest() { SeqId = 0 };
      this._reorderingBufferSUT.Store(rawPacketBatchRequest);
      var isFirstIteration = true;
      //Act & Assert
      foreach (var item in this._reorderingBufferSUT)
      {
        Assert.Equal(rawPacketBatchRequest, item);

        Assert.True(isFirstIteration);
        isFirstIteration = false;
      }

      Assert.True(this._reorderingBufferSUT.IsEmpty);

      this._reorderingBufferSUT.Store(rawPacketBatchRequest);

      Assert.False(this._reorderingBufferSUT.IsEmpty);
    }
  }
}
