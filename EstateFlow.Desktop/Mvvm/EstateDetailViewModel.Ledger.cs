using EstateFlow.Core.Dtos;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace EstateFlow.Desktop.Mvvm;

public partial class EstateDetailViewModel
{
    // ─── Static option lists ─────────────────────────────────────────────────
    public static IReadOnlyList<string> EntryTypes => new[] { "Receipt", "Payment" };
    public static IReadOnlyList<string> CashbookTypeOptions => new[] { "All", "Receipt", "Payment" };
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

    // new entry fields
    private string _newEntryType = "Receipt";
    public string NewEntryType { get => _newEntryType; set { SetProperty(ref _newEntryType, value); OnPropertyChanged(nameof(CategorySuggestions)); } }
    private string _newEntryCategory = string.Empty;
    public string NewEntryCategory { get => _newEntryCategory; set => SetProperty(ref _newEntryCategory, value); }
    private string _newEntryDescription = string.Empty;
    public string NewEntryDescription { get => _newEntryDescription; set => SetProperty(ref _newEntryDescription, value); }
    private string _newEntryAmountText = string.Empty;
    public string NewEntryAmountText { get => _newEntryAmountText; set => SetProperty(ref _newEntryAmountText, value); }
    private DateTime _newEntryDate = DateTime.Today;
    public DateTime NewEntryDate { get => _newEntryDate; set => SetProperty(ref _newEntryDate, value); }

    public IEnumerable<string> CategorySuggestions => NewEntryType == "Receipt"
        ? new[] { "Rental income", "Interest received", "Dividends", "Policy proceeds", "Other income" }
        : new[] { "Funeral costs", "Rates & taxes", "Insurance", "Legal fees", "Bond repayment", "Transfer costs", "Other expense" };

    public AsyncRelayCommand AddCashbookEntryCommand { get; private set; } = null!;
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

    private string _newChecklistTask = string.Empty;
    public string NewChecklistTask { get => _newChecklistTask; set => SetProperty(ref _newChecklistTask, value); }

    public AsyncRelayCommand SeedChecklistCommand { get; private set; } = null!;
    public AsyncRelayCommand AddChecklistItemCommand { get; private set; } = null!;
    public RelayCommand<ChecklistItemDto> StartChecklistItemCommand { get; private set; } = null!;
    public RelayCommand<ChecklistItemDto> DoneChecklistItemCommand { get; private set; } = null!;
    public RelayCommand<ChecklistItemDto> NaChecklistItemCommand { get; private set; } = null!;
    public RelayCommand<ChecklistItemDto> ReopenChecklistItemCommand { get; private set; } = null!;
    public RelayCommand<ChecklistItemDto> DeleteChecklistItemCommand { get; private set; } = null!;

    // ─── Audit ──────────────────────────────────────────────────────────────
    public ObservableCollection<AuditEntryDto> AuditEntries { get; } = new();
    public ObservableCollection<AuditEntryDto> FilteredAudit { get; } = new();

    private string _auditSearch = string.Empty;
    public string AuditSearch
    {
        get => _auditSearch;
        set { SetProperty(ref _auditSearch, value); ApplyAuditFilter(); }
    }

    // ─── Init ────────────────────────────────────────────────────────────────
    private void InitLedgerCommands()
    {
        AddCashbookEntryCommand = new AsyncRelayCommand(DoAddCashbookEntryAsync);
        DeleteCashbookEntryCommand = new RelayCommand<CashbookEntryDto>(dto => { _ = DoDeleteCashbookEntryAsync(dto!); });
        ToggleReconciledCommand = new RelayCommand<CashbookEntryDto>(dto => { _ = DoToggleReconciledAsync(dto!); });
        DownloadCashbookPdfCommand = new AsyncRelayCommand(DoDownloadCashbookPdfAsync);
        ClearCashbookFiltersCommand = new RelayCommand(() => { CashbookTypeFilter = "All"; CashbookSearch = string.Empty; CashbookUnreconciledOnly = false; });

        SeedChecklistCommand = new AsyncRelayCommand(DoSeedChecklistAsync);
        AddChecklistItemCommand = new AsyncRelayCommand(DoAddChecklistItemAsync);
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
    private async Task DoAddCashbookEntryAsync()
    {
        if (Estate is null) return;
        if (!Money.TryParse(NewEntryAmountText, out var amount) || amount <= 0)
        {
            StatusMessage = "Enter a valid amount greater than zero.";
            return;
        }
        try
        {
            await _apiClient.AddCashbookEntryAsync(Estate.EstateId, new CreateCashbookEntryRequest
            {
                EntryType = NewEntryType,
                Category = NewEntryCategory,
                Description = NewEntryDescription,
                Amount = amount,
                EntryDate = NewEntryDate
            });
            NewEntryAmountText = string.Empty;
            NewEntryDescription = string.Empty;
            NewEntryCategory = string.Empty;
            await LoadAsync(_estateId);
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
    }

    private async Task DoDeleteCashbookEntryAsync(CashbookEntryDto dto)
    {
        if (Estate is null) return;
        if (MessageBox.Show("Delete this cashbook entry?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        try
        {
            await _apiClient.DeleteCashbookEntryAsync(Estate.EstateId, dto.EntryId);
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
                         || (e.Category ?? "").Contains(CashbookSearch, StringComparison.OrdinalIgnoreCase)))
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

    private async Task DoAddChecklistItemAsync()
    {
        if (Estate is null || string.IsNullOrWhiteSpace(NewChecklistTask)) return;
        try
        {
            await _apiClient.AddChecklistItemAsync(Estate.EstateId, new CreateChecklistItemRequest { Title = NewChecklistTask });
            NewChecklistTask = string.Empty;
            await LoadAsync(_estateId);
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
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

    // ─── Audit ──────────────────────────────────────────────────────────────
    public async Task LoadAuditAsync()
    {
        if (Estate is null) return;
        try
        {
            var list = await _apiClient.GetAuditTrailAsync(Estate.EstateId, 200);
            AuditEntries.Clear();
            foreach (var a in list) AuditEntries.Add(a);
            ApplyAuditFilter();
        }
        catch { /* best-effort */ }
    }

    private void ApplyAuditFilter()
    {
        FilteredAudit.Clear();
        foreach (var a in AuditEntries
            .Where(a => string.IsNullOrWhiteSpace(AuditSearch)
                     || a.Action.Contains(AuditSearch, StringComparison.OrdinalIgnoreCase)
                     || (a.UserName ?? "").Contains(AuditSearch, StringComparison.OrdinalIgnoreCase)
                     || (a.Changes ?? "").Contains(AuditSearch, StringComparison.OrdinalIgnoreCase)))
        {
            FilteredAudit.Add(a);
        }
    }
}