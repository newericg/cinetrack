import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import {
  CreateMediaRequest,
  MediaItem,
  MediaQueryParams,
  PagedResponse,
} from '../models/media.model';

@Injectable({ providedIn: 'root' })
export class MediaService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/media`;

  getAll(query: MediaQueryParams = {}) {
    let params = new HttpParams();
    if (query.type) params = params.set('type', query.type);
    if (query.genre) params = params.set('genre', query.genre);
    if (query.status) params = params.set('status', query.status);
    if (query.search) params = params.set('search', query.search);
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortOrder) params = params.set('sortOrder', query.sortOrder);
    if (query.page) params = params.set('page', query.page.toString());
    if (query.limit) params = params.set('limit', query.limit.toString());

    return this.http.get<PagedResponse<MediaItem>>(this.apiUrl, { params });
  }

  getById(id: string) {
    return this.http.get<MediaItem>(`${this.apiUrl}/${id}`);
  }

  getGenres() {
    return this.http.get<string[]>(`${this.apiUrl}/genres`);
  }

  create(request: CreateMediaRequest) {
    return this.http.post<MediaItem>(this.apiUrl, request);
  }

  update(id: string, request: Partial<CreateMediaRequest>) {
    return this.http.put<MediaItem>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  toggleWatched(id: string) {
    return this.http.patch<MediaItem>(`${this.apiUrl}/${id}/watched`, {});
  }

  setRating(id: string, userRating: number) {
    return this.http.patch<MediaItem>(`${this.apiUrl}/${id}/rating`, { userRating });
  }

  setEpisodes(id: string, episodesWatched: number) {
    return this.http.patch<MediaItem>(`${this.apiUrl}/${id}/episodes`, { episodesWatched });
  }
}
