using System;

namespace Bewit.Core
{
    internal static class BewitOptionsExtensions
    {
        internal static void Validate(this BewitOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Secret))
            {
                throw new ArgumentException(
                    "Value cannot be null or whitespace.", nameof(options.Secret));
            }
        }
    }
}
