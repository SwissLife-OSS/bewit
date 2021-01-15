using System.Collections.Generic;
using System.Linq;
using Host.Models;

namespace Host.Data
{
    public class DocumentsRepository
    {
        private readonly IReadOnlyList<Document> _documents;

        public DocumentsRepository(IReadOnlyList<Document> documents)
        {
            _documents = documents;
        }

        public Document? GetDocument(string name)
        {
            return _documents.SingleOrDefault(d => d.Name == name);
        }

        public IReadOnlyList<Document> GetDocuments()
        {
            return _documents;
        }
    }
}
