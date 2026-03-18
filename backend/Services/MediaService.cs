using CineTrack.API.DTOs;
using CineTrack.API.Models;
using MongoDB.Driver;

namespace CineTrack.API.Services;

public class MediaService
{
    private readonly IMongoCollection<MediaItem> _collection;

    public MediaService(MongoDbService mongoDb)
    {
        _collection = mongoDb.GetCollection<MediaItem>("mediaItems");
        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        var indexKeys = Builders<MediaItem>.IndexKeys
            .Ascending(x => x.UserId)
            .Ascending(x => x.Type)
            .Ascending(x => x.Status);
        _collection.Indexes.CreateOne(new CreateIndexModel<MediaItem>(indexKeys));
    }

    public async Task<PagedResponse<MediaItemResponse>> GetAllAsync(string userId, MediaQueryParams query)
    {
        var filter = Builders<MediaItem>.Filter.Eq(x => x.UserId, userId);

        if (query.Type.HasValue)
            filter &= Builders<MediaItem>.Filter.Eq(x => x.Type, query.Type.Value);

        if (query.Status.HasValue)
            filter &= Builders<MediaItem>.Filter.Eq(x => x.Status, query.Status.Value);

        if (!string.IsNullOrWhiteSpace(query.Genre))
            filter &= Builders<MediaItem>.Filter.AnyEq(x => x.Genres, query.Genre);

        if (!string.IsNullOrWhiteSpace(query.Search))
            filter &= Builders<MediaItem>.Filter.Regex(x => x.Title,
                new MongoDB.Bson.BsonRegularExpression(query.Search, "i"));

        var total = await _collection.CountDocumentsAsync(filter);

        var sortDefinition = BuildSort(query.SortBy, query.SortOrder);

        var items = await _collection
            .Find(filter)
            .Sort(sortDefinition)
            .Skip((query.Page - 1) * query.Limit)
            .Limit(query.Limit)
            .ToListAsync();

        return new PagedResponse<MediaItemResponse>(
            items.Select(ToResponse).ToList(),
            (int)total,
            query.Page,
            query.Limit,
            (int)Math.Ceiling((double)total / query.Limit)
        );
    }

    public async Task<MediaItemResponse?> GetByIdAsync(string userId, string id)
    {
        var item = await _collection
            .Find(x => x.Id == id && x.UserId == userId)
            .FirstOrDefaultAsync();
        return item is null ? null : ToResponse(item);
    }

    public async Task<MediaItemResponse> CreateAsync(string userId, CreateMediaRequest request)
    {
        var item = new MediaItem
        {
            UserId = userId,
            Title = request.Title,
            Type = request.Type,
            Genres = request.Genres ?? [],
            Year = request.Year,
            PosterUrl = request.PosterUrl,
            Synopsis = request.Synopsis,
            Rating = request.Rating,
            DurationMinutes = request.DurationMinutes,
            TotalEpisodes = request.TotalEpisodes,
        };

        await _collection.InsertOneAsync(item);
        return ToResponse(item);
    }

    public async Task<MediaItemResponse?> UpdateAsync(string userId, string id, UpdateMediaRequest request)
    {
        var update = Builders<MediaItem>.Update.Combine();

        if (request.Title is not null)
            update = update.Set(x => x.Title, request.Title);
        if (request.Genres is not null)
            update = update.Set(x => x.Genres, request.Genres);
        if (request.Year.HasValue)
            update = update.Set(x => x.Year, request.Year.Value);
        if (request.PosterUrl is not null)
            update = update.Set(x => x.PosterUrl, request.PosterUrl);
        if (request.Synopsis is not null)
            update = update.Set(x => x.Synopsis, request.Synopsis);
        if (request.Rating.HasValue)
            update = update.Set(x => x.Rating, request.Rating.Value);
        if (request.DurationMinutes.HasValue)
            update = update.Set(x => x.DurationMinutes, request.DurationMinutes.Value);
        if (request.TotalEpisodes.HasValue)
            update = update.Set(x => x.TotalEpisodes, request.TotalEpisodes.Value);

        var result = await _collection.FindOneAndUpdateAsync(
            x => x.Id == id && x.UserId == userId,
            update,
            new FindOneAndUpdateOptions<MediaItem> { ReturnDocument = ReturnDocument.After });

        return result is null ? null : ToResponse(result);
    }

    public async Task<bool> DeleteAsync(string userId, string id)
    {
        var result = await _collection.DeleteOneAsync(x => x.Id == id && x.UserId == userId);
        return result.DeletedCount > 0;
    }

    public async Task<MediaItemResponse?> ToggleWatchedAsync(string userId, string id)
    {
        var item = await _collection
            .Find(x => x.Id == id && x.UserId == userId)
            .FirstOrDefaultAsync();

        if (item is null) return null;

        var nowWatched = !item.IsWatched;
        var update = Builders<MediaItem>.Update
            .Set(x => x.IsWatched, nowWatched)
            .Set(x => x.Status, nowWatched ? WatchStatus.Watched : WatchStatus.ToWatch)
            .Set(x => x.WatchedAt, nowWatched ? DateTime.UtcNow : null);

        var result = await _collection.FindOneAndUpdateAsync(
            x => x.Id == id && x.UserId == userId,
            update,
            new FindOneAndUpdateOptions<MediaItem> { ReturnDocument = ReturnDocument.After });

        return result is null ? null : ToResponse(result);
    }

    public async Task<MediaItemResponse?> SetRatingAsync(string userId, string id, double userRating)
    {
        var update = Builders<MediaItem>.Update.Set(x => x.UserRating, userRating);

        var result = await _collection.FindOneAndUpdateAsync(
            x => x.Id == id && x.UserId == userId,
            update,
            new FindOneAndUpdateOptions<MediaItem> { ReturnDocument = ReturnDocument.After });

        return result is null ? null : ToResponse(result);
    }

    public async Task<MediaItemResponse?> SetEpisodesWatchedAsync(string userId, string id, int episodes)
    {
        var item = await _collection.Find(x => x.Id == id && x.UserId == userId).FirstOrDefaultAsync();
        if (item is null) return null;

        var isFinished = item.TotalEpisodes.HasValue && episodes >= item.TotalEpisodes.Value;

        var update = Builders<MediaItem>.Update
            .Set(x => x.EpisodesWatched, episodes)
            .Set(x => x.Status, isFinished ? WatchStatus.Watched : WatchStatus.Watching)
            .Set(x => x.IsWatched, isFinished)
            .Set(x => x.WatchedAt, isFinished ? DateTime.UtcNow : null);

        var result = await _collection.FindOneAndUpdateAsync(
            x => x.Id == id && x.UserId == userId,
            update,
            new FindOneAndUpdateOptions<MediaItem> { ReturnDocument = ReturnDocument.After });

        return result is null ? null : ToResponse(result);
    }

    public async Task<List<string>> GetGenresAsync(string userId)
    {
        var items = await _collection
            .Find(x => x.UserId == userId)
            .Project(x => x.Genres)
            .ToListAsync();

        return items.SelectMany(g => g).Distinct().OrderBy(g => g).ToList();
    }

    internal async Task<List<MediaItem>> GetRawByUserAsync(string userId)
    {
        return await _collection.Find(x => x.UserId == userId).ToListAsync();
    }

    private static SortDefinition<MediaItem> BuildSort(string? sortBy, string? order)
    {
        var isAsc = order?.ToLower() == "asc";
        return sortBy?.ToLower() switch
        {
            "title" => isAsc
                ? Builders<MediaItem>.Sort.Ascending(x => x.Title)
                : Builders<MediaItem>.Sort.Descending(x => x.Title),
            "year" => isAsc
                ? Builders<MediaItem>.Sort.Ascending(x => x.Year)
                : Builders<MediaItem>.Sort.Descending(x => x.Year),
            "rating" => isAsc
                ? Builders<MediaItem>.Sort.Ascending(x => x.Rating)
                : Builders<MediaItem>.Sort.Descending(x => x.Rating),
            "watchedat" => isAsc
                ? Builders<MediaItem>.Sort.Ascending(x => x.WatchedAt)
                : Builders<MediaItem>.Sort.Descending(x => x.WatchedAt),
            _ => Builders<MediaItem>.Sort.Descending(x => x.CreatedAt),
        };
    }

    private static MediaItemResponse ToResponse(MediaItem item) => new(
        item.Id!,
        item.Title,
        item.Type.ToString(),
        item.Genres,
        item.Year,
        item.PosterUrl,
        item.Synopsis,
        item.Rating,
        item.UserRating,
        item.Status.ToString(),
        item.IsWatched,
        item.DurationMinutes,
        item.TotalEpisodes,
        item.EpisodesWatched,
        item.CreatedAt,
        item.WatchedAt
    );
}
