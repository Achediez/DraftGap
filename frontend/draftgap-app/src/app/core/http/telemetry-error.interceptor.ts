/**
 * @file Enhanced Telemetry & Error Handling Interceptor
 * @description Functional HttpInterceptor for handling API errors (429, 503) and adding telemetry headers.
 * Integrates with NotificationService for user-facing alerts.
 */

import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { NotificationService } from '../services';

/**
 * Telemetry data captured for each request.
 */
interface TelemetryData {
  timestamp: number;
  url: string;
  method: string;
  userAgent: string;
  appVersion: string;
}

/**
 * Functional interceptor for telemetry and error handling.
 * 
 * Features:
 * - Adds telemetry headers (X-Request-Timestamp, X-Client-Version, X-Client-Name)
 * - Handles rate limiting (429) with notification
 * - Handles service unavailable (503) with notification
 * - Logs all errors by severity level (warn/error)
 * - Preserves error for service consumption
 */
export const telemetryErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const notificationService = inject(NotificationService);
  const telemetry = createTelemetryData(req.url, req.method);

  const requestWithTelemetry = req.clone({
    setHeaders: {
      'X-Request-Timestamp': telemetry.timestamp.toString(),
      'X-Client-Version': telemetry.appVersion,
      'X-Client-Name': 'DraftGap-Frontend'
    }
  });

  return next(requestWithTelemetry).pipe(
    catchError(error => {
      logError(error, telemetry, notificationService);
      return throwError(() => error);
    })
  );
};

/**
 * Create telemetry data for a request.
 */
function createTelemetryData(url: string, method: string): TelemetryData {
  return {
    timestamp: Date.now(),
    url,
    method,
    userAgent: navigator.userAgent,
    appVersion: getAppVersion()
  };
}

/**
 * Get application version.
 */
function getAppVersion(): string {
  return '1.0.0'; // Update this with dynamic version if available
}

/**
 * Log HTTP error details for debugging and show notifications.
 * Critical errors (429, 503) trigger user notifications via NotificationService.
 */
function logError(
  error: HttpErrorResponse,
  telemetry: TelemetryData,
  notificationService: NotificationService
): void {
  const errorLog = {
    timestamp: new Date(telemetry.timestamp).toISOString(),
    status: error.status,
    statusText: error.statusText,
    url: telemetry.url,
    method: telemetry.method,
    message: error.message,
    errorBody: error.error
  };

  if (error.status === 429) {
    console.warn('[TelemetryInterceptor] Rate limit exceeded (429)', errorLog);
    const retryAfter = error.headers.get('Retry-After');
    notificationService.showRateLimit(
      retryAfter ? parseInt(retryAfter, 10) : undefined
    );
  } else if (error.status === 503) {
    console.error('[TelemetryInterceptor] Service unavailable (503)', errorLog);
    notificationService.showServiceUnavailable();
  } else if (error.status === 404) {
    console.warn('[TelemetryInterceptor] Resource not found (404)', errorLog);
  } else if (error.status >= 400 && error.status < 500) {
    console.warn('[TelemetryInterceptor] Client error', errorLog);
  } else if (error.status >= 500) {
    console.error('[TelemetryInterceptor] Server error', errorLog);
  } else if (error.status === 0) {
    console.error('[TelemetryInterceptor] Network error', errorLog);
  } else {
    console.error('[TelemetryInterceptor] Unknown error', errorLog);
  }
}

