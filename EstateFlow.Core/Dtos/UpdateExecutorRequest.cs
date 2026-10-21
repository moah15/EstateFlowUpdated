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

