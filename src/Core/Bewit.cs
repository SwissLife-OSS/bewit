using System;
using Newtonsoft.Json;

namespace Bewit
{
    public class Bewit<T>
        where T: notnull
    {
        [JsonConstructor]
        public Bewit(Token token, T payload, string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
            {
                throw new ArgumentException(nameof(hash));
            }

            Token = token ?? throw new ArgumentNullException(nameof(token));
            Payload = payload ?? throw new ArgumentNullException(nameof(payload));
            Hash = hash;
        }

        public Token Token { get; }

        public T Payload { get; }

        public string Hash { get; }
    }
}
