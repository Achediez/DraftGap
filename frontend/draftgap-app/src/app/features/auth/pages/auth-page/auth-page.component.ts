import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-auth-page',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './auth-page.component.html',
  styleUrl: './auth-page.component.scss'
})
export class AuthPageComponent {

  readonly mode          = signal<'login' | 'register'>('login');
  readonly isSubmitting  = signal(false);
  readonly errorMessage  = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  private readonly fb          = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router      = inject(Router);

  readonly loginForm = this.fb.group({
    email:    ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  readonly registerForm = this.fb.group({
    email:    ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    riotId:   ['', [Validators.required]],
    region:   ['EUW', [Validators.required]]
  });

  readonly regions = [
    { value: 'EUW',  label: 'EUW'  },
    { value: 'EUNE', label: 'EUNE' },
    { value: 'NA',   label: 'NA'   },
    { value: 'KR',   label: 'KR'   },
    { value: 'BR',   label: 'BR'   },
    { value: 'LAN',  label: 'LAN'  },
    { value: 'LAS',  label: 'LAS'  },
    { value: 'OCE',  label: 'OCE'  },
    { value: 'TR',   label: 'TR'   },
    { value: 'RU',   label: 'RU'   }
  ];

  setMode(mode: 'login' | 'register'): void {
    this.mode.set(mode);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

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
        email:    this.loginForm.value.email    ?? '',
        password: this.loginForm.value.password ?? ''
      })
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          // Token is already stored by AuthService via tap().
          // No manual localStorage writes needed here.
          this.successMessage.set(`Welcome ${response.email}`);
          this.router.navigate(['/dashboard']);
        },
        error: (error) => this.errorMessage.set(this.mapError(error))
      });
  }

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
        email:    this.registerForm.value.email    ?? '',
        password: this.registerForm.value.password ?? '',
        riotId:   this.registerForm.value.riotId   ?? '',
        region:   this.registerForm.value.region   ?? 'EUW'
      })
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          // Token is stored by AuthService. Redirect directly to dashboard.
          this.router.navigate(['/dashboard']);
        },
        error: (error) => this.errorMessage.set(this.mapError(error))
      });
  }

  private mapError(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      return error.error?.error ?? 'Could not complete the request.';
    }
    return 'An unexpected error occurred.';
  }
}
