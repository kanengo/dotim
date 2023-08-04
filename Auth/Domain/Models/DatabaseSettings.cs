namespace Auth.Domain.Models;

public class DatabaseSettings
{
    public string? Connection { get; set; }
    
    public string? Database { get; set; }
    
    public string? AccountCollection { get; set; }
}