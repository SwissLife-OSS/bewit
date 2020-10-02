using System.Threading;
using System.Threading.Tasks;
using Bewit.Core;

namespace Bewit.Generation
{
    public interface IBewitTokenGenerator<T>
    {
        Task<BewitToken<T>> GenerateBewitTokenAsync(
            T payload,
            CancellationToken cancellationToken);
    }
}
