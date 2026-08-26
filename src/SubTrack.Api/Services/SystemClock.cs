using SubTrack.Domain.Interfaces;

namespace SubTrack.Api.Services;

/// <summary>The real <see cref="IClock"/> implementation, backed by the system clock.</summary>
public class SystemClock : IClock
{
    /// <inheritdoc/>
    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
}
