namespace Comet.Models;

public class RingBuffer
{
    private byte[] _buf;
    private int _size = 0;
    private int _r = 0;
    private int _w = 0;
    private bool _isEmpty = true;

    public int Length
    {
        get
        {
            if (_r == _w)
            {
                return _isEmpty ? 0 : _size;
            }

            return _w > _r ? _w - _r : _size - _r + _w;
        }
    }

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

    public RingBuffer(int size)
    {
        size = _ceilToPowerOfTow(size);
        
        _buf = new byte[size];
        _size = size;
    }

    public int Write(ArraySegment<byte> p)
    {
        var n = p.Count;
        if (n == 0)
            return 0;

        var free = Available();
        if (n > free)
        {
            //grow
        }

        if (_w >= _r)
        {
            var c1 = _size - _w;
            if (c1 > n)
            {
                p.CopyTo(new ArraySegment<byte>(_buf,_w, _size));
                _w += n;
            }
            else
            {
                p[..c1].CopyTo(new ArraySegment<byte>(_buf, _w, _size));
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

        if (_w == _size)
            _w = 0;

        _isEmpty = false;
        
        return 0;
    }

    public int Available()
    {
        if (_r == _w)
        {
            return _isEmpty ? _size : 0;
        }

        return _w < _r ? _r - _w : _size - _w + _r;
    }
    
}