using MudBlazor;

namespace SubTrack.Web.Theme;

/// <summary>Dark palette matching the SubTrack design reference — the app's only theme.</summary>
public static class SubTrackTheme
{
    public static MudTheme Theme { get; } = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = "#3b82f6",
            Secondary = "#a3a3a3",
            Background = "#0a0a0a",
            Surface = "#161616",
            AppbarBackground = "#141414",
            DrawerBackground = "#141414",
            TextPrimary = "#f5f5f5",
            TextSecondary = "#a3a3a3",
            LinesDefault = "#2a2a2a",
            Divider = "#2a2a2a",
            TableLines = "#2a2a2a",
            ActionDefault = "#a3a3a3",
            Error = "#ef4444",
            ErrorDarken = "#7f1d1d",
            Warning = "#f59e0b",
            WarningDarken = "#78350f",
            Success = "#22c55e",
            SuccessDarken = "#14532d",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px",
        },
    };
}
