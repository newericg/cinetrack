import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AchievementService } from '../../core/services/achievement.service';
import {
  Achievement,
  AchievementCategory,
  AchievementsResponse,
} from '../../core/models/achievement.model';

type CategoryFilter = 'All' | AchievementCategory;

interface CategoryTab {
  label: string;
  value: CategoryFilter;
}

@Component({
  selector: 'app-achievements',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './achievements.component.html',
  styleUrl: './achievements.component.scss',
})
export class AchievementsComponent implements OnInit {
  private readonly achievementService = inject(AchievementService);

  readonly data = signal<AchievementsResponse | null>(null);
  readonly isLoading = signal(false);
  readonly searchQuery = signal('');
  readonly activeCategory = signal<CategoryFilter>('All');

  readonly categories: CategoryTab[] = [
    { label: 'All Badges', value: 'All' },
    { label: 'Movie Buff', value: 'MovieBuff' },
    { label: 'Series Marathoner', value: 'SeriesMarathoner' },
    { label: 'Anime Expert', value: 'AnimeExpert' },
    { label: 'General', value: 'General' },
  ];

  readonly filteredAchievements = computed(() => {
    const response = this.data();
    if (!response) return [];

    const query = this.searchQuery().toLowerCase();
    const cat = this.activeCategory();

    return response.achievements.filter((a) => {
      const matchesCat = cat === 'All' || a.category === cat;
      const matchesSearch =
        !query ||
        a.name.toLowerCase().includes(query) ||
        a.description.toLowerCase().includes(query);
      return matchesCat && matchesSearch;
    });
  });

  ngOnInit() {
    this.load();
  }

  load() {
    this.isLoading.set(true);
    this.achievementService.getAchievements().subscribe({
      next: (res) => {
        this.data.set(res);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  setCategory(cat: CategoryFilter) {
    this.activeCategory.set(cat);
  }

  onSearch(value: string) {
    this.searchQuery.set(value);
  }

  progressLabel(a: Achievement): string {
    if (a.isCompleted) return 'COMPLETED';
    return `${a.progressPercent}%`;
  }

  currentLabel(a: Achievement): string {
    if (a.isLocked) return `? / ${a.targetValue}`;
    return `${a.currentValue} / ${a.targetValue}`;
  }
}
