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
        _reportService = reportService;
        _logger = logger;
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserReport>>> GetUserReports()
    {
        try
        {
            var reports = await _reportService.GetUserReports();
            return Ok(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user reports");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("posts")]
    public async Task<ActionResult<IEnumerable<PostReport>>> GetPostReports()
    {
        try
        {
            var reports = await _reportService.GetPostReports();
            return Ok(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving post reports");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("reports/{reportType}/{reportId}/status")]
    public async Task<ActionResult> UpdateReportStatus(
        string reportType,
        int reportId, 
        [FromBody] ReportStatus newStatus)
    {
        try
        {
            var result = await _reportService.UpdateReportStatus(reportId, reportType, newStatus);
            return result ? Ok() : BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating report status");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("users/{userId}/freeze")]
    public async Task<ActionResult> FreezeUser(int userId)
    {
        try
        {
            var result = await _reportService.FreezeUser(userId);
            return result ? Ok(new { message = "User has been frozen" }) 
                        : BadRequest(new { message = "Failed to freeze user" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error freezing user");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpDelete("posts/{postId}")]
    public async Task<ActionResult> DeletePost(int postId)
    {
        try
        {
            var result = await _reportService.DeletePost(postId);
            return result ? Ok(new { message = "Post has been deleted" }) 
                        : BadRequest(new { message = "Failed to delete post" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
