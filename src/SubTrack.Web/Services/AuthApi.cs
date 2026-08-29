using SubTrack.Web.ApiClient;

namespace SubTrack.Web.Services;

public class AuthApi(ISubTrackApiClient client) : IAuthApi
{
    public async Task<AuthResult> RegisterAsync(string email, string password)
    {
        try
        {
            var response = await client.RegisterAsync(new RegisterRequest { Email = email, Password = password });
            return new AuthResult(true, response.Token, response.Email, null);
        }
        catch (ApiException ex) when (ex.StatusCode == 409)
        {
            return new AuthResult(false, null, null, "An account with this email already exists.");
        }
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            var response = await client.LoginAsync(new LoginRequest { Email = email, Password = password });
            return new AuthResult(true, response.Token, response.Email, null);
        }
        catch (ApiException ex) when (ex.StatusCode == 401)
        {
            return new AuthResult(false, null, null, "Invalid email or password.");
        }
    }
}
