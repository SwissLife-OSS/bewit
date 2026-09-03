namespace Bewit;

public interface IBewitTokenIdGenerator
{
    Guid CreateTokenId();
}

internal sealed class BewitTokenIdGenerator : IBewitTokenIdGenerator
{
    public Guid CreateTokenId() => Guid.NewGuid();
}
