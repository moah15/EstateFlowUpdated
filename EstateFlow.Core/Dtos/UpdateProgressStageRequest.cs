namespace EstateFlow.Core.Dtos;

public class UpdateProgressStageRequest
{
    public Guid StageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public DateTime? TargetDate { get; set; }
    public string Status { get; set; } = "InProgress";
}
