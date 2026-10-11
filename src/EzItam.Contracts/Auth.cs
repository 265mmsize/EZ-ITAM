namespace EzItam.Contracts;

public sealed class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class TokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string[] Roles { get; init; } = Array.Empty<string>();
}

public sealed class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
