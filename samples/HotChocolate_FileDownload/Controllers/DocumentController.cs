using System;
using Bewit;
using Bewit.Exceptions;
using Host.Data;
using Host.Models;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers
{
    [Route("api/document")]
    [Controller]
    public class DocumentController : ControllerBase
    {
        private readonly DocumentsRepository _documentRepository;
        private readonly IBewitTokenValidator<string> _validator;

        public DocumentController(
            DocumentsRepository documentRepository,
            IBewitTokenValidator<string> validator)
        {
            _documentRepository = documentRepository;
            _validator = validator;
        }

        [HttpGet("{documentName}")]
        public async Task<ActionResult> DownloadDocument(
            string documentName,
            CancellationToken cancellationToken)
        {
            string? token = Request.Query["bewit"];
            try
            {
                string protectedPath = await _validator.ValidateAsync(
                    new BewitToken<string>(token ?? string.Empty), cancellationToken);
                if (!string.Equals(
                        protectedPath,
                        Request.Path.Value,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }
            }
            catch (BewitException)
            {
                return Forbid();
            }

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
