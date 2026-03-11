/**
 * @file Telemetry Error Interceptor Tests
 * @description Unit tests for telemetryErrorInterceptor.
 */

import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HttpClient } from '@angular/common/http';
import { telemetryErrorInterceptor } from './telemetry-error.interceptor';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

describe('TelemetryErrorInterceptor', () => {
  let httpClient: HttpClient;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(
          withInterceptors([telemetryErrorInterceptor])
        )
      ]
    });

    httpClient = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should add telemetry headers to requests', () => {
    const testUrl = '/api/test';

    httpClient.get(testUrl).subscribe();

    const req = httpMock.expectOne(testUrl);

    expect(req.request.headers.has('X-Request-Timestamp')).toBe(true);
    expect(req.request.headers.has('X-Client-Version')).toBe(true);
    expect(req.request.headers.has('X-Client-Name')).toBe(true);
    expect(req.request.headers.get('X-Client-Name')).toBe('DraftGap-Frontend');

    req.flush({});
  });

  it('should pass through successful responses', () => {
    const testUrl = '/api/test';
    const mockData = { id: 1, name: 'Test' };

    httpClient.get(testUrl).subscribe(response => {
      expect(response).toEqual(mockData);
    });

    const req = httpMock.expectOne(testUrl);
    req.flush(mockData);
  });

  it('should handle 404 error responses', () => {
    const testUrl = '/api/nonexistent';
    let errorOccurred = false;

    httpClient.get(testUrl).subscribe({
      error: (error) => {
        expect(error.status).toBe(404);
        errorOccurred = true;
      }
    });

    const req = httpMock.expectOne(testUrl);
    req.flush(null, { status: 404, statusText: 'Not Found' });

    expect(errorOccurred).toBe(true);
  });

  it('should handle 429 rate limit error', () => {
    const testUrl = '/api/test';
    let errorOccurred = false;

    httpClient.get(testUrl).subscribe({
      error: (error) => {
        expect(error.status).toBe(429);
        errorOccurred = true;
      }
    });

    const req = httpMock.expectOne(testUrl);
    req.flush(null, { status: 429, statusText: 'Too Many Requests' });

    expect(errorOccurred).toBe(true);
  });

  it('should handle 500 server error', () => {
    const testUrl = '/api/test';
    let errorOccurred = false;

    httpClient.get(testUrl).subscribe({
      error: (error) => {
        expect(error.status).toBe(500);
        errorOccurred = true;
      }
    });

    const req = httpMock.expectOne(testUrl);
    req.flush(null, { status: 500, statusText: 'Internal Server Error' });

    expect(errorOccurred).toBe(true);
  });

  it('should work with POST requests', () => {
    const testUrl = '/api/test';
    const body = { data: 'test' };

    httpClient.post(testUrl, body).subscribe();

    const req = httpMock.expectOne(testUrl);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should work with PUT requests', () => {
    const testUrl = '/api/test/1';
    const body = { data: 'updated' };

    httpClient.put(testUrl, body).subscribe();

    const req = httpMock.expectOne(testUrl);
    expect(req.request.method).toBe('PUT');
    req.flush({});
  });

  it('should work with DELETE requests', () => {
    const testUrl = '/api/test/1';

    httpClient.delete(testUrl).subscribe();

    const req = httpMock.expectOne(testUrl);
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });
});
