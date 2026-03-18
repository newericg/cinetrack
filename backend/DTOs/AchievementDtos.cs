namespace CineTrack.API.DTOs;

public record AchievementDto(
    string Id,
    string Name,
    string Description,
    string Icon,
    string Category,
    int? Level,
    int CurrentValue,
    int TargetValue,
    int ProgressPercent,
    bool IsCompleted,
    bool IsLocked
);

public record AchievementsResponse(
    List<AchievementDto> Achievements,
    int TotalUnlocked,
    int TotalAchievements,
    int RarestCount,
    int CompletionPercent
);
