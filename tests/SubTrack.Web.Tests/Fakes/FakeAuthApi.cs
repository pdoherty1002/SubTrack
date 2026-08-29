using SubTrack.Web.Services;

namespace SubTrack.Web.Tests.Fakes;

/// <summary>A fake <see cref="IAuthApi"/> returning a canned result, so Login/Register can be tested without real HTTP.</summary>
public class FakeAuthApi : IAuthApi
{
    public AuthResult LoginResult { get; set; } = new(true, "header.payload.signature", "user@example.com", null);
    public AuthResult RegisterResult { get; set; } = new(true, "header.payload.signature", "user@example.com", null);

    public Task<AuthResult> LoginAsync(string email, string password) => Task.FromResult(LoginResult);

    public Task<AuthResult> RegisterAsync(string email, string password) => Task.FromResult(RegisterResult);
}
