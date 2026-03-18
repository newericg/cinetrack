using CineTrack.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineTrack.API.Controllers;

/// <summary>Development-only endpoints. Only available when ASPNETCORE_ENVIRONMENT=Development.</summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class DevController : ControllerBase
{
    private readonly DataSeeder _seeder;
    private readonly IWebHostEnvironment _env;

    public DevController(DataSeeder seeder, IWebHostEnvironment env)
    {
        _seeder = seeder;
        _env = env;
    }

    /// <summary>POST /api/dev/seed?force=true — repopulate catalog for seed user (no auth required).</summary>
    [HttpPost("seed")]
    public async Task<IActionResult> Seed([FromQuery] bool force = false)
    {
        if (!_env.IsDevelopment())
            return NotFound();

        var inserted = await _seeder.SeedAsync(force);
        return Ok(new { inserted, message = inserted > 0 ? $"{inserted} items added." : force ? "Catalog cleared and repopulated." : "Catalog already seeded." });
    }
}
