using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubTrack.Api.Contracts;
using SubTrack.Api.Services;

namespace SubTrack.Api.Controllers;

/// <summary>Endpoints for viewing, adding, and removing the caller's own subscriptions.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly SubscriptionService _subscriptionService;

    /// <summary>Creates the controller, given its service (supplied by dependency injection).</summary>
    public SubscriptionsController(SubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    /// <summary>Gets the caller's own subscriptions, sorted by next renewal date ascending.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var subscriptions = await _subscriptionService.GetForUserAsync(userId);
        return Ok(subscriptions);
    }

    /// <summary>Creates a new subscription owned by the caller.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateSubscriptionRequest request)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var subscription = await _subscriptionService.CreateAsync(userId, request);
        return CreatedAtAction(nameof(GetAll), subscription);
    }

    /// <summary>
    /// Deletes one of the caller's subscriptions. Returns 404 both when it doesn't exist
    /// and when it belongs to another user — the two cases aren't distinguished.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var deleted = await _subscriptionService.DeleteAsync(userId, id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    // Pulled from the validated token, not from anything the client sent — this is
    // the whole point of putting UserId on a separate parameter back in the service
    // methods rather than trusting the request body.
    private bool TryGetUserId(out Guid userId)
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(userIdClaim, out userId);
    }
}
