using System;
using Newtonsoft.Json;

namespace Bewit
{
    public class Bewit<T> : Token
    {
        public Bewit()
        {
        }

        [JsonConstructor]
        public Bewit(string nonce, DateTime expirationDate, T payload, string hash)
            : base(nonce, expirationDate)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (string.IsNullOrWhiteSpace(hash))
            {
                throw new ArgumentException(nameof(hash));
            }

            Payload = payload;
            Hash = hash;
        }

        public T Payload { get; }

        public string Hash { get; }
    }
}
