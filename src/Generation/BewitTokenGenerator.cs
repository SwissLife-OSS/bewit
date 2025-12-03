using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Validation.Generation.Exceptions;
using Newtonsoft.Json;

namespace Bewit.Generation
{
    internal class BewitTokenGenerator<T>:
        IBewitTokenGenerator<T>,
        IIdentifiableBewitTokenGenerator<T>
            where T : notnull
    {
        private readonly TimeSpan _tokenDuration;
        private readonly ICryptographyService _cryptographyService;
        private readonly IVariablesProvider _variablesProvider;
        private readonly INonceRepository _repository;

        public BewitTokenGenerator(
            BewitOptions options,
            BewitPayloadContext context)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _tokenDuration = options.TokenDuration;
            _cryptographyService = context.CreateCryptographyService?.Invoke()
                ?? throw new BewitMissingConfigurationException(nameof(BewitPayloadContext.CreateCryptographyService));
            _variablesProvider = context.CreateVariablesProvider?.Invoke()
                ?? throw new BewitMissingConfigurationException(nameof(BewitPayloadContext.CreateVariablesProvider));
            _repository = context.CreateRepository?.Invoke()
                ?? throw new BewitMissingConfigurationException(nameof(BewitPayloadContext.CreateRepository));
        }

        public Task<BewitToken<T>> GenerateBewitTokenAsync(
            T payload,
            Dictionary<string, object> extraProperties,
            CancellationToken cancellationToken
            )
        {
            var token = Token.Create(CreateNextToken(), CreateExpirationDate());
            token.ExtraProperties = extraProperties;
            return GenerateBewitTokenImplAsync(payload, token, cancellationToken);

        }

        public Task<BewitToken<T>> GenerateIdentifiableBewitTokenAsync(
            T payload,
            string identifier,
            Dictionary<string, object> extraProperties,
            CancellationToken cancellationToken)
        {
            var token = new IdentifiableToken(identifier, CreateNextToken(), CreateExpirationDate());
            token.ExtraProperties = extraProperties;
            return GenerateBewitTokenImplAsync(payload, token, cancellationToken);
        }

        public async Task InvalidateIdentifier(
            string identifier,
            CancellationToken cancellationToken)
        {
            await _repository.DeleteIdentifier(identifier, cancellationToken);
        }

        private async Task<BewitToken<T>> GenerateBewitTokenImplAsync(
            T payload,
            Token token,
            CancellationToken cancellationToken)
        {
            Bewit<T> bewit = CreateBewit(token, payload);
            await _repository.InsertOneAsync(bewit.Token, cancellationToken);

            // Refactor: TypeNameHandling.All
            var serializedBewit = JsonConvert.SerializeObject(bewit);
            var base64Bewit = Convert.ToBase64String(Encoding.UTF8.GetBytes(serializedBewit));

            return new BewitToken<T>(base64Bewit);
        }

        private string CreateNextToken() =>
            _variablesProvider.NextToken.ToString("D", CultureInfo.InvariantCulture);

        private DateTime CreateExpirationDate() =>
            _variablesProvider.UtcNow.AddTicks(_tokenDuration.Ticks);

        private Bewit<T> CreateBewit(Token token, T payload)
        {
            var hash = _cryptographyService.GetHash(token.Nonce, token.ExpirationDate, payload);
            return new Bewit<T>(token, payload, hash);
        }
    }
}
