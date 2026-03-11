/**
 * @file Enhanced Telemetry & Error Handling Interceptor
 * @description Functional HttpInterceptor for handling API errors (429, 404) and adding telemetry headers.
 */

import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

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
 */
export const telemetryErrorInterceptor: HttpInterceptorFn = (req, next) => {
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
      logError(error, telemetry);
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
  return '1.0.0';
}

/**
 * Log HTTP error details for debugging.
 */
function logError(error: HttpErrorResponse, telemetry: TelemetryData): void {
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
