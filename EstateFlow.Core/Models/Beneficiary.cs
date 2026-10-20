namespace EstateFlow.Core.Models;

public class Beneficiary
{
    public Guid BeneficiaryId { get; set; } = Guid.NewGuid();

    public Guid EstateId { get; set; }

    public Estate? Estate { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string IdNumber { get; set; } = string.Empty;

    public string Relationship { get; set; } = string.Empty;

    public string HeirType { get; set; } = string.Empty;

    // Used only for estate communication / automated monthly reports.
    public string? Email { get; set; }

    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? Surname { get; set; }

    public string? RelationshipDescription { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? MaritalStatus { get; set; }

    // Inheritance: cash legacy + fraction of residue. Special bequests live on the asset.
    public string? WillClause { get; set; }
    public decimal CashLegacyAmount { get; set; }
    public int? ResidueNumerator { get; set; }
    public int? ResidueDenominator { get; set; }
    public decimal AdvancesPaid { get; set; }

    public bool IsSurvivingSpouse { get; set; }
    public bool IsMinor { get; set; }
    public string? GuardianOrTrustee { get; set; }
    public bool IsPredeceased { get; set; }

    public bool IsUltimateBeneficiaryPreviousEstate { get; set; }
    public string? BareDominiumFraction { get; set; }

    public int DisplayOrder { get; set; }
}
