using System;

namespace Bewit.Core
{
    public class VariablesProvider : IVariablesProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public Guid NextToken => Guid.NewGuid();
    }
}
