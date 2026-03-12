/**
 * @file Summoner Dashboard Component
 * @description Displays summoner statistics and match data using Angular Signals.
 * Demonstrates clean architecture pattern with reactive data flow.
 */

import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SummonerService } from '../../../core/services';

/**
 * Dashboard component for displaying League of Legends summoner statistics.
 * 
 * Features:
 * - Search summoner by Riot ID (gameName#tagLine)
 * - Display ranked statistics with metrics (KDA, winrate, trend)
 * - Show recent match history with performance analysis
 * - Real-time loading and error states using Signals
 * - SWR pattern: shows stale data while refreshing
 * 
 * @example
 * // In routing module
 * const routes = [
 *   { path: 'dashboard', component: SummonerDashboardComponent }
 * ];
 */
@Component({
  selector: 'app-summoner-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './summoner-dashboard.component.html',
  styleUrl: './summoner-dashboard.component.scss'
})
export class SummonerDashboardComponent implements OnInit {
  private readonly summonerService = inject(SummonerService);

  // Expose service signals to template
  readonly currentSummoner = this.summonerService.currentSummoner;
  readonly dashboard = this.summonerService.dashboard;
  readonly isLoading = this.summonerService.isLoading;
  readonly isRefreshing = this.summonerService.isRefreshing;
  readonly hasError = this.summonerService.hasError;
  readonly isDashboardReady = this.summonerService.isDashboardReady;
  readonly derivedMetrics = this.summonerService.derivedMetrics;

  /** Input field for Riot ID search */
  searchRiotId = '';

  ngOnInit(): void {
    // Optional: Load default summoner if available from localStorage
    const lastSummoner = localStorage.getItem('lastSummoner');
    if (lastSummoner) {
      this.searchRiotId = lastSummoner;
    }
  }

  /**
   * Search for summoner by Riot ID.
   */
  async onSearch(): Promise<void> {
    if (!this.searchRiotId.trim()) {
      return;
    }

    // Save last searched summoner
    localStorage.setItem('lastSummoner', this.searchRiotId);

    // Fetch dashboard (implements SWR pattern)
    await this.summonerService.fetchDashboard(this.searchRiotId);
  }

  /**
   * Refresh current summoner data.
   */
  async onRefresh(): Promise<void> {
    if (this.currentSummoner()) {
      await this.summonerService.fetchDashboard(this.currentSummoner());
    }
  }

  /**
   * Clear dashboard and reset search.
   */
  onClear(): void {
    this.searchRiotId = '';
    this.summonerService.clearDashboard();
  }

  /**
   * Get trend indicator for display.
   */
  getTrendIndicator(): string {
    const trend = this.derivedMetrics().trend;
    switch (trend) {
      case 'up':
        return '📈 Improving';
      case 'down':
        return '📉 Declining';
      default:
        return '➡️ Stable';
    }
  }

  /**
   * Format KDA for display.
   */
  formatKDA(): string {
    const kda = this.derivedMetrics().kda;
    return kda.toFixed(2);
  }

  /**
   * Format winrate percentage.
   */
  formatWinrate(): string {
    const winrate = this.derivedMetrics().winrate;
    return `${winrate.toFixed(1)}%`;
  }

  /**
   * Get state message based on loading/error/ready states.
   */
  getStateMessage(): string {
    const state = this.dashboard()?.dashboardState;
    
    if (this.isLoading() && !this.dashboard()) {
      return 'Loading summoner data...';
    }
    if (this.isRefreshing()) {
      return 'Refreshing data in background...';
    }
    if (this.hasError()) {
      return `Error: ${this.dashboard()?.error || 'Unknown error'}`;
    }
    return '';
  }
}
