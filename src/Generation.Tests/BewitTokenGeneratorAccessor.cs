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
            : base(new BewitOptions {TokenDuration = tokenDuration}, cryptographyService, variablesProvider, new MemoryNonceRepository())
        {
        }

        internal async ValueTask<Bewit<T>> InvokeGenerateBewitAsync(
            T payload, CancellationToken cancellationToken)
        {
            return await GenerateBewitAsync(payload, cancellationToken);
        }
    }
}
