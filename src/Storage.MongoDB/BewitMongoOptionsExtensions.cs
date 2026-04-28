using System;

namespace Bewit.Storage.MongoDB
{
    internal static class BewitMongoOptionsExtensions
    {
        internal static void Validate(
            this MongoNonceOptions options,
            bool requireConnectionString = true)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (requireConnectionString && string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                throw new ArgumentException(
                    "Value cannot be null or whitespace.",
                    nameof(options.ConnectionString));
            }

            if (string.IsNullOrWhiteSpace(options.DatabaseName))
            {
                throw new ArgumentException(
                    "Value cannot be null or whitespace.",
                    nameof(options.DatabaseName));
            }
        }
    }
}
