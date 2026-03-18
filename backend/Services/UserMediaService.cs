using CineTrack.API.DTOs;
using CineTrack.API.Models;
using MongoDB.Driver;

namespace CineTrack.API.Services;

public class UserMediaService
{
    private readonly IMongoCollection<UserMediaItem> _userMedia;
    private readonly IMongoCollection<CatalogItem> _catalog;
    private readonly CatalogService _catalogService;

    public UserMediaService(MongoDbService mongoDb, CatalogService catalogService)
    {
        _userMedia = mongoDb.GetCollection<UserMediaItem>("userMediaItems");
        _catalog = mongoDb.GetCollection<CatalogItem>("catalog");
        _catalogService = catalogService;
    }

    public async Task<PagedResponse<MediaItemResponse>> GetMyListAsync(string userId, MediaQueryParams query)
    {
        var userFilter = Builders<UserMediaItem>.Filter.Eq(x => x.UserId, userId);

        if (query.Status.HasValue)
            userFilter &= Builders<UserMediaItem>.Filter.Eq(x => x.Status, query.Status.Value);

        var userItems = await _userMedia.Find(userFilter).ToListAsync();
        var catalogIds = userItems.Select(x => x.CatalogItemId).Distinct().ToList();

        if (catalogIds.Count == 0)
            return new PagedResponse<MediaItemResponse>([], 0, query.Page, query.Limit, 0);

        var catalogFilter = Builders<CatalogItem>.Filter.In(x => x.Id, catalogIds);

        if (query.Type.HasValue)
            catalogFilter &= Builders<CatalogItem>.Filter.Eq(x => x.Type, query.Type.Value);

        if (!string.IsNullOrWhiteSpace(query.Genre))
            catalogFilter &= Builders<CatalogItem>.Filter.AnyEq(x => x.Genres, query.Genre);

        if (!string.IsNullOrWhiteSpace(query.Search))
            catalogFilter &= Builders<CatalogItem>.Filter.Regex(x => x.Title,
                new MongoDB.Bson.BsonRegularExpression(query.Search, "i"));

        var catalogItems = await _catalog.Find(catalogFilter).ToListAsync();
        var overlayByCatalogId = userItems.ToDictionary(x => x.CatalogItemId);

        var combined = catalogItems
            .Where(c => overlayByCatalogId.ContainsKey(c.Id!))
            .Select(c => CatalogService.ToMediaItemResponse(c, overlayByCatalogId[c.Id!]))
            .ToList();

        var total = combined.Count;
        var sortOrder = query.SortOrder?.ToLower() == "asc";
        combined = query.SortBy?.ToLower() switch
        {
            "title" => sortOrder ? combined.OrderBy(x => x.Title, StringComparer.OrdinalIgnoreCase).ToList() : combined.OrderByDescending(x => x.Title, StringComparer.OrdinalIgnoreCase).ToList(),
            "year" => sortOrder ? combined.OrderBy(x => x.Year).ToList() : combined.OrderByDescending(x => x.Year).ToList(),
            "rating" => sortOrder ? combined.OrderBy(x => x.UserRating ?? 0).ToList() : combined.OrderByDescending(x => x.UserRating ?? 0).ToList(),
            "watchedat" => sortOrder ? combined.OrderBy(x => x.WatchedAt ?? DateTime.MinValue).ToList() : combined.OrderByDescending(x => x.WatchedAt ?? DateTime.MinValue).ToList(),
            "addedat" => sortOrder ? combined.OrderBy(x => x.AddedAt ?? DateTime.MinValue).ToList() : combined.OrderByDescending(x => x.AddedAt ?? DateTime.MinValue).ToList(),
            _ => sortOrder ? combined.OrderBy(x => x.AddedAt ?? x.CreatedAt).ToList() : combined.OrderByDescending(x => x.AddedAt ?? x.CreatedAt).ToList(),
        };

        var paged = combined
            .Skip((query.Page - 1) * query.Limit)
            .Take(query.Limit)
            .ToList();

        return new PagedResponse<MediaItemResponse>(
            paged,
            total,
            query.Page,
            query.Limit,
            (int)Math.Ceiling((double)total / query.Limit)
        );
    }

    public async Task<MediaItemResponse?> AddToListAsync(string userId, AddToListRequest request)
    {
        var exists = await _catalogService.ExistsAsync(request.CatalogItemId);
        if (!exists) return null;

        var existing = await _userMedia
            .Find(x => x.UserId == userId && x.CatalogItemId == request.CatalogItemId)
            .FirstOrDefaultAsync();
        if (existing is not null) return await _catalogService.GetByIdAsync(request.CatalogItemId, userId);

        var userItem = new UserMediaItem
        {
            UserId = userId,
            CatalogItemId = request.CatalogItemId,
            Status = request.Status ?? WatchStatus.ToWatch,
        };

        await _userMedia.InsertOneAsync(userItem);
        return await _catalogService.GetByIdAsync(request.CatalogItemId, userId);
    }

    public async Task<bool> RemoveFromListAsync(string userId, string catalogItemId)
    {
        var result = await _userMedia.DeleteOneAsync(x => x.UserId == userId && x.CatalogItemId == catalogItemId);
        return result.DeletedCount > 0;
    }

    public async Task<MediaItemResponse?> ToggleWatchedAsync(string userId, string catalogItemId)
    {
        var userItem = await _userMedia
            .Find(x => x.UserId == userId && x.CatalogItemId == catalogItemId)
            .FirstOrDefaultAsync();
        if (userItem is null) return null;

        var nowWatched = !userItem.IsWatched;
        var update = Builders<UserMediaItem>.Update
            .Set(x => x.IsWatched, nowWatched)
            .Set(x => x.Status, nowWatched ? WatchStatus.Watched : WatchStatus.ToWatch)
            .Set(x => x.WatchedAt, nowWatched ? DateTime.UtcNow : null);

        await _userMedia.UpdateOneAsync(
            x => x.UserId == userId && x.CatalogItemId == catalogItemId,
            update);

        return await _catalogService.GetByIdAsync(catalogItemId, userId);
    }

    public async Task<MediaItemResponse?> SetRatingAsync(string userId, string catalogItemId, double userRating)
    {
        var result = await _userMedia.UpdateOneAsync(
            x => x.UserId == userId && x.CatalogItemId == catalogItemId,
            Builders<UserMediaItem>.Update.Set(x => x.UserRating, userRating));
        if (result.MatchedCount == 0) return null;

        return await _catalogService.GetByIdAsync(catalogItemId, userId);
    }

    public async Task<MediaItemResponse?> SetEpisodesWatchedAsync(string userId, string catalogItemId, int episodes)
    {
        var userItem = await _userMedia
            .Find(x => x.UserId == userId && x.CatalogItemId == catalogItemId)
            .FirstOrDefaultAsync();
        if (userItem is null) return null;

        var catalogItem = await _catalog.Find(x => x.Id == catalogItemId).FirstOrDefaultAsync();
        var totalEpisodes = catalogItem?.TotalEpisodes ?? 0;
        var isFinished = totalEpisodes > 0 && episodes >= totalEpisodes;

        var update = Builders<UserMediaItem>.Update
            .Set(x => x.EpisodesWatched, episodes)
            .Set(x => x.Status, isFinished ? WatchStatus.Watched : WatchStatus.Watching)
            .Set(x => x.IsWatched, isFinished)
            .Set(x => x.WatchedAt, isFinished ? DateTime.UtcNow : null);

        await _userMedia.UpdateOneAsync(
            x => x.UserId == userId && x.CatalogItemId == catalogItemId,
            update);

        return await _catalogService.GetByIdAsync(catalogItemId, userId);
    }

    public async Task<bool> IsInUserListAsync(string userId, string catalogItemId)
    {
        return await _userMedia
            .Find(x => x.UserId == userId && x.CatalogItemId == catalogItemId)
            .AnyAsync();
    }

    internal async Task<List<(CatalogItem Catalog, UserMediaItem User)>> GetRawByUserAsync(string userId)
    {
        var userItems = await _userMedia.Find(x => x.UserId == userId).ToListAsync();
        if (userItems.Count == 0) return [];

        var catalogIds = userItems.Select(x => x.CatalogItemId).Distinct().ToList();
        var catalogItems = await _catalog.Find(x => catalogIds.Contains(x.Id!)).ToListAsync();
        var catalogById = catalogItems.ToDictionary(x => x.Id!);

        return userItems
            .Where(u => catalogById.ContainsKey(u.CatalogItemId))
            .Select(u => (catalogById[u.CatalogItemId], u))
            .ToList();
    }
}
