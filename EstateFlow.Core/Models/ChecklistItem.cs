using System.ComponentModel.DataAnnotations;

namespace EstateFlow.Core.Models;

// Generic per-estate task. One table drives the SARS / tax tracker, the Master's
// requirements, creditor and asset tasks, the L&D inspection / objection register and
// the transfer tracker – distinguished by Category.
//
// Status values: Open | InProgress | Done | NotApplicable
public class ChecklistItem
{
    [Key]
    public Guid ItemId { get; set; } = Guid.NewGuid();
    public Guid EstateId { get; set; }
    public Estate? Estate { get; set; }

    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
}
