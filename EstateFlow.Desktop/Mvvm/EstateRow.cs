using EstateFlow.Core.Dtos;

namespace EstateFlow.Desktop.Mvvm;

// Display wrapper around EstateSummaryDto used by the Dashboard and Estates screens.
public class EstateRow
{
    public EstateRow(EstateSummaryDto dto)
    {
        EstateId = dto.EstateId;
        FileNumber = dto.FileNumber;
        DeceasedName = string.IsNullOrWhiteSpace(dto.DeceasedFullName) ? "Unnamed estate" : dto.DeceasedFullName!;
        Status = string.IsNullOrWhiteSpace(dto.EstateStatus) ? "Unknown" : dto.EstateStatus;
        Controller = dto.ControllerName ?? string.Empty;
        OpenDiary = dto.OpenDiaryCount;
        DateOfDeath = dto.DateOfDeath;

        (StatusColor, StatusTint) = Status switch
        {
            "Reported" => ("#9C7A3C", "#F3ECDD"),
            "In Progress" => ("#6E2A2A", "#F0E3E3"),
            "Finalised" => ("#3F5B45", "#E2EBE4"),
            _ => ("#6B6E76", "#ECEAE5")
        };

        DiaryText = OpenDiary == 0 ? "None open" : $"{OpenDiary} open";
        DiaryColor = OpenDiary == 0 ? "#9CA0A8" : "#9C7A3C";

        DateOfDeathText = DateOfDeath == default ? "–" : DateOfDeath.ToString("dd MMM yyyy");
        DaysText = BuildAge(DateOfDeath);

        var parts = DeceasedName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Initials = parts.Length == 0 ? "?" :
            parts.Length == 1 ? parts[0][..1].ToUpperInvariant()
            : (parts[0][..1] + parts[^1][..1]).ToUpperInvariant();

        SearchBlob = string.Join(' ', FileNumber, DeceasedName, Status, Controller,
            DateOfDeath.ToString("dd MMM yyyy"), DateOfDeath.ToString("yyyy-MM-dd")).ToLowerInvariant();
    }

    public Guid EstateId { get; }
    public string FileNumber { get; }
    public string DeceasedName { get; }
    public string Status { get; }
    public string Controller { get; }
    public int OpenDiary { get; }
    public DateTime DateOfDeath { get; }

    public string StatusColor { get; }
    public string StatusTint { get; }
    public string DiaryText { get; }
    public string DiaryColor { get; }
    public string DateOfDeathText { get; }
    public string DaysText { get; }
    public string Initials { get; }
    public string SearchBlob { get; }

    private static string BuildAge(DateTime d)
    {
        if (d == default) return string.Empty;
        var days = (DateTime.Today - d.Date).Days;
        if (days < 0) return string.Empty;
        if (days == 0) return "today";
        if (days < 31) return $"{days} day{(days == 1 ? "" : "s")} ago";
        if (days < 365) { var m = days / 30; return $"{m} month{(m == 1 ? "" : "s")} ago"; }
        var y = days / 365; return $"{y} year{(y == 1 ? "" : "s")} ago";
    }
}

public class StatusChip : BaseViewModel
{
    public string Name { get; init; } = string.Empty;

    private int _count;
    public int Count { get => _count; set => SetProperty(ref _count, value); }

    private bool _isActive;
    public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }
}

public class StatusBar
{
    public string Name { get; init; } = string.Empty;
    public int Count { get; init; }
    public double Percent { get; init; }
    public string Color { get; init; } = "#6B6E76";
}
