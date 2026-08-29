using SubTrack.Web.ApiClient;

namespace SubTrack.Web.Services;

/// <summary>
/// Thin wrapper over the generated <c>SubTrackApiClient</c>'s subscription operations — the
/// seam that lets dashboard components be tested with a fake instead of a real HTTP call.
/// </summary>
public interface ISubscriptionsApi
{
    Task<IReadOnlyList<SubscriptionDto>> GetAllAsync();
    Task<SubscriptionDto> CreateAsync(CreateSubscriptionRequest request);
    Task<bool> DeleteAsync(int id);
}
