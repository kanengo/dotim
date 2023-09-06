using NanoidDotNet;

namespace Linker.Domains;

public record class ServiceIdentity
{
    public static readonly ServiceIdentity Instance = new(); 
    
    public string UniqueId { get; }

    public CancellationTokenSource CancellationTokenSource { get; set; } = new();
        
    private ServiceIdentity()
    {
        UniqueId = Nanoid.Generate();
    }
}