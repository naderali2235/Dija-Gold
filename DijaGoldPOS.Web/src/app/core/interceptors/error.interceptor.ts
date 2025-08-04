import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { NotificationService } from '../services/notification.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private notificationService: NotificationService) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        // Don't show notifications for certain endpoints (like auth refresh)
        const skipNotification = this.shouldSkipNotification(request.url, error.status);
        
        if (!skipNotification) {
          this.handleError(error);
        }

        return throwError(() => error);
      })
    );
  }

  private handleError(error: HttpErrorResponse): void {
    let message = 'An unexpected error occurred';
    let title = 'Error';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      message = error.error.message;
      title = 'Network Error';
    } else {
      // Server-side error
      if (error.error && error.error.message) {
        message = error.error.message;
      } else {
        switch (error.status) {
          case 0:
            title = 'Connection Error';
            message = 'Unable to connect to server. Please check your internet connection.';
            break;
          case 400:
            title = 'Invalid Request';
            message = 'The request contains invalid data. Please check your input.';
            break;
          case 401:
            title = 'Authentication Required';
            message = 'Please log in to continue.';
            break;
          case 403:
            title = 'Access Denied';
            message = 'You do not have permission to perform this action.';
            break;
          case 404:
            title = 'Not Found';
            message = 'The requested resource was not found.';
            break;
          case 409:
            title = 'Conflict';
            message = 'The request conflicts with the current state of the resource.';
            break;
          case 422:
            title = 'Validation Error';
            message = 'The request contains validation errors.';
            break;
          case 429:
            title = 'Too Many Requests';
            message = 'Too many requests. Please try again later.';
            break;
          case 500:
            title = 'Server Error';
            message = 'Internal server error. Please try again later.';
            break;
          case 502:
            title = 'Bad Gateway';
            message = 'Server is temporarily unavailable. Please try again later.';
            break;
          case 503:
            title = 'Service Unavailable';
            message = 'Service is temporarily unavailable. Please try again later.';
            break;
          case 504:
            title = 'Gateway Timeout';
            message = 'Request timeout. Please try again.';
            break;
          default:
            title = `Error ${error.status}`;
            message = error.message || `Server returned error code ${error.status}`;
        }
      }
    }

    // Show error notification
    this.notificationService.showError(message, title);

    // Log error for debugging
    console.error('HTTP Error:', {
      status: error.status,
      message: error.message,
      url: error.url,
      error: error.error
    });
  }

  private shouldSkipNotification(url: string, status: number): boolean {
    // Skip notifications for auth refresh failures (handled by auth interceptor)
    if (url.includes('/auth/refresh-token') && status === 401) {
      return true;
    }

    // Skip notifications for auth login failures (handled by login component)
    if (url.includes('/auth/login') && status === 401) {
      return true;
    }

    // Skip notifications for validation errors on forms (handled by components)
    if (status === 400 || status === 422) {
      return true;
    }

    return false;
  }
}