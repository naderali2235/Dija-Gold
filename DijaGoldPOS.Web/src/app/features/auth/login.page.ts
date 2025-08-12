import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, finalize } from 'rxjs/operators';

import { AuthService } from '@core/services/auth.service';
import { NotificationService } from '@core/services/notification.service';
import { environment } from '@environments/environment';

@Component({
  selector: 'app-login-page',
  templateUrl: './login.page.html',
  styleUrls: ['./login.page.scss']
})
export class LoginPageComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  loginForm: FormGroup;
  isLoading = false;
  hidePassword = true;
  returnUrl = '/dashboard';
  
  // Environment data for display
  appName = environment.appName;
  brandLogo = environment.brand?.logoPath;
  contactInfo = environment.brand?.contact;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private notificationService: NotificationService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.loginForm = this.createLoginForm();
  }

  ngOnInit(): void {
    // Get return URL from route parameters or default to dashboard
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
    
    // If already authenticated, redirect
    if (this.authService.isAuthenticated) {
      this.router.navigate([this.returnUrl]);
    }

    // Set default credentials for development
    if (!environment.production) {
      this.loginForm.patchValue({
        username: 'manager@dijagold.com',
        password: 'Manager123!'
      });
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createLoginForm(): FormGroup {
    return this.formBuilder.group({
      username: ['', [
        Validators.required,
        Validators.maxLength(100)
      ]],
      password: ['', [
        Validators.required,
        Validators.minLength(6),
        Validators.maxLength(100)
      ]],
      rememberMe: [false]
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.isLoading = true;
    const loginRequest = this.loginForm.value;

    this.authService.login(loginRequest)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.isLoading = false;
        })
      )
      .subscribe({
        next: (response) => {
          this.notificationService.showSuccess(
            `Welcome back, ${response.user.fullName}!`,
            'Login Successful'
          );
          
          // Navigate to return URL or dashboard
          this.router.navigate([this.returnUrl]);
        },
        error: (error) => {
          this.handleLoginError(error);
        }
      });
  }

  private handleLoginError(error: any): void {
    let errorMessage = 'Login failed. Please check your credentials.';
    
    if (error?.error?.message) {
      errorMessage = error.error.message;
    } else if (error?.message) {
      errorMessage = error.message;
    }

    this.notificationService.showError(errorMessage, 'Login Failed');
    
    // Clear password field on error
    this.loginForm.patchValue({ password: '' });
    this.loginForm.get('password')?.focus();
  }

  private markFormGroupTouched(): void {
    Object.keys(this.loginForm.controls).forEach(key => {
      const control = this.loginForm.get(key);
      control?.markAsTouched();
    });
  }

  // Helper methods for template
  isFieldInvalid(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.loginForm.get(fieldName);
    
    if (field?.errors) {
      if (field.errors['required']) {
        return `${this.getFieldDisplayName(fieldName)} is required`;
      }
      if (field.errors['minlength']) {
        return `${this.getFieldDisplayName(fieldName)} must be at least ${field.errors['minlength'].requiredLength} characters`;
      }
      if (field.errors['maxlength']) {
        return `${this.getFieldDisplayName(fieldName)} cannot exceed ${field.errors['maxlength'].requiredLength} characters`;
      }
    }
    
    return '';
  }

  private getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      username: 'Username',
      password: 'Password'
    };
    
    return displayNames[fieldName] || fieldName;
  }

  togglePasswordVisibility(): void {
    this.hidePassword = !this.hidePassword;
  }

  // Quick login methods for development/demo
  loginAsManager(): void {
    if (!environment.production) {
      this.loginForm.patchValue({
        username: 'manager@dijagold.com',
        password: 'Manager123!'
      });
      this.onSubmit();
    }
  }

  loginAsCashier(): void {
    if (!environment.production) {
      this.loginForm.patchValue({
        username: 'cashier@dijagold.com',
        password: 'Cashier123!'
      });
      this.onSubmit();
    }
  }
}