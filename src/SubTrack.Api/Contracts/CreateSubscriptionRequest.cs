using System.ComponentModel.DataAnnotations;
using SubTrack.Domain.Entities;

namespace SubTrack.Api.Contracts;

/// <summary>What a user submits to start tracking a new subscription.</summary>
/// <param name="Name">The subscription's display name (e.g. "Netflix").</param>
/// <param name="Cost">The amount charged per billing cycle.</param>
/// <param name="BillingCycle">How often this subscription renews.</param>
/// <param name="NextRenewalDate">The next date this subscription is due to renew.</param>
/// <param name="SubscriptionType">The type of subscription created by the user.</param>
/// <param name="ReminderEnabled">Whether the user wants to be reminded before this subscription renews. Defaults to true.</param>
public record CreateSubscriptionRequest(
    [Required, MinLength(1)] string Name,
    [Range(0.01, 100000)] decimal Cost,
    BillingCycle BillingCycle,
    DateOnly NextRenewalDate,
    SubscriptionType SubscriptionType,
    bool ReminderEnabled = true
);
