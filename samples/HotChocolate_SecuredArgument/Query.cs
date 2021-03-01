using System;
using System.Collections.Generic;
using Host.Data;
using Host.Models;

namespace Host
{
    public class Query
    {
        private readonly DocumentsRepository _repository;

        public Query(DocumentsRepository repository)
        {
            _repository = repository
                ?? throw new ArgumentNullException(nameof(repository));
        }

        public IReadOnlyList<Document> GetDocuments()
        {
            return _repository.GetDocuments();
        }

        public Document GetSecretDocument(
            GetSecretDocumentInput input)
        {
            return _repository.GetDocument(input.Name)!;
        }
    }
}
