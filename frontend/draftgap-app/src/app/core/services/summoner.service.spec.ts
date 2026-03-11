/**
 * @file Summoner Service Tests
 * @description Unit tests for SummonerService with Signal state management.
 */

import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { SummonerService } from './summoner.service';
import { DashboardSummary } from '../models';

describe('SummonerService', () => {
  let service: SummonerService;
  let httpMock: HttpTestingController;

  const mockDashboardData: DashboardSummary = {
    rankedOverview: {
      soloQueue: {
        queueType: 'RANKED_SOLO_5x5',
        tier: 'GOLD',
        rank: 'II',
        leaguePoints: 67,
        wins: 15,
        losses: 10,
        totalGames: 25,
        winrate: 60.0
      },
      flexQueue: null
    },
    recentMatches: [
      {
        matchId: 'EUW1_123456',
        gameCreation: 1694812800000,
        gameDuration: 1875,
        championName: 'Ahri',
        win: true,
        kills: 5,
        deaths: 2,
        assists: 12,
        kda: 8.5,
        teamPosition: 'MID'
      }
    ],
    performanceStats: {
      totalMatches: 20,
      wins: 12,
      losses: 8,
      winrate: 60.0,
      avgKills: 4.2,
      avgDeaths: 2.1,
      avgAssists: 8.5,
      avgKda: 6.05
    },
    topChampions: [
      {
        championId: 103,
        championName: 'Ahri',
        gamesPlayed: 25,
        wins: 18,
        winrate: 72.0,
        avgKda: 7.2
      }
    ]
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [SummonerService]
    });

    service = TestBed.inject(SummonerService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('initialization', () => {
    it('should create the service', () => {
      expect(service).toBeTruthy();
    });

    it('should initialize with empty currentSummoner', () => {
      expect(service.currentSummoner()).toBe('');
    });

    it('should initialize with loading false', () => {
      expect(service.isLoading()).toBe(false);
    });

    it('should initialize with no error', () => {
      expect(service.hasError()).toBe(false);
    });

    it('should initialize with isDashboardReady false', () => {
      expect(service.isDashboardReady()).toBe(false);
    });
  });

  describe('fetchDashboard - Success', () => {
    it('should fetch dashboard successfully', () => {
      const riotId = 'TestPlayer#NA1';

      service.fetchDashboard(riotId);

      expect(service.isLoading()).toBe(true);

      const req = httpMock.expectOne(
        req => req.url.includes('/api/dashboard/summary') && req.url.includes(riotId)
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockDashboardData);

      expect(service.isLoading()).toBe(false);
      expect(service.hasError()).toBe(false);
      expect(service.isDashboardReady()).toBe(true);
      expect(service.dashboard()).toEqual(mockDashboardData);
    });

    it('should handle ranked overview data', () => {
      const riotId = 'TestPlayer#NA1';

      service.fetchDashboard(riotId);

      const req = httpMock.expectOne(
        req => req.url.includes('/api/dashboard/summary')
      );
      req.flush(mockDashboardData);

      const dashboard = service.dashboard();
      expect(dashboard?.rankedOverview?.soloQueue?.tier).toBe('GOLD');
      expect(dashboard?.rankedOverview?.soloQueue?.winrate).toBe(60.0);
    });

    it('should handle recent matches data', () => {
      const riotId = 'TestPlayer#NA1';

      service.fetchDashboard(riotId);

      const req = httpMock.expectOne(
        req => req.url.includes('/api/dashboard/summary')
      );
      req.flush(mockDashboardData);

      const matches = service.dashboard()?.recentMatches || [];
      expect(matches.length).toBe(1);
      expect(matches[0].championName).toBe('Ahri');
      expect(matches[0].win).toBe(true);
    });

    it('should handle top champions data', () => {
      const riotId = 'TestPlayer#NA1';

      service.fetchDashboard(riotId);

      const req = httpMock.expectOne(
        req => req.url.includes('/api/dashboard/summary')
      );
      req.flush(mockDashboardData);

      const topChamps = service.dashboard()?.topChampions || [];
      expect(topChamps.length).toBe(1);
      expect(topChamps[0].championName).toBe('Ahri');
      expect(topChamps[0].winrate).toBe(72.0);
    });
  });

  describe('fetchDashboard - Errors', () => {
    it('should handle 404 Not Found error', () => {
      const riotId = 'InvalidPlayer#XX';

      service.fetchDashboard(riotId);

      const req = httpMock.expectOne(
        req => req.url.includes('/api/dashboard/summary')
      );
      req.flush(null, { status: 404, statusText: 'Not Found' });

      expect(service.isLoading()).toBe(false);
      expect(service.hasError()).toBe(true);
      expect(service.isDashboardReady()).toBe(false);
      expect(service.dashboardState().error).toContain('not found');
    });

    it('should handle 429 Rate Limited error', () => {
      const riotId = 'TestPlayer#NA1';

      service.fetchDashboard(riotId);

      const req = httpMock.expectOne(
        req => req.url.includes('/api/dashboard/summary')
      );
      req.flush(null, { status: 429, statusText: 'Too Many Requests' });

      expect(service.hasError()).toBe(true);
      expect(service.dashboardState().error).toContain('Rate limited');
    });

    it('should handle 400 Bad Request error', () => {
      const riotId = '';

      service.fetchDashboard(riotId);

      expect(service.hasError()).toBe(true);
      expect(service.dashboardState().error).toContain('cannot be empty');
    });

    it('should handle 500 Server error', () => {
      const riotId = 'TestPlayer#NA1';

      service.fetchDashboard(riotId);

      const req = httpMock.expectOne(
        req => req.url.includes('/api/dashboard/summary')
      );
      req.flush(null, { status: 500, statusText: 'Internal Server Error' });

      expect(service.hasError()).toBe(true);
      expect(service.dashboardState().error).toContain('Server error');
    });

    it('should validate empty Riot ID', () => {
      service.fetchDashboard('');

      expect(service.dashboardState().error).toBe('Riot ID cannot be empty');
      expect(service.isLoading()).toBe(false);
    });

    it('should validate whitespace-only Riot ID', () => {
      service.fetchDashboard('   ');

      expect(service.dashboardState().error).toBe('Riot ID cannot be empty');
    });
  });

  describe('clearDashboard', () => {
    it('should reset all state', () => {
      service.fetchDashboard('TestPlayer#NA1');

      const req = httpMock.expectOne(
        req => req.url.includes('/api/dashboard/summary')
      );
      req.flush(mockDashboardData);

      expect(service.isDashboardReady()).toBe(true);

      service.clearDashboard();

      expect(service.currentSummoner()).toBe('');
      expect(service.isLoading()).toBe(false);
      expect(service.hasError()).toBe(false);
      expect(service.isDashboardReady()).toBe(false);
      expect(service.dashboard()).toBeNull();
    });
  });

  describe('Computed Signals', () => {
    it('isLoading should track loading state', () => {
      expect(service.isLoading()).toBe(false);

      service.fetchDashboard('Test#NA1');

      expect(service.isLoading()).toBe(true);

      const req = httpMock.expectOne(
        req => req.url.includes('/api/dashboard/summary')
      );
      req.flush(mockDashboardData);

      expect(service.isLoading()).toBe(false);
    });

    it('hasError should track error state', () => {
      expect(service.hasError()).toBe(false);

      service.fetchDashboard('Test#NA1');

      const req = httpMock.expectOne(
        req => req.url.includes('/api/dashboard/summary')
      );
      req.flush(null, { status: 404, statusText: 'Not Found' });

      expect(service.hasError()).toBe(true);
    });

    it('isDashboardReady should be true only when ready', () => {
      expect(service.isDashboardReady()).toBe(false);

      service.fetchDashboard('Test#NA1');

      expect(service.isDashboardReady()).toBe(false);

      const req = httpMock.expectOne(
        req => req.url.includes('/api/dashboard/summary')
      );
      req.flush(mockDashboardData);

      expect(service.isDashboardReady()).toBe(true);
    });

    it('dashboard should extract data from state', () => {
      expect(service.dashboard()).toBeNull();

      service.fetchDashboard('Test#NA1');

      const req = httpMock.expectOne(
        req => req.url.includes('/api/dashboard/summary')
      );
      req.flush(mockDashboardData);

      expect(service.dashboard()).toBe(mockDashboardData);
    });
  });

  describe('URL handling', () => {
    it('should properly encode Riot ID in URL', () => {
      const riotId = 'Test Player#N@1';

      service.fetchDashboard(riotId);

      const req = httpMock.expectOne(
        req => req.url.includes(encodeURIComponent(riotId))
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockDashboardData);

      expect(service.isDashboardReady()).toBe(true);
    });
  });
});
