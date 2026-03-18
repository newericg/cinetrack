import { Component, inject } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  template: `
    <div class="min-h-screen bg-black text-white flex flex-col items-center justify-center gap-6">
      <h1 class="text-4xl font-bold text-[#FF0000]">CineTrack</h1>
      <p class="text-gray-400">Welcome, {{ authService.currentUser()?.name }}!</p>
      <button
        (click)="authService.logout()"
        class="bg-[#FF0000] hover:bg-red-600 text-white font-bold py-2 px-6 rounded-lg transition-all"
      >
        Logout
      </button>
    </div>
  `,
})
export class DashboardComponent {
  readonly authService = inject(AuthService);
}
