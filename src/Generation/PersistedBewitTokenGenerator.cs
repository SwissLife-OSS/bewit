using System;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Core;

namespace Bewit.Generation
{
    public class PersistedBewitTokenGenerator<T> : BewitTokenGenerator<T>
    {
        private readonly INonceRepository _repository;

        public PersistedBewitTokenGenerator(
            TimeSpan tokenDuration,
            ICryptographyService cryptographyService,
            IVariablesProvider variablesProvider,
            INonceRepository repository)
        : base(tokenDuration, cryptographyService, variablesProvider)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            _repository = repository;
        }

        protected override async Task<Bewit<T>> GenerateBewitAsync(
            T payload, CancellationToken cancellationToken)
        {
            Bewit<T> bewit =
                await base.GenerateBewitAsync(payload, cancellationToken);
            await _repository.InsertOneAsync(bewit, cancellationToken);
            return bewit;
        }
    }
}
