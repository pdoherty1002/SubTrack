using SubTrack.Web.ApiClient;

namespace SubTrack.Web.Services;

/// <summary>
/// Labels for the generated <c>BillingCycle</c>/<c>SubscriptionType</c> enums. The API's
/// OpenAPI spec exposes them as bare integers (no named enum values), so NSwag generates
/// them as <c>_0</c>, <c>_1</c>, etc. — display names are mapped here instead, ordinal-matched
/// against the real backend enums (SubTrack.Domain.Entities.BillingCycle/SubscriptionType).
/// </summary>
public static class EnumDisplay
{
    public static readonly (BillingCycle Value, string Label)[] BillingCycles =
    [
        (BillingCycle._0, "Monthly"),
        (BillingCycle._1, "Yearly"),
    ];

    public static readonly (SubscriptionType Value, string Label)[] SubscriptionTypes =
    [
        (SubscriptionType._0, "Other"),
        (SubscriptionType._1, "Streaming"),
        (SubscriptionType._2, "Fitness"),
        (SubscriptionType._3, "Gaming"),
        (SubscriptionType._4, "News"),
        (SubscriptionType._5, "Utilities"),
    ];

    public static string LabelFor(BillingCycle value) =>
        BillingCycles.First(o => o.Value == value).Label;

    public static string LabelFor(SubscriptionType value) =>
        SubscriptionTypes.First(o => o.Value == value).Label;
}
