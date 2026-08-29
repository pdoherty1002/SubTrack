using SubTrack.Web.ApiClient;

namespace SubTrack.Web.Services;

public class SubscriptionsApi(ISubTrackApiClient client) : ISubscriptionsApi
{
    public async Task<IReadOnlyList<SubscriptionDto>> GetAllAsync()
    {
        var result = await client.SubscriptionsAllAsync();
        return result.ToList();
    }

    public Task<SubscriptionDto> CreateAsync(CreateSubscriptionRequest request) =>
        client.SubscriptionsPOSTAsync(request);

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await client.SubscriptionsDELETEAsync(id);
            return true;
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return false;
        }
    }
}
