using Comet.Domains.Buffers;

namespace Comet.Domains.Protocol;

public class Protocol
{
    private readonly RingBuffer _buffer;

    public Protocol(int size)
    {
        _buffer = new RingBuffer(size);
    }
}