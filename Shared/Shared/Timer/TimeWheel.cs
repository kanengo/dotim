namespace Shared.Timer;

public class TimeWheel<TValue> : IDisposable
{
    private readonly TimeSpan _interval;

    private readonly TimeList[] _near;

    private readonly TimeList[][] _t;

    private uint _time;

    private SpinLock _locker = new();

    private readonly CancellationTokenSource _cancellationTokenSource;
    
    private const int TimeNearShift = 8;
    private const int TimeNear = 1 << TimeNearShift;
    private const int TimeNearMask = TimeNear - 1;
    private const int TimeLevelShift = 6;
    private const int TimeLevel = 1 << TimeLevelShift;
    private const int TimeLevelMask = TimeLevel - 1;
    
    private class TimeNode
    {
        public uint Expire { get; set; }
        public TValue? Data { get; set; }
        public TimeNode? Next = default;
    }

    private struct TimeList
    {
        public readonly TimeNode Head;

        private TimeNode _tail;

        public TimeList()
        {
            var node = new TimeNode();
            Head = node;
            _tail = node;
        }

        public TimeNode? Clear()
        {
            var node = Head.Next;
            Head.Next = null;
            _tail = Head;

            return node;
        }

        public void AddNode(TimeNode node)
        {
            if (Head == _tail)
            {
                Head.Next = node;
            }
            else
            {
                _tail.Next = node;
            }

            _tail = node;
            node.Next = null;
        }
    }

    public  class TimeWheelEventArgs : EventArgs
    {
        public TValue? Data { get; }

        public TimeWheelEventArgs(TValue? data = default)
        {
            Data = data;
        }
    }
    
    public event EventHandler<TimeWheelEventArgs>? OnTick;
    
    public TimeWheel(TimeSpan interval, CancellationToken cancellationToken = default)
    {
        _interval = interval;

        _near = new TimeList[TimeNear];
        for (var i = 0; i < _near.Length; i++)
        {
            _near[i] = new TimeList();
        }
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        _t = new TimeList[4][];
        for (var i = 0; i < _t.Length; i++)
        {
            _t[i] = new TimeList[TimeLevel];
            for (var i1 = 0; i1 < _t[i].Length; i1++)
            {
                _t[i][i1] = new TimeList();
            }
        }
    }

    public void Start()
    {
        Task.Run(async () =>
        {
            var lastTick = DateTime.Now;
            while (true)
            {
                if (_cancellationTokenSource.IsCancellationRequested) break;
                
                var nextTick = lastTick.Add(_interval);
                lastTick = nextTick;
                await Task.Delay(nextTick.Subtract(DateTime.Now), _cancellationTokenSource.Token).ConfigureAwait(false);
                
                _tick();
            }
        }, _cancellationTokenSource.Token);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        GC.SuppressFinalize(this);
    }

    public void Timeout(TimeSpan timeout, TValue data)
    {
        if (timeout <= TimeSpan.Zero)
        {
            OnTick?.Invoke(this, new TimeWheelEventArgs(data));
            return;
        }
        
        var node = new TimeNode()
        {
            
            Expire = _time + (uint)(timeout.Ticks/_interval.Ticks),
            Data = data
        };
        
        var lockTaken = false;
        try
        {
            _locker.Enter(ref lockTaken);
           _addNode(node);
        }
        finally
        {
            if (lockTaken)
                _locker.Exit(false);
        }
    }

    private void _addNode(TimeNode node)
    {
        var t = node.Expire;
        var ct = _time;

        if ((t | TimeNearMask) == (ct | TimeNearMask))
        {
            _near[t & TimeNearMask].AddNode(node);
        } else
        {
            var mask = (uint)(TimeNear << TimeLevelShift);
            var i = 0;
            for (; i < 3; i++)
            {
                if ((t | (mask - 1)) == (ct | (mask - 1)))
                {
                   break; 
                }

                mask <<= TimeLevelShift;
            }
            _t[i][(t>>(TimeNearShift+i*TimeLevelShift))&TimeLevelMask].AddNode(node);
        }
    }

    private void _moveList(int level, int bucket)
    {
        var current = _t[level][bucket].Clear();

        while (current is not null)
        {
            var node = current;
            current = current.Next;
            node.Next = null;
            _addNode(node);
        }
    }

    private void _shift()
    {
        _time += 1;
        var ct = _time;

        if (ct == 0)
        {
            _moveList(3, 0);
        }
        else
        {
            var mask = TimeNear;
            var t = ct >> TimeNearShift;
            var i = 0;
            while ((ct & (mask - 1)) == 0)
            {
                var idx = t & TimeLevelMask;
                if (idx != 0)
                {
                    _moveList(i, (int)idx);
                    break;
                }

                mask <<= TimeLevelShift;
                t >>= TimeLevelShift;
                i += 1;
            }
        }
    }

    private void _execute(ref bool lockTaken)
    {
        var idx = _time & TimeNearMask;
        while (_near[idx].Head.Next is not null)
        {
            var current = _near[idx].Clear();
            if (lockTaken) _locker.Exit(false);
            
            while (current is not null)
            {
                OnTick?.Invoke(this, new TimeWheelEventArgs(current.Data));
                current = current.Next;
            }

            lockTaken = false;
            _locker.Enter(ref lockTaken);
        }
    }
    
    private void _tick()
    {
        var lockTaken = false;
        try
        {
            _locker.Enter(ref lockTaken);
            _shift();
            _execute(ref lockTaken);
        }
        finally
        {
          if (lockTaken)
              _locker.Exit(false);
        }
    }
}