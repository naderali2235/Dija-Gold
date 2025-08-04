import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';

import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';
import { UserRole } from '../models/enums';

@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate, CanActivateChild {
  constructor(
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    return this.checkRole(route);
  }

  canActivateChild(
    childRoute: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    return this.checkRole(childRoute);
  }

  private checkRole(route: ActivatedRouteSnapshot): boolean {
    // Check if user is authenticated first
    if (!this.authService.isAuthenticated) {
      this.router.navigate(['/auth/login']);
      return false;
    }

    // Get required roles from route data
    const requiredRoles: UserRole[] = route.data['roles'] || [];
    
    // If no roles specified, allow access
    if (requiredRoles.length === 0) {
      return true;
    }

    // Check if user has any of the required roles
    const hasRequiredRole = this.authService.hasAnyRole(requiredRoles);
    
    if (!hasRequiredRole) {
      this.handleAccessDenied(requiredRoles);
      return false;
    }

    return true;
  }

  private handleAccessDenied(requiredRoles: UserRole[]): void {
    const roleNames = requiredRoles.join(' or ');
    
    this.notificationService.showError(
      `Access denied. This feature requires ${roleNames} role.`,
      'Access Denied'
    );
    
    // Redirect to appropriate page based on user role
    if (this.authService.isManager()) {
      this.router.navigate(['/dashboard']);
    } else if (this.authService.isCashier()) {
      this.router.navigate(['/pos']);
    } else {
      this.router.navigate(['/auth/login']);
    }
  }
}