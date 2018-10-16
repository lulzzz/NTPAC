using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace NTPAC.Reassembling.Tests
{
  public class Ipv4ReassemblingTests : ReassemblingTestBase
  {
    public Ipv4ReassemblingTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void Ipv4_defragmentation()
    {
      var frames = this.GetFramesFromPcap("isa-http_fragment.pcap", true);
      Assert.Equal(2, frames.Count());
    }
  }
}
