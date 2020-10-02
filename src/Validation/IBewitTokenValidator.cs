using System.Threading;
using System.Threading.Tasks;
using Bewit.Core;

namespace Bewit.Validation
{
    public interface IBewitTokenValidator<T>
    {
        Task<T> ValidateBewitTokenAsync(
            BewitToken<T> bewit,
            CancellationToken cancellationToken);
    }
}
