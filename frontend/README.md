# DraftGap Frontend - Architecture & Implementation Guide

## 📋 Overview

This document describes the frontend architecture for DraftGap, following **Clean Architecture patterns** with the structure: **Core/Features/Shared**.

### Key Technologies
- **Angular 21.1** - Framework
- **Angular Signals** - Unidirectional reactive state management
- **TypeScript 5.9** - Type-safe development
- **Vitest 4.0** - Unit testing framework
- **RxJS 7.8** - Async operations support

---

## 🏗️ Architecture Structure

### Directory Layout

```
src/app/
├── core/
│   ├── models/                    # Domain Models (Interfaces)
│   │   ├── summoner.model.ts
│   │   ├── ranked.model.ts
│   │   ├── match.model.ts
│   │   └── index.ts
│   ├── services/                  # Application Services with Signals
│   │   ├── summoner.service.ts
│   │   ├── summoner.service.spec.ts
│   │   └── index.ts
│   ├── http/                      # HTTP Interceptors
│   │   ├── auth.interceptor.ts
│   │   ├── telemetry-error.interceptor.ts
│   │   ├── telemetry-error.interceptor.spec.ts
│   │   └── index.ts
│   ├── config/
│   └── auth/
├── features/                      # Feature Modules
│   ├── dashboard/
│   ├── admin/
│   ├── auth/
│   └── champions/                 # Stats module (future)
├── shared/                        # Shared Components & Utils
│   └── (to-be-populated)
├── app.config.ts                  # Application Configuration
└── app.ts                         # Root Component
```

---

## 📊 Data Flow Architecture

### Signal-Based State Management

The **SummonerService** implements a unidirectional data flow using Angular Signals:

```
User Action (fetchDashboard)
    ↓
[Loading State = true]
    ↓
HTTP Request to Backend
    ↓
Response Received
    ↓
[Success/Error State Updated]
    ↓
Computed Signals Calculated
    ↓
Components Re-render (via Signals)
```

### State Flow Diagram

```
┌─────────────────────────────────────────────┐
│         SummonerService Signals             │
├─────────────────────────────────────────────┤
│                                             │
│  currentSummoner: Signal<string>            │
│      └─> Tracks current summoner name       │
│                                             │
│  dashboardState: Signal<QueryResult>        │
│      └─> Contains { data, loading, error }  │
│                                             │
│  ┌─ Computed Signals (Auto-update) ┐       │
│  │ • isLoading (for spinners)       │       │
│  │ • hasError (for errors)          │       │
│  │ • isDashboardReady (for UI)      │       │
│  │ • dashboard (extract data)       │       │
│  └──────────────────────────────────┘       │
│                                             │
└─────────────────────────────────────────────┘
```

---

## 🎯 Core Components & Services

### 1. Domain Models (`core/models/`)

Represent the data contracts from the backend API.

#### `summoner.model.ts`
- **SummonerInfo**: Basic summoner data (name, level, icon)
- **DashboardSummary**: Complete dashboard with ranked, matches, stats
- **QueryResult<T>**: Generic wrapper for async operation states

#### `ranked.model.ts`
- **RankedQueueStats**: Single queue stats (Solo/Duo or Flex)
- **RankedOverview**: Combined ranked data

#### `match.model.ts`
- **RecentMatch**: Individual match summary
- **PerformanceStats**: Aggregated K/D/A statistics
- **TopChampion**: Champion statistics

### 2. SummonerService (`core/services/summoner.service.ts`)

Central service managing all summoner data with Signal-based state.

#### Key Signals

```typescript
// Primary Signals
readonly currentSummoner = signal<string>('');
readonly dashboardState = signal<QueryResult<DashboardSummary>>({ ... });

// Computed Signals (auto-update)
readonly isLoading = computed(() => this.dashboardState().loading);
readonly hasError = computed(() => this.dashboardState().error !== null);
readonly isDashboardReady = computed(() => 
  state.data !== null && !state.loading && !state.error
);
readonly dashboard = computed(() => this.dashboardState().data);
```

#### Key Methods

```typescript
// Fetch dashboard data
async fetchDashboard(riotId: string): Promise<void>

// Clear all state
clearDashboard(): void

// Handle API errors with user-friendly messages
private handleApiError(error: HttpErrorResponse): string
```

#### Error Handling

- **404**: "Summoner not found. Please check the Riot ID."
- **429**: "Rate limited by Riot API. Please try again later."
- **400**: "Invalid Riot ID format. Use: gameName#tagLine"
- **401/403**: "Authentication failed. Please log in again."
- **500**: "Server error. Please try again later."
- **Network Error**: "Network error. Please check your connection."

### 3. Telemetry Error Interceptor (`core/http/telemetry-error.interceptor.ts`)

Functional HTTP interceptor for request/response handling.

#### Features

- **Telemetry Headers**: Adds X-Request-Timestamp, X-Client-Version, X-Client-Name
- **Error Logging**: Detailed logging for all error codes
- **429 Handling**: Identifies rate limit errors for service handling
- **404 Handling**: Gracefully handles not-found errors

#### Telemetry Headers

```typescript
'X-Request-Timestamp': Date.now().toString()    // Request time
'X-Client-Version': '1.0.0'                     // App version
'X-Client-Name': 'DraftGap-Frontend'            // Client identifier
```

---

## 🔄 Usage Example

### In Component

```typescript
import { Component, inject, effect } from '@angular/core';
import { SummonerService } from '../core/services';

@Component({
  selector: 'app-dashboard',
  template: `
    <div *ngIf="summonerService.isLoading()">Loading...</div>
    <div *ngIf="summonerService.hasError()">
      Error: {{ summonerService.dashboardState().error }}
    </div>
    <div *ngIf="summonerService.isDashboardReady()">
      <app-ranked-overview [data]="summonerService.dashboard()?.rankedOverview!" />
    </div>
  `
})
export class DashboardComponent {
  summonerService = inject(SummonerService);

  ngOnInit() {
    // Reactive effect on data change
    effect(() => {
      const dashboard = this.summonerService.dashboard();
      if (dashboard) {
        console.log('Dashboard updated:', dashboard);
      }
    });

    // Fetch data
    this.summonerService.fetchDashboard('DG Achediez#H10');
  }
}
```

---

## 🧪 Testing Architecture

### Test Structure

- **`summoner.service.spec.ts`**: Service tests with Signal verification
  - ✅ Initialization tests
  - ✅ Success state tests
  - ✅ Error state tests (404, 429, 400, 500, network)
  - ✅ Computed Signal tests
  - ✅ URL encoding tests

- **`telemetry-error.interceptor.spec.ts`**: Interceptor tests
  - ✅ Telemetry header injection
  - ✅ Error handling (all status codes)
  - ✅ HTTP method compatibility
  - ✅ Network error handling

### Running Tests

```bash
# Run all tests
npm test

# Run specific test file
npm test -- summoner.service.spec.ts

# Run with coverage
npm test -- --coverage
```

### Test Coverage Goals

- **Services**: 90%+ coverage
- **Interceptors**: 85%+ coverage
- **Models**: N/A (interfaces only)

---

## 📡 API Integration

### Backend Endpoints Used

#### Dashboard Summary
```
GET /api/dashboard/summary?riotId={gameName%23tagLine}
Response: DashboardSummary
```

**Query Parameters:**
- `riotId`: Riot ID in format "gameName#tagLine" (URL encoded)

**Response Example:**
```json
{
  "rankedOverview": {
    "soloQueue": {
      "queueType": "RANKED_SOLO_5x5",
      "tier": "GOLD",
      "rank": "II",
      "leaguePoints": 67,
      "wins": 15,
      "losses": 10,
      "totalGames": 25,
      "winrate": 60.0
    },
    "flexQueue": null
  },
  "recentMatches": [
    {
      "matchId": "EUW1_123456",
      "gameCreation": 1694812800000,
      "gameDuration": 1875,
      "championName": "Ahri",
      "win": true,
      "kills": 5,
      "deaths": 2,
      "assists": 12,
      "kda": 8.5,
      "teamPosition": "MID"
    }
  ],
  "performanceStats": {
    "totalMatches": 20,
    "wins": 12,
    "losses": 8,
    "winrate": 60.0,
    "avgKills": 4.2,
    "avgDeaths": 2.1,
    "avgAssists": 8.5,
    "avgKda": 6.05
  },
  "topChampions": [
    {
      "championId": 103,
      "championName": "Ahri",
      "gamesPlayed": 25,
      "wins": 18,
      "winrate": 72.0,
      "avgKda": 7.2
    }
  ]
}
```

---

## 🔐 Security & Best Practices

### HTTP Interceptor Chain

```typescript
// In app.config.ts
provideHttpClient(
  withInterceptors([
    telemetryErrorInterceptor,  // Telemetry & error logging
    authInterceptor              // JWT attachment
  ])
)
```

### Error Handling Strategy

1. **Local Validation**: Input validation before API calls
2. **API Error Mapping**: Transform backend errors to user messages
3. **Logging**: Detailed error logging for debugging
4. **Graceful Degradation**: UI shows appropriate error messages

### Rate Limiting (429)

Currently, the interceptor logs rate limit errors. Future enhancement: implement exponential backoff retry.

---

## 📦 Signal Lifecycle & Effects

### Automatic Dependency Tracking

Signals in Angular 21 automatically track dependencies:

```typescript
// This computed signal auto-updates when dashboardState changes
readonly isDashboardReady = computed(() => {
  const state = this.dashboardState();
  return state.data !== null && !state.loading && !state.error;
});

// In component - automatic updates
effect(() => {
  const ready = this.summonerService.isDashboardReady();
  console.log('Dashboard ready:', ready);
});
```

---

## 🚀 CI/CD Integration

### Build Process

```bash
# Development
npm start             # ng serve --proxy-config proxy.conf.json

# Production
npm run build         # ng build

# Testing
npm test              # ng test (vitest)
```

### Quality Gates

- ✅ TypeScript compilation (no errors)
- ✅ All unit tests pass (exit code 0)
- ✅ No lint errors (if configured)

### Example Pipeline

```yaml
test:
  script:
    - npm test
    - npm run build
  only:
    - merge_requests
    - main
```

---

## 📚 Additional Resources

- [Angular Signals Documentation](https://angular.io/guide/signals)
- [Angular 21 Release Notes](https://angular.io/guide/releases)
- [RxJS Learning Path](https://rxjs.dev)
- [HTTP Interceptors](https://angular.io/guide/http-interceptors)

---

## 🔄 Future Enhancements

- [ ] Implement exponential backoff retry for 429 errors
- [ ] Add caching layer for dashboard data (Signal-based)
- [ ] Implement optimistic UI updates
- [ ] Add request debouncing for rapid re-fetches
- [ ] Expand telemetry tracking (performance metrics)
- [ ] Add analytics events for user interactions

---

**Last Updated**: March 11, 2026  
**Version**: 1.0.0  
**Author**: Frontend Team
