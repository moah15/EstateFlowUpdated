namespace EstateFlow.Core.Models;

public class AutomatedReport
{
    public Guid ReportId { get; set; } = Guid.NewGuid();
    public Guid EstateId { get; set; }
    public Estate? Estate { get; set; }

    public DateTime ReportPeriod { get; set; }
    public string RecipientType { get; set; } = string.Empty; // Executor | Heir
    public DateTime? SentDate { get; set; }
    public bool IsPaused { get; set; }
}