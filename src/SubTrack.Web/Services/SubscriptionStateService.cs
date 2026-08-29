using SubTrack.Web.ApiClient;

namespace SubTrack.Web.Services;

/// <summary>
/// Holds the current user's subscriptions for the dashboard. Kept intentionally simple —
/// a single scoped list with an <see cref="OnChange"/> event is enough at this scope; no
/// state-management library is warranted.
/// </summary>
public class SubscriptionStateService(ISubscriptionsApi api)
{
    private readonly List<SubscriptionDto> _subscriptions = [];

    public IReadOnlyList<SubscriptionDto> Subscriptions => _subscriptions;

    public event Action? OnChange;

    public async Task LoadAsync()
    {
        var result = await api.GetAllAsync();
        _subscriptions.Clear();
        _subscriptions.AddRange(result.OrderBy(s => s.NextRenewalDate));
        OnChange?.Invoke();
    }

    public async Task AddAsync(CreateSubscriptionRequest request)
    {
        var created = await api.CreateAsync(request);
        _subscriptions.Add(created);
        _subscriptions.Sort((a, b) => a.NextRenewalDate.CompareTo(b.NextRenewalDate));
        OnChange?.Invoke();
    }

    public async Task RemoveAsync(int id)
    {
        var removed = await api.DeleteAsync(id);
        if (removed)
        {
            _subscriptions.RemoveAll(s => s.Id == id);
            OnChange?.Invoke();
        }
    }

    /// <summary>Flips the reminder switch locally only — the API has no endpoint to persist this to.</summary>
    public void SetReminderLocally(int id, bool value)
    {
        var subscription = _subscriptions.FirstOrDefault(s => s.Id == id);
        if (subscription is not null)
        {
            subscription.ReminderEnabled = value;
            OnChange?.Invoke();
        }
    }
}
