/**
 * @file NotificationService Unit Tests
 * @description Tests for notification service functionality including display, auto-dismiss, and types.
 */

import { TestBed } from '@angular/core/testing';
import { NotificationService, Notification } from './notification.service';

describe('NotificationService', () => {
  let service: NotificationService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [NotificationService]
    });
    service = TestBed.inject(NotificationService);
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
    TestBed.resetTestingModule();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('showSuccess', () => {
    it('should add success notification to list', () => {
      service.showSuccess('Success', 'Operation completed');
      expect(service.notifications().length).toBe(1);
      expect(service.notifications()[0].type).toBe('success');
    });

    it('should auto-dismiss after duration', () => {
      service.showSuccess('Success', 'Message', { duration: 1000 });
      expect(service.notifications().length).toBe(1);
      vi.advanceTimersByTime(1000);
      expect(service.notifications().length).toBe(0);
    });
  });

  describe('showError', () => {
    it('should add error notification with longer duration', () => {
      service.showError('Error', 'Something failed');
      const notif = service.notifications()[0];
      expect(notif.type).toBe('error');
      expect(notif.duration).toBe(5000);
    });
  });

  describe('showWarning', () => {
    it('should add warning notification', () => {
      service.showWarning('Warning', 'Be careful');
      const notif = service.notifications()[0];
      expect(notif.type).toBe('warning');
      expect(notif.duration).toBe(4000);
    });
  });

  describe('showInfo', () => {
    it('should add info notification', () => {
      service.showInfo('Info', 'For your information');
      const notif = service.notifications()[0];
      expect(notif.type).toBe('info');
    });
  });

  describe('showRateLimit', () => {
    it('should show rate limit warning with default 60 seconds', () => {
      service.showRateLimit();
      expect(service.notifications()[0].type).toBe('warning');
      expect(service.notifications()[0].message).toContain('60');
    });

    it('should use custom retry-after seconds if provided', () => {
      service.showRateLimit(30);
      expect(service.notifications()[0].message).toContain('30');
    });

    it('should be non-dismissible and persistent', () => {
      service.showRateLimit();
      const notif = service.notifications()[0];
      expect(notif.dismissible).toBe(false);
      expect(notif.duration).toBe(0);
    });
  });

  describe('showServiceUnavailable', () => {
    it('should show service unavailable error', () => {
      service.showServiceUnavailable();
      const notif = service.notifications()[0];
      expect(notif.type).toBe('error');
      expect(notif.message).toContain('temporarily unavailable');
      expect(notif.dismissible).toBe(false);
    });
  });

  describe('dismiss', () => {
    it('should remove notification by id', () => {
      service.showSuccess('Test', 'Message', { duration: 0 });
      const notifId = service.notifications()[0].id;

      service.dismiss(notifId);
      expect(service.notifications().length).toBe(0);
    });

    it('should not dismiss other notifications', () => {
      service.showSuccess('Test 1', 'Message 1', { duration: 0 });
      service.showError('Test 2', 'Message 2', { duration: 0 });

      const firstId = service.notifications()[0].id;
      service.dismiss(firstId);

      expect(service.notifications().length).toBe(1);
      expect(service.notifications()[0].type).toBe('error');
    });
  });

  describe('dismissAll', () => {
    it('should clear all notifications', () => {
      service.showSuccess('Test 1', 'Message 1', { duration: 0 });
      service.showError('Test 2', 'Message 2', { duration: 0 });
      service.showWarning('Test 3', 'Message 3', { duration: 0 });

      expect(service.notifications().length).toBe(3);

      service.dismissAll();
      expect(service.notifications().length).toBe(0);
    });
  });

  describe('notification persistence', () => {
    it('should persist notifications when duration is 0', () => {
      service.showError('Error', 'Persistent', { duration: 0 });
      vi.advanceTimersByTime(10000);
      expect(service.notifications().length).toBe(1);
    });

    it('should generate unique ids for each notification', () => {
      service.showSuccess('Test 1', 'Message 1', { duration: 0 });
      service.showSuccess('Test 2', 'Message 2', { duration: 0 });

      const ids = service.notifications().map(n => n.id);
      expect(new Set(ids).size).toBe(2);
    });
  });
});
