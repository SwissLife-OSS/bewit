using System.Threading;
using System.Threading.Tasks;
using Bewit;
using Bewit.Generation;
using Microsoft.Extensions.DependencyInjection;

namespace Host.Types
{
    public class Mutation
    {
        private readonly IBewitTokenGenerator<FooPayload> _fooPayloadGenerator;
        private readonly IBewitTokenGenerator<BarPayload> _barPayloadGenerator;
        private readonly INonceRepository _barNonceRepository;

        public Mutation(
            IBewitTokenGenerator<FooPayload> fooPayloadGenerator,
            IBewitTokenGenerator<BarPayload> barPayloadGenerator,
            [FromKeyedServices("Host.Types.BarPayload")] INonceRepository barNonceRepository)
        {
            _fooPayloadGenerator = fooPayloadGenerator;
            _barPayloadGenerator = barPayloadGenerator;
            _barNonceRepository = barNonceRepository;
        }

        public async Task<string> InvalidateBewitTokens(
            string identifier,
            CancellationToken cancellationToken)
        {
            await _barNonceRepository.DeleteIdentifierAsync(identifier, cancellationToken);

            return identifier;
        }

        public async Task<string> CreateBewitToken(string value)
        {
            return (await _fooPayloadGenerator
                    .GenerateBewitTokenAsync(
                        new FooPayload { Value = value },
                        default))
                .ToString();
        }

        public async Task<string> CreateIdentifiableBewitToken(string identifier)
        {
            return (await _barPayloadGenerator
                    .GenerateBewitTokenAsync(
                        new BarPayload(),
                        new BewitTokenOptions { Identifier = identifier },
                        default))
                .ToString();
        }
    }
}
