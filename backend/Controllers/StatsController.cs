using System.Security.Claims;
using CineTrack.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatsController : ControllerBase
{
    private readonly StatsService _statsService;

    public StatsController(StatsService statsService)
    {
        _statsService = statsService;
    }

    private string UserId => User.FindFirstValue("sub")!;

    // GET /api/stats
    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _statsService.GetStatsAsync(UserId);
        return Ok(stats);
    }
}
