/**
 * @file Summoner Service with Signals & SWR Pattern
 * @description Central service for managing summoner statistics and dashboard data using Angular Signals.
 * Implements Stale-While-Revalidate (SWR) pattern with unidirectional data flow.
 */

import { Injectable, signal, computed, effect } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { DashboardSummary, SummonerInfo, QueryResult } from '../models';
import { NotificationService } from './notification.service';

export interface DerivedMetrics {
  kda: number; // (Kills + Assists) / Deaths, default 0 if deaths = 0
  winrate: number; // % of wins out of total games
  trend: 'up' | 'down' | 'neutral'; // Based on recent performance
  lpPerGame: number; // Average LP gained/lost per game
}

/**
 * Service for managing League of Legends summoner statistics and dashboard data.
 * 
 * Implements Stale-While-Revalidate (SWR) pattern:
 * - If data exists, show it immediately while refresh happens in background
 * - Prevents UI jank from loading spinners on repeated queries
 * 
 * Signal-based state management:
 * - `currentSummoner`: Currently selected summoner name
 * - `dashboardState`: Query result state (data, loading, error)
 * - `derivedMetrics`: Computed metrics (KDA, winrate, trend, LP/game)
 * - `isRefreshing`: Distinguishes between initial load and background refresh
 * 
 * @example
 * constructor(private summonerService: SummonerService) {
 *   // Auto-subscribe to dashboard changes
 *   effect(() => {
 *     const metrics = this.summonerService.derivedMetrics();
 *     console.log('KDA:', metrics.kda, 'Winrate:', metrics.winrate);
 *   });
 * }
 * 
 * // Fetch dashboard (SWR: shows stale data + refreshes in bg if available)
 * await this.summonerService.fetchDashboard('gameName#tagLine');
 */
@Injectable({
  providedIn: 'root'
})
export class SummonerService {
  private readonly apiBaseUrl = '/api';
  private refreshAbortControllers = new Map<string, AbortController>();

  /** Current summoner name (Riot ID format: gameName#tagLine) */
  readonly currentSummoner = signal<string>('');

  /** Query result state containing data, loading, and error states */
  readonly dashboardState = signal<QueryResult<DashboardSummary>>({
    data: null,
    loading: false,
    error: null
  });

  /** Flag to distinguish initial load vs background refresh (SWR) */
  readonly isRefreshing = signal<boolean>(false);

  /** Computed signal: True when dashboard is currently loading or refreshing */
  readonly isLoading = computed(() => 
    this.dashboardState().loading || this.isRefreshing()
  );

  /** Computed signal: True when dashboard has an error */
  readonly hasError = computed(() => this.dashboardState().error !== null);

  /** Computed signal: True when dashboard data is valid and ready for display */
  readonly isDashboardReady = computed(() => {
    const state = this.dashboardState();
    return state.data !== null && !state.error;
  });

  /** Computed signal: Extract dashboard data from state */
  readonly dashboard = computed(() => this.dashboardState().data);

  /** Computed signal: Derived metrics from dashboard data (KDA, winrate, trend) */
  readonly derivedMetrics = computed<DerivedMetrics>(() => {
    const dashboard = this.dashboard();
    const stats = dashboard?.performanceStats;
    
    if (!dashboard || !stats) {
      return {
        kda: 0,
        winrate: 0,
        trend: 'neutral',
        lpPerGame: 0
      };
    }

    // KDA is already calculated by backend
    const kda = stats.avgKda ?? 0;

    // Winrate from performance stats
    const winrate = stats.winrate ?? 0;

    // Determine trend (based on win/loss ratio in recent matches)
    const totalMatches = stats.totalMatches ?? 1;
    const wins = stats.wins ?? 0;
    const winRatio = wins / totalMatches;
    const trend: 'up' | 'down' | 'neutral' = 
      winRatio >= 0.55 ? 'up' : winRatio <= 0.45 ? 'down' : 'neutral';

    // Calculate LP per game from ranked overview
    const soloQueue = dashboard.rankedOverview?.soloQueue;
    const lpPerGame = soloQueue 
      ? (soloQueue.leaguePoints / (soloQueue.totalGames ?? 1)) 
      : 0;

    return { kda, winrate, trend, lpPerGame };
  });

  constructor(
    private http: HttpClient,
    private notificationService: NotificationService
  ) {
    // Log state changes for debugging
    effect(() => {
      const state = this.dashboardState();
      if (state.error) {
        console.warn('[SummonerService] Dashboard error:', state.error);
      }
    });
  }

  /**
   * Fetch dashboard summary for a summoner (SWR Pattern).
   * If data exists, shows it immediately and refreshes in background.
   * Updates dashboardState Signal with loading, success, or error state.
   * 
   * SWR (Stale-While-Revalidate):
   * - First request: Load from API (loading = true)
   * - Cached request: Show stale data (isRefreshing = true, UI not blocked)
   * 
   * @param riotId - Summoner Riot ID in format "gameName#tagLine"
   * @throws Updates error state instead of throwing
   */
  async fetchDashboard(riotId: string): Promise<void> {
    if (!riotId?.trim()) {
      this.dashboardState.set({
        data: null,
        loading: false,
        error: 'Riot ID cannot be empty'
      });
      return;
    }

    this.currentSummoner.set(riotId);

    // SWR Logic: Check if we have stale data
    const hasStaleData = this.dashboardState().data !== null;

    if (hasStaleData) {
      // Show stale data while refreshing in background
      this.isRefreshing.set(true);
    } else {
      // Initial load: show loading state
      this.dashboardState.set({
        data: null,
        loading: true,
        error: null
      });
    }

    try {
      const dashboard = await firstValueFrom(
        this.http.get<DashboardSummary>(
          `${this.apiBaseUrl}/dashboard/summary?riotId=${encodeURIComponent(riotId)}`
        )
      );

      // Update with success state
      this.dashboardState.set({
        data: dashboard,
        loading: false,
        error: null
      });

      this.isRefreshing.set(false);
      this.notificationService.showSuccess(
        'Dashboard Updated',
        `Data for ${riotId} loaded successfully.`
      );
    } catch (error) {
      const errorMessage = this.handleApiError(error as HttpErrorResponse);
      
      // Only show error state if no stale data exists
      if (!hasStaleData) {
        this.dashboardState.set({
          data: null,
          loading: false,
          error: errorMessage
        });
      } else {
        // Keep stale data but show error notification
        this.notificationService.showWarning(
          'Refresh Failed',
          errorMessage
        );
      }

      this.isRefreshing.set(false);
    }
  }

  /**
   * Clear dashboard state and reset to initial state.
   */
  clearDashboard(): void {
    this.currentSummoner.set('');
    this.dashboardState.set({
      data: null,
      loading: false,
      error: null
    });
    this.isRefreshing.set(false);
  }

  /**
   * Handle API errors and return user-friendly messages.
   * Also emits notifications for critical errors (429, 503).
   * 
   * @param error - HTTP error response
   * @returns User-friendly error message
   */
  private handleApiError(error: HttpErrorResponse): string {
    switch (error.status) {
      case 429:
        const retryAfter = error.headers.get('Retry-After');
        const seconds = retryAfter ? parseInt(retryAfter, 10) : undefined;
        this.notificationService.showRateLimit(seconds);
        return 'Rate limited by Riot API. Please try again later.';

      case 503:
        this.notificationService.showServiceUnavailable();
        return 'Service temporarily unavailable. Please try again later.';

      case 404:
        return 'Summoner not found. Please check the Riot ID.';

      case 400:
        return 'Invalid Riot ID format. Use: gameName#tagLine';

      case 401:
      case 403:
        return 'Authentication failed. Please log in again.';

      case 500:
        return 'Server error. Please try again later.';

      case 0:
        return 'Network error. Please check your connection.';

      default:
        return error.error?.message || 'An error occurred while fetching data.';
    }
  }
}
