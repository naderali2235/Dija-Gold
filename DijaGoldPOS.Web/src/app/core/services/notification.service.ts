import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface Notification {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title?: string;
  message: string;
  duration?: number;
  persistent?: boolean;
  actions?: NotificationAction[];
}

export interface NotificationAction {
  label: string;
  action: () => void;
  style?: 'primary' | 'secondary' | 'danger';
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();

  private defaultDuration = 5000; // 5 seconds

  constructor() {}

  showSuccess(message: string, title?: string, duration?: number): string {
    return this.show({
      type: 'success',
      title: title || 'Success',
      message,
      duration: duration || this.defaultDuration
    });
  }

  showError(message: string, title?: string, persistent: boolean = false): string {
    return this.show({
      type: 'error',
      title: title || 'Error',
      message,
      duration: persistent ? undefined : this.defaultDuration * 2, // Longer for errors
      persistent
    });
  }

  showWarning(message: string, title?: string, duration?: number): string {
    return this.show({
      type: 'warning',
      title: title || 'Warning',
      message,
      duration: duration || this.defaultDuration
    });
  }

  showInfo(message: string, title?: string, duration?: number): string {
    return this.show({
      type: 'info',
      title: title || 'Information',
      message,
      duration: duration || this.defaultDuration
    });
  }

  showCustom(notification: Partial<Notification>): string {
    const fullNotification: Notification = {
      id: this.generateId(),
      type: 'info',
      message: '',
      duration: this.defaultDuration,
      ...notification
    };

    return this.show(fullNotification);
  }

  // Show confirmation dialog
  showConfirmation(
    message: string,
    title: string = 'Confirm Action',
    confirmLabel: string = 'Confirm',
    cancelLabel: string = 'Cancel'
  ): Promise<boolean> {
    return new Promise((resolve) => {
      const id = this.show({
        type: 'warning',
        title,
        message,
        persistent: true,
        actions: [
          {
            label: cancelLabel,
            action: () => {
              this.dismiss(id);
              resolve(false);
            },
            style: 'secondary'
          },
          {
            label: confirmLabel,
            action: () => {
              this.dismiss(id);
              resolve(true);
            },
            style: 'primary'
          }
        ]
      });
    });
  }

  // Show action notification
  showAction(
    message: string,
    actionLabel: string,
    actionCallback: () => void,
    title?: string,
    type: Notification['type'] = 'info'
  ): string {
    return this.show({
      type,
      title,
      message,
      persistent: true,
      actions: [
        {
          label: 'Dismiss',
          action: () => {
            // Will be handled by dismiss method
          },
          style: 'secondary'
        },
        {
          label: actionLabel,
          action: actionCallback,
          style: 'primary'
        }
      ]
    });
  }

  dismiss(id: string): void {
    const currentNotifications = this.notificationsSubject.value;
    const updatedNotifications = currentNotifications.filter(n => n.id !== id);
    this.notificationsSubject.next(updatedNotifications);
  }

  dismissAll(): void {
    this.notificationsSubject.next([]);
  }

  // Dismiss all notifications of a specific type
  dismissByType(type: Notification['type']): void {
    const currentNotifications = this.notificationsSubject.value;
    const updatedNotifications = currentNotifications.filter(n => n.type !== type);
    this.notificationsSubject.next(updatedNotifications);
  }

  private show(notification: Notification): string {
    const id = notification.id || this.generateId();
    const fullNotification: Notification = { ...notification, id };

    const currentNotifications = this.notificationsSubject.value;
    this.notificationsSubject.next([...currentNotifications, fullNotification]);

    // Auto-dismiss if duration is set
    if (fullNotification.duration && !fullNotification.persistent) {
      setTimeout(() => {
        this.dismiss(id);
      }, fullNotification.duration);
    }

    return id;
  }

  private generateId(): string {
    return `notification_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  // Helper methods for common scenarios
  showSaveSuccess(itemName: string = 'Item'): string {
    return this.showSuccess(`${itemName} saved successfully`);
  }

  showDeleteSuccess(itemName: string = 'Item'): string {
    return this.showSuccess(`${itemName} deleted successfully`);
  }

  showUpdateSuccess(itemName: string = 'Item'): string {
    return this.showSuccess(`${itemName} updated successfully`);
  }

  showValidationError(message: string = 'Please correct the errors and try again'): string {
    return this.showError(message, 'Validation Error');
  }

  showNetworkError(): string {
    return this.showError(
      'Unable to connect to server. Please check your internet connection.',
      'Network Error',
      true
    );
  }

  showPermissionError(): string {
    return this.showError(
      'You do not have permission to perform this action.',
      'Access Denied'
    );
  }

  // Business-specific notifications
  showTransactionSuccess(transactionNumber: string, amount: number): string {
    return this.showSuccess(
      `Transaction ${transactionNumber} completed successfully. Amount: ${amount.toFixed(2)} EGP`,
      'Transaction Completed'
    );
  }

  showInventoryWarning(productName: string, currentStock: number): string {
    return this.showWarning(
      `Low stock alert: ${productName} has only ${currentStock} units remaining`,
      'Low Stock Alert'
    );
  }

  showPriceUpdateNotification(message: string): string {
    return this.showInfo(message, 'Price Update');
  }
}