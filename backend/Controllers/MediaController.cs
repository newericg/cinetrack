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
    private readonly MediaService _mediaService;

    public MediaController(MediaService mediaService)
    {
        _mediaService = mediaService;
    }

    private string UserId => User.FindFirstValue("sub")!;

    // GET /api/media?type=Movie&genre=Action&status=Watched&search=dune&page=1&limit=20
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] MediaQueryParams query)
    {
        var result = await _mediaService.GetAllAsync(UserId, query);
        return Ok(result);
    }

    // GET /api/media/genres
    [HttpGet("genres")]
    public async Task<IActionResult> GetGenres()
    {
        var genres = await _mediaService.GetGenresAsync(UserId);
        return Ok(genres);
    }

    // GET /api/media/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var item = await _mediaService.GetByIdAsync(UserId, id);
        return item is null ? NotFound() : Ok(item);
    }

    // POST /api/media
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMediaRequest request)
    {
        var item = await _mediaService.CreateAsync(UserId, request);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    // PUT /api/media/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateMediaRequest request)
    {
        var item = await _mediaService.UpdateAsync(UserId, id, request);
        return item is null ? NotFound() : Ok(item);
    }

    // DELETE /api/media/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _mediaService.DeleteAsync(UserId, id);
        return deleted ? NoContent() : NotFound();
    }

    // PATCH /api/media/{id}/watched — toggle watched
    [HttpPatch("{id}/watched")]
    public async Task<IActionResult> ToggleWatched(string id)
    {
        var item = await _mediaService.ToggleWatchedAsync(UserId, id);
        return item is null ? NotFound() : Ok(item);
    }

    // PATCH /api/media/{id}/rating
    [HttpPatch("{id}/rating")]
    public async Task<IActionResult> SetRating(string id, [FromBody] SetRatingRequest request)
    {
        var item = await _mediaService.SetRatingAsync(UserId, id, request.UserRating);
        return item is null ? NotFound() : Ok(item);
    }

    // PATCH /api/media/{id}/episodes
    [HttpPatch("{id}/episodes")]
    public async Task<IActionResult> SetEpisodes(string id, [FromBody] SetEpisodesRequest request)
    {
        var item = await _mediaService.SetEpisodesWatchedAsync(UserId, id, request.EpisodesWatched);
        return item is null ? NotFound() : Ok(item);
    }
}
