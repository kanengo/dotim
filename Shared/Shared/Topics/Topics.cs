namespace Shared.Topics;

public abstract record CloudEventTopics
{
    public abstract record Linker
    {
        public const string LinkStateChange = "{0}/link/state";
    }
}