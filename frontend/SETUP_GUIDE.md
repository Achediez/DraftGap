# DraftGap Frontend - Setup & Integration Guide

## 🚀 Quick Start with SummonerService

### Step 1: Inject the Service

```typescript
import { Component, inject } from '@angular/core';
import { SummonerService } from '../core/services';

@Component({
  selector: 'app-my-component',
  standalone: true,
  template: `...`
})
export class MyComponent {
  // Inject the service
  summonerService = inject(SummonerService);
}
```

### Step 2: Call fetchDashboard

```typescript
export class MyComponent implements OnInit {
  summonerService = inject(SummonerService);

  ngOnInit() {
    // Fetch data (Riot ID format: gameName#tagLine)
    this.summonerService.fetchDashboard('DG Achediez#H10');
  }
}
```

### Step 3: Bind Signals to Template

```html
<!-- Loading state -->
<div *ngIf="summonerService.isLoading()">
  <mat-spinner></mat-spinner>
  <p>Loading summoner statistics...</p>
</div>

<!-- Error state -->
<div *ngIf="summonerService.hasError()" class="error-box">
  {{ summonerService.dashboardState().error }}
</div>

<!-- Success state -->
<div *ngIf="summonerService.isDashboardReady()">
  <!-- Access dashboard data -->
  <div *ngIf="summonerService.dashboard()?.rankedOverview?.soloQueue as soloQueue">
    <p>{{ soloQueue.tier }} {{ soloQueue.rank }}</p>
    <p>{{ soloQueue.wins }}W - {{ soloQueue.losses }}L</p>
  </div>

  <!-- Recent matches -->
  <app-match-list [matches]="summonerService.dashboard()?.recentMatches || []" />

  <!-- Top champions -->
  <app-champion-list [champions]="summonerService.dashboard()?.topChampions || []" />
</div>
```

---

## 🔄 Reactive Effects in Components

### Subscribe to State Changes

```typescript
import { effect } from '@angular/core';

export class DashboardComponent implements OnInit {
  summonerService = inject(SummonerService);

  ngOnInit() {
    // Track when dashboard data changes
    effect(() => {
      const dashboard = this.summonerService.dashboard();
      if (dashboard) {
        console.log('Dashboard data loaded:', dashboard);
        // Perform side-effects: analytics, caching, etc.
        this.trackDashboardLoad(dashboard);
      }
    });

    // Track when errors occur
    effect(() => {
      const error = this.summonerService.dashboardState().error;
      if (error) {
        this.toastr.error(error);
      }
    });

    // Track loading state
    effect(() => {
      const loading = this.summonerService.isLoading();
      if (loading) {
        this.progressBar.start();
      } else {
        this.progressBar.complete();
      }
    });
  }
}
```

---

## 🧪 Integration Testing

### Example: Dashboard Component Test

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DashboardComponent } from './dashboard.component';
import { SummonerService } from '../core/services';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let summonerService: SummonerService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardComponent, HttpClientTestingModule],
      providers: [SummonerService]
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    summonerService = TestBed.inject(SummonerService);
  });

  it('should display loading spinner when fetching', () => {
    summonerService.fetchDashboard('Test#NA1');

    fixture.detectChanges();

    const spinner = fixture.nativeElement.querySelector('mat-spinner');
    expect(spinner).toBeTruthy();
  });

  it('should display dashboard data when ready', async () => {
    // Mock dashboard
    summonerService.dashboardState.set({
      data: {
        rankedOverview: { ... },
        recentMatches: [ ... ],
        performanceStats: { ... },
        topChampions: [ ... ]
      },
      loading: false,
      error: null
    });

    fixture.detectChanges();

    expect(summonerService.isDashboardReady()).toBe(true);
    const dashboardSection = fixture.nativeElement.querySelector('.dashboard');
    expect(dashboardSection).toBeTruthy();
  });

  it('should display error message on failure', () => {
    summonerService.dashboardState.set({
      data: null,
      loading: false,
      error: 'Summoner not found'
    });

    fixture.detectChanges();

    const errorBox = fixture.nativeElement.querySelector('.error-box');
    expect(errorBox?.textContent).toContain('Summoner not found');
  });
});
```

---

## 📱 Component Examples

### Example 1: Search Box Component

```typescript
import { Component, inject } from '@angular/core';
import { SummonerService } from '../core/services';

@Component({
  selector: 'app-summoner-search',
  standalone: true,
  template: `
    <div class="search-box">
      <input 
        type="text" 
        placeholder="Riot ID (gameName#tagLine)"
        #searchInput
      />
      <button (click)="handleSearch(searchInput.value)">
        Search
      </button>
    </div>
  `
})
export class SummonerSearchComponent {
  summonerService = inject(SummonerService);

  handleSearch(riotId: string) {
    this.summonerService.fetchDashboard(riotId);
  }
}
```

### Example 2: Statistics Display Component

```typescript
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SummonerService } from '../core/services';

@Component({
  selector: 'app-stats-display',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div *ngIf="summonerService.isDashboardReady()">
      <div class="ranked-stats">
        <h3>Ranked Stats</h3>
        <div *ngIf="summonerService.dashboard()?.rankedOverview?.soloQueue as solo">
          <p>Solo/Duo: {{ solo.tier }} {{ solo.rank }} - {{ solo.leaguePoints }} LP</p>
          <p>Winrate: {{ solo.winrate }}%</p>
        </div>
      </div>

      <div class="performance-stats">
        <h3>Performance</h3>
        <div *ngIf="summonerService.dashboard()?.performanceStats as stats">
          <p>Avg KDA: {{ stats.avgKda | number: '1.2-2' }}</p>
          <p>Winrate: {{ stats.winrate }}%</p>
        </div>
      </div>
    </div>
  `
})
export class StatsDisplayComponent {
  summonerService = inject(SummonerService);
}
```

---

## 🔐 Interceptor Configuration

### Current Interceptor Chain

The app has two interceptors that run in order:

```
Request
  ↓
[telemetryErrorInterceptor] - Adds headers, handles errors
  ↓
[authInterceptor] - Attaches JWT token
  ↓
Backend API
  ↓
Response
  ↓
[telemetryErrorInterceptor] - Logs errors
  ↓
[authInterceptor] - No response handling
  ↓
Component
```

### Interceptor Responsibilities

#### telemetryErrorInterceptor
- ✅ Adds X-Request-Timestamp header
- ✅ Adds X-Client-Version header
- ✅ Adds X-Client-Name header
- ✅ Logs HTTP errors (404, 429, 500, etc.)
- ✅ Preserves error details for service handling

#### authInterceptor
- ✅ Attaches JWT token to Authorization header
- ✅ Only adds header if token exists

---

## 🧹 Cleanup & Best Practices

### Clearing Service State

```typescript
export class MyComponent implements OnDestroy {
  summonerService = inject(SummonerService);

  ngOnDestroy() {
    // Clear state when component is destroyed
    this.summonerService.clearDashboard();
  }
}
```

### Avoiding Signal Anti-Patterns

```typescript
// ❌ DON'T: Subscribe to readable signal
// Signals aren't Observables!
this.summonerService.dashboard$.subscribe(...);

// ✅ DO: Use Signals directly in templates
{{ summonerService.dashboard()?.rankedOverview?.tier }}

// ✅ DO: Use effects for side-effects
effect(() => {
  const dashboard = this.summonerService.dashboard();
  if (dashboard) {
    // Handle side-effect
  }
});

// ❌ DON'T: Mutate Signal values
const state = this.summonerService.dashboardState();
state.data = newData;  // Won't trigger reactivity!

// ✅ DO: Use .set() or .update()
this.summonerService.dashboardState.set({
  data: newData,
  loading: false,
  error: null
});
```

---

## 🐛 Debugging

### Enable Service Logging

Add to `summoner.service.ts`:

```typescript
constructor(private http: HttpClient) {
  // Log state changes
  effect(() => {
    const state = this.dashboardState();
    console.log('[SummonerService] State:', state);
  });
}
```

### Browser DevTools

Open DevTools → Console and check:

```javascript
// Check service state in console
ng.getComponent(document.body).summonerService.dashboardState()

// Check loading state
ng.getComponent(document.body).summonerService.isLoading()

// Check if ready
ng.getComponent(document.body).summonerService.isDashboardReady()
```

---

## 📚 Further Reading

- [Angular Signals Guide](https://angular.io/guide/signals)
- [HTTP Interceptors](https://angular.io/guide/http-interceptors)
- [Effect Documentation](https://angular.io/guide/signals#effects)
- [Computed Signals](https://angular.io/guide/signals#computed)

---

## ❓ FAQ

### Q: How do I update the API base URL?

A: Update `app.config.ts` and modify the `apiBaseUrl` in `summoner.service.ts`:

```typescript
// In summoner.service.ts
private readonly apiBaseUrl = '/api';  // Change this
```

### Q: Can I use this with stand-alone components?

A: Yes! All services are provided at root level with `providedIn: 'root'`.

### Q: How do I handle multiple concurrent requests?

A: Currently, each `fetchDashboard()` call overwrites the previous request. For parallel requests, extend the service to support multiple instances or use a different state management approach.

### Q: Are Signals reactive like Observables?

A: Yes, but they're synchronous and don't support operators like Observables. Use Signals for state, RxJS for streams.

---

**Last Updated**: March 11, 2026  
**Version**: 1.0.0
