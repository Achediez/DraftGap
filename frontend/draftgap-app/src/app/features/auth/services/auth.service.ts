import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { AuthTokenService } from '../../../core/auth/auth-token.service';
import { AuthApiService } from '../data/auth-api.service';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(
    private readonly api: AuthApiService,
    private readonly tokenService: AuthTokenService
  ) {}

  // Performs login and stores the JWT on success.
  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.api.login(payload).pipe(tap((response) => this.tokenService.setToken(response.token)));
  }

  // Performs registration and stores the JWT on success.
  register(payload: RegisterRequest): Observable<AuthResponse> {
    return this.api
      .register(payload)
      .pipe(tap((response) => this.tokenService.setToken(response.token)));
  }

  // Clears local auth state.
  logout(): void {
    this.tokenService.clear();
  }
}
