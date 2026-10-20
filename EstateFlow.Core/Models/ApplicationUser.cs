using Microsoft.AspNetCore.Identity;

namespace EstateFlow.Core.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    // Links this login to the staff record used across the domain model,
    // for EstateController/FirmAdministrator roles. Null for Executor/Heir
    // accounts (those authenticate via the self-service portal in Part 10).
    public Guid? ControllerId { get; set; }
}