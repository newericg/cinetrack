export interface GenreStat {
  genre: string;
  count: number;
}

export interface MonthStat {
  month: string;
  count: number;
}

export interface Stats {
  totalItems: number;
  watchedItems: number;
  toWatchItems: number;
  watchingItems: number;
  droppedItems: number;
  totalMovies: number;
  watchedMovies: number;
  totalSeries: number;
  watchedSeries: number;
  totalAnime: number;
  watchedAnime: number;
  totalHoursWatched: number;
  averageUserRating: number;
  topGenres: GenreStat[];
  watchedByMonth: MonthStat[];
}
