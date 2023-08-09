using Sequence;

namespace Auth.Infrastructure.Rpc;

public class RpcClientAggregator
{
    public Sequencer.SequencerClient SequencerClient { get; }

    public RpcClientAggregator(Sequencer.SequencerClient sequencerClient)
    {
        SequencerClient = sequencerClient;
    }
}