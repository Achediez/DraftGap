
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
    games: localStorage.getItem('isAdmin') === '1' ? 320 : 120,
    iconUrl: 'https://ddragon.leagueoflegends.com/cdn/13.24.1/img/profileicon/4419.png'
  };

  stats = [
    { label: 'KDA', value: '4.2', color: '#00bba3' },
    { label: 'Winrate', value: this.user.winrate + '%', color: '#3fa7ff' },
    { label: 'Rango', value: this.user.rank, color: '#ffe156' },
    { label: 'Partidas', value: this.user.games, color: '#ff6f61' }
  ];

  recentMatches = [
    { champion: 'Jinx', championImg: 'https://ddragon.leagueoflegends.com/cdn/13.24.1/img/champion/Jinx.png', result: 'Victoria', kda: '12/3/8', date: '2026-02-17' },
    { champion: 'Thresh', championImg: 'https://ddragon.leagueoflegends.com/cdn/13.24.1/img/champion/Thresh.png', result: 'Derrota', kda: '1/7/14', date: '2026-02-16' },
    { champion: 'Ahri', championImg: 'https://ddragon.leagueoflegends.com/cdn/13.24.1/img/champion/Ahri.png', result: 'Victoria', kda: '8/2/5', date: '2026-02-15' }
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
