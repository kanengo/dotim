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

 
        


        rb.Discard(3);
        
        Assert.That(rb.Length, Is.EqualTo(2));

        rb.Write(Encoding.UTF8.GetBytes("abcde"));
        
        var sp = rb.PeekAll();
        
  
        Assert.That(Encoding.UTF8.GetString(sp.Tail.ToArray(), sp.Tail.Offset, sp.Tail.Count), Is.EqualTo("de"));
 
        
        Console.WriteLine(Encoding.UTF8.GetString(rb.Bytes() ?? Array.Empty<byte>()));

        var iter = sp.Iterator();
        while (iter.MoveNext())
        {
            Console.Write(Convert.ToChar(iter.Current));
        }
        Console.WriteLine();
        
        Assert.Pass();
    }
}