using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Bewit.Extensions.HotChocolate.Validation
{
    public class BewitContext : IBewitContext
    {
        private readonly SemaphoreSlim _sync = new SemaphoreSlim(1, 1);
        private object _value;

        public async Task SetAsync(object value)
        {
            await _sync.WaitAsync();

            try
            {
                _value = value;
            }
            finally
            {
                _sync.Release();
            }
        }

        public async Task<object> GetAsync()
        {
            await _sync.WaitAsync();

            try
            {
                return _value;
            }
            finally
            {
                _sync.Release();
            }
        }

        public async Task<T> GetAsync<T>()
            where T : class
        {
            await _sync.WaitAsync();

            try
            {
                if (_value is T value)
                {
                    return value;
                }

                // Refactor: Check direct T when TypeNameHandling.All is done
                if (_value is JObject jObject)
                {
                    return jObject.ToObject<T>();
                }

                throw new InvalidOperationException(
                    $"Context type {_value.GetType().Name} not convertible to {typeof(T).Name}");
            }
            finally
            {
                _sync.Release();
            }
        }
    }
}
