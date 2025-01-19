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
    public async Task<IActionResult> BuyAccess([FromQuery] string sellerUsername)
    {
        try
        {
            var buyerUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(buyerUsername))
                return Unauthorized("User not authenticated");

            if (buyerUsername == sellerUsername)
                return BadRequest("Cannot subscribe to yourself");

            var seller = _userService.GetUser(sellerUsername);
            if (seller == null)
                return NotFound("Seller not found");

            var result = await _subscriptionService.BuyAccess(buyerUsername, sellerUsername);
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
    public async Task<IActionResult> CancelSubscription([FromQuery] string sellerUsername)
    {
        try
        {
            var buyerUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(buyerUsername))
                return Unauthorized("User not authenticated");

            var result = await _subscriptionService.Cancel(buyerUsername, sellerUsername);
            return result ? Ok("Subscription cancelled") : BadRequest("Failed to cancel subscription");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("check")]
    [Authorize]
    public async Task<IActionResult> CheckSubscription([FromQuery] string sellerUsername)
    {
        try
        {
            var buyerUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(buyerUsername))
                return Unauthorized("User not authenticated");

            var isSubscribed = await _subscriptionService.IsSubscribed(buyerUsername, sellerUsername);
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
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized("User not authenticated");

            var subscriptions = await _subscriptionService.GetAllSubscriptions(username);
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
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized("User not authenticated");

            var subscribers = await _subscriptionService.GetAllSubscribers(username);
            return Ok(subscribers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("fee")]
    public async Task<IActionResult> GetAccessFee([FromQuery] string sellerUsername)
    {
        try
        {
            var fee = await _subscriptionService.GetAccessFee(sellerUsername);
            return Ok(new { fee });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
