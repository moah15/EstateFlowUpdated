namespace EstateFlow.Core.Models;

public class Liability
{
    public Guid LiabilityId { get; set; } = Guid.NewGuid();
    public Guid EstateId { get; set; }
    public Estate? Estate { get; set; }

    public string CreditorName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }

    public string? Category { get; set; }
    public string? AccountReference { get; set; }
    public decimal InterestAfterDeath { get; set; }
    public string? Status { get; set; } = "Admitted";
    public DateTime? DateLodged { get; set; }
    public string? SecuredAgainst { get; set; }
    public bool IsTakenOverByHeir { get; set; }
    public bool IsPaid { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime? DatePaid { get; set; }
    public bool IsCoveredByCreditLife { get; set; }
}
