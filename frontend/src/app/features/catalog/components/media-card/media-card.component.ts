import { Component, input, output, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MediaItem } from '../../../../core/models/media.model';

@Component({
  selector: 'app-media-card',
  standalone: true,
  templateUrl: './media-card.component.html',
  styleUrl: './media-card.component.scss',
})
export class MediaCardComponent {
  private readonly router = inject(Router);

  readonly item = input.required<MediaItem>();
  readonly addToList = output<string>();
  readonly removeFromList = output<string>();
  readonly toggleWatched = output<string>();
  readonly setRating = output<{ id: string; rating: number }>();

  navigateToDetail() {
    this.router.navigate(['/media', this.item().id]);
  }

  onAddToList(event: Event) {
    event.stopPropagation();
    this.addToList.emit(this.item().id);
  }

  onRemoveFromList(event: Event) {
    event.stopPropagation();
    this.removeFromList.emit(this.item().id);
  }

  onToggleWatched(event: Event) {
    event.stopPropagation();
    this.toggleWatched.emit(this.item().id);
  }

  onStarClick(event: Event) {
    event.stopPropagation();
    const current = this.item().userRating ?? 0;
    const next = current >= 10 ? 0 : Math.min(current + 1, 10);
    this.setRating.emit({ id: this.item().id, rating: next });
  }

  get posterFallback(): string {
    return `https://placehold.co/300x450/111111/FF0000?text=${encodeURIComponent(this.item().title)}`;
  }
}
