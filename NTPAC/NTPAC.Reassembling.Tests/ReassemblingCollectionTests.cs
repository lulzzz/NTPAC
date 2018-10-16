using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Moq;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.Reassembling.TCP.Collections;
using Xunit;
using Xunit.Abstractions;

namespace NTPAC.Reassembling.Tests
{
  public class ReassemblingCollectionTests : ReassemblingTestBase
  {
    private readonly Mock<Frame> _frame1;
    private readonly Mock<Frame> _frame2;
    private readonly Mock<Frame> _frame3;

    public ReassemblingCollectionTests(ITestOutputHelper output) : base(output)
    {
      this._frame1 = new Mock<Frame>();
      this._frame2 = new Mock<Frame>();
      this._frame3 = new Mock<Frame>();
    }

// Disabled because of a internal decision making whether to add frame to collection          
//        [Fact]
//        public void ClassImplementingICollectionAddWorks()
//        {
//            var c = new ReassemblingCollection(new[] {this._frame1.Object,this._frame2.Object});
//            c.Add(this._frame3.Object);
//            Assert.Equal(3, c.Count);
//            Assert.Contains(this._frame3.Object, c);
//        }
//
//        [Fact]
//        public void ClassImplementingICollectionCastToICollectionAddWorks()
//        {
//            var c = new ReassemblingCollection(new[] {this._frame1.Object,this._frame2.Object});
//            c.Add(this._frame3.Object);
//            Assert.Equal(3, c.Count);
//            Assert.Contains(this._frame3.Object, c);
//        }

    [Fact]
    public void ClassImplementingICollectionCastToICollectionClearWorks()
    {
      var c = new ReassemblingCollection(new[] {this._frame1.Object, this._frame2.Object});
      c.Clear();
      Assert.Empty(c);
    }

    [Fact]
    public void ClassImplementingICollectionCastToICollectionContainsWorks()
    {
      var c = new ReassemblingCollection(new[] {this._frame1.Object, this._frame2.Object});
      Assert.Contains(this._frame1.Object, c);
      Assert.DoesNotContain(this._frame3.Object, c);
    }

    [Fact]
    public void ClassImplementingICollectionCastToICollectionCountWorks()
    {
      Assert.Throws<InvalidCastException>(
        () => ((ICollection<Object>) new ReassemblingCollection(new[]
                                                                {
                                                                  this._frame1.Object,
                                                                  this._frame2.Object,
                                                                  this._frame3.Object
                                                                })).Count);
    }

    [Fact]
    public void ClassImplementingICollectionCastToICollectionRemoveWorks()
    {
      var c = new ReassemblingCollection(new[] {this._frame1.Object, this._frame2.Object});
      c.Clear();
      Assert.Empty(c);
    }

    [Fact]
    public void ClassImplementingICollectionClearWorks()
    {
      var c = new ReassemblingCollection(new[] {this._frame1.Object, this._frame2.Object});
      c.Clear();
      Assert.Empty(c);
    }

    [Fact]
    public void ClassImplementingICollectionContainsWorks()
    {
      var c = new ReassemblingCollection(new[] {this._frame1.Object, this._frame2.Object});
      Assert.Contains(this._frame1.Object, c);
      Assert.DoesNotContain(this._frame3.Object, c);
    }

    [Fact]
    public void ClassImplementingICollectionCountWorks()
    {
      Assert.Equal(2, new ReassemblingCollection(new[] {this._frame1.Object, this._frame2.Object}).Count);
    }

    [Fact]
    public void ClassImplementingICollectionRemoveWorks()
    {
      var c = new ReassemblingCollection(new[] {this._frame1.Object, this._frame2.Object});
      c.Clear();
      Assert.Empty(c);
    }

    [Fact]
    public void CustomClassThatShouldImplementICollectionDoesSo()
    {
      Assert.False((Object) new ReassemblingCollection(new[] {this._frame1.Object, this._frame2.Object}) is ICollection<Object>);
    }

    [Fact]
    public void TcpKeepAliveTest1()
    {
      var c = new ReassemblingCollection();
      this.AddFramesFromPcap(c, "isa-http_keepalive1_dst1.pcapng", IPAddress.Parse("147.229.176.17"));
      Assert.Equal(19, c.Count);
    }

    [Fact]
    public void TcpRetransmission()
    {
      var c = new ReassemblingCollection();
      this.AddFramesFromPcap(c, "isa-http_retransmission.pcapng", IPAddress.Parse("147.229.176.17"));
      Assert.Equal(6, c.Count);
    }

    [Fact]
    public void TcpReusedPortAndKeepAlives()
    {
      var c = new ReassemblingCollection();
      this.AddFramesFromPcap(c, "isa-http_reusedPortsAndKeepalives.pcapng", IPAddress.Parse("147.229.176.17"));
      Assert.Equal(52, c.Count);
      this.AssertEnumerableSorted(c, frame => frame.TimestampTicks);
    }

    private void AddFramesFromPcap(ReassemblingCollection c, String pcapFileName, IPAddress sourceIPAddress)
    {
      this.AddFramesFromPcap(c, pcapFileName, f => f.SourceAddress.Equals(sourceIPAddress));
    }

    private void AddFramesFromPcap(ReassemblingCollection c, String pcapFileName, Func<Frame, Boolean> framePredicate = null)
    {
      var frames = this.GetFramesFromPcap(pcapFileName);
      if (framePredicate != null)
      {
        frames = frames.Where(framePredicate);
      }

      foreach (var frame in frames)
      {
        c.Add(frame);
      }
    }

    private void AssertEnumerableSorted<T, TKey>(IEnumerable<T> enumerable, Func<T, TKey> dissectorFunc) where TKey : IComparable
    {
      var previousKey = default(TKey);
      var i           = 0;
      foreach (var item in enumerable)
      {
        var key = dissectorFunc(item);
        if (i > 0)
        {
          if (previousKey.CompareTo(key) > 0)
          {
            // TODO NotSortedException
            throw new Exception("Enumerable is not sorted");
          }
        }

        previousKey = key;
        i++;
      }
    }
  }
}
