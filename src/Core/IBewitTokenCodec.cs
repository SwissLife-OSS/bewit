namespace Bewit;

public interface IBewitTokenCodec<TPayload> where TPayload : notnull
{
    BewitTokenFormat Format { get; }
    bool CanRead(string token);
    BewitDecodedToken<TPayload> DecodeAndVerify(string token);
}
