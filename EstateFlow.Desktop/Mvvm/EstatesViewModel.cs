using System.Collections.ObjectModel;
using System.Windows.Input;
using EstateFlow.Desktop.Services;

namespace EstateFlow.Desktop.Mvvm;

public class EstatesViewModel : BaseViewModel
{
    private const string AllStatuses = "All";

    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigation;
    private List<EstateRow> _all = new();

    public ObservableCollection<EstateRow> Estates { get; } = new();
    public ObservableCollection<StatusChip> StatusChips { get; } = new();

    public List<string> SortOptions { get; } = new()
    {
        "Newest death first",
        "Oldest death first",
        "File number",
        "Deceased name (A–Z)",
        "Most open diary items"
    };

    private string _sortOption = "Newest death first";
    public string SortOption
    {
        get => _sortOption;
        set { if (SetProperty(ref _sortOption, value)) ApplyFilter(); }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) ApplyFilter(); }
    }

    private string _selectedStatus = AllStatuses;

    private bool _isLoading;
    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

    private string _statusMessage = string.Empty;
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

    private string _resultSummary = string.Empty;
    public string ResultSummary { get => _resultSummary; set => SetProperty(ref _resultSummary, value); }

    private bool _isEmpty;
    public bool IsEmpty { get => _isEmpty; set => SetProperty(ref _isEmpty, value); }

    public ICommand LoadEstatesCommand { get; }
    public ICommand OpenEstateCommand { get; }
    public ICommand NewEstateCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand SelectStatusCommand { get; }

    public EstatesViewModel(IApiClient apiClient, INavigationService navigation)
    {
        _apiClient = apiClient;
        _navigation = navigation;

        LoadEstatesCommand = new AsyncRelayCommand(async _ => await LoadEstatesAsync());
        OpenEstateCommand = new RelayCommand(p => { if (p is Guid id) _navigation.NavigateToEstateDetail(id); });
        NewEstateCommand = new RelayCommand(_ => _navigation.NavigateToNewEstate());
        ClearSearchCommand = new RelayCommand(_ => SearchText = string.Empty);
        SelectStatusCommand = new RelayCommand(p =>
        {
            _selectedStatus = p as string ?? AllStatuses;
            foreach (var c in StatusChips) c.IsActive = c.Name == _selectedStatus;
            ApplyFilter();
        });

        _ = LoadEstatesAsync();
    }

    private async Task LoadEstatesAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading estates...";
        try
        {
            var estates = await _apiClient.GetEstatesAsync();
            _all = estates.Select(e => new EstateRow(e)).ToList();
            BuildChips();
            StatusMessage = string.Empty;
            ApplyFilter();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not load estates: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void BuildChips()
    {
        StatusChips.Clear();
        StatusChips.Add(new StatusChip { Name = AllStatuses, Count = _all.Count, IsActive = _selectedStatus == AllStatuses });
        foreach (var g in _all.GroupBy(e => e.Status).OrderBy(g => g.Key))
            StatusChips.Add(new StatusChip { Name = g.Key, Count = g.Count(), IsActive = _selectedStatus == g.Key });

        if (!StatusChips.Any(c => c.IsActive))
        {
            _selectedStatus = AllStatuses;
            StatusChips[0].IsActive = true;
        }
    }

    private void ApplyFilter()
    {
        IEnumerable<EstateRow> q = _all;

        if (_selectedStatus != AllStatuses)
            q = q.Where(e => e.Status == _selectedStatus);

        var tokens = (SearchText ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.ToLowerInvariant())
            .ToList();

        foreach (var t in tokens)
            q = q.Where(e => e.SearchBlob.Contains(t));

        q = SortOption switch
        {
            "Oldest death first" => q.OrderBy(e => e.DateOfDeath),
            "File number" => q.OrderBy(e => e.FileNumber, StringComparer.OrdinalIgnoreCase),
            "Deceased name (A–Z)" => q.OrderBy(e => e.DeceasedName, StringComparer.OrdinalIgnoreCase),
            "Most open diary items" => q.OrderByDescending(e => e.OpenDiary).ThenBy(e => e.DeceasedName),
            _ => q.OrderByDescending(e => e.DateOfDeath)
        };

        var result = q.ToList();

        Estates.Clear();
        foreach (var e in result) Estates.Add(e);

        IsEmpty = !IsLoading && result.Count == 0;
        ResultSummary = _all.Count == 0
            ? string.Empty
            : result.Count == _all.Count
                ? $"{_all.Count} estate{(_all.Count == 1 ? "" : "s")}"
                : $"{result.Count} of {_all.Count} estates";
    }
}
