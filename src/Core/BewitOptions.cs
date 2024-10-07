using System;

namespace Bewit
{
    public class BewitOptions
    {
        public BewitOptions(string secret)
        {
            Secret = secret;
            TokenDuration = TimeSpan.FromMinutes(1);
        }

        /// <summary>
        /// Secret used for the hash generation.
        /// Mandatory.
        /// </summary>
        public string Secret { get; }

        /// <summary>
        /// Duration of the Token.
        /// Optional. Default is 60 seconds
        /// </summary>
        public TimeSpan TokenDuration { get; set; }
    }
}
