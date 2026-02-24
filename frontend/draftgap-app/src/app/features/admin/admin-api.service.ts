import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private readonly baseUrl = '/api/admin'; // Ajusta si tu backend usa otro prefijo

  constructor(private http: HttpClient) {}

  getUsers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/users`);
  }

  getUserById(userId: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/users/${userId}`);
  }

  deleteUser(userId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/users/${userId}`);
  }

  /**
   * Crea un usuario usando el endpoint de registro oficial (AuthController)
   * Permite marcar como admin si el backend lo soporta por email o flag.
   */
  addUser(user: any): Observable<any> {
    // El endpoint de registro es /api/auth/register
    // El backend determina si es admin por email o por el campo isAdmin si lo soporta
    return this.http.post('/api/auth/register', user);
  }
}
