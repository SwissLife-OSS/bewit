namespace Host
{
    public class GetSecretDocumentInput
    {
        public string Name { get; set; }
        public string BewitToken { get; set; }
    }

    public class CreateBewitTokenInput
    {
        public TokenType TokenType { get; set; }
    }

    public enum TokenType
    {
        FooPayload,
        BarPayload
    }
}
