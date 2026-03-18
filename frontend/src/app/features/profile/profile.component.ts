import {
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { StatsService } from '../../core/services/stats.service';
import { User } from '../../core/models/auth.model';
import { Stats } from '../../core/models/stats.model';

type ProfileTab = 'account' | 'notifications' | 'privacy' | 'services';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
})
export class ProfileComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly statsService = inject(StatsService);
  private readonly fb = inject(FormBuilder);

  readonly user = signal<User | null>(null);
  readonly stats = signal<Stats | null>(null);
  readonly isLoading = signal(true);
  readonly activeTab = signal<ProfileTab>('account');
  readonly isSaving = signal(false);
  readonly saveSuccess = signal(false);
  readonly saveError = signal('');

  readonly profileForm = this.fb.group({
    name: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    timezone: [''],
  });

  readonly watchDays = computed(() =>
    Math.floor((this.stats()?.totalHoursWatched ?? 0) / 24)
  );

  readonly watchHours = computed(() =>
    (this.stats()?.totalHoursWatched ?? 0) % 24
  );

  readonly moviePercent = computed(() => {
    const s = this.stats();
    if (!s) return 0;
    const total = s.totalMovies + s.totalSeries + s.totalAnime;
    return total === 0 ? 0 : Math.round((s.totalMovies / total) * 100);
  });

  readonly seriesPercent = computed(() => 100 - this.moviePercent());

  readonly joinedDate = computed(() => {
    const raw = this.user()?.createdAt;
    if (!raw) return '';
    return new Date(raw).toLocaleDateString('en-US', {
      month: 'short',
      year: 'numeric',
    });
  });

  readonly avatarLetter = computed(
    () => this.user()?.name?.charAt(0)?.toUpperCase() ?? '?'
  );

  readonly hasAvatar = computed(() => !!this.user()?.avatarUrl);

  ngOnInit() {
    this.authService.getMe().subscribe({
      next: (user) => {
        this.user.set(user);
        this.profileForm.patchValue({
          name: user.name,
          email: user.email,
          timezone: user.timezone ?? '',
        });
        this.isLoading.set(false);
      },
      error: () => {
        const fallback = this.authService.currentUser();
        this.user.set(fallback);
        if (fallback) {
          this.profileForm.patchValue({ name: fallback.name, email: fallback.email });
        }
        this.isLoading.set(false);
      },
    });

    this.statsService.getStats().subscribe({
      next: (s) => this.stats.set(s),
    });
  }

  setTab(tab: ProfileTab) {
    this.activeTab.set(tab);
  }

  onSave() {
    if (this.profileForm.invalid || this.isSaving()) return;
    this.isSaving.set(true);
    this.saveSuccess.set(false);
    this.saveError.set('');

    const { name, email, timezone } = this.profileForm.value;
    this.authService
      .updateProfile({
        name: name ?? undefined,
        email: email ?? undefined,
        timezone: timezone || undefined,
      })
      .subscribe({
        next: (user) => {
          this.user.set(user);
          this.isSaving.set(false);
          this.saveSuccess.set(true);
          this.profileForm.markAsPristine();
          setTimeout(() => this.saveSuccess.set(false), 3500);
        },
        error: () => {
          this.isSaving.set(false);
          this.saveError.set('Failed to save changes. Please try again.');
        },
      });
  }

  onCancel() {
    const u = this.user();
    if (u) {
      this.profileForm.patchValue({
        name: u.name,
        email: u.email,
        timezone: u.timezone ?? '',
      });
    }
    this.profileForm.markAsPristine();
    this.saveError.set('');
  }
}
