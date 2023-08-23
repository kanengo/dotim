using System.Xml;
using Comet.Domains.Buffers;
using Comet.Exceptions;
using Grpc.Core;

namespace Comet.Domains.Protocol;

public class Protocol
{
    private readonly RingBuffer _cachedBuffer;
    
    private Stage _stage = Stage.Empty;

    private int _remainder = 0;

    private const int HeaderSize = 2;
    
    private enum Stage
    {
        Empty,
        Pending,
    }
    

    public Protocol(int size)
    {
        _cachedBuffer = new RingBuffer(size);
    }

    public void Write(ArraySegment<byte> buffer)
    {
        _cachedBuffer.Write(buffer);
    }

    public byte[]? CheckCompleteData()
    {
        switch (_stage)
        {
            case Stage.Empty:
                if (_cachedBuffer.Length >= HeaderSize)
                {
                    var sp = _cachedBuffer.Peek(HeaderSize);
                    var count = HeaderSize;

                    int dataSize = default;
                    
                    var iter = sp.Iterator();
                    while (iter.MoveNext())
                    {
                        var b = iter.Current;
                        dataSize += Convert.ToInt32(b) << (count - 1);
                        count--;

                        if (count < 0)
                        {
                            break;
                        }
                    }

                    _remainder = dataSize;
                }
                break;
            case Stage.Pending:
                break;
            default:
                break;
        }

        return default;
    }
    
}