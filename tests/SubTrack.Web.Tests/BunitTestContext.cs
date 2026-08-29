using Bunit;

namespace SubTrack.Web.Tests;

/// <summary>
/// bUnit's synchronous teardown can throw for MudBlazor's internal services that only
/// implement <see cref="IAsyncDisposable"/> (e.g. KeyInterceptorService) — a known
/// bUnit/MudBlazor test-harness limitation, not an app bug: real Blazor hosts dispose
/// these asynchronously without issue. Swallowed here so it doesn't fail otherwise-passing tests.
/// </summary>
public abstract class BunitTestContext : BunitContext
{
    protected override void Dispose(bool disposing)
    {
        try
        {
            base.Dispose(disposing);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("IAsyncDisposable"))
        {
        }
    }
}
