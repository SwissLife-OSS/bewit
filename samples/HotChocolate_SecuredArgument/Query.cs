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

        public Document GetSecretDocumentWithStatelessBewit(
            GetSecretDocumentInput input)
        {
            return _repository.GetDocument(input.Name)!;
        }

        public Document GetSecretDocumentWithStatefulBewit(
            GetSecretDocumentInput input)
        {
            return _repository.GetDocument(input.Name)!;
        }
    }
}
