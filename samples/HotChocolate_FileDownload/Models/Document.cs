namespace Host.Models
{
    public class Document
    {
        public Document(string name, byte[] content, string contentType)
        {
            Name = name;
            Content = content;
            ContentType = contentType;
        }

        public string Name { get; }
        public byte[] Content { get; }
        public string ContentType { get; }
    }
}
