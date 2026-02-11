import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { APP_SETTINGS, AppSettings } from '../../../core/config/app-settings';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  // Base URL for backend API.
  private readonly baseUrl: string;

  constructor(
    private readonly http: HttpClient,
    @Inject(APP_SETTINGS) settings: AppSettings
  ) {
    this.baseUrl = settings.apiBaseUrl;
  }

  // Calls POST /auth/login
  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/auth/login`, payload);
  }

  // Calls POST /auth/register
  register(payload: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/auth/register`, payload);
  }
}
