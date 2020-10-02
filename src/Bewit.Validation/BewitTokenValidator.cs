using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Core;
using Bewit.Validation.Exceptions;
using Newtonsoft.Json;

namespace Bewit.Validation
{
    public class BewitTokenValidator<T>: IBewitTokenValidator<T>
    {
        private readonly ICryptographyService _cryptographyService;
        private readonly IVariablesProvider _variablesProvider;

        public BewitTokenValidator(
            ICryptographyService cryptographyService,
            IVariablesProvider variablesProvider)
        {
            if (cryptographyService == null)
            {
                throw new ArgumentNullException(nameof(cryptographyService));
            }

            if (variablesProvider == null)
            {
                throw new ArgumentNullException(nameof(variablesProvider));
            }

            _cryptographyService = cryptographyService;
            _variablesProvider = variablesProvider;
        }

        public async Task<T> ValidateBewitTokenAsync(
            BewitToken<T> bewit,
            CancellationToken cancellationToken)
        {
            Bewit<T> bewitInternal = DeserializeBewitWithoutValidation(bewit);

            Bewit<T> validatedBewit =
                await ValidateBewitAsync(bewitInternal, cancellationToken);

            return validatedBewit.Payload;
        }

        private Bewit<T> DeserializeBewitWithoutValidation(BewitToken<T> bewit)
        {
            string base64Bewit = bewit.ToString();

            string serializedBewit =
                Encoding.UTF8.GetString(Convert.FromBase64String(base64Bewit));

            // Refactor: TypeNameHandling.All
            Bewit<T> bewitInternal =
                JsonConvert.DeserializeObject<Bewit<T>>(serializedBewit);
            return bewitInternal;
        }


        protected virtual Task<Bewit<T>> ValidateBewitAsync(
            Bewit<T> bewit,
            CancellationToken cancellationToken)
        {
            if (bewit.ExpirationDate < _variablesProvider.UtcNow)
            {
                throw new BewitExpiredException();
            }

            var hashToMatch = _cryptographyService.GetHash(
                bewit.Nonce,
                bewit.ExpirationDate,
                bewit.Payload);

            if (!string.Equals(hashToMatch, bewit.Hash,
                StringComparison.InvariantCulture))
            {
                throw new BewitInvalidException();
            }

            return Task.FromResult(bewit);
        }
    }
}
