namespace EstateFlow.Core.Models;

public class EstateAdvertisement
{
    public Guid AdvertisementId { get; set; } = Guid.NewGuid();
    public Guid EstateId { get; set; }
    public Estate? Estate { get; set; }
    public string AdvertisementType { get; set; } = "Section29"; // or "Section35"
    public DateTime? GazetteDate { get; set; }
    public string? GazetteNumber { get; set; }
    public string? NewspaperName { get; set; }
    public DateTime? NewspaperDate { get; set; }
    public DateTime? ClaimDeadline { get; set; }
    public bool ObjectionsReceived { get; set; }
    public string? Notes { get; set; }
}