namespace Bewit;

/// <summary>
/// Default implementation of <see cref="IVariablesProvider"/> using system clock and random GUIDs.
/// </summary>
internal sealed class VariablesProvider : IVariablesProvider
{
    public DateTime UtcNow => DateTime.UtcNow;

    public Guid NextToken => Guid.NewGuid();
}
