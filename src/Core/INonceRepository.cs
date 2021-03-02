using System.Threading;
using System.Threading.Tasks;

namespace Bewit.Core
{
    public interface INonceRepository
    {
        ValueTask InsertOneAsync(Token token, CancellationToken cancellationToken);

        ValueTask<Token> TakeOneAsync(string token, CancellationToken cancellationToken);
    }
}
