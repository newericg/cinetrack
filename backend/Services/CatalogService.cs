using CineTrack.API.DTOs;
using CineTrack.API.Models;
using MongoDB.Driver;

namespace CineTrack.API.Services;

public class CatalogService
{
    private readonly IMongoCollection<CatalogItem> _catalog;
    private readonly IMongoCollection<UserMediaItem> _userMedia;

    public CatalogService(MongoDbService mongoDb)
    {
        _catalog = mongoDb.GetCollection<CatalogItem>("catalog");
        _userMedia = mongoDb.GetCollection<UserMediaItem>("userMediaItems");
        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        _catalog.Indexes.CreateOne(new CreateIndexModel<CatalogItem>(
            Builders<CatalogItem>.IndexKeys.Ascending(x => x.Type)));
        _userMedia.Indexes.CreateOne(new CreateIndexModel<UserMediaItem>(
            Builders<UserMediaItem>.IndexKeys.Combine(
                Builders<UserMediaItem>.IndexKeys.Ascending(x => x.UserId),
                Builders<UserMediaItem>.IndexKeys.Ascending(x => x.CatalogItemId)),
            new CreateIndexOptions { Unique = true }));
    }

    /// <summary>Catálogo global — todos veem. Se userId informado, inclui overlay (status, rating) dos itens na lista do usuário.</summary>
    public async Task<PagedResponse<MediaItemResponse>> GetCatalogAsync(CatalogQueryParams query, string? userId = null)
    {
        var filter = Builders<CatalogItem>.Filter.Empty;

        if (query.Type.HasValue)
            filter &= Builders<CatalogItem>.Filter.Eq(x => x.Type, query.Type.Value);

        if (!string.IsNullOrWhiteSpace(query.Genre))
            filter &= Builders<CatalogItem>.Filter.AnyEq(x => x.Genres, query.Genre);

        if (!string.IsNullOrWhiteSpace(query.Search))
            filter &= Builders<CatalogItem>.Filter.Regex(x => x.Title,
                new MongoDB.Bson.BsonRegularExpression(query.Search, "i"));

        var total = await _catalog.CountDocumentsAsync(filter);
        var sortDefinition = BuildCatalogSort(query.SortBy, query.SortOrder);

        var catalogItems = await _catalog
            .Find(filter)
            .Sort(sortDefinition)
            .Skip((query.Page - 1) * query.Limit)
            .Limit(query.Limit)
            .ToListAsync();

        Dictionary<string, UserMediaItem>? userOverlays = null;
        if (!string.IsNullOrEmpty(userId))
        {
            var catalogIds = catalogItems.Select(c => c.Id!).ToList();
            var overlays = await _userMedia
                .Find(x => x.UserId == userId && catalogIds.Contains(x.CatalogItemId))
                .ToListAsync();
            userOverlays = overlays.ToDictionary(x => x.CatalogItemId);
        }

        var items = catalogItems.Select(c =>
        {
            var overlay = userOverlays?.GetValueOrDefault(c.Id!);
            return ToMediaItemResponse(c, overlay);
        }).ToList();

        return new PagedResponse<MediaItemResponse>(
            items,
            (int)total,
            query.Page,
            query.Limit,
            (int)Math.Ceiling((double)total / query.Limit)
        );
    }

    public async Task<MediaItemResponse?> GetByIdAsync(string catalogItemId, string? userId = null)
    {
        var catalogItem = await _catalog.Find(x => x.Id == catalogItemId).FirstOrDefaultAsync();
        if (catalogItem is null) return null;

        UserMediaItem? overlay = null;
        if (!string.IsNullOrEmpty(userId))
        {
            overlay = await _userMedia
                .Find(x => x.UserId == userId && x.CatalogItemId == catalogItemId)
                .FirstOrDefaultAsync();
        }

        return ToMediaItemResponse(catalogItem, overlay);
    }

    public async Task<List<string>> GetGenresAsync()
    {
        var genres = await _catalog
            .Find(FilterDefinition<CatalogItem>.Empty)
            .Project(x => x.Genres)
            .ToListAsync();
        return genres.SelectMany(g => g).Distinct().OrderBy(g => g).ToList();
    }

    public async Task<bool> ExistsAsync(string catalogItemId)
    {
        return await _catalog.Find(x => x.Id == catalogItemId).AnyAsync();
    }

    internal static MediaItemResponse ToMediaItemResponse(CatalogItem c, UserMediaItem? overlay)
    {
        var inList = overlay is not null;
        return new MediaItemResponse(
            c.Id!,
            c.Id!,
            c.Title,
            c.Type.ToString(),
            c.Genres,
            c.Year,
            c.PosterUrl,
            c.BackdropUrl,
            c.Synopsis,
            c.Rating,
            overlay?.UserRating,
            overlay?.Status.ToString() ?? "ToWatch",
            overlay?.IsWatched ?? false,
            c.DurationMinutes,
            c.TotalEpisodes,
            overlay?.EpisodesWatched ?? 0,
            c.CreatedAt,
            overlay?.WatchedAt,
            overlay?.AddedAt,
            c.Director,
            c.Writers,
            c.Studio,
            c.Cast.Select(x => new CastMemberDto(x.Name, x.Character, x.PhotoUrl)).ToList(),
            c.TrailerUrl,
            inList
        );
    }

    private static SortDefinition<CatalogItem> BuildCatalogSort(string? sortBy, string? order)
    {
        var isAsc = order?.ToLower() == "asc";
        return sortBy?.ToLower() switch
        {
            "title" => isAsc
                ? Builders<CatalogItem>.Sort.Ascending(x => x.Title)
                : Builders<CatalogItem>.Sort.Descending(x => x.Title),
            "year" => isAsc
                ? Builders<CatalogItem>.Sort.Ascending(x => x.Year)
                : Builders<CatalogItem>.Sort.Descending(x => x.Year),
            "rating" => isAsc
                ? Builders<CatalogItem>.Sort.Ascending(x => x.Rating)
                : Builders<CatalogItem>.Sort.Descending(x => x.Rating),
            _ => Builders<CatalogItem>.Sort.Descending(x => x.CreatedAt),
        };
    }
}
