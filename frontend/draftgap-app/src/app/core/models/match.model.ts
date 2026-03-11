/**
 * @file Match Models
 * @description Domain models for League of Legends match data.
 */

/**
 * Simplified recent match data for dashboard display.
 * @example { matchId: 'EUW1_123456', gameCreation: 1694812800000, gameDuration: 1875, championName: 'Ahri', win: true, kills: 5, deaths: 2, assists: 12, kda: 8.5, teamPosition: 'MID' }
 */
export interface RecentMatch {
  matchId: string;
  gameCreation: number;
  gameDuration: number;
  championName: string;
  win: boolean;
  kills: number;
  deaths: number;
  assists: number;
  kda: number;
  teamPosition: string;
}

/**
 * Aggregated performance statistics calculated over recent matches.
 * @example { totalMatches: 20, wins: 12, losses: 8, winrate: 60.0, avgKills: 4.2, avgDeaths: 2.1, avgAssists: 8.5, avgKda: 6.05 }
 */
export interface PerformanceStats {
  totalMatches: number;
  wins: number;
  losses: number;
  winrate: number;
  avgKills: number;
  avgDeaths: number;
  avgAssists: number;
  avgKda: number;
}

/**
 * Top champion data for the summoner.
 * @example { championId: 103, championName: 'Ahri', gamesPlayed: 25, wins: 18, winrate: 72.0, avgKda: 7.2 }
 */
export interface TopChampion {
  championId: number;
  championName: string;
  gamesPlayed: number;
  wins: number;
  winrate: number;
  avgKda: number;
}
