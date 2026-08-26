using Microsoft.EntityFrameworkCore;
using SubTrack.Domain.Entities;
using SubTrack.Domain.Interfaces;
using SubTrack.Infrastructure.Persistence;

namespace SubTrack.Api.Services;

/// <summary>
/// The daily background job's business logic: rolls overdue subscriptions forward to
/// their next renewal, and sends a reminder to anyone whose subscription renews tomorrow.
/// </summary>
/// <remarks>
/// Queries <see cref="AppDbContext"/> directly rather than through <see cref="IRepository{T,TKey}"/>,
/// since it needs to reason across every user's subscriptions at once (find everyone
/// renewing tomorrow, group by user) rather than one user's data at a time.
/// </remarks>
public class ReminderService
{
    private readonly AppDbContext _context;
    private readonly IClock _clock;
    private readonly INotificationSender _notificationSender;

    /// <summary>Creates the service, given the database context, clock, and notification sender (supplied by dependency injection).</summary>
    public ReminderService(AppDbContext context, IClock clock, INotificationSender notificationSender)
    {
        _context = context;
        _clock = clock;
        _notificationSender = notificationSender;
    }

    /// <summary>
    /// Runs the full daily cycle: first rolls forward any subscription whose renewal
    /// date has arrived or passed, then sends reminders for whatever renews tomorrow.
    /// </summary>
    public async Task ProcessDailyRemindersAsync()
    {
        var today = _clock.Today;

        await RollOverDueSubscriptionsAsync(today);
        await SendTomorrowRemindersAsync(today);
    }

    // Advances every overdue subscription to its next future renewal date, one billing
    // cycle at a time — looping in case several cycles were missed (e.g. the job didn't
    // run for a few months).
    private async Task RollOverDueSubscriptionsAsync(DateOnly today)
    {
        var dueSubscriptions = await _context.Subscriptions
            .Where(s => s.NextRenewalDate <= today)
            .ToListAsync();

        if (dueSubscriptions.Count == 0)
            return;

        foreach (var subscription in dueSubscriptions)
        {
            while (subscription.NextRenewalDate <= today)
            {
                subscription.NextRenewalDate = subscription.BillingCycle == BillingCycle.Monthly
                    ? subscription.NextRenewalDate.AddMonths(1)
                    : subscription.NextRenewalDate.AddYears(1);
            }
        }

        await _context.SaveChangesAsync();
    }

    // Finds every reminder-enabled subscription renewing tomorrow and notifies its
    // owner once, alongside every other reminder-enabled subscription that user has.
    private async Task SendTomorrowRemindersAsync(DateOnly today)
    {
        var tomorrow = today.AddDays(1);

        var dueTomorrow = await _context.Subscriptions
            .Where(s => s.NextRenewalDate == tomorrow && s.ReminderEnabled)
            .ToListAsync();

        if (dueTomorrow.Count == 0)
            return;

        var affectedUserIds = dueTomorrow.Select(s => s.UserId).Distinct().ToList();

        var users = await _context.Users
            .Where(u => affectedUserIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        // Every reminder-enabled subscription belonging to an affected user — used to
        // build each user's "also coming up" list. Subscriptions with ReminderEnabled
        // false are excluded here, so they can never appear in anyone's digest.
        var reminderEnabledByUser = await _context.Subscriptions
            .Where(s => affectedUserIds.Contains(s.UserId) && s.ReminderEnabled)
            .ToListAsync();

        var subscriptionsByUser = reminderEnabledByUser
            .GroupBy(s => s.UserId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var subscription in dueTomorrow)
        {
            var user = users[subscription.UserId];
            var dueInfo = new SubscriptionRenewalInfo(subscription, DaysRemaining(subscription.NextRenewalDate, today));

            var others = subscriptionsByUser[subscription.UserId]
                .Where(s => s.Id != subscription.Id)
                .Select(s => new SubscriptionRenewalInfo(s, DaysRemaining(s.NextRenewalDate, today)))
                .ToList();

            await _notificationSender.SendReminderEmailAsync(user, dueInfo, others);
        }
    }

    private static int DaysRemaining(DateOnly renewalDate, DateOnly today) => renewalDate.DayNumber - today.DayNumber;
}
