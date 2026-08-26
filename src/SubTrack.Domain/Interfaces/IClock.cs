namespace SubTrack.Domain.Interfaces;

/// <summary>
/// An abstraction over "what day is it". Exists so that date-dependent logic — such as
/// <c>ReminderService</c>'s renewal rollover and reminder checks — can be unit tested
/// against a fixed, fake date instead of the real system clock.
/// </summary>
public interface IClock
{
    /// <summary>The current date.</summary>
    DateOnly Today { get; }
}
