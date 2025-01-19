using Back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Back.Models;

namespace Back.Controllers;

[ApiController]
[Route("[controller]")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IUserService _userService;

    public SubscriptionController(ISubscriptionService subscriptionService, IUserService userService)
    {
        _subscriptionService = subscriptionService;
        _userService = userService;
    }

    [HttpPost("buy")]
    [Authorize]
    public async Task<IActionResult> BuyAccess([FromQuery] string sellerId)
    {
        try
        {
            var buyerId = User.Identity?.Name;
            if (string.IsNullOrEmpty(buyerId))
                return Unauthorized("User not authenticated");

            if (buyerId == sellerId)
                return BadRequest("Cannot subscribe to yourself");

            var seller = _userService.GetUser(sellerId);
            if (seller == null)
                return NotFound("Seller not found");

            var result = await _subscriptionService.BuyAccess(buyerId, sellerId);
            if (!result)
                return BadRequest("Failed to process subscription. Please check your wallet balance.");

            return Ok("Subscription successful");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("cancel")]
    [Authorize]
    public async Task<IActionResult> CancelSubscription([FromQuery] string sellerId)
    {
        try
        {
            var buyerId = User.Identity?.Name;
            if (string.IsNullOrEmpty(buyerId))
                return Unauthorized("User not authenticated");

            var result = await _subscriptionService.Cancel(buyerId, sellerId);
            return result ? Ok("Subscription cancelled") : BadRequest("Failed to cancel subscription");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("check")]
    [Authorize]
    public async Task<IActionResult> CheckSubscription([FromQuery] string sellerId)
    {
        try
        {
            var buyerId = User.Identity?.Name;
            if (string.IsNullOrEmpty(buyerId))
                return Unauthorized("User not authenticated");

            var isSubscribed = await _subscriptionService.IsSubscribed(buyerId, sellerId);
            return Ok(new { isSubscribed });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("subscriptions")]
    [Authorize]
    public async Task<IActionResult> GetSubscriptions()
    {
        try
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var subscriptions = await _subscriptionService.GetAllSubscriptions(userId);
            return Ok(subscriptions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("subscribers")]
    [Authorize]
    public async Task<IActionResult> GetSubscribers()
    {
        try
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var subscribers = await _subscriptionService.GetAllSubscribers(userId);
            return Ok(subscribers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("fee")]
    public async Task<IActionResult> GetAccessFee([FromQuery] string sellerId)
    {
        try
        {
            var fee = await _subscriptionService.GetAccessFee(sellerId);
            return Ok(new { fee });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
