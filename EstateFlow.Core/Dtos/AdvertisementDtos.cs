namespace EstateFlow.Core.Dtos;

public class AdvertisementDto
{
    public Guid AdvertisementId { get; set; }
    public string AdvertisementType { get; set; } = string.Empty;
    public string? GazetteNumber { get; set; }
    public DateTime? GazetteDate { get; set; }
    public string? NewspaperName { get; set; }
    public DateTime? NewspaperDate { get; set; }
    public DateTime? ClaimDeadline { get; set; }
    public bool ObjectionsReceived { get; set; }
    public string? Notes { get; set; }
}

public class UpsertAdvertisementRequest
{
    /// <summary>"Section29" or "Section35"</summary>
    public string AdvertisementType { get; set; } = string.Empty;

    public string? GazetteNumber { get; set; }
    public DateTime? GazetteDate { get; set; }
    public string? NewspaperName { get; set; }
    public DateTime? NewspaperDate { get; set; }
    public DateTime? ClaimDeadline { get; set; }
    public bool ObjectionsReceived { get; set; }
    public string? Notes { get; set; }
}
