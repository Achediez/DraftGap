import { Injectable, signal, computed } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { AuthTokenService } from '../../../core/auth/auth-token.service';
import { AuthApiService } from '../data/auth-api.service';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {

  // Signal-based reactive state — initialized from persisted token on page reload.
  private readonly _currentUser = signal<AuthResponse | null>(null);

  // Public read-only derived signals for consumers.
  readonly isLoggedIn  = computed(() => this._currentUser() !== null);
  readonly currentUser = computed(() => this._currentUser());
  readonly isAdmin     = computed(() => this._currentUser()?.isAdmin ?? false);

  constructor(
    private readonly api: AuthApiService,
    private readonly tokenService: AuthTokenService
  ) {
    // Restore session from localStorage on service instantiation.
    this.restoreSession();
  }

  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.api.login(payload).pipe(
      tap(response => this.persistSession(response))
    );
  }

  register(payload: RegisterRequest): Observable<AuthResponse> {
    return this.api.register(payload).pipe(
      tap(response => this.persistSession(response))
    );
  }

  logout(): void {
    this._currentUser.set(null);
    this.tokenService.clear();
  }

  // Stores token and updates in-memory state atomically.
  private persistSession(response: AuthResponse): void {
    this.tokenService.setToken(response.token);
    this._currentUser.set(response);
  }

  // Checks for an existing token on startup to restore authenticated state.
  private restoreSession(): void {
    const token = this.tokenService.getToken();
    if (token) {
      // Token exists but user data isn't cached — call /auth/me here
      // if you need the full user object on reload, or keep minimal state.
      this._currentUser.set({ token } as AuthResponse);
    }
  }
}
