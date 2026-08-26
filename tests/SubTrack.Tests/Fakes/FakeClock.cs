using SubTrack.Domain.Interfaces;

namespace SubTrack.Tests.Fakes;

/// <summary>A fake <see cref="IClock"/> whose "today" is set explicitly, for deterministic date-based tests.</summary>
public class FakeClock : IClock
{
    public FakeClock(DateOnly today) => Today = today;

    public DateOnly Today { get; set; }
}
