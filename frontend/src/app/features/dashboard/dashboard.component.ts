import {
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  ViewChild,
  computed,
  inject,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  Chart,
  ArcElement,
  BarElement,
  CategoryScale,
  LinearScale,
  Tooltip,
  DoughnutController,
  BarController,
} from 'chart.js';
import { AuthService } from '../../core/services/auth.service';
import { I18nService } from '../../core/services/i18n.service';
import { MediaService } from '../../core/services/media.service';
import { StatsService } from '../../core/services/stats.service';
import { Stats } from '../../core/models/stats.model';
import { MediaItem } from '../../core/models/media.model';

Chart.register(
  ArcElement,
  BarElement,
  CategoryScale,
  LinearScale,
  Tooltip,
  DoughnutController,
  BarController
);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('genreCanvas') genreCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('barCanvas') barCanvas!: ElementRef<HTMLCanvasElement>;

  private readonly statsService = inject(StatsService);
  private readonly mediaService = inject(MediaService);
  readonly authService = inject(AuthService);
  readonly i18n = inject(I18nService);

  readonly stats = signal<Stats | null>(null);
  readonly recentlyWatched = signal<MediaItem[]>([]);
  readonly upNext = signal<MediaItem[]>([]);
  readonly isLoading = signal(true);

  private genreChart?: Chart;
  private barChart?: Chart;

  readonly firstName = computed(() => {
    const name = this.authService.currentUser()?.name ?? '';
    return name.split(' ')[0];
  });

  ngOnInit() {
    this.loadData();
  }

  ngAfterViewInit() {
    // Charts init after stats load
  }

  private loadData() {
    this.statsService.getStats().subscribe({
      next: (s) => {
        this.stats.set(s);
        this.isLoading.set(false);
        setTimeout(() => this.initCharts(s), 50);
      },
      error: () => this.isLoading.set(false),
    });

    this.mediaService
      .getAll({ status: 'Watched', sortBy: 'watchedAt', sortOrder: 'desc', limit: 10 })
      .subscribe((res) => this.recentlyWatched.set(res.items));

    this.mediaService
      .getAll({ status: 'Watching', sortBy: 'createdAt', sortOrder: 'desc', limit: 6 })
      .subscribe((res) => this.upNext.set(res.items));
  }

  private initCharts(s: Stats) {
    this.initGenreChart(s);
    this.initBarChart(s);
  }

  private initGenreChart(s: Stats) {
    if (!this.genreCanvas?.nativeElement) return;
    this.genreChart?.destroy();

    const labels = s.topGenres.slice(0, 5).map((g) => g.genre);
    const data = s.topGenres.slice(0, 5).map((g) => g.count);
    const colors = ['#E11D48', '#ffffff', '#6b7280', '#fb7185', '#374151'];

    this.genreChart = new Chart(this.genreCanvas.nativeElement, {
      type: 'doughnut',
      data: {
        labels,
        datasets: [{ data, backgroundColor: colors, borderWidth: 0, hoverOffset: 4 }],
      },
      options: {
        cutout: '80%',
        plugins: { legend: { display: false }, tooltip: { enabled: true } },
      },
    });
  }

  private initBarChart(s: Stats) {
    if (!this.barCanvas?.nativeElement) return;
    this.barChart?.destroy();

    const sorted = [...s.watchedByMonth].slice(-8);
    const labels = sorted.map((m) => {
      const [year, month] = m.month.split('-');
      return new Date(+year, +month - 1).toLocaleString('default', { month: 'short' });
    });
    const data = sorted.map((m) => m.count);

    const ctx = this.barCanvas.nativeElement.getContext('2d')!;
    const gradient = ctx.createLinearGradient(0, 0, 0, 300);
    gradient.addColorStop(0, '#E11D48');
    gradient.addColorStop(1, 'rgba(225, 29, 72, 0.08)');

    this.barChart = new Chart(this.barCanvas.nativeElement, {
      type: 'bar',
      data: {
        labels,
        datasets: [
          {
            label: 'Items Watched',
            data,
            backgroundColor: gradient,
            borderRadius: 8,
            maxBarThickness: 30,
          },
        ],
      },
      options: {
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          y: {
            grid: { color: 'rgba(255,255,255,0.05)' },
            ticks: { color: '#94a3b8' },
          },
          x: { grid: { display: false }, ticks: { color: '#94a3b8' } },
        },
      },
    });
  }

  posterFallback(title: string): string {
    return `https://placehold.co/300x450/111111/FF0000?text=${encodeURIComponent(title)}`;
  }

  ngOnDestroy() {
    this.genreChart?.destroy();
    this.barChart?.destroy();
  }
}
