export type MediaType = 'Movie' | 'Series' | 'Anime';
export type WatchStatus = 'ToWatch' | 'Watching' | 'Watched' | 'Dropped';

export interface MediaItem {
  id: string;
  title: string;
  type: MediaType;
  genres: string[];
  year: number;
  posterUrl: string | null;
  synopsis: string | null;
  rating: number | null;
  userRating: number | null;
  status: WatchStatus;
  isWatched: boolean;
  durationMinutes: number | null;
  totalEpisodes: number | null;
  episodesWatched: number;
  createdAt: string;
  watchedAt: string | null;
}

export interface PagedResponse<T> {
  items: T[];
  total: number;
  page: number;
  limit: number;
  totalPages: number;
}

export interface MediaQueryParams {
  type?: MediaType;
  genre?: string;
  status?: WatchStatus;
  search?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
  page?: number;
  limit?: number;
}

export interface CreateMediaRequest {
  title: string;
  type: MediaType;
  genres?: string[];
  year: number;
  posterUrl?: string;
  synopsis?: string;
  rating?: number;
  durationMinutes?: number;
  totalEpisodes?: number;
}
