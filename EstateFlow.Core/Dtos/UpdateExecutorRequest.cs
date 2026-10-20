namespace EstateFlow.Core.Dtos;

public class UpdateExecutorRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? IdNumber { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Role { get; set; }
    public string? MastersOffice { get; set; }
    public string? LettersOfExecutorshipRef { get; set; }
    public DateTime? LettersOfExecutorshipDate { get; set; }
}

public class UpsertAdvertisementRequest
{
    public string AdvertisementType { get; set; } = "Section29";
    public DateTime? GazetteDate { get; set; }
    public string? GazetteNumber { get; set; }
    public string? NewspaperName { get; set; }
    public DateTime? NewspaperDate { get; set; }
    public DateTime? ClaimDeadline { get; set; }
    public bool ObjectionsReceived { get; set; }
    public string? Notes { get; set; }
}