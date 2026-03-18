using System.ComponentModel.DataAnnotations;
using CineTrack.API.Models;

namespace CineTrack.API.DTOs;

public record CastMemberDto(string Name, string? Character, string? PhotoUrl);

public record CreateMediaRequest(
    [Required] string Title,
    [Required] MediaType Type,
    List<string>? Genres,
    int Year,
    string? PosterUrl,
    string? BackdropUrl,
    string? Synopsis,
    double? Rating,
    int? DurationMinutes,
    int? TotalEpisodes,
    string? Director,
    List<string>? Writers,
    string? Studio,
    List<CastMemberDto>? Cast,
    string? TrailerUrl
);

public record UpdateMediaRequest(
    string? Title,
    List<string>? Genres,
    int? Year,
    string? PosterUrl,
    string? BackdropUrl,
    string? Synopsis,
    double? Rating,
    int? DurationMinutes,
    int? TotalEpisodes,
    string? Director,
    List<string>? Writers,
    string? Studio,
    List<CastMemberDto>? Cast,
    string? TrailerUrl
);

public record SetRatingRequest(
    [Range(0.0, 10.0)] double UserRating
);

public record SetEpisodesRequest(
    [Range(0, int.MaxValue)] int EpisodesWatched
);

public record AddToListRequest(
    [Required] string CatalogItemId,
    WatchStatus? Status = null
);

/// <summary>Item do catálogo global (sem dados do usuário).</summary>
public record CatalogItemResponse(
    string Id,
    string Title,
    string Type,
    List<string> Genres,
    int Year,
    string? PosterUrl,
    string? BackdropUrl,
    string? Synopsis,
    double? Rating,
    int? DurationMinutes,
    int? TotalEpisodes,
    string? Director,
    List<string> Writers,
    string? Studio,
    List<CastMemberDto> Cast,
    string? TrailerUrl
);

/// <summary>Item do catálogo + overlay do usuário (quando adicionou à lista).</summary>
public record MediaItemResponse(
    string Id,
    string CatalogItemId,
    string Title,
    string Type,
    List<string> Genres,
    int Year,
    string? PosterUrl,
    string? BackdropUrl,
    string? Synopsis,
    double? Rating,
    double? UserRating,
    string Status,
    bool IsWatched,
    int? DurationMinutes,
    int? TotalEpisodes,
    int EpisodesWatched,
    DateTime CreatedAt,
    DateTime? WatchedAt,
    DateTime? AddedAt,
    string? Director,
    List<string> Writers,
    string? Studio,
    List<CastMemberDto> Cast,
    string? TrailerUrl,
    bool InUserList
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
    public string? SortBy { get; set; } = "addedAt";
    public string? SortOrder { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}

public class CatalogQueryParams
{
    public MediaType? Type { get; set; }
    public string? Genre { get; set; }
    public string? Search { get; set; }
    public string? SortBy { get; set; } = "createdAt";
    public string? SortOrder { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}
