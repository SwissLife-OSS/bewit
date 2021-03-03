using System;

namespace Bewit
{
    public interface IVariablesProvider
    {
        DateTime UtcNow { get; }
        Guid NextToken { get; }
    }
}
