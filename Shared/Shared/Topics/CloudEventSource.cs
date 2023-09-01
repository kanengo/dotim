namespace Shared.Topics;

public abstract record CloudEventSource
{
    public abstract record Linker
    {
        public const string LinkConnectSate = "{0}.{1}.connect";
    }
}
