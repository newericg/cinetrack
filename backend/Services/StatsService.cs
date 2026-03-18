using CineTrack.API.DTOs;
using CineTrack.API.Models;

namespace CineTrack.API.Services;

public class StatsService
{
    private readonly UserMediaService _userMediaService;

    public StatsService(UserMediaService userMediaService)
    {
        _userMediaService = userMediaService;
    }

    public async Task<StatsResponse> GetStatsAsync(string userId)
    {
        var all = await _userMediaService.GetRawByUserAsync(userId);

        var watched = all.Where(x => x.User.IsWatched).ToList();
        var movies = all.Where(x => x.Catalog.Type == MediaType.Movie).ToList();
        var series = all.Where(x => x.Catalog.Type == MediaType.Series).ToList();
        var anime = all.Where(x => x.Catalog.Type == MediaType.Anime).ToList();

        var totalMinutes = watched.Sum(x =>
        {
            if (x.Catalog.Type == MediaType.Movie)
                return x.Catalog.DurationMinutes ?? 0;
            return (x.Catalog.TotalEpisodes ?? x.User.EpisodesWatched) * 24;
        });

        var ratings = all.Where(x => x.User.UserRating.HasValue).Select(x => x.User.UserRating!.Value).ToList();
        var avgRating = ratings.Count > 0 ? Math.Round(ratings.Average(), 1) : 0;

        var topGenres = all
            .SelectMany(x => x.Catalog.Genres)
            .GroupBy(g => g)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => new GenreStat(g.Key, g.Count()))
            .ToList();

        var watchedByMonth = watched
            .Where(x => x.User.WatchedAt.HasValue)
            .GroupBy(x => x.User.WatchedAt!.Value.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .TakeLast(12)
            .Select(g => new MonthStat(g.Key, g.Count()))
            .ToList();

        return new StatsResponse(
            TotalItems: all.Count,
            WatchedItems: watched.Count,
            ToWatchItems: all.Count(x => x.User.Status == WatchStatus.ToWatch),
            WatchingItems: all.Count(x => x.User.Status == WatchStatus.Watching),
            DroppedItems: all.Count(x => x.User.Status == WatchStatus.Dropped),
            TotalMovies: movies.Count,
            WatchedMovies: movies.Count(x => x.User.IsWatched),
            TotalSeries: series.Count,
            WatchedSeries: series.Count(x => x.User.IsWatched),
            TotalAnime: anime.Count,
            WatchedAnime: anime.Count(x => x.User.IsWatched),
            TotalHoursWatched: totalMinutes / 60,
            AverageUserRating: avgRating,
            TopGenres: topGenres,
            WatchedByMonth: watchedByMonth
        );
    }
}
