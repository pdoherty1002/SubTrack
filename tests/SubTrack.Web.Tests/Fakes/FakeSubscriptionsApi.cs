using SubTrack.Web.ApiClient;
using SubTrack.Web.Services;

namespace SubTrack.Web.Tests.Fakes;

/// <summary>A fake <see cref="ISubscriptionsApi"/> backed by an in-memory list, for testing state/components without real HTTP.</summary>
public class FakeSubscriptionsApi : ISubscriptionsApi
{
    public List<SubscriptionDto> Subscriptions { get; set; } = [];

    public Task<IReadOnlyList<SubscriptionDto>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<SubscriptionDto>>(Subscriptions.ToList());

    public Task<SubscriptionDto> CreateAsync(CreateSubscriptionRequest request)
    {
        var created = new SubscriptionDto
        {
            Id = Subscriptions.Count + 1,
            Name = request.Name,
            Cost = request.Cost,
            BillingCycle = request.BillingCycle,
            NextRenewalDate = request.NextRenewalDate,
            ReminderEnabled = request.ReminderEnabled,
            SubscriptionType = request.SubscriptionType,
        };
        Subscriptions.Add(created);
        return Task.FromResult(created);
    }

    public Task<bool> DeleteAsync(int id) => Task.FromResult(Subscriptions.RemoveAll(s => s.Id == id) > 0);
}
