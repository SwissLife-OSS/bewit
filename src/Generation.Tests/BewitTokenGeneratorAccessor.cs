using System;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Core;

namespace Bewit.Generation.Tests
{
    internal class BewitTokenGeneratorAccessor<T>: BewitTokenGenerator<T>
    {
        public BewitTokenGeneratorAccessor(
            TimeSpan tokenDuration, 
            ICryptographyService cryptographyService, 
            IVariablesProvider variablesProvider) 
            : base(tokenDuration, cryptographyService, variablesProvider)
        {
        }

        internal async Task<Bewit<T>> InvokeGenerateBewitAsync(
            T payload, CancellationToken cancellationToken)
        {
            return await GenerateBewitAsync(payload, cancellationToken);
        }
    }
}
