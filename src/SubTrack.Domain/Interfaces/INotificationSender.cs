using SubTrack.Domain.Entities;

namespace SubTrack.Domain.Interfaces;

/// <summary>A subscription paired with how many days remain until it renews.</summary>
/// <param name="Subscription">The subscription in question.</param>
/// <param name="DaysRemaining">The number of days between today and <see cref="Domain.Entities.Subscription.NextRenewalDate"/>.</param>
public record SubscriptionRenewalInfo(Subscription Subscription, int DaysRemaining);

/// <summary>
/// Sends renewal reminder notifications to users. The real implementation (email, push,
/// etc.) is wired up separately — this abstraction just lets <c>ReminderService</c> be
/// unit tested without sending anything for real.
/// </summary>
public interface INotificationSender
{
    /// <summary>
    /// Notifies a user that one of their subscriptions is renewing soon.
    /// </summary>
    /// <param name="user">The user to notify.</param>
    /// <param name="dueSubscription">The subscription that triggered this reminder (renewing tomorrow).</param>
    /// <param name="otherActiveSubscriptions">
    /// Every other reminder-enabled subscription this user has, so the notification can
    /// show a fuller picture of what's coming up.
    /// </param>
    Task SendReminderEmailAsync(
        ApplicationUser user,
        SubscriptionRenewalInfo dueSubscription,
        IReadOnlyList<SubscriptionRenewalInfo> otherActiveSubscriptions);
}
