using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Generation;

namespace Host.Types
{
    public class Mutation
    {
        private readonly IBewitTokenGenerator<FooPayload> _fooPayloadGenerator;
        private readonly IIdentifiableBewitTokenGenerator<BarPayload> _barPayloadGenerator;

        public Mutation(
            IBewitTokenGenerator<FooPayload> fooPayloadGenerator,
            IIdentifiableBewitTokenGenerator<BarPayload> barPayloadGenerator)
        {
            _fooPayloadGenerator = fooPayloadGenerator;
            _barPayloadGenerator = barPayloadGenerator;
        }

        public async Task<string> InvalidateBewitTokens(
            string identifier,
            CancellationToken cancellationToken)
        {
            await _barPayloadGenerator.InvalidateIdentifier(identifier, cancellationToken);

            return identifier;
        }

        public async Task<string> CreateBewitToken(string value)
        {
            return (await _fooPayloadGenerator
                    .GenerateBewitTokenAsync(
                        new FooPayload {Value = value},
                        new Dictionary<string, object>(),
                        default))
                .ToString();
        }

        public async Task<string> CreateIdentifiableBewitToken(string identifier)
        {
            return (await _barPayloadGenerator
                    .GenerateIdentifiableBewitTokenAsync(
                        new BarPayload(), identifier, new Dictionary<string, object>(), default))
                .ToString();
        }
    }
}
