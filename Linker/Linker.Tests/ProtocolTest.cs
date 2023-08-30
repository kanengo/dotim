using Linker.Domains.Protocol;

namespace Linker.Tests;

public class ProtocolTest
{
    [Test]
    public void TestProtocol()
    {
        var protocol = new Protocol(32);

        ushort size = 16;
        var b = new byte[2];

        b[0] = (byte)(size >> 8);
        b[1] = (byte)(size & ((1 << 8) - 1));

        b = b.Concat(new byte[] { 1,2,3,4,5,6,7,8}).ToArray();
        
        Assert.That(protocol.BufferAvailable, Is.EqualTo(32));
        
        protocol.Write(new ArraySegment<byte>(b));

        Assert.That(protocol.BufferAvailable, Is.EqualTo(22));
        
        var data = protocol.CheckCompleteData();
            
        Assert.That(protocol.BufferAvailable, Is.EqualTo(24));
        
        Assert.That(data, Is.Null);
        
        protocol.Write(new ArraySegment<byte>(new byte[] {9,10,11,12,13,14,15,16 }));
        Assert.That(protocol.BufferAvailable, Is.EqualTo(16));
        data = protocol.CheckCompleteData();
        
        Assert.That(data, Is.Not.Null);
        Assert.That(protocol.BufferAvailable, Is.EqualTo(32));
        
        Assert.That(data, Is.EqualTo(new byte[] {1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16}));
        
        Assert.That(protocol.BufferAvailable, Is.EqualTo(32));
    }
}