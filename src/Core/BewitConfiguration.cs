using System;

namespace Bewit;

public class BewitConfiguration
{
    public BewitConfiguration(string secret, TimeSpan tokenDuration)
    {
        Secret = secret;
        TokenDuration = tokenDuration;
    }

    /// <summary>
    /// Secret used for the hash generation.
    /// Mandatory.
    /// </summary>
    public string Secret { get; set; }

    /// <summary>
    /// Duration of the Token.
    /// Optional. Default is 60 seconds
    /// </summary>
    public TimeSpan TokenDuration { get; set; }
}
