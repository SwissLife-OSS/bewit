using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Bewit.Generation
{
    internal class BewitTokenGenerator<T>: IBewitTokenGenerator<T>
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
                ?? throw new ArgumentNullException(nameof(BewitPayloadContext.CreateCryptographyService));
            _variablesProvider = context.CreateVariablesProvider?.Invoke()
                ?? throw new ArgumentNullException(nameof(BewitPayloadContext.CreateVariablesProvider));
            _repository = context.CreateRepository?.Invoke()
                ?? throw new ArgumentNullException(nameof(BewitPayloadContext.CreateRepository));
        }

        public async Task<BewitToken<T>> GenerateBewitTokenAsync(
            T payload,
            CancellationToken cancellationToken)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            Bewit<T> bewit = CreateBewit(payload);
            await _repository.InsertOneAsync(bewit.Token, cancellationToken);

            // Refactor: TypeNameHandling.All
            var serializedBewit = JsonConvert.SerializeObject(bewit);
            var base64Bewit = Convert.ToBase64String(Encoding.UTF8.GetBytes(serializedBewit));

            return new BewitToken<T>(base64Bewit);
        }

        public Task<BewitToken<T>> GenerateIdentifiableBewitTokenAsync(
            T payload,
            string identifier,
            CancellationToken cancellationToken)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            // TODO use an identifiable token when create the bewit
            Bewit<T> bewit = CreateBewit(payload);

            return default;
        }

        private Bewit<T> CreateBewit(T payload)
        {
            var nextToken = _variablesProvider.NextToken.ToString("D", CultureInfo.InvariantCulture);
            DateTime expirationDate = _variablesProvider.UtcNow.AddTicks(_tokenDuration.Ticks);

            var hash = _cryptographyService.GetHash(nextToken, expirationDate, payload);
            var token = Token.Create(nextToken, expirationDate);
            return new Bewit<T>(token, payload, hash);
        }
    }
}
