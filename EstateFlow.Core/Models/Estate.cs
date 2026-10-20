
namespace EstateFlow.Core.Models;

public class Estate
{
    public ICollection<CashbookEntry> CashbookEntries { get; set; } = new List<CashbookEntry>();
    public ICollection<ChecklistItem> ChecklistItems { get; set; } = new List<ChecklistItem>();
    public ICollection<EstateAdvertisement> Advertisements { get; set; } = new List<EstateAdvertisement>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public Guid EstateId { get; set; } = Guid.NewGuid();
    public string FileNumber { get; set; } = string.Empty;
    public string? MastersReference { get; set; }
    public DateTime DateOfDeath { get; set; }
    public string EstateStatus { get; set; } = "Reported"; // matches state diagram start
    public bool IsDutiable { get; set; }


    // Part 3 stakeholder feedback — firm may act as agent rather than nominated executor.
    public bool FirmActsAsAgent { get; set; }
    public string? AgentName { get; set; }
    public string? AgentReference { get; set; }
    public string? AgentEmail { get; set; }
    public string? AgentPhone { get; set; }

    // Used in the draft Liquidation & Distribution Account calculation.
    public decimal AdministrationExpenses { get; set; }
    // L&D account — General settings
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
    public string? SourceOfEstate { get; set; }
    public string? EstateBankName { get; set; }
    public string? EstateBankBranchCode { get; set; }
    public string? EstateBankAccountNumber { get; set; }
    public bool HasMultipleLdAccounts { get; set; }
    public int PreviousSpousesCount { get; set; }
    public decimal PreviousSpouseRebateUsed { get; set; }

    // FK — the controller assigned to this estate
    public Guid ControllerId { get; set; }
    public EstateController? Controller { get; set; }

    // Navigation collections — one-to-many from Estate
    public Deceased? Deceased { get; set; }
    public ICollection<Executor> Executors { get; set; } = new List<Executor>();
    public ICollection<Beneficiary> Beneficiaries { get; set; } = new List<Beneficiary>();
    public ICollection<ImmovableProperty> ImmovableProperties { get; set; } = new List<ImmovableProperty>();
    public ICollection<MovableAsset> MovableAssets { get; set; } = new List<MovableAsset>();
    public ICollection<FinancialClaim> FinancialClaims { get; set; } = new List<FinancialClaim>();
    public ICollection<Liability> Liabilities { get; set; } = new List<Liability>();
    public ICollection<DiaryEntry> DiaryEntries { get; set; } = new List<DiaryEntry>();
    public ICollection<ProgressStage> ProgressStages { get; set; } = new List<ProgressStage>();
    public ICollection<ScannedDocument> ScannedDocuments { get; set; } = new List<ScannedDocument>();
    public ICollection<AutomatedReport> AutomatedReports { get; set; } = new List<AutomatedReport>();

    // ─── Estate Duty extras ───────────────────────────────────────────────────
    public bool EstateDutyHigherRate { get; set; }
    public decimal EstateDutyThreshold { get; set; } = 3_500_000m;
    public decimal DeemedPropertyForDuty { get; set; }
    public string? DeemedPropertyNote { get; set; }
    public decimal OtherDutyDeductions { get; set; }
    public string? OtherDutyDeductionsNote { get; set; }
}