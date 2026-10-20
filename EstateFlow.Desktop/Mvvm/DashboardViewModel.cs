using System.Collections.ObjectModel;
using System.Windows.Input;
using EstateFlow.Desktop.Services;

namespace EstateFlow.Desktop.Mvvm;

public class DashboardViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigation;

    public ObservableCollection<StatusBar> StatusBreakdown { get; } = new();
    public ObservableCollection<EstateRow> NeedsAttention { get; } = new();
    public ObservableCollection<EstateRow> RecentEstates { get; } = new();

    public string Greeting { get; }
    public string TodayText { get; } = DateTime.Now.ToString("dddd, d MMMM yyyy");

    private int _totalEstates;
    public int TotalEstates { get => _totalEstates; set => SetProperty(ref _totalEstates, value); }

    private int _activeEstates;
    public int ActiveEstates { get => _activeEstates; set => SetProperty(ref _activeEstates, value); }

    private int _finalisedEstates;
    public int FinalisedEstates { get => _finalisedEstates; set => SetProperty(ref _finalisedEstates, value); }

    private int _openDiaryTotal;
    public int OpenDiaryTotal { get => _openDiaryTotal; set => SetProperty(ref _openDiaryTotal, value); }

    private bool _hasAttention;
    public bool HasAttention { get => _hasAttention; set => SetProperty(ref _hasAttention, value); }

    private bool _hasEstates;
    public bool HasEstates { get => _hasEstates; set => SetProperty(ref _hasEstates, value); }

    private bool _isLoading;
    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

    private string _statusMessage = string.Empty;
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

    public ICommand LoadEstatesCommand { get; }
    public ICommand OpenEstateCommand { get; }
    public ICommand NewEstateCommand { get; }
    public ICommand ViewAllEstatesCommand { get; }
    public ICommand OpenDiaryCommand { get; }

    public DashboardViewModel(IApiClient apiClient, INavigationService navigation, IAuthState authState)
    {
        _apiClient = apiClient;
        _navigation = navigation;

        var hour = DateTime.Now.Hour;
        var salutation = hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening";
        var first = (authState.FullName ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        Greeting = string.IsNullOrWhiteSpace(first) ? salutation : $"{salutation}, {first}";

        LoadEstatesCommand = new AsyncRelayCommand(async _ => await LoadEstatesAsync());
        OpenEstateCommand = new RelayCommand(p => { if (p is Guid id) _navigation.NavigateToEstateDetail(id); });
        NewEstateCommand = new RelayCommand(_ => _navigation.NavigateToNewEstate());
        ViewAllEstatesCommand = new RelayCommand(_ => _navigation.NavigateToEstates());
        OpenDiaryCommand = new RelayCommand(_ => _navigation.NavigateToDiary());

        _ = LoadEstatesAsync();
    }

    private static string ColorFor(string status) => status switch
    {
        "Reported" => "#9C7A3C",
        "In Progress" => "#6E2A2A",
        "Finalised" => "#3F5B45",
        _ => "#6B6E76"
    };

    private async Task LoadEstatesAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading...";
        try
        {
            var rows = (await _apiClient.GetEstatesAsync()).Select(e => new EstateRow(e)).ToList();

            TotalEstates = rows.Count;
            FinalisedEstates = rows.Count(r => r.Status == "Finalised");
            ActiveEstates = TotalEstates - FinalisedEstates;
            OpenDiaryTotal = rows.Sum(r => r.OpenDiary);
            HasEstates = rows.Count > 0;

            StatusBreakdown.Clear();
            foreach (var g in rows.GroupBy(r => r.Status).OrderByDescending(g => g.Count()))
            {
                StatusBreakdown.Add(new StatusBar
                {
                    Name = g.Key,
                    Count = g.Count(),
                    Percent = rows.Count == 0 ? 0 : g.Count() * 100.0 / rows.Count,
                    Color = ColorFor(g.Key)
                });
            }

            NeedsAttention.Clear();
            foreach (var r in rows.Where(r => r.OpenDiary > 0 && r.Status != "Finalised")
                                  .OrderByDescending(r => r.OpenDiary).Take(6))
                NeedsAttention.Add(r);
            HasAttention = NeedsAttention.Count > 0;

            RecentEstates.Clear();
            foreach (var r in rows.OrderByDescending(r => r.DateOfDeath).Take(6))
                RecentEstates.Add(r);

            StatusMessage = rows.Count == 0 ? "No estates yet." : string.Empty;
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
}
