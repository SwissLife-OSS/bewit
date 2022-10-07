using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bewit
{
    internal class DefaultNonceRepository : INonceRepository
    {
        private static readonly ValueTask EmptyTask = new ValueTask();
        private static readonly ValueTask<Token> EmptyToken = new ValueTask<Token>(Token.Empty);

        public ValueTask InsertOneAsync(Token token, CancellationToken cancellationToken)
        {
            return EmptyTask;
        }

        public ValueTask<Token> TakeOneAsync(string token, CancellationToken cancellationToken)
        {
            return EmptyToken;
        }

        public ValueTask DeleteIdentifier(string identifier, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Only stateful bewit support invalidation");
        }
    }
}
