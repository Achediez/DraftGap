import { Component, ElementRef, HostListener, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { DashboardApiService } from './dashboard-api.service';

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
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  isAdmin = false;
  user: any = {};

  summaryCards: Array<{ label: string; value: string }> = [];
  rankedOverview = {
    tier: 'Sin clasificar',
    lp: 0,
    mmr: '-',
    lpDelta: '-'
  };

  topPicks: Array<{ champion: string; games: number; winrate: string }> = [];

  friends: Array<{ name: string; lane: string; status: string }> = [];

  recentPlayers: Array<{ name: string; lane: string; result: string }> = [];

  friendRequestRiotId = '';
  friendRequestMessage: string | null = null;
  friendRequestError: string | null = null;
  sentFriendRequests: Array<{ riotId: string; status: string }> = [];
  syncLoading = false;
  syncMessage: string | null = null;
  syncError: string | null = null;

  championMasteries: Array<{ champion: string; mastery: number; points: string; winrate: string }> = [];

  recentMatches: Array<{
    champion: string;
    championImg: string;
    result: string;
    kda: string;
    cs: number | string;
    mode: string;
    date: string;
  }> = [];

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
    private dashboardApi: DashboardApiService,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {
    // Si no hay token, redirige a login
    if (!localStorage.getItem('draftgap_token')) {
      window.location.href = '/auth';
      return;
    }

    this.loadDashboardData();
  }

  private loadDashboardData(): void {
    forkJoin({
      profile: this.dashboardApi.getProfile(),
      summary: this.dashboardApi.getDashboardSummary().pipe(catchError(() => of(null))),
      champions: this.dashboardApi.getChampionStats().pipe(catchError(() => of([]))),
      ranked: this.dashboardApi.getRankedStats().pipe(catchError(() => of(null))),
      matches: this.dashboardApi.getMatches(1, 12).pipe(catchError(() => of(null)))
    }).subscribe({
      next: ({ profile, summary, champions, ranked, matches }) => {
        const profileIconId = profile.summoner?.profileIconId;

        this.user = {
          email: profile.email,
          riotId: profile.riotId ?? '-',
          region: profile.region ?? null,
          isAdmin: profile.isAdmin,
          lastSync: profile.lastSync,
          iconUrl: profileIconId
            ? `https://ddragon.leagueoflegends.com/cdn/13.24.1/img/profileicon/${profileIconId}.png`
            : null
        };

        this.isAdmin = profile.isAdmin;
        localStorage.setItem('isAdmin', profile.isAdmin ? '1' : '0');

        const soloQueue = ranked?.soloQueue ?? summary?.rankedOverview?.soloQueue;
        this.rankedOverview = {
          tier: soloQueue?.tier && soloQueue?.rank ? `${soloQueue.tier} ${soloQueue.rank}` : 'Sin clasificar',
          lp: soloQueue?.leaguePoints ?? 0,
          mmr: soloQueue ? `${Math.round(soloQueue.winrate)}% WR` : '-',
          lpDelta: soloQueue ? `${soloQueue.wins}W / ${soloQueue.losses}L` : '-'
        };

        const matchSource = matches?.items?.length
          ? matches.items.map(match => ({
              championName: match.championName,
              win: match.win,
              kills: match.kills,
              deaths: match.deaths,
              assists: match.assists,
              gameCreation: match.gameCreation,
              queueId: match.queueId,
              teamPosition: match.teamPosition
            }))
          : (summary?.recentMatches ?? []).map(match => ({
              championName: match.championName,
              win: match.win,
              kills: match.kills,
              deaths: match.deaths,
              assists: match.assists,
              gameCreation: match.gameCreation,
              queueId: 0,
              teamPosition: match.teamPosition
            }));

        this.recentMatches = matchSource.map(match => ({
          champion: match.championName,
          championImg: `https://ddragon.leagueoflegends.com/cdn/13.24.1/img/champion/${match.championName}.png`,
          result: match.win ? 'Victoria' : 'Derrota',
          kda: `${match.kills}/${match.deaths}/${match.assists}`,
          cs: '-',
          mode: this.getQueueLabel(match.queueId, match.teamPosition),
          date: new Date(match.gameCreation).toLocaleDateString()
        }));

        this.topPicks = (summary?.topChampions ?? []).map(champion => ({
          champion: champion.championName,
          games: champion.gamesPlayed,
          winrate: `${Math.round(champion.winrate)}%`
        }));

        if (champions.length > 0) {
          this.championMasteries = champions.slice(0, 10).map(champion => ({
            champion: champion.championName,
            mastery: champion.gamesPlayed,
            points: `${champion.gamesPlayed} partidas`,
            winrate: `${Math.round(champion.winrate)}%`
          }));
        }

        this.summaryCards = [
          {
            label: 'Últimas partidas',
            value: `${summary?.performanceStats?.totalMatches ?? this.recentMatches.length}`
          },
          {
            label: 'Winrate reciente',
            value: summary?.performanceStats ? `${Math.round(summary.performanceStats.winrate)}%` : this.getRecentWinrate()
          },
          { label: 'LP actual', value: `${this.rankedOverview.lp} LP` },
          {
            label: 'Última sync',
            value: profile.lastSync ? new Date(profile.lastSync).toLocaleDateString() : '-'
          }
        ];

        this.cdr.detectChanges();
      },
      error: (_err: HttpErrorResponse) => {
        window.location.href = '/auth';
      }
    });
  }

  private getQueueLabel(queueId: number, teamPosition?: string): string {
    if (queueId === 420) {
      return 'Ranked Solo';
    }

    if (queueId === 440) {
      return 'Ranked Flex';
    }

    if (teamPosition) {
      return teamPosition;
    }

    return 'Partida';
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

  sendFriendRequest() {
    this.friendRequestError = null;
    this.friendRequestMessage = null;

    const riotId = this.friendRequestRiotId.trim();
    if (!riotId) {
      this.friendRequestError = 'Introduce un Riot ID.';
      return;
    }

    if (!riotId.includes('#')) {
      this.friendRequestError = 'El Riot ID debe tener formato gameName#tagLine.';
      return;
    }

    const alreadyFriend = this.friends.some(
      friend => friend.name.toLowerCase() === riotId.toLowerCase()
    );

    if (alreadyFriend) {
      this.friendRequestError = 'Ese jugador ya está en tu lista de amigos.';
      return;
    }

    this.dashboardApi.searchUserByRiotId(riotId).subscribe({
      next: (result) => {
        if (!result.isActive) {
          this.friendRequestError = 'El usuario existe pero no está activo.';
          return;
        }

        this.sentFriendRequests.unshift({ riotId, status: 'Pendiente' });
        this.friendRequestMessage = 'Usuario encontrado. Solicitud registrada como pendiente.';
        this.friendRequestRiotId = '';
      },
      error: () => {
        this.friendRequestError = 'No se encontró ese Riot ID en DraftGap.';
      }
    });
  }

  refreshProfileData() {
    this.syncLoading = true;
    this.syncMessage = null;
    this.syncError = null;

    this.dashboardApi.triggerSync().subscribe({
      next: () => {
        this.syncLoading = false;
        this.syncMessage = 'Sincronización iniciada. Los datos se actualizarán en breve.';
        this.loadDashboardData();
      },
      error: (err: HttpErrorResponse) => {
        this.syncLoading = false;
        this.syncError = err?.error?.error || 'No se pudo iniciar la sincronización.';
      }
    });
  }
}
