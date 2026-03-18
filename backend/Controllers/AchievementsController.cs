using System.Security.Claims;
using CineTrack.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AchievementsController : ControllerBase
{
    private readonly AchievementService _achievementService;

    public AchievementsController(AchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;

    // GET /api/achievements
    [HttpGet]
    public async Task<IActionResult> GetAchievements()
    {
        var result = await _achievementService.GetAchievementsAsync(UserId);
        return Ok(result);
    }
}
