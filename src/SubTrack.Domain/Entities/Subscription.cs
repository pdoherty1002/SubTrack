namespace SubTrack.Domain.Entities;

/// <summary>A recurring subscription a user is tracking, with its next renewal date.</summary>
public class Subscription
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Foreign key to the owning user. Required.</summary>
    public Guid UserId { get; set; }

    /// <summary>Navigation property to the owning user.</summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>The subscription's display name (e.g. "Netflix").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The amount charged per billing cycle.</summary>
    public decimal Cost { get; set; }

    /// <summary>How often this subscription renews.</summary>
    public BillingCycle BillingCycle { get; set; }

    /// <summary>The next date this subscription is due to renew.</summary>
    public DateOnly NextRenewalDate { get; set; }

    /// <summary>Whether the user wants to be reminded before this subscription renews.</summary>
    public bool ReminderEnabled { get; set; } = true;

    /// <summary>When this subscription was first added.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
