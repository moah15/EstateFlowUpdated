namespace EstateFlow.Core.Models;

public class EstateController
{
    public Guid ControllerId { get; set; } = Guid.NewGuid();
    public string StaffName { get; set; } = string.Empty;
    public string Role { get; set; } = "EstateController"; // EstateController | FirmAdministrator
    public string Email { get; set; } = string.Empty;

    public ICollection<Estate> AssignedEstates { get; set; } = new List<Estate>();
}