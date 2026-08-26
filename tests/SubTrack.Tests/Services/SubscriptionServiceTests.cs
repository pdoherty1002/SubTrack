using System.Linq.Expressions;
using Moq;
using SubTrack.Api.Contracts;
using SubTrack.Api.Services;
using SubTrack.Domain.Entities;
using SubTrack.Domain.Interfaces;

namespace SubTrack.Tests.Services;

public class SubscriptionServiceTests
{
    // Wires GetByConditionAsync to actually compile and apply the predicate the service
    // passes in, against a fixed backing list — so ownership filtering is genuinely
    // exercised rather than just assumed from a canned return value.
    private static Mock<IRepository<Subscription, int>> CreateRepoOver(List<Subscription> allSubscriptions)
    {
        var mockRepo = new Mock<IRepository<Subscription, int>>();
        mockRepo
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<Subscription, bool>>>()))
            .Returns((Expression<Func<Subscription, bool>> predicate) =>
                Task.FromResult<IReadOnlyList<Subscription>>(allSubscriptions.Where(predicate.Compile()).ToList()));

        return mockRepo;
    }

    [Fact]
    public async Task GetForUserAsync_ReturnsOnlyCallersSubscriptions_SortedByNextRenewalDateAscending()
    {
        // Arrange
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        var allSubscriptions = new List<Subscription>
        {
            new() { Id = 1, UserId = userA, Name = "Netflix", NextRenewalDate = new DateOnly(2026, 9, 15) },
            new() { Id = 2, UserId = userA, Name = "Spotify", NextRenewalDate = new DateOnly(2026, 8, 30) },
            new() { Id = 3, UserId = userB, Name = "Disney+", NextRenewalDate = new DateOnly(2026, 8, 25) }
        };

        var mockRepo = CreateRepoOver(allSubscriptions);
        var service = new SubscriptionService(mockRepo.Object);

        // Act
        var result = await service.GetForUserAsync(userA);

        // Assert — only userA's two subscriptions, earliest renewal first.
        Assert.Equal(2, result.Count);
        Assert.Equal("Spotify", result[0].Name);
        Assert.Equal("Netflix", result[1].Name);
    }

    [Fact]
    public async Task GetForUserAsync_ExcludesAnotherUsersSubscriptions()
    {
        // Arrange — ownership enforcement on GET: userB has data, but userA has none.
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        var allSubscriptions = new List<Subscription>
        {
            new() { Id = 1, UserId = userB, Name = "Disney+", NextRenewalDate = new DateOnly(2026, 8, 25) }
        };

        var mockRepo = CreateRepoOver(allSubscriptions);
        var service = new SubscriptionService(mockRepo.Object);

        // Act
        var result = await service.GetForUserAsync(userA);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_CreatesSubscriptionOwnedByCaller()
    {
        // Arrange
        var mockRepo = new Mock<IRepository<Subscription, int>>();
        var userId = Guid.NewGuid();
        var request = new CreateSubscriptionRequest("Netflix", 15.99m, BillingCycle.Monthly, new DateOnly(2026, 9, 1));

        // Act
        var service = new SubscriptionService(mockRepo.Object);
        var result = await service.CreateAsync(userId, request);

        // Assert — check what the caller gets back.
        Assert.Equal("Netflix", result.Name);
        Assert.Equal(15.99m, result.Cost);
        Assert.True(result.ReminderEnabled);

        // Assert — check the entity that was actually staged for persistence carries
        // the userId passed in, not anything derived from the request body.
        mockRepo.Verify(r => r.AddAsync(It.Is<Subscription>(s => s.UserId == userId)), Times.Once);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrueAndRemoves_WhenOwnedByCaller()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscription = new Subscription { Id = 1, UserId = userId, Name = "Netflix" };

        var mockRepo = new Mock<IRepository<Subscription, int>>();
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(subscription);
        mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var service = new SubscriptionService(mockRepo.Object);

        // Act
        var result = await service.DeleteAsync(userId, 1);

        // Assert
        Assert.True(result);
        mockRepo.Verify(r => r.Remove(subscription), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenSubscriptionBelongsToAnotherUser()
    {
        // Arrange — ownership enforcement on DELETE: subscription exists, but not for this caller.
        var owner = Guid.NewGuid();
        var otherUser = Guid.NewGuid();
        var subscription = new Subscription { Id = 1, UserId = owner, Name = "Netflix" };

        var mockRepo = new Mock<IRepository<Subscription, int>>();
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(subscription);

        var service = new SubscriptionService(mockRepo.Object);

        // Act
        var result = await service.DeleteAsync(otherUser, 1);

        // Assert — same false as "not found", and nothing was ever removed.
        Assert.False(result);
        mockRepo.Verify(r => r.Remove(It.IsAny<Subscription>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenSubscriptionDoesNotExist()
    {
        // Arrange
        var mockRepo = new Mock<IRepository<Subscription, int>>();
        mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Subscription?)null);

        var service = new SubscriptionService(mockRepo.Object);

        // Act
        var result = await service.DeleteAsync(Guid.NewGuid(), 999);

        // Assert
        Assert.False(result);
        mockRepo.Verify(r => r.Remove(It.IsAny<Subscription>()), Times.Never);
    }
}
