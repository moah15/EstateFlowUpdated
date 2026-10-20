using Microsoft.AspNetCore.Http;

namespace EstateFlow.Api.Models;

public class UploadDocumentRequest
{
    public IFormFile File { get; set; } = null!;

    public string DocumentType { get; set; } = string.Empty;
}