namespace EstateFlow.Core.Models;

public class Deceased
{
    public Guid DeceasedId { get; set; } = Guid.NewGuid();
    public Guid EstateId { get; set; }
    public Estate? Estate { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string? MaritalStatus { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? Surname { get; set; }
    public string? PlaceOfDeath { get; set; }
    public string? MatrimonialRegime { get; set; }
    public string? SpouseFullName { get; set; }
    public string? SpouseIdNumber { get; set; }
    public DateTime? WillDate { get; set; }
    public string? WillLocation { get; set; }
    // Note: date of death lives on Estate; residency/address fields
    // deliberately omitted here per client requirement F/G (see doc pain point G)
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool LivedInSouthAfrica { get; set; } = true;
    public DateTime? CodicilDate { get; set; }
}