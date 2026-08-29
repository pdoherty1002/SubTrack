using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace SubTrack.Web.Services;

/// <summary>
/// Builds Blazor's <see cref="AuthenticationState"/> from whatever token is currently in
/// <see cref="TokenStore"/>. An expired token is treated the same as no token at all.
/// </summary>
public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly TokenStore _tokenStore;
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public JwtAuthStateProvider(TokenStore tokenStore)
    {
        _tokenStore = tokenStore;
        _tokenStore.OnChange += () => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = _tokenStore.Token;
        if (string.IsNullOrEmpty(token))
            return Task.FromResult(new AuthenticationState(Anonymous));

        var claims = JwtParser.ParseClaims(token);
        var expiry = JwtParser.GetExpiry(claims);
        if (expiry is null || expiry <= DateTimeOffset.UtcNow)
            return Task.FromResult(new AuthenticationState(Anonymous));

        var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }
}
