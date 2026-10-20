namespace EstateFlow.Core.Models;

public class ScannedDocument
{
    public Guid DocumentId { get; set; } = Guid.NewGuid();
    public Guid EstateId { get; set; }
    public Estate? Estate { get; set; }

    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty; // auto-generated: EstateRef_DocType_Date
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    public string BlobReference { get; set; } = string.Empty; // secure Azure Blob path/SAS ref
}