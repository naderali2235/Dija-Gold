import { Directive, Input, TemplateRef, ViewContainerRef, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AuthService } from '@core/services/auth.service';
import { UserRole } from '@core/models/enums';

@Directive({
  selector: '[appPermission]'
})
export class PermissionDirective implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private hasView = false;

  @Input() set appPermission(roles: UserRole | UserRole[] | string | string[]) {
    this.requiredRoles = Array.isArray(roles) ? roles : [roles];
    this.updateView();
  }

  @Input() appPermissionElse?: TemplateRef<any>;

  private requiredRoles: (UserRole | string)[] = [];

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Subscribe to auth state changes
    this.authService.authState$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.updateView();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateView(): void {
    const hasPermission = this.checkPermission();

    if (hasPermission && !this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    } else if (!hasPermission && this.hasView) {
      this.viewContainer.clear();
      this.hasView = false;
      
      // Show alternative template if provided
      if (this.appPermissionElse) {
        this.viewContainer.createEmbeddedView(this.appPermissionElse);
      }
    }
  }

  private checkPermission(): boolean {
    if (!this.authService.isAuthenticated) {
      return false;
    }

    if (this.requiredRoles.length === 0) {
      return true;
    }

    // Convert string roles to UserRole enum if needed
    const userRoles = this.requiredRoles.map(role => {
      if (typeof role === 'string') {
        return role as UserRole;
      }
      return role;
    });

    return this.authService.hasAnyRole(userRoles);
  }
}