using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bewit
{
    public class Token
    {
        public static readonly Token Empty = new Token(string.Empty, DateTime.MinValue);

        public Token()
        {
        }

        [JsonConstructor]
        protected Token(string nonce, DateTime expirationDate)
        {
            Nonce = nonce;
            ExpirationDate = expirationDate;
        }

        public string Nonce { get; private set; }

        public DateTime ExpirationDate { get; private set; }

        [JsonIgnore]
        public bool? IsDeleted { get; set; } = false;

        [JsonIgnore]
        public Dictionary<string, object> ExtraProperties { get; set; }

        public static Token Create(string nonce, DateTime expirationDate)
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

            return new Token(nonce, expirationDate);
        }
    }
}
