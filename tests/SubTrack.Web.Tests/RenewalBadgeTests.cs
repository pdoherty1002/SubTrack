using MudBlazor;
using SubTrack.Web.Shared;

namespace SubTrack.Web.Tests;

public class RenewalBadgeTests
{
    [Theory]
    [InlineData(-1, Color.Error)]
    [InlineData(0, Color.Error)]
    [InlineData(1, Color.Error)]
    [InlineData(2, Color.Error)]
    [InlineData(3, Color.Warning)]
    [InlineData(7, Color.Warning)]
    [InlineData(8, Color.Success)]
    [InlineData(30, Color.Success)]
    public void ColorFor_MapsDaysUntilRenewalToTheDesignReferenceThresholds(int daysUntil, Color expected)
    {
        Assert.Equal(expected, RenewalBadge.ColorFor(daysUntil));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, false)]
    [InlineData(-1, false)]
    public void IsUrgent_OnlyTrueForOneOrTwoDaysOut(int daysUntil, bool expected)
    {
        Assert.Equal(expected, RenewalBadge.IsUrgent(daysUntil));
    }
}
