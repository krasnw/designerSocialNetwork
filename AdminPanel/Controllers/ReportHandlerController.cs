namespace AdminPanel.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AdminPanel.Services.Interfaces;
using AdminPanel.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportHandlerController : ControllerBase
{
    private readonly IReportHandlerService _reportService;
    private readonly ILogger<ReportHandlerController> _logger;

    public ReportHandlerController(IReportHandlerService reportService, ILogger<ReportHandlerController> logger)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserReport>>> GetUserReports()
    {
        try
        {
            var reports = await _reportService.GetUserReports();
            if (!reports.Any())
            {
                return NoContent();
            }
            return Ok(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user reports");
            return StatusCode(500, new { message = "Failed to retrieve user reports", details = ex.Message });
        }
    }

    [HttpGet("posts")]
    public async Task<ActionResult<IEnumerable<PostReport>>> GetPostReports()
    {
        try
        {
            var reports = await _reportService.GetPostReports();
            if (!reports.Any())
            {
                return NoContent();
            }
            return Ok(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving post reports");
            return StatusCode(500, new { message = "Failed to retrieve post reports", details = ex.Message });
        }
    }

    [HttpPost("users/{userReportId}/dismiss")]
    public async Task<ActionResult> DismissUserReport(int userReportId)
    {
        if (userReportId <= 0)
        {
            return BadRequest(new { message = "Invalid report ID" });
        }

        try
        {
            var result = await _reportService.DismissUserReport(userReportId);
            if (!result)
            {
                return NotFound(new { message = "Report not found or already dismissed" });
            }
            return Ok(new { message = "User report has been dismissed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dismissing user report {ReportId}", userReportId);
            return StatusCode(500, new { message = "Failed to dismiss user report", details = ex.Message });
        }
    }

    [HttpPost("posts/{postReportId}/dismiss")]
    public async Task<ActionResult> DismissPostReport(int postReportId)
    {
        if (postReportId <= 0)
        {
            return BadRequest(new { message = "Invalid report ID" });
        }

        try
        {
            var result = await _reportService.DismissPostReport(postReportId);
            if (!result)
            {
                return NotFound(new { message = "Report not found or already dismissed" });
            }
            return Ok(new { message = "Post report has been dismissed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dismissing post report {ReportId}", postReportId);
            return StatusCode(500, new { message = "Failed to dismiss post report", details = ex.Message });
        }
    }

    [HttpPost("users/{username}/toggle-freeze")]
    public async Task<ActionResult> ToggleUserFreeze(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest(new { message = "Username cannot be empty" });
        }

        try
        {
            var isFrozen = await _reportService.IsFrozen(username);
            bool result;
            string message;

            if (isFrozen)
            {
                result = await _reportService.UnfreezeUser(username);
                message = "User has been unfrozen successfully";
            }
            else
            {
                result = await _reportService.FreezeUser(username);
                message = "User has been frozen successfully";
            }

            if (!result)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new { message, isFrozen = !isFrozen });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user freeze state for {Username}", username);
            return StatusCode(500, new { message = "Failed to update user freeze state", details = ex.Message });
        }
    }

    [HttpDelete("posts/{postId}")]
    public async Task<ActionResult> DeletePost(int postId)
    {
        if (postId <= 0)
        {
            return BadRequest(new { message = "Invalid post ID" });
        }

        try
        {
            var result = await _reportService.DeletePost(postId);
            if (!result)
            {
                return NotFound(new { message = "Post not found" });
            }
            return Ok(new { message = "Post has been deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post {PostId}", postId);
            return StatusCode(500, new { message = "Failed to delete post", details = ex.Message });
        }
    }

    [HttpGet("users/frozen")]
    public async Task<ActionResult<IEnumerable<UserMiniDTO>>> GetFrozenUsers()
    {
        try
        {
            var users = await _reportService.GetFrozenUsers();
            if (!users.Any())
            {
                return NoContent();
            }
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving frozen users");
            return StatusCode(500, new { message = "Failed to retrieve frozen users", details = ex.Message });
        }
    }
}
