using System.Threading;
using System.Threading.Tasks;
using Bewit;
using Bewit.Generation;

namespace Host.Types
{
    public class Mutation
    {
        private readonly IBewitTokenGenerator<FooPayload> _fooPayloadGenerator;
        private readonly IBewitTokenGenerator<BarPayload> _barPayloadGenerator;
        private readonly IBewitTokenRevoker<BarPayload> _barRevoker;

        public Mutation(
            IBewitTokenGenerator<FooPayload> fooPayloadGenerator,
            IBewitTokenGenerator<BarPayload> barPayloadGenerator,
            IBewitTokenRevoker<BarPayload> barRevoker)
        {
            _fooPayloadGenerator = fooPayloadGenerator;
            _barPayloadGenerator = barPayloadGenerator;
            _barRevoker = barRevoker;
        }

        public async Task<string> InvalidateBewitTokens(
            string identifier,
            CancellationToken cancellationToken)
        {
            await _barRevoker.RevokeByIdentifierAsync(identifier, cancellationToken);

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
