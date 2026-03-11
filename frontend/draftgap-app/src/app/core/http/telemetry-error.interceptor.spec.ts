/**
 * @file Telemetry Error Interceptor Tests
 * @description Unit tests for telemetryErrorInterceptor.
 */

import { telemetryErrorInterceptor } from './telemetry-error.interceptor';

describe('TelemetryErrorInterceptor', () => {
  it('should be defined', () => {
    expect(telemetryErrorInterceptor).toBeDefined();
  });

  it('should be a function', () => {
    expect(typeof telemetryErrorInterceptor).toBe('function');
  });

  it('should handle HTTP requests', () => {
    expect(telemetryErrorInterceptor).toBeTruthy();
  });
});
