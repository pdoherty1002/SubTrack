using System.Text;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using SubTrack.Domain.Entities;
using SubTrack.Domain.Interfaces;

namespace SubTrack.Api.Services;

/// <summary>Sends renewal reminder emails via Amazon SES.</summary>
public class SesNotificationSender : INotificationSender
{
    private readonly IAmazonSimpleEmailServiceV2 _sesClient;
    private readonly string _fromAddress;

    public SesNotificationSender(IAmazonSimpleEmailServiceV2 sesClient, IConfiguration configuration)
    {
        _sesClient = sesClient;
        _fromAddress = configuration["Ses:FromAddress"]
            ?? throw new InvalidOperationException("Ses:FromAddress is not configured.");
    }

    public async Task SendReminderEmailAsync(
        ApplicationUser user,
        SubscriptionRenewalInfo dueSubscription,
        IReadOnlyList<SubscriptionRenewalInfo> otherActiveSubscriptions)
    {
        var request = new SendEmailRequest
        {
            FromEmailAddress = _fromAddress,
            Destination = new Destination { ToAddresses = new List<string> { user.Email } },
            Content = new EmailContent
            {
                Simple = new Message
                {
                    Subject = new Content { Data = $"{dueSubscription.Subscription.Name} renews tomorrow" },
                    Body = new Body { Text = new Content { Data = BuildBody(dueSubscription, otherActiveSubscriptions) } }
                }
            }
        };

        await _sesClient.SendEmailAsync(request);
    }

    private static string BuildBody(
        SubscriptionRenewalInfo dueSubscription,
        IReadOnlyList<SubscriptionRenewalInfo> otherActiveSubscriptions)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{dueSubscription.Subscription.Name} (£{dueSubscription.Subscription.Cost}) renews tomorrow.");

        if (otherActiveSubscriptions.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Also coming up:");
            foreach (var info in otherActiveSubscriptions.OrderBy(i => i.DaysRemaining))
                sb.AppendLine($"- {info.Subscription.Name}: {info.DaysRemaining} day(s)");
        }

        return sb.ToString();
    }
}