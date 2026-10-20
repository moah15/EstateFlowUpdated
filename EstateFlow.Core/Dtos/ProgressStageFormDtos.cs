namespace EstateFlow.Core.Dtos;

public class CreateProgressStageRequest
{
    public string StageName { get; set; } = string.Empty;
    public DateTime? TargetDate { get; set; }
    public string Status { get; set; } = "InProgress";
}
