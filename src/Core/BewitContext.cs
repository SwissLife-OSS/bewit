using System;
using Newtonsoft.Json.Linq;

namespace Bewit
{
    public class BewitContext
    {
        public BewitContext(object value)
        {
            Value = value;
        }

        public object Value { get; }

        public T Get<T>() where T : class
        {
            if (Value is T value)
            {
                return value;
            }

            // Refactor: Check direct T when TypeNameHandling.All is done
            if (Value is JObject jObject)
            {
                return jObject.ToObject<T>()!;
            }

            throw new InvalidOperationException(
                $"Context type {Value.GetType().Name} not convertible to {typeof(T).Name}");
        }
    }
}
