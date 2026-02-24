import { Injectable } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth.models';

/**
 * Servicio de autenticación mock para pruebas sin backend ni base de datos.
 * Permite login con dos usuarios de prueba: uno normal y uno admin.
 */
@Injectable({ providedIn: 'root' })
export class MockAuthService {
  // Usuarios de prueba
  private readonly users = [
    {
      email: 'dg@gmail.com',
      password: 'dg1234',
      isAdmin: false,
      riotId: 'DGUser#EUW',
      region: 'EUW',
      puuid: 'mock-puuid-user'
    },
    {
      email: 'dgadmin@gmail.com',
      password: 'dgad1234',
      isAdmin: true,
      riotId: 'DGAdmin#EUW',
      region: 'EUW',
      puuid: 'mock-puuid-admin'
    }
  ];

  /** Simula login: devuelve token y datos si las credenciales coinciden. */
  login(payload: LoginRequest): Observable<AuthResponse> {
    const user = this.users.find(
      u => u.email === payload.email && u.password === payload.password
    );
    if (!user) {
      return throwError(() => new Error('Credenciales incorrectas'));
    }
    // Simula un JWT y respuesta
    const response: AuthResponse = {
      token: 'mock-jwt-token',
      email: user.email,
      riotId: user.riotId,
      puuid: user.puuid,
      region: user.region,
      isAdmin: user.isAdmin,
      expiresAt: new Date(Date.now() + 3600 * 1000).toISOString()
    };
    return of(response);
  }

  /** Simula registro: siempre falla (solo login permitido en mock). */
  register(_payload: RegisterRequest): Observable<AuthResponse> {
    return throwError(() => new Error('Registro deshabilitado en modo demo.'));
  }

  /** Simula logout (no hace nada en mock). */
  logout(): void {}
}
