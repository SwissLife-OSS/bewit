using System;

namespace Bewit.Core
{
    internal class VariablesProvider : IVariablesProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public Guid NextToken => Guid.NewGuid();
    }
}
