namespace EstateFlow.Core.Dtos;

public class CreateDiaryEntryRequest
{
    public DateTime DueDate { get; set; }
    public string Note { get; set; } = string.Empty;
    public string NotifyBy { get; set; } = "Email";
}

public class UpdateDiaryEntryStatusRequest
{
    public bool IsCompleted { get; set; }
}