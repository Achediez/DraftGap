
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

  // Datos simulados de usuario y estadísticas
  user = {
    email: localStorage.getItem('isAdmin') === '1' ? 'dgadmin@gmail.com' : 'dg@gmail.com',
    riotId: localStorage.getItem('isAdmin') === '1' ? 'DGAdmin#EUW' : 'DGUser#EUW',
    region: 'EUW',
    rank: localStorage.getItem('isAdmin') === '1' ? 'Diamond IV' : 'Gold II',
    winrate: localStorage.getItem('isAdmin') === '1' ? 58 : 51,
    games: localStorage.getItem('isAdmin') === '1' ? 320 : 120
  };

  recentMatches = [
    { champion: 'Jinx', result: 'Victoria', kda: '12/3/8', date: '2026-02-17' },
    { champion: 'Thresh', result: 'Derrota', kda: '1/7/14', date: '2026-02-16' },
    { champion: 'Ahri', result: 'Victoria', kda: '8/2/5', date: '2026-02-15' }
  ];

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
