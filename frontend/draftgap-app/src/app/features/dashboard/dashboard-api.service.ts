import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { APP_SETTINGS, AppSettings } from '../../core/config/app-settings';

export interface ProfileResponse {
  userId: string;
  email: string;
  riotId: string | null;
  region: string | null;
  lastSync: string | null;
  isAdmin: boolean;
  createdAt: string;
  summoner: {
    puuid: string;
    summonerName: string | null;
    profileIconId: number | null;
    summonerLevel: number | null;
  } | null;
}

export interface DashboardSummaryResponse {
  rankedOverview: {
    soloQueue: {
      queueType: string;
      tier: string | null;
      rank: string | null;
      leaguePoints: number;
      wins: number;
      losses: number;
      totalGames: number;
      winrate: number;
    } | null;
    flexQueue: {
      queueType: string;
      tier: string | null;
      rank: string | null;
      leaguePoints: number;
      wins: number;
      losses: number;
      totalGames: number;
      winrate: number;
    } | null;
  } | null;
  recentMatches: Array<{
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
  }>;
  performanceStats: {
    totalMatches: number;
    wins: number;
    losses: number;
    winrate: number;
    avgKills: number;
    avgDeaths: number;
    avgAssists: number;
    avgKda: number;
  } | null;
  topChampions: Array<{
    championId: number;
    championName: string;
    gamesPlayed: number;
    wins: number;
    winrate: number;
    avgKda: number;
  }>;
}

export interface ChampionStatsResponse {
  championId: number;
  championName: string;
  imageUrl: string | null;
  gamesPlayed: number;
  wins: number;
  losses: number;
  winrate: number;
  avgKills: number;
  avgDeaths: number;
  avgAssists: number;
  avgKda: number;
}

export interface RankedStatsResponse {
  soloQueue: {
    queueType: string;
    tier: string | null;
    rank: string | null;
    leaguePoints: number;
    wins: number;
    losses: number;
    totalGames: number;
    winrate: number;
    updatedAt: string;
  } | null;
  flexQueue: {
    queueType: string;
    tier: string | null;
    rank: string | null;
    leaguePoints: number;
    wins: number;
    losses: number;
    totalGames: number;
    winrate: number;
    updatedAt: string;
  } | null;
}

export interface MatchListItemResponse {
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
  queueId: number;
}

export interface PaginatedMatchesResponse {
  items: MatchListItemResponse[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

@Injectable({ providedIn: 'root' })
export class DashboardApiService {
  private readonly baseUrl: string;

  constructor(
    private readonly http: HttpClient,
    @Inject(APP_SETTINGS) settings: AppSettings
  ) {
    this.baseUrl = settings.apiBaseUrl;
  }

  getProfile(): Observable<ProfileResponse> {
    return this.http.get<ProfileResponse>(`${this.baseUrl}/profile`);
  }

  getDashboardSummary(): Observable<DashboardSummaryResponse> {
    return this.http.get<DashboardSummaryResponse>(`${this.baseUrl}/dashboard/summary`);
  }

  getChampionStats(): Observable<ChampionStatsResponse[]> {
    return this.http.get<ChampionStatsResponse[]>(`${this.baseUrl}/champions/stats`);
  }

  getRankedStats(): Observable<RankedStatsResponse> {
    return this.http.get<RankedStatsResponse>(`${this.baseUrl}/ranked`);
  }

  getMatches(page: number, pageSize: number): Observable<PaginatedMatchesResponse> {
    return this.http.get<PaginatedMatchesResponse>(`${this.baseUrl}/matches?page=${page}&pageSize=${pageSize}`);
  }

  triggerSync(): Observable<{ status?: string; message?: string }> {
    return this.http.post<{ status?: string; message?: string }>(`${this.baseUrl}/sync/trigger`, {});
  }

  searchUserByRiotId(riotId: string): Observable<{ userId: string; riotId: string; isActive: boolean }> {
    return this.http.post<{ userId: string; riotId: string; isActive: boolean }>(`${this.baseUrl}/friends/search`, {
      riotId
    });
  }
}
