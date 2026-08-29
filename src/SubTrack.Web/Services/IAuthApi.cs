namespace SubTrack.Web.Services;

public record AuthResult(bool Success, string? Token, string? Email, string? ErrorMessage);

/// <summary>
/// Thin wrapper over the generated <c>SubTrackApiClient</c>'s auth operations — the seam
/// that lets Login/Register be tested with a fake instead of a real HTTP call.
/// </summary>
public interface IAuthApi
{
    Task<AuthResult> RegisterAsync(string email, string password);
    Task<AuthResult> LoginAsync(string email, string password);
}
