namespace Host
{
    public record GetSecretDocumentInput(string Name, string BewitToken);

    public record CreateBewitTokenInput(TokenType TokenType);

    public enum TokenType
    {
        FooPayload,
        BarPayload
    }
}
