using System.Threading;
using System.Threading.Tasks;
using Bewit.Core;

namespace Bewit.Validation.Tests
{
    internal class BewitTokenValidatorAccessor<T>: BewitTokenValidator<T>
    {
        internal BewitTokenValidatorAccessor(
            ICryptographyService cryptographyService, 
            IVariablesProvider variablesProvider,
            INonceRepository nonceRepository) : 
            base(cryptographyService, variablesProvider, nonceRepository)
        {
        }

        internal async ValueTask<Bewit<T>> InvokeValidateBewitAsync(
            Bewit<T> bewit,
            CancellationToken cancellationToken)
        {
            return await ValidateBewitAsync(bewit, cancellationToken);
        }
    }
}
