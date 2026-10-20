namespace EstateFlow.Core.Models;

public class ProgressStage
{
    public Guid StageId { get; set; } = Guid.NewGuid();
    public Guid EstateId { get; set; }
    public Estate? Estate { get; set; }

    public string StageName { get; set; } = string.Empty; // e.g. "Assets & Liabilities Captured"
    public DateTime? TargetDate { get; set; }
    public string Status { get; set; } = "Pending"; // Pending | InProgress | Complete
}