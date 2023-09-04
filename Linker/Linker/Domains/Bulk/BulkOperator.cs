using System.Collections.Concurrent;

namespace Linker.Domains.Bulk;

public class BulkOperator<TValue>
{
    private BlockingCollection<TValue> _queue;
    
    
    
    public BulkOperator(int bulkSize, TimeSpan maxOperateDuration)
    {
        _queue = new BlockingCollection<TValue>(bulkSize);
        
    }
}