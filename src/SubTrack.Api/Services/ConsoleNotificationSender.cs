using SubTrack.Domain.Entities;
using SubTrack.Domain.Interfaces;

namespace SubTrack.Api.Services;

/// <summary>
/// Stub <see cref="INotificationSender"/> that just logs to the console. Real email
/// delivery (AWS SES or otherwise) is being wired up separately — this exists purely
/// so <see cref="ReminderService"/> has something to call in the meantime.
/// </summary>
public class ConsoleNotificationSender : INotificationSender
{
    /// <inheritdoc/>
    public Task SendReminderEmailAsync(
        ApplicationUser user,
        SubscriptionRenewalInfo dueSubscription,
        IReadOnlyList<SubscriptionRenewalInfo> otherActiveSubscriptions)
    {
        Console.WriteLine(
            $"[Reminder] {user.Email}: \"{dueSubscription.Subscription.Name}\" renews in {dueSubscription.DaysRemaining} day(s). " +
            $"Also coming up: {string.Join(", ", otherActiveSubscriptions.Select(o => $"{o.Subscription.Name} ({o.DaysRemaining}d)"))}");

        return Task.CompletedTask;
    }
}
