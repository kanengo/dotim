using NanoidDotNet;

namespace Linker.Domains;

public record class ServiceIdentity
{
    public static ServiceIdentity Instance = new ServiceIdentity(); 
    
    public string UniqueId { get; }
        
    private ServiceIdentity()
    {
        UniqueId = Nanoid.Generate();
    }
}