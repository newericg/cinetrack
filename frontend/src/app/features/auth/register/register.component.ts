import { Component, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { I18nService } from '../../../core/services/i18n.service';

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value;
  const confirm = control.get('confirmPassword')?.value;
  return password && confirm && password !== confirm ? { passwordMismatch: true } : null;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  readonly i18n = inject(I18nService);

  readonly isLoading = this.authService.isLoading;
  readonly isDarkMode = signal(true);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group(
    {
      name: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
    },
    { validators: passwordMatchValidator }
  );

  toggleTheme() {
    this.isDarkMode.update((v) => !v);
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.errorMessage.set(null);
    const { name, email, password } = this.form.getRawValue();
    this.authService.register({ name, email, password }).subscribe({
      error: (err) => {
        const msg = err?.error?.message ?? 'Registration failed. Please try again.';
        this.errorMessage.set(msg);
      },
    });
  }

  get nameInvalid()    { const c = this.form.controls.name;            return c.invalid && c.touched; }
  get emailInvalid()   { const c = this.form.controls.email;           return c.invalid && c.touched; }
  get passwordInvalid(){ const c = this.form.controls.password;        return c.invalid && c.touched; }
  get confirmInvalid() {
    const c = this.form.controls.confirmPassword;
    const mismatch = this.form.hasError('passwordMismatch') && c.touched;
    return (c.invalid && c.touched) || mismatch;
  }
}
