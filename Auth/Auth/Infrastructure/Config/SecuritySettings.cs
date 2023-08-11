namespace Auth.Infrastructure.Config;

public class SecuritySettings
{
    public const string Security = "Security";
    
    public string? Key { get; set; }
    
    public int Expiration { get; set; }
}