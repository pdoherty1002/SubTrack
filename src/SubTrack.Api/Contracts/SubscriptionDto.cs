using SubTrack.Domain.Entities;

namespace SubTrack.Api.Contracts;

/// <summary>
/// A subscription, as exposed by the API. Maps to a
/// <see cref="SubTrack.Domain.Entities.Subscription"/> entity.
/// </summary>
/// <param name="Id">The subscription's database identifier.</param>
/// <param name="Name">The subscription's display name (e.g. "Netflix").</param>
/// <param name="Cost">The amount charged per billing cycle.</param>
/// <param name="BillingCycle">How often this subscription renews.</param>
/// <param name="NextRenewalDate">The next date this subscription is due to renew.</param>
/// <param name="ReminderEnabled">Whether the user wants to be reminded before this subscription renews.</param>
/// <param name="SubscriptionType">The category this subscription belongs to.</param>
public record SubscriptionDto(
    int Id,
    string Name,
    decimal Cost,
    BillingCycle BillingCycle,
    DateOnly NextRenewalDate,
    bool ReminderEnabled,
    SubscriptionType SubscriptionType
);
