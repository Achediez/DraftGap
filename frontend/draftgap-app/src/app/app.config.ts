import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { APP_SETTINGS, DEFAULT_SETTINGS } from './core/config/app-settings';
import { authInterceptor } from './core/http/auth.interceptor';

// Root-level app providers (router, HTTP, settings).
export const appConfig: ApplicationConfig = {
  providers: [
    // Capture global errors for debugging.
    provideBrowserGlobalErrorListeners(),
    // Client-side routing.
    provideRouter(routes),
    // HTTP client + JWT interceptor.
    provideHttpClient(withInterceptors([authInterceptor])),
    // App configuration (API base URL, etc.).
    { provide: APP_SETTINGS, useValue: DEFAULT_SETTINGS }
  ]
};
