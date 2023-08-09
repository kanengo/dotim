using Grpc.Core;
using Sequence.Infrastructure;

namespace Sequence.Services;

public class SequencerService: Sequencer.SequencerBase
{
    private readonly IncrementIdService _incrementIdService;
    
    public SequencerService(IncrementIdService incrementIdService)
    {
        _incrementIdService = incrementIdService;
    }
    
    public override async Task<GetBizIncrementIdReply> GetBizIncrementId(GetBizIncrementIdRequest request, ServerCallContext context)
    {
        var maxId = await _incrementIdService.GetIncrementId(request.BizId);

        return await Task.FromResult(new GetBizIncrementIdReply
        {
            MaxId = maxId
        });
    }
}