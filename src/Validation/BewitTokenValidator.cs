using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Validation.Exceptions;
using Newtonsoft.Json;

namespace Bewit.Validation
{
    public class BewitTokenValidator<T> : IBewitTokenValidator<T>
    {
        private readonly ICryptographyService _cryptographyService;
        private readonly IVariablesProvider _variablesProvider;
        private readonly INonceRepository _repository;

        public BewitTokenValidator(BewitPayloadContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _cryptographyService = context.CreateCryptographyService?.Invoke()
                ?? throw new ArgumentNullException(nameof(BewitPayloadContext.CreateCryptographyService));
            _variablesProvider = context.CreateVariablesProvider?.Invoke()
                ?? throw new ArgumentNullException(nameof(BewitPayloadContext.CreateVariablesProvider));
            _repository = context.CreateRepository?.Invoke()
                ?? throw new ArgumentNullException(nameof(BewitPayloadContext.CreateRepository));
        }

        public async Task<T> ValidateBewitTokenAsync(
            BewitToken<T> bewit,
            CancellationToken cancellationToken)
        {
            Bewit<T> bewitInternal = DeserializeBewitWithoutValidation(bewit);
            Bewit<T> validatedBewit = await ValidateBewitAsync(bewitInternal, cancellationToken);

            return validatedBewit.Payload;
        }

        private Bewit<T> DeserializeBewitWithoutValidation(BewitToken<T> bewit)
        {
            var base64Bewit = bewit.ToString();
            var serializedBewit = Encoding.UTF8.GetString(Convert.FromBase64String(base64Bewit));

            // Refactor: TypeNameHandling.All
            return JsonConvert.DeserializeObject<Bewit<T>>(serializedBewit);
        }

        protected async ValueTask<Bewit<T>> ValidateBewitAsync(
            Bewit<T> bewit,
            CancellationToken cancellationToken)
        {
            if (bewit is null)
            {
                throw new BewitNotFoundException();
            }

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

            Token token = await _repository.TakeOneAsync(bewit.Nonce, cancellationToken);
            if (token != null)
            {
                return bewit;
            }

            throw new BewitNotFoundException();
        }
    }
}
