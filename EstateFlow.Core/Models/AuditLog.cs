using System.ComponentModel.DataAnnotations;

namespace EstateFlow.Core.Models;

public class AuditLog
{
    [Key]
    public Guid AuditId { get; set; } = Guid.NewGuid();
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string? UserName { get; set; }
    public Guid? EstateId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Changes { get; set; }
}