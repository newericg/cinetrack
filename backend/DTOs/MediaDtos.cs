using System.ComponentModel.DataAnnotations;
using CineTrack.API.Models;

namespace CineTrack.API.DTOs;

public record CreateMediaRequest(
    [Required] string Title,
    [Required] MediaType Type,
    List<string>? Genres,
    int Year,
    string? PosterUrl,
    string? Synopsis,
    double? Rating,
    int? DurationMinutes,
    int? TotalEpisodes
);

public record UpdateMediaRequest(
    string? Title,
    List<string>? Genres,
    int? Year,
    string? PosterUrl,
    string? Synopsis,
    double? Rating,
    int? DurationMinutes,
    int? TotalEpisodes
);

public record SetRatingRequest(
    [Range(0.0, 10.0)] double UserRating
);

public record SetEpisodesRequest(
    [Range(0, int.MaxValue)] int EpisodesWatched
);

public record MediaItemResponse(
    string Id,
    string Title,
    string Type,
    List<string> Genres,
    int Year,
    string? PosterUrl,
    string? Synopsis,
    double? Rating,
    double? UserRating,
    string Status,
    bool IsWatched,
    int? DurationMinutes,
    int? TotalEpisodes,
    int EpisodesWatched,
    DateTime CreatedAt,
    DateTime? WatchedAt
);

public record PagedResponse<T>(
    List<T> Items,
    int Total,
    int Page,
    int Limit,
    int TotalPages
);

public class MediaQueryParams
{
    public MediaType? Type { get; set; }
    public string? Genre { get; set; }
    public WatchStatus? Status { get; set; }
    public string? Search { get; set; }
    public string? SortBy { get; set; } = "createdAt";
    public string? SortOrder { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}
