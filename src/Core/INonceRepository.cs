using System.Threading;
using System.Threading.Tasks;

namespace Bewit.Core
{
    public interface INonceRepository
    {
        Task InsertOneAsync(
            Token token, CancellationToken cancellationToken);

        Task<Token> FindOneAndDeleteAsync(
            string token, CancellationToken cancellationToken);
    }
}
