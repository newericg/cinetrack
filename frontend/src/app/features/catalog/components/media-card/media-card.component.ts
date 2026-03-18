import { Component, input, output } from '@angular/core';
import { MediaItem } from '../../../../core/models/media.model';

@Component({
  selector: 'app-media-card',
  standalone: true,
  templateUrl: './media-card.component.html',
  styleUrl: './media-card.component.scss',
})
export class MediaCardComponent {
  readonly item = input.required<MediaItem>();
  readonly toggleWatched = output<string>();
  readonly setRating = output<{ id: string; rating: number }>();

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
