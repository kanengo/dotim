using Grpc.Core;
using Sequence.Infrastructure;
using Sequence.Infrastructure.Data;

namespace Sequence.Services;

public class SequencerService: Sequencer.SequencerBase
{
    private readonly InfrastructureAggregator _infrastructureAggregator;
    
    public SequencerService(InfrastructureAggregator infrastructureAggregator)
    {
        _infrastructureAggregator = infrastructureAggregator;
    }
    
    public override async Task<GetBizIncrementIdReply> GetBizIncrementId(GetBizIncrementIdRequest request, ServerCallContext context)
    {
        var maxId = await _infrastructureAggregator.IncrementIdService.GetIncrementId(request.BizId);

        return await Task.FromResult(new GetBizIncrementIdReply
        {
            MaxId = maxId
        });
    }
}