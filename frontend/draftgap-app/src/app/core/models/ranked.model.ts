/**
 * @file Ranked Statistics Models
 * @description Domain models for League of Legends ranked statistics.
 * These interfaces mirror the DTOs from the backend API.
 */

/**
 * Ranked statistics for a specific queue (Solo/Duo or Flex).
 * @example { queueType: 'RANKED_SOLO_5x5', tier: 'GOLD', rank: 'II', leaguePoints: 67, wins: 15, losses: 10, totalGames: 25, winrate: 60.0 }
 */
export interface RankedQueueStats {
  queueType: string;
  tier: string | null;
  rank: string | null;
  leaguePoints: number;
  wins: number;
  losses: number;
  totalGames: number;
  winrate: number;
}

/**
 * Overview of ranked statistics for both Solo/Duo and Flex queues.
 */
export interface RankedOverview {
  soloQueue: RankedQueueStats | null;
  flexQueue: RankedQueueStats | null;
}
