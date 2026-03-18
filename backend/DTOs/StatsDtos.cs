namespace CineTrack.API.DTOs;

public record StatsResponse(
    int TotalItems,
    int WatchedItems,
    int ToWatchItems,
    int WatchingItems,
    int DroppedItems,
    int TotalMovies,
    int WatchedMovies,
    int TotalSeries,
    int WatchedSeries,
    int TotalAnime,
    int WatchedAnime,
    int TotalHoursWatched,
    double AverageUserRating,
    List<GenreStat> TopGenres,
    List<MonthStat> WatchedByMonth
);

public record GenreStat(string Genre, int Count);
public record MonthStat(string Month, int Count);
