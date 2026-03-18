using System.Security.Claims;
using CineTrack.API.DTOs;
using CineTrack.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    private readonly CatalogService _catalogService;

    public CatalogController(CatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    private string? UserId => User.Identity?.IsAuthenticated == true
        ? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")
        : null;

    /// <summary>GET /api/catalog — catálogo global. Se autenticado, inclui overlay (status, rating) dos itens na lista do usuário.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] CatalogQueryParams query)
    {
        var result = await _catalogService.GetCatalogAsync(query, UserId);
        return Ok(result);
    }

    [HttpGet("genres")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGenres()
    {
        var genres = await _catalogService.GetGenresAsync();
        return Ok(genres);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(string id)
    {
        var item = await _catalogService.GetByIdAsync(id, UserId);
        return item is null ? NotFound() : Ok(item);
    }
}
