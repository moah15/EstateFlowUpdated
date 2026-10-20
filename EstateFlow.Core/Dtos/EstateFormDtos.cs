namespace EstateFlow.Core.Dtos;

public class CreateEstateRequest
{
    public string FileNumber { get; set; } = string.Empty;
    public string? MastersReference { get; set; }
    public DateTime DateOfDeath { get; set; }
    public bool IsDutiable { get; set; }
    public bool FirmActsAsAgent { get; set; }
    public string? AgentName { get; set; }
    public string? AgentReference { get; set; }
    public string? AgentEmail { get; set; }
    public string? AgentPhone { get; set; }
    public decimal AdministrationExpenses { get; set; }

    public DateTime? DateOfAccount { get; set; }
    public string? PlaceOfAccount { get; set; }
    public string AccountType { get; set; } = "First and Final";
    public bool IsTestate { get; set; } = true;
    public string? MastersOffice { get; set; }
    public DateTime? LettersOfExecutorshipDate { get; set; }
    public decimal ExecutorFeePercentage { get; set; } = 3.5m;
    public bool IsVatRegistered { get; set; } = true;
    public decimal VatRate { get; set; } = 15m;
    public decimal MastersFee { get; set; }
    public decimal EstateDutyRebate { get; set; } = 3500000m;
    public decimal EstateDutyRate { get; set; } = 20m;

    public string DeceasedFullName { get; set; } = string.Empty;
    public string DeceasedIdNumber { get; set; } = string.Empty;
    public string? DeceasedMaritalStatus { get; set; }
    public string? DeceasedFirstName { get; set; }
    public string? DeceasedMiddleName { get; set; }
    public string? DeceasedSurname { get; set; }
    public string? DeceasedPlaceOfDeath { get; set; }
    public string? DeceasedMatrimonialRegime { get; set; }
    public string? DeceasedSpouseFullName { get; set; }
    public string? DeceasedSpouseIdNumber { get; set; }
    public DateTime? DeceasedWillDate { get; set; }
    public string? DeceasedWillLocation { get; set; }
    public string? SourceOfEstate { get; set; }
    public string? EstateBankName { get; set; }
    public string? EstateBankBranchCode { get; set; }
    public string? EstateBankAccountNumber { get; set; }
    public bool HasMultipleLdAccounts { get; set; }
    public int PreviousSpousesCount { get; set; }
    public decimal PreviousSpouseRebateUsed { get; set; }

    public string? DeceasedGender { get; set; }
    public DateTime? DeceasedDateOfBirth { get; set; }
    public bool DeceasedLivedInSouthAfrica { get; set; } = true;
    public DateTime? DeceasedCodicilDate { get; set; }

    // ─── Estate Duty extras ───────────────────────────────────────────────────
    public bool EstateDutyHigherRate { get; set; }
    public decimal EstateDutyThreshold { get; set; } = 3_500_000m;
    public decimal DeemedPropertyForDuty { get; set; }
    public string? DeemedPropertyNote { get; set; }
    public decimal OtherDutyDeductions { get; set; }
    public string? OtherDutyDeductionsNote { get; set; }
}

public class UpdateEstateRequest
{
    public string FileNumber { get; set; } = string.Empty;
    public string? MastersReference { get; set; }
    public DateTime DateOfDeath { get; set; }
    public bool IsDutiable { get; set; }
    public bool FirmActsAsAgent { get; set; }
    public string? AgentName { get; set; }
    public string? AgentReference { get; set; }
    public string? AgentEmail { get; set; }
    public string? AgentPhone { get; set; }
    public decimal AdministrationExpenses { get; set; }

    public DateTime? DateOfAccount { get; set; }
    public string? PlaceOfAccount { get; set; }
    public string AccountType { get; set; } = "First and Final";
    public bool IsTestate { get; set; } = true;
    public string? MastersOffice { get; set; }
    public DateTime? LettersOfExecutorshipDate { get; set; }
    public decimal ExecutorFeePercentage { get; set; } = 3.5m;
    public bool IsVatRegistered { get; set; } = true;
    public decimal VatRate { get; set; } = 15m;
    public decimal MastersFee { get; set; }
    public decimal EstateDutyRebate { get; set; } = 3500000m;
    public decimal EstateDutyRate { get; set; } = 20m;

    public string EstateStatus { get; set; } = string.Empty;

    public string DeceasedFullName { get; set; } = string.Empty;
    public string DeceasedIdNumber { get; set; } = string.Empty;
    public string? DeceasedMaritalStatus { get; set; }
    public string? DeceasedFirstName { get; set; }
    public string? DeceasedMiddleName { get; set; }
    public string? DeceasedSurname { get; set; }
    public string? DeceasedPlaceOfDeath { get; set; }
    public string? DeceasedMatrimonialRegime { get; set; }
    public string? DeceasedSpouseFullName { get; set; }
    public string? DeceasedSpouseIdNumber { get; set; }
    public DateTime? DeceasedWillDate { get; set; }
    public string? DeceasedWillLocation { get; set; }
    public string? SourceOfEstate { get; set; }
    public string? EstateBankName { get; set; }
    public string? EstateBankBranchCode { get; set; }
    public string? EstateBankAccountNumber { get; set; }
    public bool HasMultipleLdAccounts { get; set; }
    public int PreviousSpousesCount { get; set; }
    public decimal PreviousSpouseRebateUsed { get; set; }

    public string? DeceasedGender { get; set; }
    public DateTime? DeceasedDateOfBirth { get; set; }
    public bool DeceasedLivedInSouthAfrica { get; set; } = true;
    public DateTime? DeceasedCodicilDate { get; set; }

    // ─── Estate Duty extras ───────────────────────────────────────────────────
    public bool EstateDutyHigherRate { get; set; }
    public decimal EstateDutyThreshold { get; set; } = 3_500_000m;
    public decimal DeemedPropertyForDuty { get; set; }
    public string? DeemedPropertyNote { get; set; }
    public decimal OtherDutyDeductions { get; set; }
    public string? OtherDutyDeductionsNote { get; set; }
}