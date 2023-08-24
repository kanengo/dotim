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

    public int BufferAvailable => _cachedBuffer.Available;
    
    private enum Stage
    {
        Empty,
        Pending,
    }
    

    public Protocol(int size)
    {
        _cachedBuffer = new RingBuffer(size);
    }

    public int Write(ArraySegment<byte> buffer)
    {
        return _cachedBuffer.Write(buffer);
    }

    public byte[]? CheckCompleteData()
    {
        while (true)
        {
            switch (_stage)
            {
                case Stage.Empty:
                    if (_cachedBuffer.Length < HeaderSize) return default;
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
                    _cachedBuffer.Discard(HeaderSize);

                    //超出数据限制
                    if (_remainder > _cachedBuffer.Size)
                    {
                        throw new ConnectException(ConnectErrorCode.ReceivedBufferTooLarge);
                    }
                    _stage = Stage.Pending;
                    break;
                case Stage.Pending:
                    if (_cachedBuffer.Length < _remainder) return default;
                    
                    var data = new byte[_remainder];
                    var n = _cachedBuffer.Read(data);

                    if (n != _remainder)
                    {
                        throw new ConnectException(ConnectErrorCode.BufferInvalid, "buffer read n invalid");
                    }

                    _stage = Stage.Empty;
                    _remainder = 0;
                    
                    return data;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        

        return default;
    }
    
}