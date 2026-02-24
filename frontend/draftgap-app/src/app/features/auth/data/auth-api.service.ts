import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { APP_SETTINGS, AppSettings } from '../../../core/config/app-settings';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth.models';
import { AuthTokenService } from '../../../core/auth/auth-token.service';
import { HttpHeaders } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  // Base URL for backend API.
  private readonly baseUrl: string;

  constructor(
    private readonly http: HttpClient,
    @Inject(APP_SETTINGS) settings: AppSettings,
    private readonly tokenService: AuthTokenService // Inyectamos el servicio para acceder al token
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

  /**
   * Llama a GET /auth/me para obtener los datos del usuario autenticado.
   * Añade la cabecera Authorization con el token JWT guardado en localStorage.
   */
  getCurrentUser(): Observable<any> {
    const token = this.tokenService.getToken();
    const headers = token ? new HttpHeaders({ 'Authorization': `Bearer ${token}` }) : undefined;
    return this.http.get<any>(`${this.baseUrl}/auth/me`, { headers });
  }
}
