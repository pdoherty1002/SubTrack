using MudBlazor;

namespace SubTrack.Web.Shared;

/// <summary>Maps days-until-renewal to the badge color thresholds from the design reference.</summary>
public static class RenewalBadge
{
    public static Color ColorFor(int daysUntil) => daysUntil switch
    {
        <= 2 => Color.Error,
        <= 7 => Color.Warning,
        _ => Color.Success,
    };

    public static bool IsUrgent(int daysUntil) => daysUntil is >= 1 and <= 2;

    public static string Label(int daysUntil) => daysUntil switch
    {
        < 0 => "overdue",
        0 => "today",
        1 => "1 day",
        _ => $"{daysUntil} days",
    };
}
