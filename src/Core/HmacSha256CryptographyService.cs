using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Bewit
{
    public class HmacSha256CryptographyService: ICryptographyService
    {
        private readonly string _secret;

        public HmacSha256CryptographyService(BewitOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.Secret))
            {
                throw new ArgumentNullException(nameof(options.Secret));
            }

            _secret = options.Secret;
        }

        public string GetHash<T>(string token, DateTime expirationDate, T payload)
        {
            HMACSHA256 sha256 = null;
            try
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(_secret);
                sha256 = new HMACSHA256(keyBytes);

                var toHash = new Dictionary<string, object>
                {
                    {nameof(token), token},
                    {nameof(expirationDate), expirationDate},
                    {nameof(payload), payload}
                };

                string hashable = JsonConvert.SerializeObject(toHash);

                byte[] hash =
                    sha256.ComputeHash(Encoding.UTF8.GetBytes(hashable));

                return Convert.ToBase64String(hash);
            }
            finally
            {
                sha256?.Dispose();
            }
        }
    }
}
