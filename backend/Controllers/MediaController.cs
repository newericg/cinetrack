using System.Security.Claims;
using CineTrack.API.DTOs;
using CineTrack.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly UserMediaService _userMediaService;

    public MediaController(UserMediaService userMediaService)
    {
        _userMediaService = userMediaService;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;

    /// <summary>GET /api/media — lista individual do usuário (itens que adicionou ao catálogo).</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyList([FromQuery] MediaQueryParams query)
    {
        var result = await _userMediaService.GetMyListAsync(UserId, query);
        return Ok(result);
    }

    /// <summary>POST /api/media — adiciona item do catálogo à lista do usuário.</summary>
    [HttpPost]
    public async Task<IActionResult> AddToList([FromBody] AddToListRequest request)
    {
        var item = await _userMediaService.AddToListAsync(UserId, request);
        return item is null ? NotFound() : CreatedAtAction(null, new { id = item.CatalogItemId }, item);
    }

    /// <summary>DELETE /api/media/{catalogItemId} — remove da lista do usuário.</summary>
    [HttpDelete("{catalogItemId}")]
    public async Task<IActionResult> RemoveFromList(string catalogItemId)
    {
        var deleted = await _userMediaService.RemoveFromListAsync(UserId, catalogItemId);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPatch("{catalogItemId}/watched")]
    public async Task<IActionResult> ToggleWatched(string catalogItemId)
    {
        var item = await _userMediaService.ToggleWatchedAsync(UserId, catalogItemId);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPatch("{catalogItemId}/rating")]
    public async Task<IActionResult> SetRating(string catalogItemId, [FromBody] SetRatingRequest request)
    {
        var item = await _userMediaService.SetRatingAsync(UserId, catalogItemId, request.UserRating);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPatch("{catalogItemId}/episodes")]
    public async Task<IActionResult> SetEpisodes(string catalogItemId, [FromBody] SetEpisodesRequest request)
    {
        var item = await _userMediaService.SetEpisodesWatchedAsync(UserId, catalogItemId, request.EpisodesWatched);
        return item is null ? NotFound() : Ok(item);
    }
}
