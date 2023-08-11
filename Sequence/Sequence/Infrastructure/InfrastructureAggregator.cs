using Sequence.Infrastructure.Config;
using Sequence.Infrastructure.Data;
using static Sequence.Infrastructure.Config.DatabaseSettings;

namespace Sequence.Infrastructure;

public class InfrastructureAggregator
{
    private readonly ILogger<InfrastructureAggregator> _logger;
    private readonly IConfiguration _configuration;

    public IncrementIdService? IncrementIdService { get; } 
    
    public InfrastructureAggregator(ILogger<InfrastructureAggregator> logger,IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;


        if (_configuration.GetSection(Database).Get<DatabaseSettings>() is {} databaseSettings)
        {
            var databaseService = new DatabaseService(databaseSettings);
            IncrementIdService = new IncrementIdService(databaseService);
        }

    }
}