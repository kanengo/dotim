using Auth.Infrastructure.Config;
using Auth.Infrastructure.Data;
using Shared.Models;

namespace Auth.Infrastructure;

public class InfrastructureAggregator
{
    private readonly ILogger<InfrastructureAggregator> _logger;
    
    public AccountService? AccountService { get; }
    
    public JwtHmacSha256 Jwt { get; }
    
    public InfrastructureAggregator(ILogger<InfrastructureAggregator> logger,IConfiguration configuration)
    {
        _logger = logger;

        var databaseSettings = configuration.GetSection(DatabaseSettings.Database).Get<DatabaseSettings>();
        if (databaseSettings is not null)
        {
            var databaseService = new DatabaseService(databaseSettings);
            AccountService = new AccountService(databaseService);
        }
        
        //init jwt
        var securitySettings = configuration.GetSection(SecuritySettings.Security).Get<SecuritySettings>();
        var key = "";
        var expiration = 0;
        if (securitySettings is not null)
        {
            key = securitySettings.Key;
            expiration = securitySettings.Expiration;
        }
        key ??= "xVyWSWXYFyU23GeSWPVwv8Su1#YIdINV13#z#e&$Y%Gpi4s81NZlZDdZL5SfOrq!" +
                "Q1bLm7g0aC7FD2UwPbP$ek2D8P#8yMnqjJxp3^0cs0JYbOP0^#vn4K2KtnTgKaQb";
        Jwt = new JwtHmacSha256(key, expiration);

    }
}