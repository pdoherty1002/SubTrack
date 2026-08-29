using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using SubTrack.Web.Pages;
using SubTrack.Web.Services;
using SubTrack.Web.Tests.Fakes;

namespace SubTrack.Web.Tests;

public class DashboardLayoutTests : BunitTestContext
{
    private readonly FakeSubscriptionsApi _api = new();

    public DashboardLayoutTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new SubscriptionStateService(_api));
        Services.AddSingleton<AuthenticationStateProvider, FakeAuthStateProvider>();
        Services.AddMudServices();
    }

    [Fact]
    public void SubscriptionListContainer_KeepsAFixedHeightRegardlessOfItemCount()
    {
        _api.Subscriptions = Enumerable.Range(1, 25)
            .Select(i => SampleData.Subscription(id: i, name: $"Sub {i}", daysUntilRenewal: i))
            .ToList();

        var cut = Render<Dashboard>();

        cut.WaitForAssertion(() => Assert.Contains("Sub 25", cut.Markup));

        // The left column and the scrollable list both carry a fixed height + overflow,
        // regardless of how many rows were just rendered — this is what stops the page
        // from growing taller as subscriptions are added.
        Assert.Contains("height: 560px", cut.Markup);
        Assert.Contains("subtrack-scroll-list", cut.Markup);
    }

    [Fact]
    public void UrgentBanner_OnlyRendersWhenSomethingRenewsWithinTwoDays()
    {
        _api.Subscriptions = [SampleData.Subscription(id: 1, name: "Netflix", daysUntilRenewal: 10)];

        var cut = Render<Dashboard>();

        cut.WaitForAssertion(() => Assert.Contains("Netflix", cut.Markup));
        Assert.DoesNotContain("renews in", cut.Markup);
    }
}
