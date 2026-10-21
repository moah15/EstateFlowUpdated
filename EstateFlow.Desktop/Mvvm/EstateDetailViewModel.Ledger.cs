using EstateFlow.Core.Dtos;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace EstateFlow.Desktop.Mvvm;

// One choice in the cashbook "Type" list.
public sealed class CashbookTypeOption
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public bool IsReceipt { get; init; }
}

public partial class EstateDetailViewModel
{
    // ─── Static option lists ─────────────────────────────────────────────────
    public static IReadOnlyList<CashbookTypeOption> EntryTypeOptions { get; } = new[]
    {
        new CashbookTypeOption { Value = CashbookTypes.IncomeAfterDeath, Label = "Receipt – Income after death (rent, interest, dividends)", IsReceipt = true },
        new CashbookTypeOption { Value = CashbookTypes.CapitalReceipt, Label = "Receipt – Capital (sale proceeds, funds collected)", IsReceipt = true },
        new CashbookTypeOption { Value = CashbookTypes.AdministrationCost, Label = "Payment – Administration cost", IsReceipt = false },
        new CashbookTypeOption { Value = CashbookTypes.CreditorPayment, Label = "Payment – Creditor", IsReceipt = false },
        new CashbookTypeOption { Value = CashbookTypes.Distribution, Label = "Payment – Distribution to heir", IsReceipt = false }
    };

    public static IReadOnlyList<string> CashbookTypeOptions { get; } =
        new[] { "All" }.Concat(CashbookTypes.All).ToArray();

    public static IReadOnlyList<string> ChecklistStatusOptions => new[] { "All", "Open", "InProgress", "Done", "NotApplicable" };

    // ─── Cashbook ───────────────────────────────────────────────────────────
    public ObservableCollection<CashbookEntryDto> CashbookEntries { get; } = new();
    public ObservableCollection<CashbookEntryDto> FilteredCashbook { get; } = new();

    private string _cashbookTypeFilter = "All";
    public string CashbookTypeFilter
    {
        get => _cashbookTypeFilter;
        set { SetProperty(ref _cashbookTypeFilter, value); ApplyCashbookFilter(); }
    }

    private string _cashbookSearch = string.Empty;
    public string CashbookSearch
    {
        get => _cashbookSearch;
        set { SetProperty(ref _cashbookSearch, value); ApplyCashbookFilter(); }
    }

    private bool _cashbookUnreconciledOnly;
    public bool CashbookUnreconciledOnly
    {
        get => _cashbookUnreconciledOnly;
        set { SetProperty(ref _cashbookUnreconciledOnly, value); ApplyCashbookFilter(); }
    }

    public decimal CashbookTotalReceipts => CashbookEntries.Where(e => e.IsReceipt).Sum(e => e.Amount);
    public decimal CashbookTotalPayments => CashbookEntries.Where(e => !e.IsReceipt).Sum(e => e.Amount);
    public decimal CashbookBalance => CashbookTotalReceipts - CashbookTotalPayments;
    public int CashbookUnreconciledCount => CashbookEntries.Count(e => !e.IsReconciled);

    // new / edited entry fields
    private Guid? _editingCashbookId;
    public bool IsEditingCashbook => _editingCashbookId.HasValue;

    private string _newEntryType = CashbookTypes.IncomeAfterDeath;
    public string NewEntryType
    {
        get => _newEntryType;
        set
        {
            if (SetProperty(ref _newEntryType, value))
                NotifyCashbookFormChanged();
        }
    }

    private string _newEntryCategory = string.Empty;
    public string NewEntryCategory { get => _newEntryCategory; set => SetProperty(ref _newEntryCategory, value); }
    private string _newEntryDescription = string.Empty;
    public string NewEntryDescription { get => _newEntryDescription; set => SetProperty(ref _newEntryDescription, value); }
    private string _newEntryPayerOrPayee = string.Empty;
    public string NewEntryPayerOrPayee { get => _newEntryPayerOrPayee; set => SetProperty(ref _newEntryPayerOrPayee, value); }
    private string _newEntryReference = string.Empty;
    public string NewEntryReference { get => _newEntryReference; set => SetProperty(ref _newEntryReference, value); }
    private string _newEntryAmountText = string.Empty;
    public string NewEntryAmountText { get => _newEntryAmountText; set => SetProperty(ref _newEntryAmountText, value); }
    private DateTime _newEntryDate = DateTime.Today;
    public DateTime NewEntryDate { get => _newEntryDate; set => SetProperty(ref _newEntryDate, value); }

    // Headings that follow the chosen type (receipt or payment).
    public bool NewEntryIsReceipt => CashbookTypes.IsReceipt(NewEntryType);
    public string PartyLabel => NewEntryIsReceipt ? "Received From" : "Paid To";
    public string AmountLabel => NewEntryIsReceipt ? "Amount Received (R)" : "Amount Paid (R)";

    public string CashbookFormTitle =>
        IsEditingCashbook
            ? (NewEntryIsReceipt ? "EDIT RECEIPT" : "EDIT PAYMENT")
            : (NewEntryIsReceipt ? "ADD RECEIPT" : "ADD PAYMENT");

    public string CashbookSaveLabel =>
        IsEditingCashbook
            ? "Save Changes"
            : (NewEntryIsReceipt ? "+ Add Receipt" : "+ Add Payment");

    private void NotifyCashbookFormChanged()
    {
        OnPropertyChanged(nameof(CategorySuggestions));
        OnPropertyChanged(nameof(NewEntryIsReceipt));
        OnPropertyChanged(nameof(PartyLabel));
        OnPropertyChanged(nameof(AmountLabel));
        OnPropertyChanged(nameof(CashbookFormTitle));
        OnPropertyChanged(nameof(CashbookSaveLabel));
        OnPropertyChanged(nameof(IsEditingCashbook));
    }

    public IEnumerable<string> CategorySuggestions => NewEntryType switch
    {
        CashbookTypes.IncomeAfterDeath => new[]
        {
            "Rental income", "Interest received", "Dividends", "Policy proceeds", "Refund received", "Other income"
        },
        CashbookTypes.CapitalReceipt => new[]
        {
            "Sale of immovable property", "Sale of vehicle", "Sale of shares", "Bank balance collected",
            "Policy / pension proceeds", "Debtor repayment", "Other capital receipt"
        },
        CashbookTypes.AdministrationCost => new[]
        {
            "Funeral costs", "Advertising (Gazette / newspaper)", "Valuation fees", "Conveyancing / transfer costs",
            "Master's fees", "Legal fees", "Rates & taxes", "Insurance", "Bank charges", "Other expense"
        },
        CashbookTypes.CreditorPayment => new[]
        {
            "Bond / home loan", "Vehicle finance", "Credit card", "Medical", "SARS", "Municipal account", "Other creditor"
        },
        CashbookTypes.Distribution => new[]
        {
            "Cash legacy", "Residue distribution", "Advance to heir"
        },
        _ => Array.Empty<string>()
    };

    public AsyncRelayCommand AddCashbookEntryCommand { get; private set; } = null!;
    public RelayCommand<CashbookEntryDto> EditCashbookEntryCommand { get; private set; } = null!;
    public RelayCommand CancelCashbookEditCommand { get; private set; } = null!;
    public RelayCommand<CashbookEntryDto> DeleteCashbookEntryCommand { get; private set; } = null!;
    public RelayCommand<CashbookEntryDto> ToggleReconciledCommand { get; private set; } = null!;
    public AsyncRelayCommand DownloadCashbookPdfCommand { get; private set; } = null!;
    public RelayCommand ClearCashbookFiltersCommand { get; private set; } = null!;

    // ─── Checklist ──────────────────────────────────────────────────────────
    public ObservableCollection<ChecklistItemDto> ChecklistItems { get; } = new();
    public ObservableCollection<ChecklistItemDto> FilteredChecklist { get; } = new();

    private string _checklistStatusFilter = "All";
    public string ChecklistStatusFilter
    {
        get => _checklistStatusFilter;
        set { SetProperty(ref _checklistStatusFilter, value); ApplyChecklistFilter(); }
    }

    private string _checklistSearch = string.Empty;
    public string ChecklistSearch
    {
        get => _checklistSearch;
        set { SetProperty(ref _checklistSearch, value); ApplyChecklistFilter(); }
    }

    public string ChecklistSummary
    {
        get
        {
            var total = ChecklistItems.Count;
            var done = ChecklistItems.Count(i => i.IsDone);
            var over = ChecklistItems.Count(i => i.IsOverdue);
            return total == 0 ? "No tasks" : $"{done}/{total} done{(over > 0 ? $"  |  {over} overdue" : "")}";
        }
    }

    private Guid? _editingChecklistId;
    private string _editingChecklistCategory = string.Empty;
    public bool IsEditingChecklist => _editingChecklistId.HasValue;
    public string ChecklistFormTitle => IsEditingChecklist ? "EDIT TASK" : "ADD CUSTOM TASK";
    public string ChecklistSaveLabel => IsEditingChecklist ? "Save Changes" : "Add Task";

    private string _newChecklistTask = string.Empty;
    public string NewChecklistTask { get => _newChecklistTask; set => SetProperty(ref _newChecklistTask, value); }

    private DateTime? _newChecklistDueDate;
    public DateTime? NewChecklistDueDate { get => _newChecklistDueDate; set => SetProperty(ref _newChecklistDueDate, value); }

    private string _newChecklistNotes = string.Empty;
    public string NewChecklistNotes { get => _newChecklistNotes; set => SetProperty(ref _newChecklistNotes, value); }

    public AsyncRelayCommand SeedChecklistCommand { get; private set; } = null!;
    public AsyncRelayCommand AddChecklistItemCommand { get; private set; } = null!;
    public RelayCommand<ChecklistItemDto> EditChecklistItemCommand { get; private set; } = null!;
    public RelayCommand CancelChecklistEditCommand { get; private set; } = null!;
    public RelayCommand<ChecklistItemDto> StartChecklistItemCommand { get; private set; } = null!;
    public RelayCommand<ChecklistItemDto> DoneChecklistItemCommand { get; private set; } = null!;
    public RelayCommand<ChecklistItemDto> NaChecklistItemCommand { get; private set; } = null!;
    public RelayCommand<ChecklistItemDto> ReopenChecklistItemCommand { get; private set; } = null!;
    public RelayCommand<ChecklistItemDto> DeleteChecklistItemCommand { get; private set; } = null!;

    // ─── Init ────────────────────────────────────────────────────────────────
    private void InitLedgerCommands()
    {
        AddCashbookEntryCommand = new AsyncRelayCommand(DoSaveCashbookEntryAsync);
        EditCashbookEntryCommand = new RelayCommand<CashbookEntryDto>(dto => BeginEditCashbookEntry(dto!));
        CancelCashbookEditCommand = new RelayCommand(ResetCashbookForm);
        DeleteCashbookEntryCommand = new RelayCommand<CashbookEntryDto>(dto => { _ = DoDeleteCashbookEntryAsync(dto!); });
        ToggleReconciledCommand = new RelayCommand<CashbookEntryDto>(dto => { _ = DoToggleReconciledAsync(dto!); });
        DownloadCashbookPdfCommand = new AsyncRelayCommand(DoDownloadCashbookPdfAsync);
        ClearCashbookFiltersCommand = new RelayCommand(() => { CashbookTypeFilter = "All"; CashbookSearch = string.Empty; CashbookUnreconciledOnly = false; });

        SeedChecklistCommand = new AsyncRelayCommand(DoSeedChecklistAsync);
        AddChecklistItemCommand = new AsyncRelayCommand(DoSaveChecklistItemAsync);
        EditChecklistItemCommand = new RelayCommand<ChecklistItemDto>(dto => BeginEditChecklistItem(dto!));
        CancelChecklistEditCommand = new RelayCommand(ResetChecklistForm);
        StartChecklistItemCommand = new RelayCommand<ChecklistItemDto>(dto => { _ = DoChecklistStatusAsync(dto!, ChecklistStatuses.InProgress); });
        DoneChecklistItemCommand = new RelayCommand<ChecklistItemDto>(dto => { _ = DoChecklistStatusAsync(dto!, ChecklistStatuses.Done); });
        NaChecklistItemCommand = new RelayCommand<ChecklistItemDto>(dto => { _ = DoChecklistStatusAsync(dto!, ChecklistStatuses.NotApplicable); });
        ReopenChecklistItemCommand = new RelayCommand<ChecklistItemDto>(dto => { _ = DoChecklistStatusAsync(dto!, ChecklistStatuses.Open); });
        DeleteChecklistItemCommand = new RelayCommand<ChecklistItemDto>(dto => { _ = DoDeleteChecklistItemAsync(dto!); });
    }

    // ─── Refresh (called when Estate changes) ───────────────────────────────
    private void RefreshLedger()
    {
        if (Estate is null) return;

        CashbookEntries.Clear();
        foreach (var e in Estate.CashbookEntries ?? Enumerable.Empty<CashbookEntryDto>())
            CashbookEntries.Add(e);

        ChecklistItems.Clear();
        foreach (var i in Estate.ChecklistItems ?? Enumerable.Empty<ChecklistItemDto>())
            ChecklistItems.Add(i);

        ApplyCashbookFilter();
        ApplyChecklistFilter();
        RefreshCashbookTotals();
        OnPropertyChanged(nameof(ChecklistSummary));
    }

    private void RefreshCashbookTotals()
    {
        OnPropertyChanged(nameof(CashbookTotalReceipts));
        OnPropertyChanged(nameof(CashbookTotalPayments));
        OnPropertyChanged(nameof(CashbookBalance));
        OnPropertyChanged(nameof(CashbookUnreconciledCount));
    }

    // ─── Cashbook actions ────────────────────────────────────────────────────
    private async Task DoSaveCashbookEntryAsync()
    {
        if (Estate is null) return;

        if (!Money.TryParse(NewEntryAmountText, out var amount) || amount <= 0)
        {
            StatusMessage = "Enter a valid amount greater than zero.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewEntryCategory) && string.IsNullOrWhiteSpace(NewEntryDescription))
        {
            StatusMessage = "Enter a category or a description.";
            return;
        }

        var request = new CreateCashbookEntryRequest
        {
            EntryType = NewEntryType,
            Category = NewEntryCategory.Trim(),
            Description = NewEntryDescription.Trim(),
            PayerOrPayee = NewEntryPayerOrPayee.Trim(),
            Reference = NewEntryReference.Trim(),
            Amount = amount,
            EntryDate = NewEntryDate
        };

        try
        {
            var editingId = _editingCashbookId;

            if (editingId is Guid id)
                await _apiClient.UpdateCashbookEntryAsync(Estate.EstateId, id, request);
            else
                await _apiClient.AddCashbookEntryAsync(Estate.EstateId, request);

            ResetCashbookForm();
            await LoadAsync(_estateId);

            StatusMessage = editingId.HasValue
                ? "Cashbook entry saved."
                : CashbookTypes.IsReceipt(request.EntryType) ? "Receipt added." : "Payment added.";
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
    }

    private void BeginEditCashbookEntry(CashbookEntryDto dto)
    {
        _editingCashbookId = dto.EntryId;

        NewEntryType = dto.EntryType;
        NewEntryCategory = dto.Category;
        NewEntryDescription = dto.Description;
        NewEntryPayerOrPayee = dto.PayerOrPayee ?? string.Empty;
        NewEntryReference = dto.Reference ?? string.Empty;
        NewEntryAmountText = FormatMoney(dto.Amount);
        NewEntryDate = dto.EntryDate;

        NotifyCashbookFormChanged();
        StatusMessage = "Editing the cashbook entry. Change the details in the form at the top, then press Save Changes.";
    }

    // Clears the form but keeps the chosen type, so several receipts can be added in a row.
    private void ResetCashbookForm()
    {
        _editingCashbookId = null;

        NewEntryCategory = string.Empty;
        NewEntryDescription = string.Empty;
        NewEntryPayerOrPayee = string.Empty;
        NewEntryReference = string.Empty;
        NewEntryAmountText = string.Empty;
        NewEntryDate = DateTime.Today;

        NotifyCashbookFormChanged();
    }

    private async Task DoDeleteCashbookEntryAsync(CashbookEntryDto dto)
    {
        if (Estate is null) return;
        if (MessageBox.Show("Delete this cashbook entry?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        try
        {
            await _apiClient.DeleteCashbookEntryAsync(Estate.EstateId, dto.EntryId);

            if (_editingCashbookId == dto.EntryId)
                ResetCashbookForm();

            await LoadAsync(_estateId);
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
    }

    private async Task DoToggleReconciledAsync(CashbookEntryDto dto)
    {
        if (Estate is null) return;
        try
        {
            await _apiClient.SetCashbookReconciledAsync(Estate.EstateId, dto.EntryId, !dto.IsReconciled);
            await LoadAsync(_estateId);
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
    }

    private async Task DoDownloadCashbookPdfAsync()
    {
        if (Estate is null) return;
        try
        {
            var temp = await _apiClient.DownloadCashbookToTempFileAsync(Estate.EstateId);
            var dlg = new SaveFileDialog
            {
                FileName = $"Cashbook_{Estate.Deceased?.FullName ?? Estate.FileNumber}.pdf",
                Filter = "PDF|*.pdf"
            };
            if (dlg.ShowDialog() == true) File.Copy(temp, dlg.FileName, true);
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
    }

    private void ApplyCashbookFilter()
    {
        FilteredCashbook.Clear();
        foreach (var e in CashbookEntries
            .Where(e => (CashbookTypeFilter == "All" || e.EntryType == CashbookTypeFilter)
                     && (!CashbookUnreconciledOnly || !e.IsReconciled)
                     && (string.IsNullOrWhiteSpace(CashbookSearch)
                         || e.Description.Contains(CashbookSearch, StringComparison.OrdinalIgnoreCase)
                         || (e.Category ?? "").Contains(CashbookSearch, StringComparison.OrdinalIgnoreCase)
                         || (e.PayerOrPayee ?? "").Contains(CashbookSearch, StringComparison.OrdinalIgnoreCase)
                         || (e.Reference ?? "").Contains(CashbookSearch, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(e => e.EntryDate))
        {
            FilteredCashbook.Add(e);
        }
    }

    // ─── Checklist actions ──────────────────────────────────────────────────
    private async Task DoSeedChecklistAsync()
    {
        if (Estate is null) return;
        try
        {
            var added = await _apiClient.SeedChecklistAsync(Estate.EstateId);
            await LoadAsync(_estateId);
            if (added == 0) StatusMessage = "All standard tasks already exist.";
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
    }

    private async Task DoSaveChecklistItemAsync()
    {
        if (Estate is null) return;

        if (string.IsNullOrWhiteSpace(NewChecklistTask))
        {
            StatusMessage = "Enter a task description.";
            return;
        }

        var request = new CreateChecklistItemRequest
        {
            Category = _editingChecklistCategory,
            Title = NewChecklistTask.Trim(),
            DueDate = NewChecklistDueDate,
            Notes = NewChecklistNotes.Trim()
        };

        try
        {
            var editingId = _editingChecklistId;

            if (editingId is Guid id)
                await _apiClient.UpdateChecklistItemAsync(Estate.EstateId, id, request);
            else
                await _apiClient.AddChecklistItemAsync(Estate.EstateId, request);

            ResetChecklistForm();
            await LoadAsync(_estateId);
            StatusMessage = editingId.HasValue ? "Task saved." : "Task added.";
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
    }

    private void BeginEditChecklistItem(ChecklistItemDto dto)
    {
        _editingChecklistId = dto.ItemId;
        _editingChecklistCategory = dto.Category;

        NewChecklistTask = dto.Title;
        NewChecklistDueDate = dto.DueDate;
        NewChecklistNotes = dto.Notes ?? string.Empty;

        NotifyChecklistFormChanged();
        StatusMessage = "Editing the task. Change the details in the form at the top, then press Save Changes.";
    }

    private void ResetChecklistForm()
    {
        _editingChecklistId = null;
        _editingChecklistCategory = string.Empty;

        NewChecklistTask = string.Empty;
        NewChecklistDueDate = null;
        NewChecklistNotes = string.Empty;

        NotifyChecklistFormChanged();
    }

    private void NotifyChecklistFormChanged()
    {
        OnPropertyChanged(nameof(IsEditingChecklist));
        OnPropertyChanged(nameof(ChecklistFormTitle));
        OnPropertyChanged(nameof(ChecklistSaveLabel));
    }

    private async Task DoChecklistStatusAsync(ChecklistItemDto dto, string status)
    {
        if (Estate is null) return;
        try
        {
            await _apiClient.UpdateChecklistStatusAsync(Estate.EstateId, dto.ItemId, status);
            await LoadAsync(_estateId);
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
    }

    private async Task DoDeleteChecklistItemAsync(ChecklistItemDto dto)
    {
        if (Estate is null) return;
        if (MessageBox.Show("Delete this task?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        try
        {
            await _apiClient.DeleteChecklistItemAsync(Estate.EstateId, dto.ItemId);

            if (_editingChecklistId == dto.ItemId)
                ResetChecklistForm();

            await LoadAsync(_estateId);
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
    }

    private void ApplyChecklistFilter()
    {
        FilteredChecklist.Clear();
        foreach (var i in ChecklistItems
            .Where(i => (ChecklistStatusFilter == "All" || i.Status == ChecklistStatusFilter)
                     && (string.IsNullOrWhiteSpace(ChecklistSearch)
                         || i.Title.Contains(ChecklistSearch, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(i => i.SortOrder))
        {
            FilteredChecklist.Add(i);
        }
        OnPropertyChanged(nameof(ChecklistSummary));
    }
}
