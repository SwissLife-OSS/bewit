using System.Threading.Tasks;

namespace Bewit.Extensions.HotChocolate.Validation
{
    public interface IBewitContext
    {
        Task SetAsync(object value);

        Task<object> GetAsync();

        Task<T> GetAsync<T>()
            where T : class;
    }
}
