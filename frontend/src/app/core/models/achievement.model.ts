export type AchievementCategory = 'MovieBuff' | 'SeriesMarathoner' | 'AnimeExpert' | 'General';

export interface Achievement {
  id: string;
  name: string;
  description: string;
  icon: string;
  category: AchievementCategory;
  level: number | null;
  currentValue: number;
  targetValue: number;
  progressPercent: number;
  isCompleted: boolean;
  isLocked: boolean;
}

export interface AchievementsResponse {
  achievements: Achievement[];
  totalUnlocked: number;
  totalAchievements: number;
  rarestCount: number;
  completionPercent: number;
}
