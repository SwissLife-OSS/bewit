using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Bewit.Core
{
    internal class MemoryNonceRepository : INonceRepository
    {
        private static readonly ValueTask EmptyTask = new ValueTask();
        private readonly ConcurrentDictionary<string, Token> _tokens =
            new ConcurrentDictionary<string, Token>();

        public ValueTask InsertOneAsync(Token token, CancellationToken cancellationToken)
        {
            _tokens.TryAdd(token.Nonce, token);
            return EmptyTask;
        }

        public ValueTask<Token> TakeOneAsync(string token, CancellationToken cancellationToken)
        {
            _tokens.TryRemove(token, out Token value);
            return new ValueTask<Token>(value);
        }
    }
}
