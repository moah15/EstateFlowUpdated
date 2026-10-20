namespace EstateFlow.Core.Models;

public class FinancialClaim
{
    public Guid ClaimId { get; set; } = Guid.NewGuid();
    public Guid EstateId { get; set; }
    public Estate? Estate { get; set; }

    public string InstitutionName { get; set; } = string.Empty;
    public string InstitutionAddress { get; set; } = string.Empty;
    public string AccountReference { get; set; } = string.Empty;
    public decimal Value { get; set; }

    public string? ClaimType { get; set; }
    public bool IsInFavourOfEstate { get; set; } = true;
    public decimal AccruedInterest { get; set; }
    public DateTime? ValuationDate { get; set; }

    // Comma-separated BeneficiaryIds the claim is awarded to
    public string? AwardedToBeneficiaryIds { get; set; }
    public bool IsSpecialBequest { get; set; }

    public bool IsCollectedAsCash { get; set; }
    public decimal CollectedAmount { get; set; }
    public DateTime? CollectedDate { get; set; }

    public bool IsCreditLifeInsurance { get; set; }
    public string? LinkedLiabilityNote { get; set; }

    public bool RequiresInstitutionLetter() => true;
}
