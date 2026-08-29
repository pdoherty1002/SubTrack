using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace SubTrack.Web.Tests.Fakes;

/// <summary>An always-authenticated <see cref="AuthenticationStateProvider"/> with a fixed email claim.</summary>
public class FakeAuthStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity([new Claim("email", "user@example.com")], authenticationType: "test");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }
}
