using System;
using Newtonsoft.Json;

namespace Bewit.Core
{
    public class Token
    {
        public Token()
        {
        }

        [JsonConstructor]
        public Token(string nonce, DateTime expirationDate)
        {
            if (string.IsNullOrWhiteSpace(nonce))
            {
                throw new ArgumentException(
                    "Value cannot be null or whitespace.", nameof(nonce));
            }

            if (expirationDate == default)
            {
                throw new ArgumentException(
                    "Value cannot be default value for type Datetime", 
                    nameof(expirationDate));
            }

            Nonce = nonce;
            ExpirationDate = expirationDate;
        }

        public string Nonce { get; private set; }

        public DateTime ExpirationDate { get; private set; }
    }
}
