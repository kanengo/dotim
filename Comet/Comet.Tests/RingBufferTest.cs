using System.Text;
using Comet.Domains.Buffers;

namespace Comet.Tests;

public class RingBufferTest
{
    [Test]
    public void TestRingBuffer()
    {
        var rb = new RingBuffer(8);
        Assert.That(rb.Size, Is.EqualTo(8));

        var p = Encoding.UTF8.GetBytes("leeka");
        rb.Write(p);

        var sp = rb.PeekAll();
        
        Assert.That(sp.HasValue, Is.True);
        Assert.That(sp.Value.Head.HasValue, Is.True);
        Assert.That(sp.Value.Tail.HasValue, Is.False);


        rb.Discard(3);
        
        Assert.That(rb.Length, Is.EqualTo(2));

        rb.Write(Encoding.UTF8.GetBytes("abcde"));
        
        sp = rb.PeekAll();
        Assert.That(sp.HasValue, Is.True);
        Assert.That(sp?.Head.HasValue, Is.True);
        Assert.That(sp?.Tail.HasValue, Is.True);

        if (sp is { Tail: not null })
        {
            Assert.That(Encoding.UTF8.GetString(sp.Value.Tail?.ToArray(), sp.Value.Tail.Value.Offset, sp.Value.Tail.Value.Count), Is.EqualTo("de"));
        }
        
        Console.WriteLine(Encoding.UTF8.GetString(rb.Bytes() ?? Array.Empty<byte>()));
        
        Assert.Pass();
    }
}