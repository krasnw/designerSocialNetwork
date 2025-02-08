using Back.Models.UserDto;
using Back.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers;

[ApiController]
[Route("[controller]")]
public class RatingController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateRatings()
    {
        var result = await _ratingService.Calculate();
        return result ? Ok() : StatusCode(500);
    }

    [HttpGet("{username}/position")]
    public async Task<ActionResult<int>> GetUserPosition(string username)
    {
        var position = await _ratingService.GetRatingPosition(username);
        return position > 0 ? Ok(position) : NotFound();
    }

    [HttpGet]
    public async Task<ActionResult<List<UserRating>>> GetRatings([FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        var ratings = await _ratingService.GetRatings(limit, offset);
        return Ok(ratings);
    }
}
