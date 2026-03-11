import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { APP_SETTINGS, DEFAULT_SETTINGS } from './core/config/app-settings';
import { authInterceptor } from './core/http/auth.interceptor';
import { telemetryErrorInterceptor } from './core/http/telemetry-error.interceptor';

/**
 * Root-level app providers (router, HTTP, settings).
 * 
 * HTTP Interceptor Chain:
 * 1. telemetryErrorInterceptor - Adds telemetry headers & handles errors
 * 2. authInterceptor - Attaches JWT to requests
 */
export const appConfig: ApplicationConfig = {
  providers: [
    // Capture global errors for debugging.
    provideBrowserGlobalErrorListeners(),
    // Client-side routing.
    provideRouter(routes),
    // HTTP client + interceptors (order matters!)
    provideHttpClient(
      withInterceptors([
        telemetryErrorInterceptor,  // Telemetry & error handling (first)
        authInterceptor              // JWT attachment (second)
      ])
    ),
    // App configuration (API base URL, etc.).
    { provide: APP_SETTINGS, useValue: DEFAULT_SETTINGS }
  ]
};
