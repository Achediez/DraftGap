import { Component, ElementRef, HostListener, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthApiService } from '../auth/data/auth-api.service';
import { HttpErrorResponse } from '@angular/common/http';

import { Router } from '@angular/router';
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
  isAdmin = false;
  user: any = {};

  summaryCards: Array<{ label: string; value: string }> = [];
  rankedOverview = {
    tier: 'Gold II',
    lp: 67,
    mmr: '+18',
    lpDelta: '+19 / -16'
  };

  topPicks = [
    { champion: 'Jinx', games: 18, winrate: '61%' },
    { champion: 'Ahri', games: 12, winrate: '58%' },
    { champion: 'Thresh', games: 10, winrate: '54%' }
  ];

  trackedPlayers = [
    { name: 'DG SupportMain', relation: 'Amigo', lane: 'SUP' },
    { name: 'JunglerX', relation: 'Match reciente', lane: 'JG' },
    { name: 'TopPunisher', relation: 'Match reciente', lane: 'TOP' }
  ];

  championMasteries = [
    { champion: 'Jinx', mastery: 7, points: '289.450', winrate: '61%' },
    { champion: 'Ahri', mastery: 7, points: '201.103', winrate: '58%' },
    { champion: 'Thresh', mastery: 6, points: '132.904', winrate: '54%' }
  ];

  recentMatches = [
    {
      champion: 'Jinx',
      championImg: 'https://ddragon.leagueoflegends.com/cdn/13.24.1/img/champion/Jinx.png',
      result: 'Victoria',
      kda: '12/3/8',
      cs: 213,
      mode: 'Ranked Solo',
      date: '2026-02-17'
    },
    {
      champion: 'Thresh',
      championImg: 'https://ddragon.leagueoflegends.com/cdn/13.24.1/img/champion/Thresh.png',
      result: 'Derrota',
      kda: '1/7/14',
      cs: 31,
      mode: 'Ranked Solo',
      date: '2026-02-16'
    },
    {
      champion: 'Ahri',
      championImg: 'https://ddragon.leagueoflegends.com/cdn/13.24.1/img/champion/Ahri.png',
      result: 'Victoria',
      kda: '8/2/5',
      cs: 201,
      mode: 'Flex',
      date: '2026-02-15'
    },
    {
      champion: 'Jinx',
      championImg: 'https://ddragon.leagueoflegends.com/cdn/13.24.1/img/champion/Jinx.png',
      result: 'Victoria',
      kda: '10/2/9',
      cs: 225,
      mode: 'Ranked Solo',
      date: '2026-02-14'
    }
  ];

  menuOpen = false;
  activeSection:
    | 'historial'
    | 'campeones'
    | 'ranked'
    | 'jugadores'
    | 'graficos'
    | 'chatbot' = 'historial';

  /**
   * Constructor: comprueba autenticación y obtiene los datos reales del usuario.
   * @param elRef Referencia al elemento raíz del componente (para detectar clics fuera)
   * @param authApi Servicio para acceder a la API de autenticación
   */
  constructor(
    private elRef: ElementRef,
    private authApi: AuthApiService,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {
    // Si no hay token, redirige a login
    if (!localStorage.getItem('draftgap_token')) {
      window.location.href = '/auth';
      return;
    }

    this.authApi.getCurrentUser().subscribe({
      next: (data) => {
        this.user = data;
        this.isAdmin = data.isAdmin;
        localStorage.setItem('isAdmin', data.isAdmin ? '1' : '0');
        this.summaryCards = [
          { label: 'Últimas partidas', value: `${this.recentMatches.length}` },
          { label: 'Winrate reciente', value: this.getRecentWinrate() },
          { label: 'LP actual', value: `${this.rankedOverview.lp} LP` },
          {
            label: 'Última sync',
            value: data.lastSync ? new Date(data.lastSync).toLocaleDateString() : '-'
          }
        ];
        this.cdr.detectChanges();
      },
      error: (err: HttpErrorResponse) => {
        window.location.href = '/auth';
      }
    });
  }

  private getRecentWinrate(): string {
    const wins = this.recentMatches.filter(m => m.result === 'Victoria').length;
    const total = this.recentMatches.length;
    if (!total) {
      return '0%';
    }

    return `${Math.round((wins / total) * 100)}%`;
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
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    this.menuOpen = false;
    this.router.navigate(['/admin']);
  }
}
