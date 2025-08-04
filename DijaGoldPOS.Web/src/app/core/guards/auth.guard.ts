import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate, CanActivateChild {
  constructor(
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    return this.checkAuth(state.url);
  }

  canActivateChild(
    childRoute: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    return this.checkAuth(state.url);
  }

  private checkAuth(url: string): Observable<boolean> {
    if (this.authService.isAuthenticated) {
      return of(true);
    }

    // Try to refresh token if exists
    if (this.authService.currentToken) {
      return this.authService.refreshToken().pipe(
        map(() => true),
        catchError(() => {
          this.redirectToLogin(url);
          return of(false);
        })
      );
    }

    this.redirectToLogin(url);
    return of(false);
  }

  private redirectToLogin(returnUrl: string): void {
    this.notificationService.showWarning('Please log in to continue', 'Authentication Required');
    this.router.navigate(['/auth/login'], { queryParams: { returnUrl } });
  }
}