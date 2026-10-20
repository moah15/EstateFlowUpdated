namespace EstateFlow.Core.Dtos;

public static class CashbookTypes
{
    public const string IncomeAfterDeath = "Income after death";
    public const string CapitalReceipt = "Capital receipt";
    public const string AdministrationCost = "Administration cost";
    public const string CreditorPayment = "Creditor payment";
    public const string Distribution = "Distribution to heir";

    public static readonly string[] All =
    {
        IncomeAfterDeath, CapitalReceipt, AdministrationCost, CreditorPayment, Distribution
    };

    public static bool IsReceipt(string? type) =>
        type == IncomeAfterDeath || type == CapitalReceipt;
}

public class CashbookEntryDto
{
    public Guid EntryId { get; set; }
    public DateTime EntryDate { get; set; }
    public string EntryType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? PayerOrPayee { get; set; }
    public string? Reference { get; set; }
    public decimal Amount { get; set; }
    public bool IsReconciled { get; set; }

    public bool IsReceipt => CashbookTypes.IsReceipt(EntryType);
    public decimal SignedAmount => IsReceipt ? Amount : -Amount;
    public string AmountText => (IsReceipt ? "+ R " : "– R ") + Amount.ToString("N2");
    public string ReconciledText => IsReconciled ? "Reconciled" : "Not reconciled";
    public string Summary =>
        string.Join(" · ",
            new[] { EntryType, Category, PayerOrPayee, Reference }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
}

public class CreateCashbookEntryRequest
{
    public DateTime EntryDate { get; set; } = DateTime.Today;
    public string EntryType { get; set; } = CashbookTypes.AdministrationCost;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? PayerOrPayee { get; set; }
    public string? Reference { get; set; }
    public decimal Amount { get; set; }
}

public class SetReconciledRequest
{
    public bool IsReconciled { get; set; }
}

public static class ChecklistStatuses
{
    public const string Open = "Open";
    public const string InProgress = "InProgress";
    public const string Done = "Done";
    public const string NotApplicable = "NotApplicable";

    public static readonly string[] All = { Open, InProgress, Done, NotApplicable };
}

public class ChecklistItemDto
{
    public Guid ItemId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = ChecklistStatuses.Open;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public int SortOrder { get; set; }

    public bool IsDone => Status == ChecklistStatuses.Done;
    public bool IsNotApplicable => Status == ChecklistStatuses.NotApplicable;
    public bool IsClosed => IsDone || IsNotApplicable;

    public bool IsOverdue =>
        !IsClosed && DueDate.HasValue && DueDate.Value.Date < DateTime.Today;

    public string StatusLabel => Status switch
    {
        ChecklistStatuses.InProgress => "In progress",
        ChecklistStatuses.Done => "Done",
        ChecklistStatuses.NotApplicable => "Not applicable",
        _ => "Open"
    };

    public string StatusIcon => Status switch
    {
        ChecklistStatuses.Done => "[X]",
        ChecklistStatuses.InProgress => "[~]",
        ChecklistStatuses.NotApplicable => "[-]",
        _ => "[ ]"
    };

    public string DetailText =>
        string.Join(" · ",
            new[]
            {
                DueDate.HasValue ? $"Due {DueDate:dd MMM yyyy}" + (IsOverdue ? " (overdue)" : "") : null,
                IsDone && CompletedDate.HasValue ? $"Completed {CompletedDate:dd MMM yyyy}" : null,
                string.IsNullOrWhiteSpace(Reference) ? null : $"Ref {Reference}",
                string.IsNullOrWhiteSpace(Notes) ? null : Notes
            }.Where(s => !string.IsNullOrWhiteSpace(s)));
}

public class CreateChecklistItemRequest
{
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

public class UpdateChecklistStatusRequest
{
    public string Status { get; set; } = ChecklistStatuses.Open;
}

public class AuditEntryDto
{
    public DateTime TimestampUtc { get; set; }
    public string? UserName { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Changes { get; set; }

    public string WhenText => TimestampUtc.ToLocalTime().ToString("dd MMM yyyy HH:mm");
    public string Headline => $"{Action} {EntityName}" + (string.IsNullOrWhiteSpace(UserName) ? "" : $" · {UserName}");
}
