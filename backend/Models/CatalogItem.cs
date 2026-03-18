using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CineTrack.API.Models;

/// <summary>Item do catálogo global — todos os usuários veem a mesma lista.</summary>
public class CatalogItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

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

    [BsonElement("backdropUrl")]
    public string? BackdropUrl { get; set; }

    [BsonElement("synopsis")]
    public string? Synopsis { get; set; }

    [BsonElement("rating")]
    public double? Rating { get; set; }

    [BsonElement("durationMinutes")]
    public int? DurationMinutes { get; set; }

    [BsonElement("totalEpisodes")]
    public int? TotalEpisodes { get; set; }

    [BsonElement("director")]
    public string? Director { get; set; }

    [BsonElement("writers")]
    public List<string> Writers { get; set; } = [];

    [BsonElement("studio")]
    public string? Studio { get; set; }

    [BsonElement("cast")]
    public List<CastMember> Cast { get; set; } = [];

    [BsonElement("trailerUrl")]
    public string? TrailerUrl { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
