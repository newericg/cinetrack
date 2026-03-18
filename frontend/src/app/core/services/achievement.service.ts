import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { AchievementsResponse } from '../models/achievement.model';

@Injectable({ providedIn: 'root' })
export class AchievementService {
  private readonly http = inject(HttpClient);

  getAchievements() {
    return this.http.get<AchievementsResponse>(`${environment.apiUrl}/achievements`);
  }
}
