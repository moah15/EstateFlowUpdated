namespace EstateFlow.Core.Dtos;

public class DiaryEntryWithEstateDto
{
    public Guid DiaryId { get; set; }
    public Guid EstateId { get; set; }
    public string FileNumber { get; set; } = string.Empty;
    public string? DeceasedFullName { get; set; }
    public string Note { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public string NotifyBy { get; set; } = string.Empty;

    public string Urgency
    {
        get
        {
            if (IsCompleted) return "completed";
            var daysUntilDue = (DueDate.Date - DateTime.UtcNow.Date).TotalDays;
            if (daysUntilDue < 0) return "overdue";
            if (daysUntilDue <= 7) return "due-soon";
            return "normal";
        }
    }
}