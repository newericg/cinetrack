namespace CineTrack.API.Services;

internal record AchievementDefinition(
    string Id,
    string Name,
    string Description,
    string Icon,
    string Category,
    int? Level,
    int TargetValue,
    string? Prerequisite
);
