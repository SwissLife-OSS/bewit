using System.Threading;
using System.Threading.Tasks;

namespace Bewit
{
    public interface INonceRepository
    {
        ValueTask InsertOneAsync(Token token, CancellationToken cancellationToken);

        ValueTask<Token> TakeOneAsync(string token, CancellationToken cancellationToken);
    }
}
