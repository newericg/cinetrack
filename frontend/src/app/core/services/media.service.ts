import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import {
  AddToListRequest,
  CatalogQueryParams,
  MediaItem,
  MediaQueryParams,
  PagedResponse,
} from '../models/media.model';

@Injectable({ providedIn: 'root' })
export class MediaService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  /** Catálogo global — todos veem. Se autenticado, inclui overlay (status, rating). */
  getCatalog(query: CatalogQueryParams = {}) {
    let params = new HttpParams();
    if (query.type) params = params.set('type', query.type);
    if (query.genre) params = params.set('genre', query.genre);
    if (query.search) params = params.set('search', query.search);
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortOrder) params = params.set('sortOrder', query.sortOrder);
    if (query.page) params = params.set('page', query.page.toString());
    if (query.limit) params = params.set('limit', query.limit.toString());

    return this.http.get<PagedResponse<MediaItem>>(`${this.apiUrl}/catalog`, { params });
  }

  getCatalogItem(id: string) {
    return this.http.get<MediaItem>(`${this.apiUrl}/catalog/${id}`);
  }

  getCatalogGenres() {
    return this.http.get<string[]>(`${this.apiUrl}/catalog/genres`);
  }

  /** Lista individual do usuário (itens que adicionou). */
  getMyList(query: MediaQueryParams = {}) {
    let params = new HttpParams();
    if (query.type) params = params.set('type', query.type);
    if (query.genre) params = params.set('genre', query.genre);
    if (query.status) params = params.set('status', query.status);
    if (query.search) params = params.set('search', query.search);
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortOrder) params = params.set('sortOrder', query.sortOrder);
    if (query.page) params = params.set('page', query.page.toString());
    if (query.limit) params = params.set('limit', query.limit.toString());

    return this.http.get<PagedResponse<MediaItem>>(`${this.apiUrl}/media`, { params });
  }

  addToList(request: AddToListRequest) {
    return this.http.post<MediaItem>(`${this.apiUrl}/media`, request);
  }

  removeFromList(catalogItemId: string) {
    return this.http.delete<void>(`${this.apiUrl}/media/${catalogItemId}`);
  }

  toggleWatched(catalogItemId: string) {
    return this.http.patch<MediaItem>(`${this.apiUrl}/media/${catalogItemId}/watched`, {});
  }

  setRating(catalogItemId: string, userRating: number) {
    return this.http.patch<MediaItem>(`${this.apiUrl}/media/${catalogItemId}/rating`, { userRating });
  }

  setEpisodes(catalogItemId: string, episodesWatched: number) {
    return this.http.patch<MediaItem>(`${this.apiUrl}/media/${catalogItemId}/episodes`, {
      episodesWatched,
    });
  }
}
