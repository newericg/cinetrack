using CineTrack.API.DTOs;
using CineTrack.API.Models;

namespace CineTrack.API.Services;

public class StatsService
{
    private readonly MediaService _mediaService;

    public StatsService(MediaService mediaService)
    {
        _mediaService = mediaService;
    }

    public async Task<StatsResponse> GetStatsAsync(string userId)
    {
        var all = await _mediaService.GetRawByUserAsync(userId);

        var watched = all.Where(x => x.IsWatched).ToList();
        var movies = all.Where(x => x.Type == MediaType.Movie).ToList();
        var series = all.Where(x => x.Type == MediaType.Series).ToList();
        var anime = all.Where(x => x.Type == MediaType.Anime).ToList();

        var totalMinutes = watched.Sum(x =>
        {
            if (x.Type == MediaType.Movie)
                return x.DurationMinutes ?? 0;

            return (x.TotalEpisodes ?? x.EpisodesWatched) * 24;
        });

        var ratings = all.Where(x => x.UserRating.HasValue).Select(x => x.UserRating!.Value).ToList();
        var avgRating = ratings.Count > 0 ? Math.Round(ratings.Average(), 1) : 0;

        var topGenres = all
            .SelectMany(x => x.Genres)
            .GroupBy(g => g)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => new GenreStat(g.Key, g.Count()))
            .ToList();

        var watchedByMonth = watched
            .Where(x => x.WatchedAt.HasValue)
            .GroupBy(x => x.WatchedAt!.Value.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .TakeLast(12)
            .Select(g => new MonthStat(g.Key, g.Count()))
            .ToList();

        return new StatsResponse(
            TotalItems: all.Count,
            WatchedItems: watched.Count,
            ToWatchItems: all.Count(x => x.Status == WatchStatus.ToWatch),
            WatchingItems: all.Count(x => x.Status == WatchStatus.Watching),
            DroppedItems: all.Count(x => x.Status == WatchStatus.Dropped),
            TotalMovies: movies.Count,
            WatchedMovies: movies.Count(x => x.IsWatched),
            TotalSeries: series.Count,
            WatchedSeries: series.Count(x => x.IsWatched),
            TotalAnime: anime.Count,
            WatchedAnime: anime.Count(x => x.IsWatched),
            TotalHoursWatched: totalMinutes / 60,
            AverageUserRating: avgRating,
            TopGenres: topGenres,
            WatchedByMonth: watchedByMonth
        );
    }
}
