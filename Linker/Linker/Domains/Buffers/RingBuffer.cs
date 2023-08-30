
using System.Collections;

namespace Linker.Domains.Buffers;

public class RingBuffer
{
    private readonly byte[] _buf;
    private readonly ArraySegment<byte> _slice;
    private readonly int _size = 0;
    private int _r = 0;
    private int _w = 0;
    private bool _isEmpty = true;

    public bool IsEmpty => _isEmpty;

    public int Length => _buffered();

    public int Available => _available();

    public int Size => _size;
    

    private static int _ceilToPowerOfTow(int n)
    {
        if (n <= 2)
            return 2;
        n--;
        n |= n >> 1;
        n |= n >> 2;
        n |= n >> 4;
        n |= n >> 8;
        n |= n >> 16;
        n++;

        return n;
    }

    public readonly struct Separation 
    {
        public Separation(ArraySegment<byte> head = default, ArraySegment<byte> tail = default)
        {
            Head = head;
            Tail = tail;
        }

        public ArraySegment<byte> Head { get; } = ArraySegment<byte>.Empty;
        public ArraySegment<byte> Tail { get; } = ArraySegment<byte>.Empty;

        public SeparationIterator Iterator()
        {
            return new SeparationIterator(this);
        }
        public  struct SeparationIterator : IEnumerator<byte>
        {
            private readonly Separation _separation;
            private int _current = 0;
            private int _index = -1;
            
            public SeparationIterator(Separation separation)
            {
                _separation = separation;
            }
            
            public bool MoveNext()
            {
                if (_current == 0 )
                {
                    if (_index >= _separation.Head.Count - 1)
                    {
                        _index = -1;
                        _current = 1;
                    }
                    else
                    {
                        _index += 1;
                        return true;
                     
                    }
                }

                if (_index >= _separation.Tail.Count - 1) return false;
                
                _index += 1;
                return true;
            }

            public void Reset()
            {
                _current = 0;
                _index = -1;
            }

            public byte Current
            {
                get
                {
                    try
                    {
                        return _current == 0 ? _separation.Head[_index] : _separation.Tail[_index];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }

    public RingBuffer(int size)
    {
        size = _ceilToPowerOfTow(size);
        
        _buf = new byte[size];
        _size = size;

        _slice = new ArraySegment<byte>(_buf);
    }

    public Separation Peek(int n)
    {
        if (_isEmpty)
        {
            return default;
        }

        if (n <= 0)
        {
            return PeekAll();
        }

        
        
        if (_w > _r)
        {
            var m1 = _w - _r;
            if (m1 > n)
                m1 = n;
            
            return new Separation(new ArraySegment<byte>(_buf, _r, m1));
        }

        var m2 = _size - _r + _w;
        if (m2 > n)
            m2 = n;

        ArraySegment<byte> head;
        ArraySegment<byte> tail = default;
        if (_r + m2 <= _size)
        {
            head = new ArraySegment<byte>(_buf, _r, m2);
        }
        else
        {
            var c1 = _size - _r;
            head= new ArraySegment<byte>(_buf, _r, _size - _r);
            var c2 = m2 - c1;
            tail = new ArraySegment<byte>(_buf, 0, c2);
        }

        return new Separation(head, tail);
    }

    public Separation PeekAll()
    {
        if (_isEmpty)
            return default;
        
        ArraySegment<byte> head;
        ArraySegment<byte> tail = default;
        if (_w > _r)
        {
            head = new ArraySegment<byte>(_buf, _r, _w - _r);
            return new Separation(head);
        }

        head = new ArraySegment<byte>(_buf, _r, _size - _r);
        if (_w != 0)
            tail = new ArraySegment<byte>(_buf, 0, _w);

        return new Separation(head,tail);
    }

    public int Discard(int n)
    {
        if (n <= 0)
            return 0;

        var discarded = _buffered();
        if (n < discarded)
        {
            _r = (_r + n) % _size;
            if (_r == _w)
                _reset();
            return n;
        }
        
        _reset();
        return discarded;
    }

    public int Read(ArraySegment<byte> p)
    {
        if (p.Count == 0)
        {
            return 0;
        }

        if (_isEmpty)
        {
            return 0;
        }

        if (_w > _r)
        {
            var n = _w - _r;
            if (n > p.Count)
            {
                n = p.Count;
            }
            
            new ArraySegment<byte>(_buf,_r,n).CopyTo(p);
            _r += n;
            if (_r == _w)
                _reset();

            return n;
        }

        var n1 = _size - _r + _w;
        if (n1 > p.Count)
        {
            n1 = p.Count;
        }

        if (_r + n1 <= _size)
        {
            new ArraySegment<byte>(_buf, _r, n1).CopyTo(p);
        }
        else
        {
            var c1 = _size - _r;
            var segment = new ArraySegment<byte>(_buf);
            segment[_r..].CopyTo(p);
            var c2 = n1 - c1;
            segment[..c2].CopyTo(p[c1..]);
        }

        _r = (_r + n1) % _size;
        if (_r == _w)
            _reset();

        return n1;
    }
    
    public byte? ReadByte()
    {
        if (_isEmpty)
            return null;

        var b = _buf[_r];
        
        _r = (_r + 1) & (_size - 1);
        if (_r == _w)
            _reset();

        return b;
    }

    public int Write(byte[] p)
    {
        return Write(new ArraySegment<byte>(p));
    }

    public int Write(ArraySegment<byte> p)
    {
        var n = p.Count;
        if (n == 0)
            return 0;

        var free = _available();
        if (n > free)
        {
            throw new ApplicationException("no available space to write");
        }

        if (_w >= _r)
        {
            var c1 = _size - _w;
            if (c1 > n)
            {
                p.CopyTo(new ArraySegment<byte>(_buf,_w, n));
                _w += n;
            }
            else
            {
                p[..c1].CopyTo(new ArraySegment<byte>(_buf, _w, c1));
                var c2 = n - c1;
                p[c1..].CopyTo(new ArraySegment<byte>(_buf));
                _w = c2;
            }
        }
        else
        {
            p.CopyTo(new ArraySegment<byte>(_buf,_w, _size));
            _w += n;
        }
        
        _w = _w == _size ? 0 : _w;

        _isEmpty = false;
        
        return n;
    }

   

    public void WriteByte(byte b)
    {
        if (_available() < 1)
        {
            throw new ApplicationException("no available space to write");
        }

        _buf[_w] = b;
        _w = (_w + 1) & (_size - 1);

        _isEmpty = false;
    }

    public byte[]? Bytes()
    {
        if (_isEmpty)
            return default;
        
        if (_w ==_r)
        {
            var bytes = new ArraySegment<byte>(new byte[Length]);
            var slice = new ArraySegment<byte>(_buf);
            slice[_r..].CopyTo(bytes);
            var n = _size - _r;
            slice[.._w].CopyTo(bytes[n..]);
            return bytes.Array;
        }

        var bb = new ArraySegment<byte>(new byte[Length]);
        if (_w > _r)
        {
            _slice[_r.._w].CopyTo(bb);
            return bb.Array;
        }
        
        _slice[_r..].CopyTo(bb);

        if (_w == 0) return bb.Array;
        {
            var n = _size - _r;
            _slice[.._w].CopyTo(bb[n..]);
        }

        return bb.Array;
    }

    private int _buffered()
    {
        if (_r == _w)
        {
            return _isEmpty ? 0 : _size;
        }

        return _w > _r ? _w - _r : _size - _r + _w;
    }
    

    private int _available()
    {
        if (_r == _w)
        {
            return _isEmpty ? _size : 0;
        }

        return _w < _r ? _r - _w : _size - _w + _r;
    }

    private void _reset()
    {
        _isEmpty = true;
        _r = _w = 0;
        
    }
    
}