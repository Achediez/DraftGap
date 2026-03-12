/**
 * @file Notification Service
 * @description Global notification system for displaying alerts, warnings, and errors to users.
 * Used by interceptors and services to communicate status changes.
 */

import { Injectable, signal } from '@angular/core';

export type NotificationType = 'success' | 'error' | 'warning' | 'info';

export interface Notification {
  id: string;
  type: NotificationType;
  title: string;
  message: string;
  duration?: number; // milliseconds, 0 = persistent
  dismissible?: boolean;
}

/**
 * Service for managing application-wide notifications.
 * 
 * @example
 * constructor(private notificationService: NotificationService) {}
 * 
 * this.notificationService.showError(
 *   'Rate Limited',
 *   'Riot API rate limit exceeded. Retrying in 1 minute...',
 *   { duration: 5000 }
 * );
 */
@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationCounter = 0;

  /** Signal containing active notifications */
  readonly notifications = signal<Notification[]>([]);

  /**
   * Show success notification.
   */
  showSuccess(title: string, message: string, options?: Partial<Notification>): void {
    this.add({
      type: 'success',
      title,
      message,
      duration: 3000,
      dismissible: true,
      ...options
    });
  }

  /**
   * Show error notification.
   */
  showError(title: string, message: string, options?: Partial<Notification>): void {
    this.add({
      type: 'error',
      title,
      message,
      duration: 5000,
      dismissible: true,
      ...options
    });
  }

  /**
   * Show warning notification.
   */
  showWarning(title: string, message: string, options?: Partial<Notification>): void {
    this.add({
      type: 'warning',
      title,
      message,
      duration: 4000,
      dismissible: true,
      ...options
    });
  }

  /**
   * Show info notification.
   */
  showInfo(title: string, message: string, options?: Partial<Notification>): void {
    this.add({
      type: 'info',
      title,
      message,
      duration: 3000,
      dismissible: true,
      ...options
    });
  }

  /**
   * Show rate limit notification (429 error).
   */
  showRateLimit(retryAfterSeconds?: number): void {
    const seconds = retryAfterSeconds || 60;
    this.showWarning(
      'Rate Limited',
      `Too many requests. Please wait ${seconds} seconds before trying again.`,
      { duration: 0, dismissible: false }
    );
  }

  /**
   * Show service unavailable notification (503 error).
   */
  showServiceUnavailable(): void {
    this.showError(
      'Service Unavailable',
      'The Riot API is temporarily unavailable. Please try again later.',
      { duration: 0, dismissible: false }
    );
  }

  /**
   * Dismiss specific notification by ID.
   */
  dismiss(id: string): void {
    this.notifications.update(
      notifs => notifs.filter(n => n.id !== id)
    );
  }

  /**
   * Dismiss all notifications.
   */
  dismissAll(): void {
    this.notifications.set([]);
  }

  /**
   * Internal method to add notification and setup auto-dismiss.
   */
  private add(notification: Omit<Notification, 'id'>): void {
    const id = `notif-${++this.notificationCounter}-${Date.now()}`;
    const fullNotification: Notification = { ...notification, id };

    this.notifications.update(notifs => [...notifs, fullNotification]);

    // Auto-dismiss if duration is set
    if (notification.duration && notification.duration > 0) {
      setTimeout(() => this.dismiss(id), notification.duration);
    }
  }
}
