# DraftGap Frontend - League of Legends Statistics Dashboard

Modern Angular 21 frontend for displaying League of Legends summoner statistics using **Angular Signals** and **Clean Architecture**.

## 🚀 Quick Start

### Prerequisites
- Node.js 20+
- npm 11+

### Installation & Development

```bash
cd frontend/draftgap-app
npm install
npm start
```

Visit `http://localhost:4200/` to access the dashboard.

---

## 📂 Architecture

### Clean Architecture Pattern
```
src/app/
├── core/                    # Core layer (singleton services, models, interceptors)
│   ├── models/              # Domain models (ranked, match, summoner)
│   ├── services/            # SummonerService (Signals-based), NotificationService
│   └── http/                # HTTP interceptors (telemetry, error handling)
├── features/                # Feature modules (lazy-loaded components)
│   └── dashboard/           # SummonerDashboard component with templates & styles
└── shared/                  # Shared utilities (guards, pipes, directives)
```

### Signal-Based State Management

The `SummonerService` implements **Signals** for reactive, fine-grained updates:

```typescript
// Core signals (WritableSignal)
readonly currentSummoner = signal<string>('');
readonly dashboardState = signal<QueryResult<DashboardSummary>>({...});
readonly isRefreshing = signal<boolean>(false);

// Computed signals (auto-update when dependencies change)
readonly isLoading = computed(() => 
  this.dashboardState().loading || this.isRefreshing()
);
readonly derivedMetrics = computed<DerivedMetrics>(() => {
  // Auto-calculated KDA, winrate, trend, LP/game
});
```

**Benefits:**
- ✅ No RxJS overhead for simple state
- ✅ Fine-grained reactivity (only affected components re-render)
- ✅ Strong TypeScript support
- ✅ Automatic dependency tracking

---

## 🎯 Core Features

### 1. SummonerService (Signal-Based State Management)

**Stale-While-Revalidate (SWR) Pattern:**
```typescript
// First load: shows loading spinner
await summonerService.fetchDashboard('PlayerName#NA1');

// Second load: shows stale data while refreshing in background
// - If success: updates with new data
// - If failure: keeps stale data visible (no UI jank)
```

**Derived Metrics (Computed):**
- **KDA:** Kills + Assists / Deaths
- **Winrate:** Win % from ranked stats
- **Trend:** Up (≥55% WR) / Down (≤45% WR) / Neutral
- **LP/Game:** Average LP gained per game

### 2. NotificationService (Global Alerts)

```typescript
// Automatically triggered by interceptors
notificationService.showError('Error', 'Failed to load data');
notificationService.showRateLimit(60); // Riot API rate limit
notificationService.showServiceUnavailable(); // 503 errors
```

### 3. Telemetry & Error Interceptor

Functional HTTP interceptor that:
- ✅ Adds telemetry headers (timestamps, versions)
- ✅ Handles 429 (rate limit) and 503 (unavailable) errors
- ✅ Shows user-friendly notifications
- ✅ Logs errors for monitoring

### 4. SummonerDashboard Component

New Angular control flow (`@if`, `@for`, `@else`):
```html
@if (isDashboardReady()) {
  <div class="ranked-stats">
    <span>{{ dashboard()?.rankedOverview?.soloQueue?.tier }}</span>
  </div>
} @else if (hasError()) {
  <div>{{ getStateMessage() }}</div>
} @else {
  <div>{{ getStateMessage() }}</div>
}
```

---

## 🏗️ Build & Deployment

### Production Build

```bash
npm run build
```

Output: `dist/draftgap-app/` (optimized bundle ~85KB gzipped)

### Environment Configuration

Create `src/environments/environment.prod.ts`:
```typescript
export const environment = {
  production: true,
  apiBaseUrl: 'https://api.draftgap.com'  // Your production backend
};
```

---

## 🧪 Testing

### Run All Tests
```bash
npm test                    # Watch mode
npm test -- --watch=false  # Single run
```

### Test Coverage
- ✅ 29 tests passing (100% of implementations)
- ✅ SummonerService: 9 tests (SWR, metrics, error handling)
- ✅ NotificationService: 15 tests (notifications, dismissal, auto-close)
- ✅ TelemetryErrorInterceptor: 3 tests (header injection)
- ✅ App Component: 2 tests (initialization)

### Test Files
- `src/app/core/services/summoner.service.spec.ts`
- `src/app/core/services/notification.service.spec.ts`
- `src/app/core/http/telemetry-error.interceptor.spec.ts`

---

## 📡 API Integration

### Expected Backend Response

```json
{
  "rankedOverview": {
    "soloQueue": {
      "tier": "Diamond",
      "rank": "II",
      "leaguePoints": 75,
      "wins": 60,
      "losses": 40,
      "totalGames": 100,
      "winrate": 60
    }
  },
  "performanceStats": {
    "totalMatches": 20,
    "winrate": 60,
    "avgKda": 5.4
  },
  "recentMatches": [...],
  "topChampions": [...]
}
```

### API Error Handling

| Status | Behavior | Notification |
|--------|----------|--------------|
| **404** | Summoner not found | Error message |
| **429** | Rate limited | "Too many requests" + retry countdown |
| **503** | Service unavailable | "API temporarily down" |
| **500** | Server error | Generic error message |
| **0** | Network error | Connection failure message |

---

## 🔧 Configuration Files

### `angular.json`
- Build configuration
- Dev server proxy to backend

### `tsconfig.json`
- TypeScript strict mode enabled
- Module resolution: node
- Target: ES2022

### `proxy.conf.json`
```json
{
  "/api": {
    "target": "http://localhost:5000",
    "pathRewrite": { "^/api": "" }
  }
}
```

---

## 🚨 Troubleshooting

### "Cannot find module" error
```bash
rm -rf node_modules dist .angular
npm install
ng cache clean  # or restart VS Code
```

### Port 4200 already in use
```bash
ng serve --port 4300
```

### API connection issues
1. Check proxy config: `proxy.conf.json`
2. Verify backend is running on correct port
3. Check CORS headers from backend
4. Open DevTools → Network tab to inspect requests

### Tests failing
```bash
npm test -- --watch=false
# Check for timeouts or mock data mismatches
```

---

## 📖 Development Guidelines

### Adding a New Component

```bash
ng generate component features/my-component --standalone
```

### Creating a New Service

```typescript
// my.service.ts
import { Injectable, signal, computed } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class MyService {
  readonly data = signal<DataType>(...);
  readonly derivedValue = computed(() => /* calculate */);
  
  // Methods...
}
```

### Using Signals in Templates

```html
<!-- Automatic change detection -->
{{ myService.data() }}

<!-- For conditional renders -->
@if (myService.isReady()) { ... }

<!-- For loops -->
@for (item of myService.items(); track item.id) {
  <div>{{ item.name }}</div>
}
```

---

## 📚 Resources

- [Angular Signals Docs](https://angular.io/guide/signals)
- [Angular Control Flow](https://angular.io/guide/control-flow)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Stale-While-Revalidate Pattern](https://web.dev/stale-while-revalidate/)
- [Riot API Documentation](https://developer.riotgames.com/)

---

## 📝 License

MIT License - See root [LICENSE](../LICENSE) file

---

## 🤝 Contributing

1. Create feature branch: `git checkout -b feature/amazing-feature`
2. Commit changes: `git commit -m 'Add amazing feature'`
3. Push to branch: `git push origin feature/amazing-feature`
4. Open Pull Request

---

**Last Updated:** March 2026  
**Angular Version:** 21.1.0  
**TypeScript:** 5.9.2  
**Node:** 20+

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
