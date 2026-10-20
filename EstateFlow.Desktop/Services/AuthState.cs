namespace EstateFlow.Desktop.Services;

public class AuthState : IAuthState
{
    public string? Token { get; set; }
    public string? FullName { get; set; }
    public string? Role { get; set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);
}