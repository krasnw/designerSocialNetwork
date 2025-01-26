using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Back.Models;
using Back.Services.Interfaces;

namespace Back.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("reasons/user")]
        public async Task<ActionResult<IEnumerable<ReasonResponse>>> GetUserReasons()
        {
            try
            {
                var reasons = await _reportService.GetUserReasonsAsync();
                if (!reasons.Any())
                {
                    return NotFound("No reasons found for user reports");
                }
                return Ok(reasons);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("reasons/post")]
        public async Task<ActionResult<IEnumerable<ReasonResponse>>> GetPostReasons()
        {
            try
            {
                var reasons = await _reportService.GetPostReasonsAsync();
                if (!reasons.Any())
                {
                    return NotFound("No reasons found for post reports");
                }
                return Ok(reasons);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("user")]
        public async Task<ActionResult> ReportUser(CreateUserReportRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request data", errors = ModelState.Values.SelectMany(v => v.Errors) });
            }

            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest(new { message = "Username cannot be empty" });
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(new { message = "Reason cannot be empty" });
            }

            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized(new { message = "User authentication required" });
                }

                if (username == request.Username)
                {
                    return BadRequest(new { message = "You cannot report yourself" });
                }

                await _reportService.CreateUserReportAsync(username, request);
                return Ok(new { message = "Report submitted successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to submit report", details = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("post")]
        public async Task<ActionResult> ReportPost(CreatePostReportRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request data", errors = ModelState.Values.SelectMany(v => v.Errors) });
            }

            if (request.PostId <= 0)
            {
                return BadRequest(new { message = "Invalid post ID" });
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(new { message = "Reason cannot be empty" });
            }

            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized(new { message = "User authentication required" });
                }

                await _reportService.CreatePostReportAsync(username, request);
                return Ok(new { message = "Report submitted successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to submit report", details = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult<AllReportsResponse>> GetAllReports()
        {
            try
            {
                var reports = await _reportService.GetAllReportsAsync();
                if (reports.UserReports?.Any() != true && reports.PostReports?.Any() != true)
                {
                    return NotFound(new { message = "No reports found" });
                }
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve reports", details = ex.Message });
            }
        }
    }
}
