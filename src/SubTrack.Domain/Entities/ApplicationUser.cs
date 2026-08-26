namespace SubTrack.Domain.Entities;

/// <summary>A registered user, tracking their own set of subscriptions.</summary>
public class ApplicationUser
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>The user's email address. Unique — also used as their login identifier.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Hashed password. Never stored in plaintext.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>When this account was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>All subscriptions this user is tracking.</summary>
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
