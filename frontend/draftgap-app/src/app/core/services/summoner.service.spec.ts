/**
 * @file Summoner Service Tests
 * @description Basic unit tests for SummonerService with Signal state management.
 */

import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { SummonerService } from './summoner.service';
import { DashboardSummary } from '../models';

describe('SummonerService', () => {
  let service: SummonerService;
  let httpMock: HttpTestingController;

  const mockData: DashboardSummary = {
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
    recentMatches: [],
    performanceStats: null,
    topChampions: []
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

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize with empty state', () => {
    expect(service.currentSummoner()).toBe('');
    expect(service.isLoading()).toBe(false);
    expect(service.hasError()).toBe(false);
    expect(service.isDashboardReady()).toBe(false);
  });

  it('should validate empty Riot ID', async () => {
    await service.fetchDashboard('');
    expect(service.hasError()).toBe(true);
    expect(service.dashboardState().error).toContain('cannot be empty');
  });

  it('should handle 404 errors', async () => {
    const promise = service.fetchDashboard('NotFound#ID');
    httpMock.expectOne(
      req => req.url.includes('/api/dashboard/summary')
    ).flush(null, { status: 404, statusText: 'Not Found' });
    
    await promise;
    expect(service.hasError()).toBe(true);
    expect(service.dashboard()).toBeNull();
  });

  it('should handle 429 errors', async () => {
    const promise = service.fetchDashboard('Test#ID');
    httpMock.expectOne(
      req => req.url.includes('/api/dashboard/summary')
    ).flush(null, { status: 429, statusText: 'Too Many Requests' });
    
    await promise;
    expect(service.hasError()).toBe(true);
    expect(service.dashboardState().error).toContain('Rate limited');
  });

  it('should load dashboard successfully', async () => {
    const promise = service.fetchDashboard('Test#NA1');
    expect(service.isLoading()).toBe(true);

    httpMock.expectOne(
      req => req.url.includes('/api/dashboard/summary')
    ).flush(mockData);

    await promise;
    expect(service.isDashboardReady()).toBe(true);
    expect(service.hasError()).toBe(false);
    expect(service.dashboard()).toEqual(mockData);
  });

  it('should clear dashboard state', async () => {
    const promise = service.fetchDashboard('Test#NA1');
    httpMock.expectOne(
      req => req.url.includes('/api/dashboard/summary')
    ).flush(mockData);
    
    await promise;
    service.clearDashboard();

    expect(service.currentSummoner()).toBe('');
    expect(service.isDashboardReady()).toBe(false);
    expect(service.dashboard()).toBeNull();
  });

  it('computed isLoading reflects state', async () => {
    const promise = service.fetchDashboard('Test#NA1');
    expect(service.isLoading()).toBe(true);

    httpMock.expectOne(
      req => req.url.includes('/api/dashboard/summary')
    ).flush(mockData);

    await promise;
    expect(service.isLoading()).toBe(false);
  });
});
