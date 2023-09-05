using Dapr.AppCallback.Autogen.Grpc.v1;
using Google.Protobuf.Collections;
using Grpc.Core;
using Shared.Topics;
using static Shared.Topics.CloudEventType;

namespace Locator.Services;

public class DaprAppCallbackAlphaService : AppCallbackAlpha.AppCallbackAlphaBase
{
    private readonly ILogger<DaprAppCallbackAlphaService> _logger;

    private readonly IConfiguration _configuration;
    
    
    public DaprAppCallbackAlphaService(ILogger<DaprAppCallbackAlphaService> logger,IConfiguration configuration)
    {
        _logger = logger;

        _configuration = configuration;
    }


    public override Task<TopicEventBulkResponse> OnBulkTopicEventAlpha1(TopicEventBulkRequest request, ServerCallContext context)
    {
        var response = new TopicEventBulkResponse();
        
        _logger.LogDebug("OnBulkTopicEventAlpha1 Topic:{} Type:{}", request.Topic, request.Type);
        
        foreach (var t in request.Entries)
        {
            
            response.Statuses.Add(new TopicEventBulkResponseEntry
            {
                Status = TopicEventResponse.Types.TopicEventResponseStatus.Success,
                EntryId = t.EntryId
            });
        }
   
        return Task.FromResult(response);
    }
}