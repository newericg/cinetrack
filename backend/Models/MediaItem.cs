using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CineTrack.API.Models;

public enum MediaType { Movie, Series, Anime }
public enum WatchStatus { ToWatch, Watching, Watched, Dropped }

public class MediaItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userId")]
    public string UserId { get; set; } = null!;

    [BsonElement("title")]
    public string Title { get; set; } = null!;

    [BsonElement("type")]
    [BsonRepresentation(BsonType.String)]
    public MediaType Type { get; set; }

    [BsonElement("genres")]
    public List<string> Genres { get; set; } = [];

    [BsonElement("year")]
    public int Year { get; set; }

    [BsonElement("posterUrl")]
    public string? PosterUrl { get; set; }

    [BsonElement("synopsis")]
    public string? Synopsis { get; set; }

    [BsonElement("rating")]
    public double? Rating { get; set; }

    [BsonElement("userRating")]
    public double? UserRating { get; set; }

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public WatchStatus Status { get; set; } = WatchStatus.ToWatch;

    [BsonElement("isWatched")]
    public bool IsWatched { get; set; }

    [BsonElement("durationMinutes")]
    public int? DurationMinutes { get; set; }

    [BsonElement("totalEpisodes")]
    public int? TotalEpisodes { get; set; }

    [BsonElement("episodesWatched")]
    public int EpisodesWatched { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("watchedAt")]
    public DateTime? WatchedAt { get; set; }
}
