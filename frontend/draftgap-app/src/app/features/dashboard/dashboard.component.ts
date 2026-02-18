
import { Component, ElementRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Página principal tras el login/registro.
 * Inspirada en DPM, u.gg, Lolalytics, etc.
 * Muestra estadísticas y acceso a admin si el usuario es administrador.
 */
/**
 * Componente principal del dashboard de usuario.
 * Muestra información del perfil, estadísticas, partidas recientes y menú de usuario.
 * Inspirado en la estética de DPM/u.gg/Lolalytics.
 *
 * - Si el usuario es admin, muestra acceso a gestión.
 * - El menú de usuario permite cerrar sesión y acceder a gestión (si es admin).
 * - Los datos son simulados para entorno demo.
 */
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  /**
   * Indica si el usuario es administrador.
   * En una app real, esto vendría del AuthService o similar.
   */
  isAdmin = false;

  /**
   * Datos simulados del usuario logueado.
   * En producción, estos datos vendrían de la API/backend.
   */
  user = {
    email: localStorage.getItem('isAdmin') === '1' ? 'dgadmin@gmail.com' : 'dg@gmail.com',
    riotId: localStorage.getItem('isAdmin') === '1' ? 'DGAdmin#EUW' : 'DGUser#EUW',
    region: 'EUW',
    rank: localStorage.getItem('isAdmin') === '1' ? 'Diamond IV' : 'Gold II',
    winrate: localStorage.getItem('isAdmin') === '1' ? 58 : 51,
    games: localStorage.getItem('isAdmin') === '1' ? 320 : 120,
    iconUrl: 'https://ddragon.leagueoflegends.com/cdn/13.24.1/img/profileicon/4419.png'
  };

  /**
   * Estadísticas principales del usuario para mostrar en tarjetas.
   */
  stats = [
    { label: 'KDA', value: '4.2', color: '#00bba3' },
    { label: 'Winrate', value: this.user.winrate + '%', color: '#3fa7ff' },
    { label: 'Rango', value: this.user.rank, color: '#ffe156' },
    { label: 'Partidas', value: this.user.games, color: '#ff6f61' }
  ];

  /**
   * Partidas recientes simuladas para la tabla.
   */
  recentMatches = [
    { champion: 'Jinx', championImg: 'https://ddragon.leagueoflegends.com/cdn/13.24.1/img/champion/Jinx.png', result: 'Victoria', kda: '12/3/8', date: '2026-02-17' },
    { champion: 'Thresh', championImg: 'https://ddragon.leagueoflegends.com/cdn/13.24.1/img/champion/Thresh.png', result: 'Derrota', kda: '1/7/14', date: '2026-02-16' },
    { champion: 'Ahri', championImg: 'https://ddragon.leagueoflegends.com/cdn/13.24.1/img/champion/Ahri.png', result: 'Victoria', kda: '8/2/5', date: '2026-02-15' }
  ];

  /**
   * Estado de apertura/cierre del menú de usuario.
   */
  menuOpen = false;

  /**
   * Constructor: comprueba autenticación y rol admin.
   * @param elRef Referencia al elemento raíz del componente (para detectar clics fuera)
   */
  constructor(private elRef: ElementRef) {
    // Si no hay token, redirige a login
    if (!localStorage.getItem('draftgap_token')) {
      window.location.href = '/auth';
      return;
    }
    // Lee el estado de admin desde localStorage (simulación)
    this.isAdmin = localStorage.getItem('isAdmin') === '1';
  }

  /**
   * Cierra el menú si se hace clic fuera del menú de usuario.
   */
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.elRef.nativeElement.contains(event.target)) {
      this.menuOpen = false;
    }
  }

  /**
   * Abre/cierra el menú de usuario al hacer clic en el icono.
   */
  toggleMenu() {
    this.menuOpen = !this.menuOpen;
  }

  /**
   * Cierra sesión: borra datos de localStorage y redirige a login.
   */
  logout(event: Event) {
    event.stopPropagation();
    this.menuOpen = false;
    setTimeout(() => {
      localStorage.removeItem('draftgap_token');
      localStorage.removeItem('isAdmin');
      window.location.href = '/auth';
    }, 100);
  }

  /**
   * Acceso a la gestión de usuarios (solo admin).
   */
  goToAdmin(event: Event) {
    event.stopPropagation();
    this.menuOpen = false;
    setTimeout(() => {
      window.location.href = '/admin';
    }, 100);
  }
  // TODO: Cargar datos reales del usuario y estadísticas aquí.
}
