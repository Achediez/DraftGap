# DraftGap Frontend - Signal State Management Guide

## 📊 Understanding Angular Signals

Angular Signals (v21+) provide a fine-grained reactive system for state management without RxJS overhead.

---

## 🔄 Signals vs RxJS Observables

| Feature | Signals | Observables |
|---------|---------|-------------|
| **Type** | Synchronous value container | Asynchronous stream |
| **Tracking** | Automatic dependency tracking | Manual subscription |
| **Change Detection** | Very efficient | Good efficiency |
| **Learning Curve** | Low | Medium-High |
| **Async Native** | Not ideal | Excellent |

**Decision**: Use Signals for state, RxJS for async operations (HTTP calls).

---

## 🎯 SummonerService Signal Architecture

### State Structure

```typescript
// Primary State Signal
readonly dashboardState = signal<QueryResult<DashboardSummary>>({
  data: null,
  loading: false,
  error: null
});

// Query Result Type: Generic state wrapper
interface QueryResult<T> {
  data: T | null;           // Actual data or null
  loading: boolean;         // True while fetching
  error: string | null;     // Error message or null
}
```

### Computed Signals

Computed signals read from source signals and auto-update:

```typescript
// Auto-computed from dashboardState
readonly isLoading = computed(() => this.dashboardState().loading);
readonly hasError = computed(() => this.dashboardState().error !== null);
readonly isDashboardReady = computed(() => {
  const state = this.dashboardState();
  return state.data !== null && !state.loading && !state.error;
});

// Template usage - no subscriptions needed!
<div *ngIf="isLoading()">Loading...</div>
```

---

## 🔄 Unidirectional Data Flow

### State Update Pattern

```
fetchDashboard(riotId)
    ↓
Validate Input
    ↓
Set Loading State
    dashboardState.set({ data: null, loading: true, error: null })
    ↓
Make HTTP Request
    ↓
    ├─ Success
    │   └─ Set Success State
    │       dashboardState.set({ data: dashboard, loading: false, error: null })
    │
    └─ Error
        └─ Set Error State
            dashboardState.set({ data: null, loading: false, error: msg })
    ↓
Computed Signals Auto-Update
    ├─ isLoading() → false
    ├─ hasError() → true/false
    ├─ isDashboardReady() → true/false
    └─ dashboard() → data
    ↓
Components Re-render via Signals
```

### Code Example: Full Cycle

```typescript
// 1. User triggers fetch
async fetchDashboard(riotId: string): Promise<void> {
  // 2. Set loading state (one-way flow)
  this.dashboardState.set({
    data: null,
    loading: true,
    error: null
  });

  try {
    // 3. Make async call
    const dashboard = await firstValueFrom(
      this.http.get<DashboardSummary>(`/api/dashboard/summary?riotId=${riotId}`)
    );

    // 4. Update with success state
    this.dashboardState.set({
      data: dashboard,
      loading: false,
      error: null
    });
  } catch (error) {
    // 5. Update with error state
    const message = this.handleApiError(error);
    this.dashboardState.set({
      data: null,
      loading: false,
      error: message
    });
  }
}
```

---

## 📱 Component Integration

### Template Usage

```typescript
@Component({
  selector: 'app-dashboard',
  template: `
    <!-- Show loading spinner -->
    <div *ngIf="summonerService.isLoading()">
      <mat-spinner></mat-spinner>
    </div>

    <!-- Show error message -->
    <div *ngIf="summonerService.hasError()" class="error-box">
      {{ summonerService.dashboardState().error }}
    </div>

    <!-- Show data when ready -->
    <div *ngIf="summonerService.isDashboardReady()">
      <app-ranked-section [data]="summonerService.dashboard()?.rankedOverview!" />
      <app-matches-section [matches]="summonerService.dashboard()?.recentMatches!" />
    </div>
  `
})
export class DashboardComponent {
  summonerService = inject(SummonerService);
}
```

### Effects (Reactive Side-Effects)

```typescript
export class DashboardComponent implements OnInit {
  summonerService = inject(SummonerService);

  ngOnInit() {
    // Run when dashboard changes
    effect(() => {
      const dashboard = this.summonerService.dashboard();
      if (dashboard) {
        // Analytics, caching, etc.
        this.trackDashboardLoad(dashboard);
      }
    });

    // Run when loading state changes
    effect(() => {
      const loading = this.summonerService.isLoading();
      if (loading) {
        // Start progress bar, disable form, etc.
        this.showProgressBar();
      } else {
        this.hideProgressBar();
      }
    });
  }
}
```

---

## 🧪 Testing Signals

### Testing Pattern

```typescript
describe('SummonerService', () => {
  let service: SummonerService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [SummonerService]
    });
    service = TestBed.inject(SummonerService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('should update isLoading during fetch', async () => {
    // Initial state
    expect(service.isLoading()).toBe(false);

    // Start fetch
    const promise = service.fetchDashboard('Test#NA1');

    // While fetching
    expect(service.isLoading()).toBe(true);

    // Mock response
    httpMock.expectOne(/dashboard/).flush(mockData);
    await promise;

    // After fetch
    expect(service.isLoading()).toBe(false);
  });

  it('should compute isDashboardReady correctly', async () => {
    expect(service.isDashboardReady()).toBe(false);

    const promise = service.fetchDashboard('Test#NA1');
    httpMock.expectOne(/dashboard/).flush(mockData);
    await promise;

    expect(service.isDashboardReady()).toBe(true);
  });
});
```

---

## ⚡ Performance Considerations

### Automatic Change Detection

Signals provide fine-grained reactivity:

```typescript
// ✅ GOOD: Component only re-renders if isLoading() actually changes
<div *ngIf="summonerService.isLoading()">Loading...</div>

// ❌ AVOID: Creates memory leaks, manual cleanup needed
stream$ = this.summonerService.dashboardState$.asObservable();
```

### Computed Signal Caching

Computed signals cache their values:

```typescript
// isDashboardReady is only recomputed when dashboardState changes
readonly isDashboardReady = computed(() => {
  const state = this.dashboardState();
  return state.data !== null && !state.loading && !state.error;
});

// Multiple accesses use cached value
if (this.isDashboardReady()) { /* once computed */ }
if (this.isDashboardReady()) { /* cached value */ }
```

---

## 🔐 Signal Immutability Pattern

### Correct Updates

```typescript
// ✅ GOOD: Replace entire object (immutable)
this.dashboardState.set({
  data: dashboard,
  loading: false,
  error: null
});

// ❌ WRONG: Mutating field directly (bypasses reactivity)
const state = this.dashboardState();
state.data = dashboard;  // Change NOT tracked!
```

### Update Expression

```typescript
// For complex updates, use .update()
this.dashboardState.update(state => ({
  ...state,
  data: dashboard,
  loading: false
}));
```

---

## 🎓 Best Practices

### 1. Single Source of Truth

```typescript
// ✅ All state in one signal
readonly dashboardState = signal<QueryResult<DashboardSummary>>({...});

// ❌ Avoid duplicate state
readonly loading = signal(false);
readonly data = signal(null);  // Harder to keep in sync
```

### 2. Computed Signals for Derived State

```typescript
// ✅ Computed automatically when dashboardState changes
readonly isLoading = computed(() => this.dashboardState().loading);
readonly hasError = computed(() => this.dashboardState().error !== null);

// ❌ Manual updates prone to bugs
readonly loading = signal(false);
readonly error = signal(null);
// Must remember to update both!
```

### 3. Effects for Side-Effects

```typescript
// ✅ Side-effect tracked to specific signals
effect(() => {
  const error = this.summonerService.hasError();
  if (error) {
    this.toastr.error('Failed to load dashboard');
  }
});

// ❌ Manual subscriptions (memory leak risk)
this.summonerService.dashboardState$.subscribe(state => {
  if (state.error) this.toastr.error(state.error);
});
// Must remember to unsubscribe!
```

### 4. Clear API Boundaries

```typescript
// ✅ Public readonly Signals (components read them)
readonly isLoading = computed(...);
readonly dashboard = computed(...);

// ✅ Public methods (components call them)
async fetchDashboard(riotId: string): Promise<void>
clearDashboard(): void

// ✅ Private implementation details
private dashboardState = signal(...);
private handleApiError(...): string
```

---

## 🔄 Migration Path from RxJS

If refactoring existing RxJS code:

```typescript
// BEFORE: Observable-based
dashboard$ = this.http.get('/api/dashboard');

// AFTER: Signal-based
async fetchDashboard() {
  const dashboard = await firstValueFrom(
    this.http.get('/api/dashboard')
  );
  this.dashboardState.set({
    data: dashboard,
    loading: false,
    error: null
  });
}
```

---

## 📚 Further Reading

- [Angular Signals RFC](https://github.com/angular/angular/discussions/49090)
- [Signal Documentation](https://angular.io/guide/signals)
- [Performance Best Practices](https://angular.io/guide/performance-best-practices)

---

**Last Updated**: March 11, 2026  
**Version**: 1.0.0
