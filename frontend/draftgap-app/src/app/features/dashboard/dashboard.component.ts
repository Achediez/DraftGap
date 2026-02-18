import { Component } from '@angular/core';

/**
 * Página principal tras el login/registro.
 * Inspirada en DPM, u.gg, Lolalytics, etc.
 * Muestra estadísticas y acceso a admin si el usuario es administrador.
 */
@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  /**
   * Indica si el usuario es administrador.
   * En una app real, esto vendría del AuthService o similar.
   */
  isAdmin = false;

  constructor() {
    // Si no hay token, redirige a login
    if (!localStorage.getItem('draftgap_token')) {
      window.location.href = '/auth';
      return;
    }
    // Lee el estado de admin desde localStorage (simulación)
    this.isAdmin = localStorage.getItem('isAdmin') === '1';
  }
  // TODO: Cargar datos reales del usuario y estadísticas aquí.
}
