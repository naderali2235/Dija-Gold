import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

import { environment } from '@environments/environment';
import { ApiResponse, PagedResult } from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // Generic GET request
  get<T>(endpoint: string, params?: any): Observable<T> {
    const httpParams = this.buildHttpParams(params);
    
    return this.http.get<ApiResponse<T>>(`${this.apiUrl}/${endpoint}`, { params: httpParams })
      .pipe(
        map(response => this.handleResponse<T>(response)),
        catchError(this.handleError)
      );
  }

  // Generic POST request
  post<T>(endpoint: string, data?: any): Observable<T> {
    return this.http.post<ApiResponse<T>>(`${this.apiUrl}/${endpoint}`, data)
      .pipe(
        map(response => this.handleResponse<T>(response)),
        catchError(this.handleError)
      );
  }

  // Generic PUT request
  put<T>(endpoint: string, data?: any): Observable<T> {
    return this.http.put<ApiResponse<T>>(`${this.apiUrl}/${endpoint}`, data)
      .pipe(
        map(response => this.handleResponse<T>(response)),
        catchError(this.handleError)
      );
  }

  // Generic DELETE request
  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<ApiResponse<T>>(`${this.apiUrl}/${endpoint}`)
      .pipe(
        map(response => this.handleResponse<T>(response)),
        catchError(this.handleError)
      );
  }

  // Generic PATCH request
  patch<T>(endpoint: string, data?: any): Observable<T> {
    return this.http.patch<ApiResponse<T>>(`${this.apiUrl}/${endpoint}`, data)
      .pipe(
        map(response => this.handleResponse<T>(response)),
        catchError(this.handleError)
      );
  }

  // File upload
  uploadFile<T>(endpoint: string, file: File, additionalData?: any): Observable<T> {
    const formData = new FormData();
    formData.append('file', file);
    
    if (additionalData) {
      Object.keys(additionalData).forEach(key => {
        formData.append(key, additionalData[key]);
      });
    }

    return this.http.post<ApiResponse<T>>(`${this.apiUrl}/${endpoint}`, formData)
      .pipe(
        map(response => this.handleResponse<T>(response)),
        catchError(this.handleError)
      );
  }

  // File download
  downloadFile(endpoint: string, params?: any): Observable<Blob> {
    const httpParams = this.buildHttpParams(params);
    
    return this.http.get(`${this.apiUrl}/${endpoint}`, {
      params: httpParams,
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  // Helper method to build HttpParams from object
  private buildHttpParams(params?: any): HttpParams {
    let httpParams = new HttpParams();
    
    if (params) {
      Object.keys(params).forEach(key => {
        const value = params[key];
        if (value !== null && value !== undefined && value !== '') {
          if (Array.isArray(value)) {
            value.forEach(item => {
              httpParams = httpParams.append(key, item.toString());
            });
          } else if (value instanceof Date) {
            httpParams = httpParams.set(key, value.toISOString());
          } else {
            httpParams = httpParams.set(key, value.toString());
          }
        }
      });
    }
    
    return httpParams;
  }

  // Handle successful API responses
  private handleResponse<T>(response: ApiResponse<T>): T {
    if (!response.success) {
      throw new Error(response.message || 'API request failed');
    }
    
    return response.data as T;
  }

  // Handle API errors
  private handleError = (error: HttpErrorResponse): Observable<never> => {
    let errorMessage = 'An unexpected error occurred';
    
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = error.error.message;
    } else {
      // Server-side error
      if (error.error && error.error.message) {
        errorMessage = error.error.message;
      } else if (error.message) {
        errorMessage = error.message;
      } else {
        switch (error.status) {
          case 0:
            errorMessage = 'Unable to connect to server. Please check your internet connection.';
            break;
          case 400:
            errorMessage = 'Bad request. Please check your input.';
            break;
          case 401:
            errorMessage = 'Unauthorized. Please log in again.';
            break;
          case 403:
            errorMessage = 'Access denied. You do not have permission to perform this action.';
            break;
          case 404:
            errorMessage = 'The requested resource was not found.';
            break;
          case 500:
            errorMessage = 'Internal server error. Please try again later.';
            break;
          case 503:
            errorMessage = 'Service unavailable. Please try again later.';
            break;
          default:
            errorMessage = `Server error: ${error.status}`;
        }
      }
    }

    console.error('API Error:', error);
    return throwError(() => ({ message: errorMessage, status: error.status, error }));
  };
}

// Utility service for common API patterns
@Injectable({
  providedIn: 'root'
})
export class ApiHelperService {
  constructor(private apiService: ApiService) {}

  // Get paginated results
  getPaginated<T>(endpoint: string, params?: any): Observable<PagedResult<T>> {
    return this.apiService.get<PagedResult<T>>(endpoint, params);
  }

  // Search with filters
  search<T>(endpoint: string, searchParams: any): Observable<PagedResult<T>> {
    return this.apiService.get<PagedResult<T>>(`${endpoint}/search`, searchParams);
  }

  // Get by ID
  getById<T>(endpoint: string, id: number | string): Observable<T> {
    return this.apiService.get<T>(`${endpoint}/${id}`);
  }

  // Create new entity
  create<T>(endpoint: string, data: any): Observable<T> {
    return this.apiService.post<T>(endpoint, data);
  }

  // Update existing entity
  update<T>(endpoint: string, id: number | string, data: any): Observable<T> {
    return this.apiService.put<T>(`${endpoint}/${id}`, data);
  }

  // Delete entity
  deleteById<T>(endpoint: string, id: number | string): Observable<T> {
    return this.apiService.delete<T>(`${endpoint}/${id}`);
  }

  // Toggle active status
  toggleActive<T>(endpoint: string, id: number | string): Observable<T> {
    return this.apiService.patch<T>(`${endpoint}/${id}/toggle-active`);
  }

  // Export data
  export(endpoint: string, format: 'excel' | 'pdf', data?: any): Observable<Blob> {
    return this.apiService.downloadFile(`${endpoint}/export/${format}`, data);
  }
}