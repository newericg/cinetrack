import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MediaCardComponent } from './components/media-card/media-card.component';
import { MediaService } from '../../core/services/media.service';
import {
  CatalogQueryParams,
  MediaItem,
  MediaQueryParams,
  MediaType,
  WatchStatus,
} from '../../core/models/media.model';

@Component({
  selector: 'app-catalog',
  standalone: true,
  imports: [FormsModule, MediaCardComponent],
  templateUrl: './catalog.component.html',
  styleUrl: './catalog.component.scss',
})
export class CatalogComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly mediaService = inject(MediaService);

  readonly mediaType = signal<MediaType | undefined>(undefined);

  readonly items = signal<MediaItem[]>([]);
  readonly total = signal(0);
  readonly totalPages = signal(0);
  readonly isLoading = signal(false);
  readonly genres = signal<string[]>([]);

  readonly viewMode = signal<'catalog' | 'mylist'>('catalog');
  readonly selectedStatus = signal<WatchStatus | ''>('');
  readonly selectedGenre = signal('');
  readonly searchQuery = signal('');
  readonly currentPage = signal(1);
  readonly activeTypeFilter = signal<MediaType | 'All'>('All');

  readonly pageTitle = computed(() => {
    const type = this.mediaType();
    if (type === 'Movie') return 'Movies';
    if (type === 'Series') return 'Series';
    if (type === 'Anime') return 'Anime';
    return 'All';
  });

  ngOnInit() {
    const type = this.route.snapshot.data['type'] as MediaType | undefined;
    this.mediaType.set(type);
    if (type) this.activeTypeFilter.set(type);
    this.loadItems();
    this.loadGenres();
  }

  loadItems() {
    this.isLoading.set(true);

    const page = this.currentPage();
    const limit = 20;
    const typeFilter = this.activeTypeFilter();
    const genre = this.selectedGenre();
    const search = this.searchQuery();
    const status = this.selectedStatus();
    const isMyList = this.viewMode() === 'mylist';

    if (isMyList) {
      const query: MediaQueryParams = { page, limit };
      if (typeFilter !== 'All') query.type = typeFilter;
      if (genre) query.genre = genre;
      if (search) query.search = search;
      if (status) query.status = status;

      this.mediaService.getMyList(query).subscribe({
        next: (res) => this.handleResponse(res, page),
        error: () => this.isLoading.set(false),
      });
    } else {
      const query: CatalogQueryParams = { page, limit };
      if (typeFilter !== 'All') query.type = typeFilter;
      if (genre) query.genre = genre;
      if (search) query.search = search;

      this.mediaService.getCatalog(query).subscribe({
        next: (res) => this.handleResponse(res, page),
        error: () => this.isLoading.set(false),
      });
    }
  }

  private handleResponse(res: { items: MediaItem[]; total: number; totalPages: number }, page: number) {
    if (page === 1) {
      this.items.set(res.items);
    } else {
      this.items.update((prev) => [...prev, ...res.items]);
    }
    this.total.set(res.total);
    this.totalPages.set(res.totalPages);
    this.isLoading.set(false);
  }

  loadGenres() {
    this.mediaService.getCatalogGenres().subscribe((g) => this.genres.set(g));
  }

  setViewMode(mode: 'catalog' | 'mylist') {
    this.viewMode.set(mode);
    this.currentPage.set(1);
    this.loadItems();
  }

  setTypeFilter(type: MediaType | 'All') {
    if (this.mediaType()) return;
    this.activeTypeFilter.set(type);
    this.currentPage.set(1);
    this.loadItems();
  }

  onStatusChange(value: string) {
    this.selectedStatus.set(value as WatchStatus | '');
    this.currentPage.set(1);
    this.loadItems();
  }

  onGenreChange(value: string) {
    this.selectedGenre.set(value);
    this.currentPage.set(1);
    this.loadItems();
  }

  onSearch(value: string) {
    this.searchQuery.set(value);
    this.currentPage.set(1);
    this.loadItems();
  }

  loadMore() {
    if (this.currentPage() < this.totalPages()) {
      this.currentPage.update((p) => p + 1);
      this.loadItems();
    }
  }

  onAddToList(catalogItemId: string) {
    this.mediaService.addToList({ catalogItemId }).subscribe((updated) => {
      this.items.update((list) =>
        list.map((item) => (item.id === catalogItemId ? updated : item))
      );
    });
  }

  onRemoveFromList(catalogItemId: string) {
    this.mediaService.removeFromList(catalogItemId).subscribe({
      next: () => {
        if (this.viewMode() === 'mylist') {
          this.items.update((list) => list.filter((item) => item.id !== catalogItemId));
          this.total.update((t) => Math.max(0, t - 1));
        } else {
          this.items.update((list) =>
            list.map((item) =>
              item.id === catalogItemId ? { ...item, inUserList: false } : item
            )
          );
        }
      },
    });
  }

  onToggleWatched(catalogItemId: string) {
    this.mediaService.toggleWatched(catalogItemId).subscribe((updated) => {
      this.items.update((list) =>
        list.map((item) => (item.id === catalogItemId ? updated : item))
      );
    });
  }

  onSetRating(event: { id: string; rating: number }) {
    this.mediaService.setRating(event.id, event.rating).subscribe((updated) => {
      this.items.update((list) =>
        list.map((item) => (item.id === event.id ? updated : item))
      );
    });
  }

  get hasMore(): boolean {
    return this.currentPage() < this.totalPages();
  }

  get watchedCount(): number {
    return this.items().filter((i) => i.isWatched).length;
  }

  readonly typeFilters: Array<MediaType | 'All'> = ['All', 'Movie', 'Series', 'Anime'];
}
