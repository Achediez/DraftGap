/**
 * @file Summoner Models
 * @description Domain models for League of Legends summoner data.
 */

import { RankedOverview } from './ranked.model';
import { RecentMatch, PerformanceStats, TopChampion } from './match.model';

/**
 * Basic summoner information from Riot API.
 */
export interface SummonerInfo {
  puuid: string;
  summonerName: string;
  profileIconId: number;
  summonerLevel: number;
}

/**
 * Complete dashboard summary for a summoner.
 * Aggregates:
 * - rankedOverview: Solo/Duo and Flex ranked stats
 * - recentMatches: Last 10 matches
 * - performanceStats: K/D/A averages from last 20 matches
 * - topChampions: Top 5 most played champions from last 50 matches
 */
export interface DashboardSummary {
  rankedOverview: RankedOverview | null;
  recentMatches: RecentMatch[];
  performanceStats: PerformanceStats | null;
  topChampions: TopChampion[];
}

/**
 * Query result state wrapper for tracking async operations.
 * @template T The data type being fetched
 */
export interface QueryResult<T> {
  data: T | null;
  loading: boolean;
  error: string | null;
}
