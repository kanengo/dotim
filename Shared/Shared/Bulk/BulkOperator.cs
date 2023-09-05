using System.Threading.Channels;


namespace Shared.Bulk;

public class BulkOperator<TValue>
{
    private readonly Channel<TValue> _channel;

    private readonly List<TValue> _queue;

    private readonly int _size;

    public event EventHandler<BulkEventArgs>? BulkHandler;
    
    public class BulkEventArgs : EventArgs
    {
        public TValue[] Data { get; }

        public BulkEventArgs(TValue[] data)
        {
            Data = data;
        }
    }

    public async Task WriteAsync(TValue value)
    {
        await _channel.Writer.WriteAsync(value);
    }

    public BulkOperator(int bulkSize, TimeSpan maxOperateDuration, CancellationToken cancellationToken = default)
    {
        _size = bulkSize;
        _queue = new List<TValue>(_size);
        _channel = Channel.CreateBounded<TValue>(new BoundedChannelOptions(bulkSize)
        {
            SingleReader = true
        });
        Task.Run(async () =>
        {
            
            while (true)
            {   
                if (cancellationToken.IsCancellationRequested)
                    break;
                
                var cSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var cToken = cSource.Token;
                
                var channelTask = _tryGetChannelAsync(cToken);
                var delayTask = Task.Delay(maxOperateDuration, cToken);
                
                await Task.WhenAny(channelTask, delayTask);
                cSource.Cancel();
                
                _flushMessage();

            }
        }, cancellationToken);
    }

    private async Task _tryGetChannelAsync ( CancellationToken cancellationToken = default)
    {   
         await foreach (var value in _channel.Reader.ReadAllAsync(cancellationToken))
         {
             _queue.Add(value);
             if (_queue.Count != _size) continue;
             break;
         }
    }

    private void _flushMessage()
    {
        if (_queue.Count == 0) 
            return;
        
        var data = new TValue[_queue.Count];
  
      
        _queue.CopyTo(data, 0);
        _queue.Clear();
        
        BulkHandler?.Invoke(this,new BulkEventArgs(data));
    }
}