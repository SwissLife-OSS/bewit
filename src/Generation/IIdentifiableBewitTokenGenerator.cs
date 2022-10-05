using System.Threading;
using System.Threading.Tasks;

namespace Bewit.Generation;

public interface IIdentifiableBewitTokenGenerator<T>
{
    Task<BewitToken<T>> GenerateIdentifiableBewitTokenAsync(
        T payload,
        string identifier,
        CancellationToken cancellationToken);

    Task InvalidateIdentifier(
        string identifier,
        CancellationToken cancellationToken);
}
