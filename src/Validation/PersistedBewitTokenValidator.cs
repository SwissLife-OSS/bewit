using System;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Core;
using Bewit.Validation.Exceptions;

namespace Bewit.Validation
{
    public class PersistedBewitTokenValidator<T> : BewitTokenValidator<T>
    {
        private readonly INonceRepository _repository;

        public PersistedBewitTokenValidator(
            ICryptographyService cryptographyService, 
            IVariablesProvider variablesProvider,
            INonceRepository repository) 
            : base(cryptographyService, variablesProvider)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            _repository = repository;
        }

        protected override async Task<Bewit<T>> ValidateBewitAsync(
            Bewit<T> bewit, CancellationToken cancellationToken)
        {
            Bewit<T> hashValidatedBewit =
                await base.ValidateBewitAsync(bewit, cancellationToken);

            Token token = await _repository.FindOneAndDeleteAsync(
                hashValidatedBewit.Nonce, cancellationToken);

            if (token != null)
            {
                return hashValidatedBewit;
            }

            throw new BewitNotFoundException();
        }
    }
}
