using System.Threading;
using System.Threading.Tasks;
using Bewit;

namespace Host.Types
{
    public class Mutation
    {
        private readonly IBewitTokenGenerator<FooPayload> _fooPayloadGenerator;
        private readonly IBewitTokenGenerator<BarPayload> _barPayloadGenerator;
        private readonly IBewitTokenRepository<BarPayload> _barRepository;

        public Mutation(
            IBewitTokenGenerator<FooPayload> fooPayloadGenerator,
            IBewitTokenGenerator<BarPayload> barPayloadGenerator,
            IBewitTokenRepository<BarPayload> barRepository)
        {
            _fooPayloadGenerator = fooPayloadGenerator;
            _barPayloadGenerator = barPayloadGenerator;
            _barRepository = barRepository;
        }

        public async Task<string> InvalidateBewitTokens(
            string identifier,
            CancellationToken cancellationToken)
        {
            await _barRepository.RevokeByIdentifierAsync(identifier, cancellationToken);

            return identifier;
        }

        public async Task<string> CreateBewitToken(string value)
        {
            return (await _fooPayloadGenerator
                    .GenerateAsync(
                        new FooPayload { Value = value },
                        default))
                .ToString();
        }

        public async Task<string> CreateIdentifiableBewitToken(string identifier)
        {
            return (await _barPayloadGenerator
                    .GenerateAsync(
                        new BarPayload(),
                        new BewitTokenOptions { Identifier = identifier },
                        default))
                .ToString();
        }
    }
}
