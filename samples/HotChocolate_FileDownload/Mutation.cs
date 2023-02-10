using System;
using Host.Data;
using Host.Models;
using Microsoft.Extensions.Configuration;

namespace Host
{
    public class Mutation
    {
        private readonly DocumentsRepository _repository;
        private readonly string _downloadUrl;

        public Mutation(DocumentsRepository repository, IConfiguration configuration)
        {
            _repository = repository
                ?? throw new ArgumentNullException(nameof(repository));
            _downloadUrl = configuration["DownloadUrl"]!;
        }

        public string CreateDownloadLink(
            string documentName)
        {
            Document? document = _repository.GetDocument(documentName);

            if(document == null)
            {
                throw new Exception("Document could not be found.");
            }

            return string.Format(_downloadUrl, document.Name);
        }
    }
}
