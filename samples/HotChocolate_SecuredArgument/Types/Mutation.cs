using System;
using System.Threading.Tasks;
using Bewit.Generation;

namespace Host.Types
{
    public class Mutation
    {
        private readonly IBewitTokenGenerator<FooPayload> _fooPayloadGenerator;
        private readonly IBewitTokenGenerator<BarPayload> _barPayloadGenerator;

        public Mutation(
            IBewitTokenGenerator<FooPayload> fooPayloadGenerator,
            IBewitTokenGenerator<BarPayload> barPayloadGenerator)
        {
            _fooPayloadGenerator = fooPayloadGenerator;
            _barPayloadGenerator = barPayloadGenerator;
        }

        public async Task<string> CreateBewitToken(CreateBewitTokenInput input)
        {
            switch (input.TokenType)
            {
                case TokenType.FooPayload:
                    return (await _fooPayloadGenerator
                            .GenerateBewitTokenAsync(new FooPayload(), default))
                        .ToString();
                case TokenType.BarPayload:
                    return (await _barPayloadGenerator
                            .GenerateIdentifiableBewitTokenAsync(new BarPayload(), "abc", default))
                        .ToString();
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
