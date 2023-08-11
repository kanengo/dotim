namespace Sequence.Infrastructure.Config;

public class DatabaseSettings
{   
    public const string Database = "Database";
    public string? Connection { get; set; }
    
    public string? DatabaseName { get; set; }
    
    public string? IncrementIdCollection { get; set; }
}