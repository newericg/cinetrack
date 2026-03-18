import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { I18nService } from '../../../core/services/i18n.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  readonly i18n = inject(I18nService);

  readonly isLoading = this.authService.isLoading;
  readonly isDarkMode = signal(true);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  toggleTheme() {
    this.isDarkMode.update((v) => !v);
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.errorMessage.set(null);
    this.authService.login(this.form.getRawValue()).subscribe({
      error: (err) => {
        const msg = err?.error?.message ?? 'Invalid credentials. Please try again.';
        this.errorMessage.set(msg);
      },
    });
  }

  get emailInvalid() {
    const ctrl = this.form.controls.email;
    return ctrl.invalid && ctrl.touched;
  }

  get passwordInvalid() {
    const ctrl = this.form.controls.password;
    return ctrl.invalid && ctrl.touched;
  }
}
