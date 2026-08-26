using Microsoft.AspNetCore.Mvc;
using SubTrack.Api.Services;

namespace SubTrack.Api.Controllers;

/// <summary>Internal endpoint for triggering the daily reminder job. Called by EventBridge on a schedule, not by regular users.</summary>
[ApiController]
[Route("api/[controller]")]
public class RemindersController : ControllerBase
{
    private readonly ReminderService _reminderService;
    private readonly IConfiguration _configuration;

    public RemindersController(ReminderService reminderService, IConfiguration configuration)
    {
        _reminderService = reminderService;
        _configuration = configuration;
    }

    /// <summary>Runs the daily reminder cycle: rolls forward overdue subscriptions and sends tomorrow's reminders.</summary>
    [HttpPost("process")]
    public async Task<IActionResult> Process([FromHeader(Name = "X-Reminder-Secret")] string? secret)
    {
        var expectedSecret = _configuration["ReminderJob:Secret"];

        if (string.IsNullOrEmpty(expectedSecret) || secret != expectedSecret)
            return Unauthorized();

        await _reminderService.ProcessDailyRemindersAsync();
        return Ok();
    }
}