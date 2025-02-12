using Bewit.Exceptions;

namespace Bewit
{
    internal static class BewitOptionsExtensions
    {
        internal static BewitConfiguration Validate(this BewitOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Secret))
            {
                throw new InvalidSecretException();
            }

            return new BewitConfiguration(options.Secret, options.TokenDuration);
        }
    }
}
