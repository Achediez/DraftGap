import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AuthTokenService {
  // Key used to persist the JWT in localStorage.
  private readonly tokenKey = 'draftgap_token';

  // Read token if present.
  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  // Persist token after login/registration.
  setToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
  }

  // Clear token on logout.
  clear(): void {
    localStorage.removeItem(this.tokenKey);
  }
}
