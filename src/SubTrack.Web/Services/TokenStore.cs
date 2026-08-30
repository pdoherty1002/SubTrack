using Microsoft.JSInterop;

namespace SubTrack.Web.Services;

/// <summary>
/// Holds the current JWT. The in-memory field is the source of truth for the running
/// session; sessionStorage is written to (never localStorage) purely so a page refresh
/// doesn't force a re-login — it's cleared automatically when the tab closes.
/// </summary>
public class TokenStore(IJSRuntime jsRuntime)
{
    private const string StorageKey = "subtrack.token";

    public string? Token { get; private set; }

    public event Action? OnChange;

    /// <summary>Restores the token from sessionStorage on app startup, if one is there.</summary>
    public async Task InitializeAsync()
    {
        try
        {
            Token = await jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);
        }
        catch (JSException)
        {
            Token = null;
        }
    }

    public async Task SetTokenAsync(string token)
    {
        Token = token;
        await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", StorageKey, token);
        OnChange?.Invoke();
    }

    public async Task ClearAsync()
    {
        Token = null;
        await jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", StorageKey);
        OnChange?.Invoke();
    }
}
