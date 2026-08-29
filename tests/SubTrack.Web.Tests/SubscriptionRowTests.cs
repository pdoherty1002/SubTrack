using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using SubTrack.Web.Components;
using SubTrack.Web.Services;
using SubTrack.Web.Tests.Fakes;

namespace SubTrack.Web.Tests;

public class SubscriptionRowTests : BunitTestContext
{
    private readonly FakeSubscriptionsApi _api = new();
    private readonly SubscriptionStateService _state;

    public SubscriptionRowTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        _state = new SubscriptionStateService(_api);
        Services.AddSingleton(_state);
        Services.AddMudServices();
    }

    [Theory]
    [InlineData(2, "mud-chip-color-error")]
    [InlineData(5, "mud-chip-color-warning")]
    [InlineData(20, "mud-chip-color-success")]
    public void RendersBadgeColor_MatchingDaysUntilRenewal(int daysUntil, string expectedCssFragment)
    {
        var subscription = SampleData.Subscription(id: 1, name: "Netflix", daysUntilRenewal: daysUntil);

        var cut = Render<SubscriptionRow>(p => p.Add(x => x.Subscription, subscription));

        Assert.Contains(expectedCssFragment, cut.Markup);
    }

    [Fact]
    public async Task TogglingReminderSwitch_UpdatesLocalStateOnly()
    {
        var subscription = SampleData.Subscription(id: 1, name: "Netflix", daysUntilRenewal: 10, reminderEnabled: false);
        _api.Subscriptions = [subscription];
        await _state.LoadAsync();

        var cut = Render<SubscriptionRow>(p => p.Add(x => x.Subscription, _state.Subscriptions[0]));

        var toggle = cut.Find("input[type=checkbox]");
        await toggle.ChangeAsync(true);

        Assert.True(_state.Subscriptions[0].ReminderEnabled);
    }

    [Fact]
    public async Task DeleteButton_OpensConfirmDialog_BeforeRemovingAnything()
    {
        var subscription = SampleData.Subscription(id: 1, name: "Netflix", daysUntilRenewal: 10);
        _api.Subscriptions = [subscription];
        await _state.LoadAsync();

        Render<MudBlazor.MudPopoverProvider>();
        var dialogProvider = Render<MudBlazor.MudDialogProvider>();
        var cut = Render<SubscriptionRow>(p => p.Add(x => x.Subscription, _state.Subscriptions[0]));

        cut.Find("button").Click();

        dialogProvider.WaitForAssertion(() => Assert.Contains("Remove Netflix?", dialogProvider.Markup));
        Assert.Single(_state.Subscriptions);
    }
}
