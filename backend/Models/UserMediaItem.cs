using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CineTrack.API.Models;

/// <summary>Entrada na lista individual do usuário — referência ao catálogo + status/rating.</summary>
public class UserMediaItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userId")]
    public string UserId { get; set; } = null!;

    [BsonElement("catalogItemId")]
    public string CatalogItemId { get; set; } = null!;

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public WatchStatus Status { get; set; } = WatchStatus.ToWatch;

    [BsonElement("userRating")]
    public double? UserRating { get; set; }

    [BsonElement("episodesWatched")]
    public int EpisodesWatched { get; set; }

    [BsonElement("isWatched")]
    public bool IsWatched { get; set; }

    [BsonElement("watchedAt")]
    public DateTime? WatchedAt { get; set; }

    [BsonElement("addedAt")]
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
