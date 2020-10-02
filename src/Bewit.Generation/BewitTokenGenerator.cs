using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Core;
using Newtonsoft.Json;

namespace Bewit.Generation
{
    public class BewitTokenGenerator<T>: IBewitTokenGenerator<T>
    {
        private readonly TimeSpan _tokenDuration = TimeSpan.FromMinutes(1);
        private readonly ICryptographyService _cryptographyService;
        private readonly IVariablesProvider _variablesProvider;
        
        public BewitTokenGenerator(
            TimeSpan tokenDuration, 
            ICryptographyService cryptographyService, 
            IVariablesProvider variablesProvider)
        {
            if (tokenDuration != default)
            {
                _tokenDuration = tokenDuration;
            }

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

        public async Task<BewitToken<T>> GenerateBewitTokenAsync(T payload,
            CancellationToken cancellationToken)
        {
            Bewit<T> bewit =
                await GenerateBewitAsync(payload, cancellationToken);

            // Refactor: TypeNameHandling.All
            string serializedBewit = JsonConvert.SerializeObject(bewit);

            string base64Bewit = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(serializedBewit)
            );

            return new BewitToken<T>(base64Bewit);
        }

        protected virtual Task<Bewit<T>> GenerateBewitAsync(
            T payload,
            CancellationToken cancellationToken)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            string token =
                _variablesProvider.NextToken.ToString("D",
                    CultureInfo.InvariantCulture);
            DateTime expirationDate =
                _variablesProvider.UtcNow.AddTicks(_tokenDuration.Ticks);

            return
                Task.FromResult(new Bewit<T>(
                    token,
                    expirationDate,
                    payload,
                    _cryptographyService.GetHash(token, expirationDate, payload)
                ));
        }
    }
}
