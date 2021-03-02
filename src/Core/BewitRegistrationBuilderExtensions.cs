namespace Bewit.Core
{
    public static class BewitRegistrationBuilderExtensions
    {
        public static BewitRegistrationBuilder UseHmacSha256Encryption(
            this BewitRegistrationBuilder builder)
        {
            builder.GetCryptographyService =
                opt => new HmacSha256CryptographyService(opt.Secret);

            return builder;
        }
    }
}
