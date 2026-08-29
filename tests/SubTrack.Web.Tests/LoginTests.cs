using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using SubTrack.Web.Pages;
using SubTrack.Web.Services;
using SubTrack.Web.Tests.Fakes;

namespace SubTrack.Web.Tests;

public class LoginTests : BunitTestContext
{
    private readonly FakeAuthApi _authApi = new();

    public LoginTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IAuthApi>(_authApi);
        Services.AddSingleton<TokenStore>();
        Services.AddMudServices();
    }

    [Fact]
    public void RendersEmailAndPasswordFields()
    {
        var cut = Render<Login>();

        Assert.Contains("Email", cut.Markup);
        Assert.Contains("Password", cut.Markup);
        Assert.Contains("Log in", cut.Markup);
    }

    [Fact]
    public void FailedLogin_ShowsInlineErrorAndDoesNotNavigate()
    {
        _authApi.LoginResult = new AuthResult(false, null, null, "Invalid email or password.");
        var cut = Render<Login>();
        var nav = Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
        var startingUri = nav.Uri;

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Assert.Contains("Invalid email or password.", cut.Markup));
        Assert.Equal(startingUri, nav.Uri);
    }

    [Fact]
    public void SuccessfulLogin_StoresTokenAndNavigatesToDashboard()
    {
        _authApi.LoginResult = new AuthResult(true, "header.payload.signature", "user@example.com", null);
        var cut = Render<Login>();
        var tokenStore = Services.GetRequiredService<TokenStore>();
        var nav = Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Assert.Equal("header.payload.signature", tokenStore.Token));
        Assert.EndsWith("/", nav.Uri);
    }
}
