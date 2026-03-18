using CineTrack.API.DTOs;
using CineTrack.API.Models;

namespace CineTrack.API.Services;

public class AchievementService
{
    private record AchievementDefinition(
        string Id,
        string Name,
        string Description,
        string Icon,
        string Category,
        int? Level,
        int TargetValue,
        string? Prerequisite
    );

    private readonly UserMediaService _userMediaService;

    public AchievementService(UserMediaService userMediaService)
    {
        _userMediaService = userMediaService;
    }

    private static readonly List<AchievementDefinition> Definitions =
    [
        // Movie Buff
        new("first_frame",      "First Frame",      "Watch your very first movie.",            "theaters",    "MovieBuff",        1, 1,   null),
        new("casual_viewer",    "Casual Viewer",    "Watch 10 different movies.",              "theaters",    "MovieBuff",        1, 10,  null),
        new("movie_lover",      "Movie Lover",      "Watch 100 different movies.",             "movie",       "MovieBuff",        2, 100, null),
        new("cinephile_elite",  "Cinephile Elite",  "Watch 500 different movies.",             "theaters",    "MovieBuff",        3, 500, null),
        new("historian",        "Historian",        "Watch 20 movies released before 1960.",   "history_edu", "MovieBuff",        null, 20, "watched_movies_5"),

        // Series Marathoner
        new("marathoner",       "Marathoner",       "Complete 5 series.",                      "live_tv",     "SeriesMarathoner", 1, 5,  null),
        new("series_master",    "Series Master",    "Complete 20 series.",                     "live_tv",     "SeriesMarathoner", 2, 20, null),
        new("binge_king",       "Binge King",       "Complete 50 series.",                     "live_tv",     "SeriesMarathoner", 3, 50, null),

        // Anime Expert
        new("otaku_beginner",   "Otaku Beginner",   "Complete your first anime series.",       "animation",   "AnimeExpert",      1, 1,  null),
        new("otaku_spirit",     "Otaku Spirit",     "Complete 10 different anime series.",     "animation",   "AnimeExpert",      2, 10, null),
        new("anime_devotee",    "Anime Devotee",    "Complete 25 different anime series.",     "animation",   "AnimeExpert",      3, 25, null),

        // General
        new("weekend_warrior",  "Weekend Warrior",  "Watch 50 hours of content.",              "schedule",    "General",          null, 50,  null),
        new("genre_specialist", "Genre Specialist", "Watch 50 Sci-Fi titles.",                 "favorite",    "General",          null, 50,  "watched_10"),
        new("critics_eye",      "Critic's Eye",     "Rate 100 different titles.",              "auto_awesome","General",          null, 100, "rated_5"),
        new("perfectionist",    "Perfectionist",    "Give a 10/10 rating to 20 titles.",       "stars",       "General",          null, 20,  "rated_5"),
        new("library_master",   "Library Master",   "Add 200 titles to your library.",         "inventory_2", "General",          null, 200, null),
    ];

    public async Task<AchievementsResponse> GetAchievementsAsync(string userId)
    {
        var all = await _userMediaService.GetRawByUserAsync(userId);

        var watchedMovies = all.Where(x => x.Catalog.Type == MediaType.Movie && x.User.IsWatched).ToList();
        var watchedSeries = all.Where(x => x.Catalog.Type == MediaType.Series && x.User.IsWatched).ToList();
        var watchedAnime  = all.Where(x => x.Catalog.Type == MediaType.Anime && x.User.IsWatched).ToList();
        var ratedItems    = all.Where(x => x.User.UserRating.HasValue).ToList();
        var sciFiItems    = all.Where(x => x.User.IsWatched && x.Catalog.Genres.Contains("Sci-Fi")).ToList();
        var oldMovies     = watchedMovies.Where(x => x.Catalog.Year > 0 && x.Catalog.Year < 1960).ToList();
        var topRated      = all.Where(x => x.User.UserRating.HasValue && x.User.UserRating.Value >= 10).ToList();

        var totalMinutes = all.Where(x => x.User.IsWatched).Sum(x =>
        {
            if (x.Catalog.Type == MediaType.Movie)
                return x.Catalog.DurationMinutes ?? 0;
            return (x.Catalog.TotalEpisodes ?? x.User.EpisodesWatched) * 24;
        });
        var totalHours = totalMinutes / 60;

        var prerequisites = new Dictionary<string, bool>
        {
            ["watched_movies_5"] = watchedMovies.Count >= 5,
            ["watched_10"]       = all.Count(x => x.User.IsWatched) >= 10,
            ["rated_5"]          = ratedItems.Count >= 5,
        };

        var computedValues = new Dictionary<string, int>
        {
            ["first_frame"]      = watchedMovies.Count,
            ["casual_viewer"]    = watchedMovies.Count,
            ["movie_lover"]      = watchedMovies.Count,
            ["cinephile_elite"]  = watchedMovies.Count,
            ["historian"]        = oldMovies.Count,
            ["marathoner"]       = watchedSeries.Count,
            ["series_master"]    = watchedSeries.Count,
            ["binge_king"]       = watchedSeries.Count,
            ["otaku_beginner"]   = watchedAnime.Count,
            ["otaku_spirit"]     = watchedAnime.Count,
            ["anime_devotee"]    = watchedAnime.Count,
            ["weekend_warrior"]  = totalHours,
            ["genre_specialist"] = sciFiItems.Count,
            ["critics_eye"]      = ratedItems.Count,
            ["perfectionist"]    = topRated.Count,
            ["library_master"]   = all.Count,
        };

        var achievements = Definitions.Select(def =>
        {
            var current    = computedValues.GetValueOrDefault(def.Id, 0);
            var isLocked   = def.Prerequisite != null && !prerequisites.GetValueOrDefault(def.Prerequisite, false);
            var isCompleted = !isLocked && current >= def.TargetValue;
            var progress   = isLocked ? 0 : (int)Math.Min(100, Math.Round((double)current / def.TargetValue * 100));

            return new AchievementDto(
                def.Id,
                def.Name,
                def.Description,
                def.Icon,
                def.Category,
                def.Level,
                isLocked ? 0 : current,
                def.TargetValue,
                progress,
                isCompleted,
                isLocked
            );
        }).ToList();

        var totalUnlocked      = achievements.Count(a => a.IsCompleted);
        var rarestCount        = achievements.Count(a => a.IsCompleted && a.TargetValue >= 100);
        var completionPercent  = Definitions.Count > 0
            ? (int)Math.Round((double)totalUnlocked / Definitions.Count * 100)
            : 0;

        return new AchievementsResponse(
            achievements,
            totalUnlocked,
            Definitions.Count,
            rarestCount,
            completionPercent
        );
    }
}
