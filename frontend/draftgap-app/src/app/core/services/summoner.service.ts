/**
 * @file Summoner Service with Signals
 * @description Central service for managing summoner statistics and dashboard data using Angular Signals.
 * Implements unidirectional data flow with Signal-based state management.
 */

import { Injectable, signal, computed, effect } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { DashboardSummary, SummonerInfo, QueryResult } from '../models';

/**
 * Service for managing League of Legends summoner statistics and dashboard data.
 * 
 * Signal-based state management:
 * - `currentSummoner`: Currently selected summoner name
 * - `dashboardState`: Query result state (data, loading, error)
 * - `isLoading`: Computed Boolean for easy template binding
 * - `hasError`: Computed Boolean for error detection
 * - `isDashboardReady`: Computed Boolean indicating valid data availability
 * 
 * @example
 * constructor(private summonerService: SummonerService) {
 *   // Subscribe to dashboard data
 *   effect(() => {
 *     const dashboard = this.summonerService.dashboard();
 *     if (dashboard) {
 *       console.log(dashboard.rankedOverview);
 *     }
 *   });
 * }
 * 
 * // Fetch dashboard
 * this.summonerService.fetchDashboard('gameName#tagLine');
 */
@Injectable({
  providedIn: 'root'
})
export class SummonerService {
  private readonly apiBaseUrl = '/api';

  /** Current summoner name (Riot ID format: gameName#tagLine) */
  readonly currentSummoner = signal<string>('');

  /** Query result state containing data, loading, and error states */
  readonly dashboardState = signal<QueryResult<DashboardSummary>>({
    data: null,
    loading: false,
    error: null
  });

  /** Computed signal: True when dashboard is currently loading */
  readonly isLoading = computed(() => this.dashboardState().loading);

  /** Computed signal: True when dashboard has an error */
  readonly hasError = computed(() => this.dashboardState().error !== null);

  /** Computed signal: True when dashboard data is valid and ready for display */
  readonly isDashboardReady = computed(() => {
    const state = this.dashboardState();
    return state.data !== null && !state.loading && !state.error;
  });

  /** Computed signal: Extract dashboard data from state */
  readonly dashboard = computed(() => this.dashboardState().data);

  constructor(private http: HttpClient) {
    // Log state changes for debugging
    effect(() => {
      const state = this.dashboardState();
      if (state.error) {
        console.warn('[SummonerService] Dashboard error:', state.error);
      }
    });
  }

  /**
   * Fetch dashboard summary for a summoner.
   * Updates dashboardState Signal with loading, success, or error state.
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

    // Set loading state
    this.dashboardState.set({
      data: null,
      loading: true,
      error: null
    });

    try {
      const dashboard = await firstValueFrom(
        this.http.get<DashboardSummary>(`${this.apiBaseUrl}/dashboard/summary?riotId=${encodeURIComponent(riotId)}`)
      );

      // Update with success state
      this.dashboardState.set({
        data: dashboard,
        loading: false,
        error: null
      });
    } catch (error) {
      const errorMessage = this.handleApiError(error as HttpErrorResponse);
      this.dashboardState.set({
        data: null,
        loading: false,
        error: errorMessage
      });
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
  }

  /**
   * Handle API errors and return user-friendly messages.
   * Handles specific status codes: 404, 429, 400, 500, etc.
   * 
   * @param error - HTTP error response
   * @returns User-friendly error message
   */
  private handleApiError(error: HttpErrorResponse): string {
    switch (error.status) {
      case 404:
        return 'Summoner not found. Please check the Riot ID.';
      case 429:
        return 'Rate limited by Riot API. Please try again later.';
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
