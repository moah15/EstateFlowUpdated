namespace EstateFlow.Core.Dtos;

public class ReportSettingDto
{
    public Guid EstateId { get; set; }
    public string FileNumber { get; set; } = string.Empty;
    public string? DeceasedFullName { get; set; }
    public DateTime? LastSentDate { get; set; }
    public bool IsPaused { get; set; }
}

public class ToggleReportPauseRequest
{
    public bool IsPaused { get; set; }
}