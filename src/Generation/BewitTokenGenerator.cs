using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Core;
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
            ICryptographyService cryptographyService,
            IVariablesProvider variablesProvider,
            INonceRepository repository)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _tokenDuration = options.TokenDuration;
            _cryptographyService = cryptographyService
                ?? throw new ArgumentNullException(nameof(cryptographyService));
            _variablesProvider = variablesProvider
                ?? throw new ArgumentNullException(nameof(variablesProvider));
            _repository = repository
                ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<BewitToken<T>> GenerateBewitTokenAsync(
            T payload,
            CancellationToken cancellationToken)
        {
            Bewit<T> bewit = await GenerateBewitAsync(payload, cancellationToken);

            // Refactor: TypeNameHandling.All
            var serializedBewit = JsonConvert.SerializeObject(bewit);
            var base64Bewit = Convert.ToBase64String(Encoding.UTF8.GetBytes(serializedBewit));

            return new BewitToken<T>(base64Bewit);
        }

        protected async ValueTask<Bewit<T>> GenerateBewitAsync(
            T payload,
            CancellationToken cancellationToken)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var token = _variablesProvider.NextToken.ToString("D", CultureInfo.InvariantCulture);
            DateTime expirationDate = _variablesProvider.UtcNow.AddTicks(_tokenDuration.Ticks);

            var hash = _cryptographyService.GetHash(token, expirationDate, payload);
            var bewit = new Bewit<T>(token, expirationDate, payload, hash);

            await _repository.InsertOneAsync(bewit, cancellationToken);

            return bewit;
        }
    }
}
