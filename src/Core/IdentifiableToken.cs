using System;
using Newtonsoft.Json;

namespace Bewit;

public class IdentifiableToken : Token
{
    public IdentifiableToken()
    {
    }

    public IdentifiableToken(string identifier, string nonce, DateTime expirationDate)
        : base(nonce, expirationDate)
    {
        Identifier = identifier;
    }

    [JsonIgnore]
    public string Identifier { get; private set; }
}
