import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MediaService } from '../../core/services/media.service';
import { MediaItem } from '../../core/models/media.model';

@Component({
  selector: 'app-media-detail',
  standalone: true,
  imports: [],
  templateUrl: './media-detail.component.html',
  styleUrl: './media-detail.component.scss',
})
export class MediaDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly mediaService = inject(MediaService);

  readonly item = signal<MediaItem | null>(null);
  readonly similar = signal<MediaItem[]>([]);
  readonly isLoading = signal(true);
  readonly isTogglingWatched = signal(false);
  readonly userRatingInput = signal(0);
  readonly showRatingInput = signal(false);

  readonly heroImage = computed(() => {
    const i = this.item();
    return i?.backdropUrl || i?.posterUrl || null;
  });

  readonly displayRating = computed(() => {
    const r = this.item()?.rating;
    if (!r) return null;
    return (r / 2).toFixed(1);
  });

  readonly ratingDistribution = computed(() => {
    const r = this.item()?.rating ?? 5;
    const norm = r / 10;
    const star5 = Math.round(norm * norm * 85 + 5);
    const star4 = Math.round((1 - norm) * 30 + 8);
    const star3 = Math.max(2, 100 - star5 - star4 - 3 - 1);
    const star2 = 3;
    const star1 = 1;
    return [
      { label: 5, pct: star5 },
      { label: 4, pct: star4 },
      { label: 3, pct: star3 },
      { label: 2, pct: star2 },
      { label: 1, pct: star1 },
    ];
  });

  readonly formattedDuration = computed(() => {
    const mins = this.item()?.durationMinutes;
    if (!mins) return null;
    const h = Math.floor(mins / 60);
    const m = mins % 60;
    return h > 0 ? `${h}h ${m}m` : `${m}m`;
  });

  readonly typeLabel = computed(() => {
    const type = this.item()?.type;
    if (type === 'Movie') return 'movies';
    if (type === 'Series') return 'series';
    return 'anime';
  });

  readonly trailerThumbnail = computed(() => {
    const id = this.item()?.trailerUrl;
    if (!id) return null;
    return `https://img.youtube.com/vi/${id}/maxresdefault.jpg`;
  });

  readonly trailerEmbedUrl = computed(() => {
    const id = this.item()?.trailerUrl;
    if (!id) return null;
    return `https://www.youtube.com/watch?v=${id}`;
  });

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) { this.router.navigate(['/dashboard']); return; }

    this.mediaService.getCatalogItem(id).subscribe({
      next: (item) => {
        this.item.set(item);
        this.userRatingInput.set(item.userRating ?? 0);
        this.isLoading.set(false);
        this.loadSimilar(item);
      },
      error: () => {
        this.isLoading.set(false);
        this.router.navigate(['/dashboard']);
      },
    });
  }

  private loadSimilar(item: MediaItem) {
    this.mediaService.getCatalog({ type: item.type, limit: 6 }).subscribe((res) => {
      this.similar.set(res.items.filter((i) => i.id !== item.id).slice(0, 5));
    });
  }

  onAddToList() {
    const id = this.item()?.id;
    if (!id) return;
    this.mediaService.addToList({ catalogItemId: id }).subscribe((updated) => {
      this.item.set(updated);
    });
  }

  onToggleWatched() {
    const id = this.item()?.id;
    if (!id || this.isTogglingWatched() || !this.item()?.inUserList) return;
    this.isTogglingWatched.set(true);
    this.mediaService.toggleWatched(id).subscribe({
      next: (updated) => {
        this.item.set(updated);
        this.isTogglingWatched.set(false);
      },
      error: () => this.isTogglingWatched.set(false),
    });
  }

  onOpenTrailer() {
    const url = this.trailerEmbedUrl();
    if (url) window.open(url, '_blank');
  }

  toggleRatingInput() {
    this.showRatingInput.update((v) => !v);
  }

  onSetRating(rating: number) {
    const id = this.item()?.id;
    if (!id || !this.item()?.inUserList) return;
    this.userRatingInput.set(rating);
    this.mediaService.setRating(id, rating).subscribe((updated) => {
      this.item.set(updated);
      this.showRatingInput.set(false);
    });
  }

  get stars(): number[] {
    return [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
  }

  get posterFallback(): string {
    const title = this.item()?.title ?? 'N/A';
    return `https://placehold.co/300x450/111111/FF0000?text=${encodeURIComponent(title)}`;
  }

  navigateSimilar(id: string) {
    this.router.navigate(['/media', id]);
  }
}
