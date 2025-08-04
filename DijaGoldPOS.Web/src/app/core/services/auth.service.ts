import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { Router } from '@angular/router';

import { environment } from '@environments/environment';
import { 
  LoginRequest, 
  LoginResponse, 
  UserInfo, 
  ChangePasswordRequest, 
  ApiResponse,
  AuthState
} from '../models/auth.models';
import { UserRole } from '../models/enums';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = environment.apiUrl;
  private readonly storageKey = environment.settings.storage.authToken;
  private readonly userInfoKey = environment.settings.storage.userInfo;

  private authStateSubject = new BehaviorSubject<AuthState>({
    isAuthenticated: false,
    user: null,
    token: null,
    loading: false,
    error: null
  });

  public authState$ = this.authStateSubject.asObservable();
  
  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.initializeAuth();
  }

  private initializeAuth(): void {
    const token = this.getStoredToken();
    const userInfo = this.getStoredUserInfo();
    
    if (token && userInfo && !this.isTokenExpired(token)) {
      this.authStateSubject.next({
        isAuthenticated: true,
        user: userInfo,
        token: token,
        loading: false,
        error: null
      });
    } else {
      this.clearStoredAuth();
    }
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    this.setLoading(true);
    
    return this.http.post<ApiResponse<LoginResponse>>(`${this.apiUrl}/auth/login`, credentials)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Login failed');
          }
          return response.data;
        }),
        tap(loginResponse => {
          this.handleLoginSuccess(loginResponse);
        }),
        catchError(error => {
          this.handleAuthError('Login failed', error);
          return throwError(() => error);
        })
      );
  }

  logout(): Observable<any> {
    return this.http.post<ApiResponse>(`${this.apiUrl}/auth/logout`, {})
      .pipe(
        tap(() => {
          this.handleLogout();
        }),
        catchError(error => {
          // Even if logout fails on server, clear local auth
          this.handleLogout();
          return throwError(() => error);
        })
      );
  }

  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.http.post<ApiResponse>(`${this.apiUrl}/auth/change-password`, request)
      .pipe(
        map(response => {
          if (!response.success) {
            throw new Error(response.message || 'Password change failed');
          }
        }),
        catchError(error => {
          return throwError(() => error);
        })
      );
  }

  refreshToken(): Observable<LoginResponse> {
    return this.http.post<ApiResponse<LoginResponse>>(`${this.apiUrl}/auth/refresh-token`, {})
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error('Token refresh failed');
          }
          return response.data;
        }),
        tap(loginResponse => {
          this.handleLoginSuccess(loginResponse);
        }),
        catchError(error => {
          this.handleLogout();
          return throwError(() => error);
        })
      );
  }

  getCurrentUser(): Observable<UserInfo> {
    return this.http.get<ApiResponse<UserInfo>>(`${this.apiUrl}/auth/me`)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error('Failed to get user info');
          }
          return response.data;
        }),
        tap(userInfo => {
          this.updateUserInfo(userInfo);
        }),
        catchError(error => {
          return throwError(() => error);
        })
      );
  }

  // Getters for current auth state
  get isAuthenticated(): boolean {
    return this.authStateSubject.value.isAuthenticated;
  }

  get currentUser(): UserInfo | null {
    return this.authStateSubject.value.user;
  }

  get currentToken(): string | null {
    return this.authStateSubject.value.token;
  }

  get isLoading(): boolean {
    return this.authStateSubject.value.loading;
  }

  // Role checking methods
  hasRole(role: UserRole): boolean {
    const user = this.currentUser;
    return user ? user.roles.includes(role) : false;
  }

  hasAnyRole(roles: UserRole[]): boolean {
    return roles.some(role => this.hasRole(role));
  }

  isManager(): boolean {
    return this.hasRole(UserRole.Manager);
  }

  isCashier(): boolean {
    return this.hasRole(UserRole.Cashier);
  }

  // Branch access
  get currentBranchId(): number | null {
    return this.currentUser?.branch?.id || null;
  }

  get isHeadquartersUser(): boolean {
    return this.currentUser?.branch?.isHeadquarters || false;
  }

  private handleLoginSuccess(loginResponse: LoginResponse): void {
    this.storeToken(loginResponse.token);
    this.storeUserInfo(loginResponse.user);
    
    this.authStateSubject.next({
      isAuthenticated: true,
      user: loginResponse.user,
      token: loginResponse.token,
      loading: false,
      error: null
    });
  }

  private handleLogout(): void {
    this.clearStoredAuth();
    
    this.authStateSubject.next({
      isAuthenticated: false,
      user: null,
      token: null,
      loading: false,
      error: null
    });
    
    this.router.navigate(['/auth/login']);
  }

  private handleAuthError(message: string, error: any): void {
    this.authStateSubject.next({
      ...this.authStateSubject.value,
      loading: false,
      error: message
    });
  }

  private setLoading(loading: boolean): void {
    this.authStateSubject.next({
      ...this.authStateSubject.value,
      loading
    });
  }

  private updateUserInfo(userInfo: UserInfo): void {
    this.storeUserInfo(userInfo);
    this.authStateSubject.next({
      ...this.authStateSubject.value,
      user: userInfo
    });
  }

  private storeToken(token: string): void {
    localStorage.setItem(this.storageKey, token);
  }

  private storeUserInfo(userInfo: UserInfo): void {
    localStorage.setItem(this.userInfoKey, JSON.stringify(userInfo));
  }

  private getStoredToken(): string | null {
    return localStorage.getItem(this.storageKey);
  }

  private getStoredUserInfo(): UserInfo | null {
    const stored = localStorage.getItem(this.userInfoKey);
    return stored ? JSON.parse(stored) : null;
  }

  private clearStoredAuth(): void {
    localStorage.removeItem(this.storageKey);
    localStorage.removeItem(this.userInfoKey);
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload.exp * 1000; // Convert to milliseconds
      return Date.now() >= exp;
    } catch {
      return true;
    }
  }
}