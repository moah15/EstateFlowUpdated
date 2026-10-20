namespace EstateFlow.Desktop.Services;

public interface IAuthState
{
    string? Token { get; set; }
    string? FullName { get; set; }
    string? Role { get; set; }
    bool IsAuthenticated { get; }
}