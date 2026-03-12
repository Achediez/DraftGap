/**
 * @file SummonerService Unit Tests
 * @description Tests for summoner service with SWR, computed signals, and derived metrics.
 */

import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { SummonerService } from './summoner.service';
import { NotificationService } from './notification.service';
import { DashboardSummary } from '../models';

describe('SummonerService', () => {
  let service: SummonerService;
  let httpMock: HttpTestingController;

  const mockDashboard: DashboardSummary = {
    rankedOverview: {
      soloQueue: {
        queueType: 'RANKED_SOLO_5x5',
        tier: 'Diamond',
        rank: 'II',
        leaguePoints: 75,
        wins: 60,
        losses: 40,
        totalGames: 100,
        winrate: 60
      },
      flexQueue: null
    },
    recentMatches: [
      {
        matchId: '123',
        gameCreation: 1700000000000,
        gameDuration: 1680,
        championName: 'Ahri',
        win: true,
        kills: 8,
        deaths: 2,
        assists: 12,
        kda: 10,
        teamPosition: 'MID'
      }
    ],
    performanceStats: {
      totalMatches: 20,
      wins: 12,
      losses: 8,
      winrate: 60,
      avgKills: 5.5,
      avgDeaths: 2.5,
      avgAssists: 8.0,
      avgKda: 5.4
    },
    topChampions: [
      {
        championId: 103,
        championName: 'Ahri',
        gamesPlayed: 24,
        wins: 16,
        winrate: 68,
        avgKda: 6.5
      }
    ]
  };

  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [SummonerService, NotificationService]
    });

    service = TestBed.inject(SummonerService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    TestBed.resetTestingModule();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize with default state', () => {
    expect(service.currentSummoner()).toBe('');
    expect(service.dashboard()).toBeNull();
    expect(service.isLoading()).toBe(false);
  });

  it('should fetch dashboard successfully', async () => {
    const promise = service.fetchDashboard('Test#NA1');
    const req = httpMock.expectOne(r => r.url.includes('/api/dashboard/summary'));
    req.flush(mockDashboard);
    
    await promise;
    expect(service.dashboard()).toEqual(mockDashboard);
  });

  it('should handle empty Riot ID validation', async () => {
    await service.fetchDashboard('');
    expect(service.dashboardState().error).toContain('cannot be empty');
  });

  it('should handle 404 errors', async () => {
    const fetchPromise = service.fetchDashboard('Unknown#NA1');
    const req = httpMock.expectOne(r => r.url.includes('/api/dashboard/summary'));
    req.flush('Not found', { status: 404, statusText: 'Not Found' });
    
    await fetchPromise;
    expect(service.hasError()).toBe(true);
    expect(service.dashboard()).toBeNull();
  });

  it('should compute derived metrics - KDA', async () => {
    const fetchPromise = service.fetchDashboard('Test#NA1');
    const req = httpMock.expectOne(r => r.url.includes('/api/dashboard/summary'));
    req.flush(mockDashboard);

    await fetchPromise;
    const metrics = service.derivedMetrics();
    expect(metrics.kda).toBe(5.4);
  });

  it('should compute derived metrics - winrate', async () => {
    const fetchPromise = service.fetchDashboard('Test#NA1');
    const req = httpMock.expectOne(r => r.url.includes('/api/dashboard/summary'));
    req.flush(mockDashboard);

    await fetchPromise;
    const metrics = service.derivedMetrics();
    expect(metrics.winrate).toBe(60);
  });

  it('should compute trend correctly', async () => {
    const fetchPromise = service.fetchDashboard('Test#NA1');
    const req = httpMock.expectOne(r => r.url.includes('/api/dashboard/summary'));
    req.flush(mockDashboard);

    await fetchPromise;
    const metrics = service.derivedMetrics();
    expect(['up', 'down', 'neutral']).toContain(metrics.trend);
  });

  it('should initialize computed signals correctly', () => {
    expect(service.isLoading()).toBe(false);
    expect(service.hasError()).toBe(false);
    expect(service.isDashboardReady()).toBe(false);
    expect(service.derivedMetrics().kda).toBe(0);
  });
});


