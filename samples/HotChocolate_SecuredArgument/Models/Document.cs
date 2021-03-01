namespace Host.Models
{
    public class Document
    {
        public Document(string name, string contentType)
        {
            Name = name;
            ContentType = contentType;
        }

        public string Name { get; }
        public string ContentType { get; }
    }
}
