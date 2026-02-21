import { InjectionToken } from '@angular/core';

// Centralized app settings to keep configuration consistent across services.
export interface AppSettings {
  apiBaseUrl: string;
}

// Injection token for app-level configuration.
export const APP_SETTINGS = new InjectionToken<AppSettings>('APP_SETTINGS');

// Default settings (override in future via environment providers).
export const DEFAULT_SETTINGS: AppSettings = {
  apiBaseUrl: 'http://localhost:5057/api'
};
