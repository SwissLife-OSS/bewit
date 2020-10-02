using System;

namespace Bewit.Core
{
    public interface IVariablesProvider
    {
        DateTime UtcNow { get; }
        Guid NextToken { get; }
    }
}
