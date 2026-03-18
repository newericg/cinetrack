export type MediaType = 'Movie' | 'Series' | 'Anime';
export type WatchStatus = 'ToWatch' | 'Watching' | 'Watched' | 'Dropped';

export interface CastMember {
  name: string;
  character: string | null;
  photoUrl: string | null;
}

export interface MediaItem {
  id: string;
  catalogItemId: string;
  title: string;
  type: MediaType;
  genres: string[];
  year: number;
  posterUrl: string | null;
  backdropUrl: string | null;
  synopsis: string | null;
  rating: number | null;
  userRating: number | null;
  status: WatchStatus;
  isWatched: boolean;
  durationMinutes: number | null;
  totalEpisodes: number | null;
  episodesWatched: number;
  director: string | null;
  writers: string[];
  studio: string | null;
  cast: CastMember[];
  trailerUrl: string | null;
  createdAt: string;
  watchedAt: string | null;
  addedAt?: string | null;
  inUserList: boolean;
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

export interface CatalogQueryParams {
  type?: MediaType;
  genre?: string;
  search?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
  page?: number;
  limit?: number;
}

export interface AddToListRequest {
  catalogItemId: string;
  status?: WatchStatus;
}
