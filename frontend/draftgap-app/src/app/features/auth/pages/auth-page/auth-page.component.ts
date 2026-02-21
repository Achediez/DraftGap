import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';
import { Router } from '@angular/router';
// import { AuthService } from '../../services/auth.service';
import { MockAuthService } from '../../services/mock-auth.service';

@Component({
  selector: 'app-auth-page',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './auth-page.component.html',
  styleUrl: './auth-page.component.scss'
})
export class AuthPageComponent {
  // UI state: which tab is active.
  readonly mode = signal<'login' | 'register'>('login');
  // UI state: loading indicator while calling API.
  readonly isSubmitting = signal(false);
  // UI state: error/success feedback messages.
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  // Dependency injection using signals-friendly API.
  private readonly fb = inject(FormBuilder);
  // Sustituye AuthService por MockAuthService para pruebas sin backend
  private readonly authService = inject(MockAuthService);
  private readonly router = inject(Router);

  // Reactive form for login.
  readonly loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  // Reactive form for registration.
  readonly registerForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    riotId: ['', [Validators.required]],
    region: ['EUW', [Validators.required]]
  });

  // Supported Riot regions for the register form.
  readonly regions = [
    { value: 'EUW', label: 'EUW' },
    { value: 'EUNE', label: 'EUNE' },
    { value: 'NA', label: 'NA' },
    { value: 'KR', label: 'KR' },
    { value: 'BR', label: 'BR' },
    { value: 'LAN', label: 'LAN' },
    { value: 'LAS', label: 'LAS' },
    { value: 'OCE', label: 'OCE' },
    { value: 'TR', label: 'TR' },
    { value: 'RU', label: 'RU' }
  ];

  // Switch between login and register views.
  setMode(mode: 'login' | 'register'): void {
    this.mode.set(mode);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  // Submit login form to backend.
  submitLogin(): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);

    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.authService
      .login({
        email: this.loginForm.value.email ?? '',
        password: this.loginForm.value.password ?? ''
      })
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.successMessage.set(`Bienvenido ${response.email}`);
          // Guardar token y si es admin en localStorage para el dashboard
          localStorage.setItem('draftgap_token', response.token);
          localStorage.setItem('isAdmin', response.isAdmin ? '1' : '0');
          // Redirigir al dashboard
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          this.errorMessage.set(this.mapError(error));
        }
      });
  }

  // Submit registration form to backend.
  submitRegister(): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);

    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.authService
      .register({
        email: this.registerForm.value.email ?? '',
        password: this.registerForm.value.password ?? '',
        riotId: this.registerForm.value.riotId ?? '',
        region: this.registerForm.value.region ?? 'EUW'
      })
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.successMessage.set(`Cuenta creada: ${response.email}`);
          this.setMode('login');
        },
        error: (error) => {
          this.errorMessage.set(this.mapError(error));
        }
      });
  }

  // Normalize backend error messages.
  private mapError(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      return error.error?.error ?? 'No se pudo completar la solicitud.';
    }

    return 'Ocurrió un error inesperado.';
  }
}
