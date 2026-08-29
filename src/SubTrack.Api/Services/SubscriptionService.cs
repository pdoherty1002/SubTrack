using SubTrack.Api.Contracts;
using SubTrack.Domain.Entities;
using SubTrack.Domain.Interfaces;

namespace SubTrack.Api.Services;

/// <summary>
/// Owner-scoped CRUD for subscriptions — fetches and mutates <see cref="Subscription"/>
/// entities, always constrained to the calling user, and maps them to the API's
/// public <see cref="SubscriptionDto"/> shape.
/// </summary>
public class SubscriptionService
{
    private readonly IRepository<Subscription, int> _subscriptionRepo;

    /// <summary>Creates the service, given a subscription repository (supplied by dependency injection).</summary>
    public SubscriptionService(IRepository<Subscription, int> subscriptionRepo)
    {
        _subscriptionRepo = subscriptionRepo;
    }

    /// <summary>Fetches every subscription owned by the given user, sorted by next renewal date ascending.</summary>
    public async Task<IReadOnlyList<SubscriptionDto>> GetForUserAsync(Guid userId)
    {
        var subscriptions = await _subscriptionRepo.GetByConditionAsync(s => s.UserId == userId);

        return subscriptions
            .OrderBy(s => s.NextRenewalDate)
            .Select(ToDto)
            .ToList();
    }

    /// <summary>Creates a new subscription owned by the given user.</summary>
    /// <param name="userId">The Id of the owning user — taken from their token, never from the request body.</param>
    /// <param name="request">The subscription data the user submitted.</param>
    public async Task<SubscriptionDto> CreateAsync(Guid userId, CreateSubscriptionRequest request)
    {
        var subscription = new Subscription
        {
            UserId = userId,
            Name = request.Name,
            Cost = request.Cost,
            BillingCycle = request.BillingCycle,
            NextRenewalDate = request.NextRenewalDate,
            ReminderEnabled = request.ReminderEnabled,
            SubscriptionType = request.SubscriptionType
        };

        await _subscriptionRepo.AddAsync(subscription);
        await _subscriptionRepo.SaveChangesAsync();

        return ToDto(subscription);
    }

    /// <summary>
    /// Deletes a subscription, but only if it's owned by the given user. Returns false —
    /// not an error — both when the subscription doesn't exist and when it belongs to
    /// someone else, so callers can't use the difference to probe for other users' data.
    /// </summary>
    public async Task<bool> DeleteAsync(Guid userId, int id)
    {
        var subscription = await _subscriptionRepo.GetByIdAsync(id);

        if (subscription is null || subscription.UserId != userId)
            return false;

        _subscriptionRepo.Remove(subscription);
        return await _subscriptionRepo.SaveChangesAsync();
    }

    private static SubscriptionDto ToDto(Subscription s) =>
        new(s.Id, s.Name, s.Cost, s.BillingCycle, s.NextRenewalDate, s.ReminderEnabled, s.SubscriptionType);
}
