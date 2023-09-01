namespace Shared.Topics;

public abstract record CloudEventType
{
    public abstract record Linker
    {
        public const string LinkStateConnect = "state.connect";
    
        public const string LinkStateHeartbeat = "state.heartbeat";
    
        public const string LinkStateDisconnect = "state.disconnect";
    }
    
}