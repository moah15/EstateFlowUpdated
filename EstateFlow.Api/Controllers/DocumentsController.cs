using EstateFlow.Api.Models;
using EstateFlow.Api.Services.Storage;
using EstateFlow.Core.Models;
using EstateFlow.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstateFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/estates/{estateId:guid}/documents")]
public class DocumentsController : ControllerBase
{
    private static readonly string[] AllowedExtensions =
    {
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png",
        ".docx"
    };

    private const long MaxFileSize =
        20 * 1024 * 1024;

    private readonly EstateFlowDbContext _dbContext;
    private readonly IBlobStorageService _blobStorage;

    public DocumentsController(
        EstateFlowDbContext dbContext,
        IBlobStorageService blobStorage)
    {
        _dbContext = dbContext;
        _blobStorage = blobStorage;
    }

    // ======================================================
    // UPLOAD DOCUMENT
    // POST /api/estates/{estateId}/documents
    // ======================================================

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        Guid estateId,
        [FromForm] UploadDocumentRequest request)
    {
        if (request.File is null ||
            request.File.Length == 0)
        {
            return BadRequest(new
            {
                message = "Choose a document to upload."
            });
        }

        if (string.IsNullOrWhiteSpace(
                request.DocumentType))
        {
            return BadRequest(new
            {
                message = "Document type is required."
            });
        }

        if (request.File.Length > MaxFileSize)
        {
            return BadRequest(new
            {
                message =
                    "The document is too large. Maximum file size is 20 MB."
            });
        }

        var extension =
            Path.GetExtension(request.File.FileName)
                .ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            return BadRequest(new
            {
                message =
                    "Unsupported file type. Upload a PDF, JPG, JPEG, PNG or DOCX file."
            });
        }

        var estate =
            await _dbContext.Estates
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    e => e.EstateId == estateId);

        if (estate is null)
        {
            return NotFound(new
            {
                message =
                    $"Estate {estateId} was not found."
            });
        }

        var documentId =
            Guid.NewGuid();

        var safeDocumentType =
            MakeSafeFilePart(
                request.DocumentType);

        var safeEstateReference =
            MakeSafeFilePart(
                estate.FileNumber);

        var generatedFileName =
            $"{safeEstateReference}_" +
            $"{safeDocumentType}_" +
            $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_" +
            $"{documentId:N}{extension}";

        var blobReference =
            $"{estateId:N}/{generatedFileName}";

        var blobUploaded = false;

        try
        {
            await using var stream =
                request.File.OpenReadStream();

            await _blobStorage.UploadAsync(
                stream,
                blobReference,
                string.IsNullOrWhiteSpace(
                    request.File.ContentType)
                    ? "application/octet-stream"
                    : request.File.ContentType);

            blobUploaded = true;

            var document =
                new ScannedDocument
                {
                    DocumentId =
                        documentId,

                    EstateId =
                        estateId,

                    DocumentType =
                        request.DocumentType.Trim(),

                    FileName =
                        generatedFileName,

                    UploadDate =
                        DateTime.UtcNow,

                    BlobReference =
                        blobReference
                };

            _dbContext.ScannedDocuments.Add(
                document);

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                documentId =
                    document.DocumentId,

                fileName =
                    document.FileName,

                documentType =
                    document.DocumentType,

                uploadDate =
                    document.UploadDate
            });
        }
        catch (Exception ex)
        {
            if (blobUploaded)
            {
                try
                {
                    await _blobStorage.DeleteAsync(
                        blobReference);
                }
                catch
                {
                    // Keep the original failure.
                }
            }

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message =
                        "The document could not be uploaded to Azure Blob Storage. " +
                        "Check the AzureStorage connection settings on the hosted API. " +
                        $"Details: {ex.Message}"
                });
        }
    }

    // ======================================================
    // DOWNLOAD DOCUMENT
    // GET /api/estates/{estateId}/documents/{documentId}/download
    // ======================================================

    [HttpGet("{documentId:guid}/download")]
    public async Task<IActionResult> Download(
        Guid estateId,
        Guid documentId)
    {
        var document =
            await _dbContext.ScannedDocuments
                .FirstOrDefaultAsync(
                    d =>
                        d.DocumentId == documentId &&
                        d.EstateId == estateId);

        if (document is null)
        {
            return NotFound(new
            {
                message =
                    "The requested document was not found."
            });
        }

        Exception? lastStorageError = null;

        foreach (var blobReference in
                 GetPossibleBlobReferences(
                     estateId,
                     document))
        {
            try
            {
                var stream =
                    await _blobStorage.DownloadAsync(
                        blobReference);

                // Repair old SQL rows that have a blank or outdated
                // BlobReference once a matching blob is found.
                if (!string.Equals(
                        document.BlobReference,
                        blobReference,
                        StringComparison.Ordinal))
                {
                    document.BlobReference =
                        blobReference;

                    await _dbContext.SaveChangesAsync();
                }

                return File(
                    stream,
                    GetContentType(document.FileName),
                    document.FileName);
            }
            catch (FileNotFoundException ex)
            {
                lastStorageError = ex;
            }
            catch (Exception ex)
            {
                lastStorageError = ex;
                break;
            }
        }

        if (lastStorageError is FileNotFoundException)
        {
            return NotFound(new
            {
                message =
                    "The document record exists, but its file could not be found in Azure Blob Storage. " +
                    "This can happen with an older document uploaded before the current storage path was introduced. " +
                    "Upload the document again if the original file is available."
            });
        }

        return StatusCode(
            StatusCodes.Status503ServiceUnavailable,
            new
            {
                message =
                    "Azure Blob Storage could not be reached while opening this document. " +
                    "Check the AzureStorage settings on the hosted API. " +
                    $"Details: {lastStorageError?.Message ?? "Unknown storage error."}"
            });
    }

    // ======================================================
    // DELETE DOCUMENT
    // DELETE /api/estates/{estateId}/documents/{documentId}
    // ======================================================

    [HttpDelete("{documentId:guid}")]
    public async Task<IActionResult> Delete(
        Guid estateId,
        Guid documentId)
    {
        var document =
            await _dbContext.ScannedDocuments
                .FirstOrDefaultAsync(
                    d =>
                        d.DocumentId == documentId &&
                        d.EstateId == estateId);

        if (document is null)
        {
            return NotFound(new
            {
                message =
                    "The requested document was not found."
            });
        }

        try
        {
            foreach (var blobReference in
                     GetPossibleBlobReferences(
                         estateId,
                         document))
            {
                await _blobStorage.DeleteAsync(
                    blobReference);
            }

            _dbContext.ScannedDocuments.Remove(
                document);

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message =
                        "The document could not be deleted from Azure Blob Storage. " +
                        $"Details: {ex.Message}"
                });
        }
    }

    // ======================================================
    // HELPERS
    // ======================================================

    private static IEnumerable<string>
        GetPossibleBlobReferences(
            Guid estateId,
            ScannedDocument document)
    {
        var candidates =
            new List<string>();

        if (!string.IsNullOrWhiteSpace(
                document.BlobReference))
        {
            candidates.Add(
                document.BlobReference.Trim());
        }

        if (!string.IsNullOrWhiteSpace(
                document.FileName))
        {
            candidates.Add(
                $"{estateId:N}/{document.FileName}");

            // Older versions of EstateFlow may have stored
            // blobs without the estate folder prefix.
            candidates.Add(
                document.FileName);
        }

        return candidates
            .Where(x =>
                !string.IsNullOrWhiteSpace(x))
            .Distinct(
                StringComparer.OrdinalIgnoreCase);
    }

    private static string MakeSafeFilePart(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Document";

        var invalidCharacters =
            Path.GetInvalidFileNameChars();

        var cleaned =
            new string(
                value
                    .Trim()
                    .Select(c =>
                        invalidCharacters.Contains(c)
                            ? '_'
                            : c)
                    .ToArray());

        cleaned =
            cleaned.Replace(' ', '_');

        return string.IsNullOrWhiteSpace(cleaned)
            ? "Document"
            : cleaned;
    }

    private static string GetContentType(
        string fileName)
    {
        return Path.GetExtension(fileName)
            .ToLowerInvariant() switch
        {
            ".pdf" =>
                "application/pdf",

            ".jpg" or ".jpeg" =>
                "image/jpeg",

            ".png" =>
                "image/png",

            ".docx" =>
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

            _ =>
                "application/octet-stream"
        };
    }
}
