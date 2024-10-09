using System;
using Bewit.Mvc.Filter;
using Host.Data;
using Host.Models;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers
{
    [Route("api/document")]
    [Controller]
    public class DocumentController: ControllerBase
    {
        private readonly DocumentsRepository _documentRepository;

        public DocumentController(DocumentsRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        [BewitUrlAuthorization]
        [HttpGet("{documentName}")]
        public ActionResult DownloadDocument(string documentName)
        {
            Document document = _documentRepository.GetDocument(documentName)!; //can not actually be null in this example because the document's existence was already checked when the bewit was generated

            System.Net.Mime.ContentDisposition cd =
                new System.Net.Mime.ContentDisposition
                {
                    FileName = Uri.EscapeDataString(document.Name),
                    Inline = false
                };
            Response.Headers["Content-Disposition"] = cd.ToString();

            return File(document.Content, document.ContentType);
        }
    }
}
