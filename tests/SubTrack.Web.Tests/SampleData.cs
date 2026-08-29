using SubTrack.Web.ApiClient;

namespace SubTrack.Web.Tests;

internal static class SampleData
{
    public static SubscriptionDto Subscription(int id, string name, int daysUntilRenewal, bool reminderEnabled = true) =>
        new()
        {
            Id = id,
            Name = name,
            Cost = 9.99,
            BillingCycle = BillingCycle._0,
            NextRenewalDate = DateTimeOffset.Now.Date.AddDays(daysUntilRenewal),
            ReminderEnabled = reminderEnabled,
            SubscriptionType = SubscriptionType._1,
        };
}
