namespace EstateFlow.Api;

public static class AuditContext
{
    [ThreadStatic]
    private static string? _currentUser;

    public static string? CurrentUser
    {
        get => _currentUser;
        set => _currentUser = value;
    }
}