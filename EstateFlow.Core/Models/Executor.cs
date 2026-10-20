namespace EstateFlow.Core.Models;

public class Executor
{
    public Guid ExecutorId { get; set; }

    public Guid EstateId { get; set; }

    public Estate? Estate { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string IdNumber { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    // Used by the automated monthly estate report.
    // Nullable so existing executor rows continue to work after migration.
    public string? Email { get; set; }

    // Contact & address
    public string? Phone { get; set; }
    public string? Address { get; set; }

    // Letters of Executorship
    public string? LettersOfExecutorshipRef { get; set; }
    public DateTime? LettersOfExecutorshipDate { get; set; }
    public string? MastersOffice { get; set; }
}