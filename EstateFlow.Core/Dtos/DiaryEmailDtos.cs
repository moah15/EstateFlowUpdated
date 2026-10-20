namespace EstateFlow.Core.Dtos;

public class SendDiaryEmailRequest
{
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
}

public class DiaryEmailFunctionRequest
{
    public Guid EstateId { get; set; }
    public Guid DiaryId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
}
