using System;
using MongoDB.Driver.Core.Configuration;

namespace Bewit.Storage.MongoDB
{
    internal static class BewitMongoOptionsExtensions
    {
        internal static void Validate(this BewitMongoOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                throw new ArgumentException(
                    "Value cannot be null or whitespace.",
                    nameof(ConnectionString));
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
