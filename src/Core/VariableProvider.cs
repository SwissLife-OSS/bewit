using System;

namespace Bewit
{
    internal class VariablesProvider : IVariablesProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public Guid NextToken => Guid.NewGuid();
    }
}
