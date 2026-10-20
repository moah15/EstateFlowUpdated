namespace EstateFlow.Core.Dtos;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public bool RequiresTwoFactor { get; set; }
    public string? Token { get; set; }
    public string? FullName { get; set; }
    public string? Role { get; set; }
}

public class VerifyTwoFactorRequest
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "EstateController"; // EstateController | FirmAdministrator
}

public class TwoFactorSetupResponse
{
    public string SharedKey { get; set; } = string.Empty;
    public string AuthenticatorUri { get; set; } = string.Empty;
}

public class EnableTwoFactorRequest
{
    public string Code { get; set; } = string.Empty;
}