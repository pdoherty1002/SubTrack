using Microsoft.EntityFrameworkCore;
using Moq;
using SubTrack.Api.Services;
using SubTrack.Domain.Entities;
using SubTrack.Domain.Interfaces;
using SubTrack.Infrastructure.Persistence;
using SubTrack.Tests.Fakes;

namespace SubTrack.Tests.Services;

public class ReminderServiceTests
{
    // Builds a fresh, isolated in-memory AppDbContext for a single test.
    // A new Guid per call means each test gets its own empty "database" —
    // nothing left over from any other test, no cleanup needed between them.
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task ProcessDailyRemindersAsync_RollsOverSingleMissedCycle_Monthly()
    {
        // Arrange — renewal date was yesterday, one cycle overdue.
        var context = CreateContext();
        var user = new ApplicationUser { Email = "user@test.com", PasswordHash = "irrelevant" };
        context.Users.Add(user);

        var subscription = new Subscription
        {
            Id = 1,
            UserId = user.Id,
            Name = "Netflix",
            BillingCycle = BillingCycle.Monthly,
            NextRenewalDate = new DateOnly(2026, 8, 24),
            ReminderEnabled = false
        };
        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync();

        var clock = new FakeClock(new DateOnly(2026, 8, 25));
        var mockSender = new Mock<INotificationSender>();
        var service = new ReminderService(context, clock, mockSender.Object);

        // Act
        await service.ProcessDailyRemindersAsync();

        // Assert
        var updated = await context.Subscriptions.FirstAsync(s => s.Id == 1);
        Assert.Equal(new DateOnly(2026, 9, 24), updated.NextRenewalDate);
    }

    [Fact]
    public async Task ProcessDailyRemindersAsync_RollsOverMultipleMissedCycles_Monthly()
    {
        // Arrange — renewal date is three months in the past; the job "missed" three cycles.
        var context = CreateContext();
        var user = new ApplicationUser { Email = "user@test.com", PasswordHash = "irrelevant" };
        context.Users.Add(user);

        var subscription = new Subscription
        {
            Id = 1,
            UserId = user.Id,
            Name = "Netflix",
            BillingCycle = BillingCycle.Monthly,
            NextRenewalDate = new DateOnly(2026, 5, 25),
            ReminderEnabled = false
        };
        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync();

        var clock = new FakeClock(new DateOnly(2026, 8, 25));
        var mockSender = new Mock<INotificationSender>();
        var service = new ReminderService(context, clock, mockSender.Object);

        // Act
        await service.ProcessDailyRemindersAsync();

        // Assert — 05-25 -> 06-25 -> 07-25 -> 08-25 (still not future, equals today) -> 09-25.
        var updated = await context.Subscriptions.FirstAsync(s => s.Id == 1);
        Assert.Equal(new DateOnly(2026, 9, 25), updated.NextRenewalDate);
    }

    [Fact]
    public async Task ProcessDailyRemindersAsync_RollsOverMultipleMissedCycles_Yearly()
    {
        // Arrange — two years overdue.
        var context = CreateContext();
        var user = new ApplicationUser { Email = "user@test.com", PasswordHash = "irrelevant" };
        context.Users.Add(user);

        var subscription = new Subscription
        {
            Id = 1,
            UserId = user.Id,
            Name = "Domain renewal",
            BillingCycle = BillingCycle.Yearly,
            NextRenewalDate = new DateOnly(2024, 8, 25),
            ReminderEnabled = false
        };
        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync();

        var clock = new FakeClock(new DateOnly(2026, 8, 25));
        var mockSender = new Mock<INotificationSender>();
        var service = new ReminderService(context, clock, mockSender.Object);

        // Act
        await service.ProcessDailyRemindersAsync();

        // Assert — 2024-08-25 -> 2025-08-25 -> 2026-08-25 (still not future) -> 2027-08-25.
        var updated = await context.Subscriptions.FirstAsync(s => s.Id == 1);
        Assert.Equal(new DateOnly(2027, 8, 25), updated.NextRenewalDate);
    }

    [Fact]
    public async Task ProcessDailyRemindersAsync_DoesNotTouchSubscription_WhenRenewalIsInTheFuture()
    {
        // Arrange
        var context = CreateContext();
        var user = new ApplicationUser { Email = "user@test.com", PasswordHash = "irrelevant" };
        context.Users.Add(user);

        var subscription = new Subscription
        {
            Id = 1,
            UserId = user.Id,
            Name = "Netflix",
            BillingCycle = BillingCycle.Monthly,
            NextRenewalDate = new DateOnly(2026, 12, 25),
            ReminderEnabled = false
        };
        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync();

        var clock = new FakeClock(new DateOnly(2026, 8, 25));
        var mockSender = new Mock<INotificationSender>();
        var service = new ReminderService(context, clock, mockSender.Object);

        // Act
        await service.ProcessDailyRemindersAsync();

        // Assert — untouched.
        var updated = await context.Subscriptions.FirstAsync(s => s.Id == 1);
        Assert.Equal(new DateOnly(2026, 12, 25), updated.NextRenewalDate);
    }

    [Fact]
    public async Task ProcessDailyRemindersAsync_SendsReminder_ExactlyOneDayBeforeRenewal()
    {
        // Arrange
        var context = CreateContext();
        var today = new DateOnly(2026, 8, 25);
        var tomorrow = today.AddDays(1);

        var user = new ApplicationUser { Email = "user@test.com", PasswordHash = "irrelevant" };
        context.Users.Add(user);

        var dueTomorrow = new Subscription
        {
            Id = 1,
            UserId = user.Id,
            Name = "Netflix",
            BillingCycle = BillingCycle.Monthly,
            NextRenewalDate = tomorrow,
            ReminderEnabled = true
        };

        var dueLater = new Subscription
        {
            Id = 2,
            UserId = user.Id,
            Name = "Spotify",
            BillingCycle = BillingCycle.Monthly,
            NextRenewalDate = today.AddDays(10),
            ReminderEnabled = true
        };

        context.Subscriptions.AddRange(dueTomorrow, dueLater);
        await context.SaveChangesAsync();

        var clock = new FakeClock(today);
        var mockSender = new Mock<INotificationSender>();
        var service = new ReminderService(context, clock, mockSender.Object);

        // Act
        await service.ProcessDailyRemindersAsync();

        // Assert — exactly one reminder, for the subscription due tomorrow, with the
        // other subscription reported alongside it with correct days-remaining.
        mockSender.Verify(s => s.SendReminderEmailAsync(
            It.Is<ApplicationUser>(u => u.Id == user.Id),
            It.Is<SubscriptionRenewalInfo>(info => info.Subscription.Id == 1 && info.DaysRemaining == 1),
            It.Is<IReadOnlyList<SubscriptionRenewalInfo>>(others =>
                others.Count == 1 && others[0].Subscription.Id == 2 && others[0].DaysRemaining == 10)),
            Times.Once);
    }

    [Fact]
    public async Task ProcessDailyRemindersAsync_DoesNotSendReminder_WhenNothingRenewsTomorrow()
    {
        // Arrange
        var context = CreateContext();
        var today = new DateOnly(2026, 8, 25);

        var user = new ApplicationUser { Email = "user@test.com", PasswordHash = "irrelevant" };
        context.Users.Add(user);

        var subscription = new Subscription
        {
            Id = 1,
            UserId = user.Id,
            Name = "Netflix",
            BillingCycle = BillingCycle.Monthly,
            NextRenewalDate = today.AddDays(2),
            ReminderEnabled = true
        };
        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync();

        var clock = new FakeClock(today);
        var mockSender = new Mock<INotificationSender>();
        var service = new ReminderService(context, clock, mockSender.Object);

        // Act
        await service.ProcessDailyRemindersAsync();

        // Assert
        mockSender.Verify(s => s.SendReminderEmailAsync(
            It.IsAny<ApplicationUser>(), It.IsAny<SubscriptionRenewalInfo>(), It.IsAny<IReadOnlyList<SubscriptionRenewalInfo>>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessDailyRemindersAsync_ReminderDisabledSubscription_NeverTriggersAndNeverAppearsInDigest()
    {
        // Arrange
        var context = CreateContext();
        var today = new DateOnly(2026, 8, 25);
        var tomorrow = today.AddDays(1);

        var userWithReminder = new ApplicationUser { Email = "reminded@test.com", PasswordHash = "irrelevant" };
        var userWithoutReminder = new ApplicationUser { Email = "silent@test.com", PasswordHash = "irrelevant" };
        context.Users.AddRange(userWithReminder, userWithoutReminder);

        // Triggers a reminder for userWithReminder.
        var triggering = new Subscription
        {
            Id = 1,
            UserId = userWithReminder.Id,
            Name = "Netflix",
            BillingCycle = BillingCycle.Monthly,
            NextRenewalDate = tomorrow,
            ReminderEnabled = true
        };

        // Also renews tomorrow, but disabled — must never itself trigger a reminder.
        var disabledButDueTomorrow = new Subscription
        {
            Id = 2,
            UserId = userWithoutReminder.Id,
            Name = "Hulu",
            BillingCycle = BillingCycle.Monthly,
            NextRenewalDate = tomorrow,
            ReminderEnabled = false
        };

        // Same user as the triggering subscription, but disabled — must never appear
        // in that user's "also coming up" digest.
        var disabledSameUser = new Subscription
        {
            Id = 3,
            UserId = userWithReminder.Id,
            Name = "Disney+",
            BillingCycle = BillingCycle.Monthly,
            NextRenewalDate = today.AddDays(5),
            ReminderEnabled = false
        };

        // Same user, enabled — should appear in the digest.
        var enabledSameUser = new Subscription
        {
            Id = 4,
            UserId = userWithReminder.Id,
            Name = "Spotify",
            BillingCycle = BillingCycle.Monthly,
            NextRenewalDate = today.AddDays(5),
            ReminderEnabled = true
        };

        context.Subscriptions.AddRange(triggering, disabledButDueTomorrow, disabledSameUser, enabledSameUser);
        await context.SaveChangesAsync();

        var clock = new FakeClock(today);
        var mockSender = new Mock<INotificationSender>();
        var service = new ReminderService(context, clock, mockSender.Object);

        // Act
        await service.ProcessDailyRemindersAsync();

        // Assert — exactly one reminder sent overall (not two — the disabled one due
        // tomorrow never triggers its own), and its digest contains only the enabled
        // sibling subscription, never the disabled one.
        mockSender.Verify(s => s.SendReminderEmailAsync(
            It.IsAny<ApplicationUser>(), It.IsAny<SubscriptionRenewalInfo>(), It.IsAny<IReadOnlyList<SubscriptionRenewalInfo>>()),
            Times.Once);

        mockSender.Verify(s => s.SendReminderEmailAsync(
            It.Is<ApplicationUser>(u => u.Id == userWithReminder.Id),
            It.Is<SubscriptionRenewalInfo>(info => info.Subscription.Id == 1),
            It.Is<IReadOnlyList<SubscriptionRenewalInfo>>(others =>
                others.Count == 1 && others[0].Subscription.Id == 4)),
            Times.Once);
    }
}
