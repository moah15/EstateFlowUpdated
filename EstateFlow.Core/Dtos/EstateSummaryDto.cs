namespace EstateFlow.Core.Dtos;

public class EstateSummaryDto
{
    public Guid EstateId { get; set; }
    public string FileNumber { get; set; } = string.Empty;
    public string? DeceasedFullName { get; set; }
    public string EstateStatus { get; set; } = string.Empty;
    public DateTime DateOfDeath { get; set; }
    public string? ControllerName { get; set; }
    public int OpenDiaryCount { get; set; }
}