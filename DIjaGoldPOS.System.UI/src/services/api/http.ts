// Lightweight HTTP client shim to align modular APIs with the consolidated api.ts
// Provides both a generic apiRequest<T>() and a default http client with get/post/put/delete

import { API_CONFIG } from '../../config/environment';
import { getAuthToken, setAuthToken } from '../api';
import type { ApiResponse } from '../api';

export const API_BASE_URL = `${API_CONFIG.BASE_URL}/api`;

export async function apiRequest<T = any>(
  endpoint: string,
  options: RequestInit = {}
): Promise<ApiResponse<T>> {
  const url = `${API_BASE_URL}${endpoint}`;
  const token = getAuthToken();

  const defaultHeaders: HeadersInit = {
    'Content-Type': 'application/json',
  };

  if (token) {
    (defaultHeaders as any).Authorization = `Bearer ${token}`;
  }

  const config: RequestInit = {
    ...options,
    headers: {
      ...defaultHeaders,
      ...(options.headers || {}),
    },
  };

  try {
    const response = await fetch(url, config);
    const data = await response.json();

    if (!response.ok) {
      if (response.status === 401) {
        setAuthToken(null);
        // Optional redirect to login to mirror core behavior
        try { window.location.href = '/login'; } catch {}
        throw new Error('Session expired. Please login again.');
      }
      throw new Error(data?.message || `HTTP ${response.status}: ${response.statusText}`);
    }

    return data;
  } catch (error) {
    // eslint-disable-next-line no-console
    console.error(`HTTP Error (${endpoint}):`, error);
    throw error;
  }
}

// Convenience default client for common REST methods
const http = {
  get: <T = any>(endpoint: string, init?: RequestInit) =>
    apiRequest<T>(endpoint, { method: 'GET', ...(init || {}) }),
  post: <T = any>(endpoint: string, body?: any, init?: RequestInit) =>
    apiRequest<T>(endpoint, { method: 'POST', body: body != null ? JSON.stringify(body) : undefined, ...(init || {}) }),
  put: <T = any>(endpoint: string, body?: any, init?: RequestInit) =>
    apiRequest<T>(endpoint, { method: 'PUT', body: body != null ? JSON.stringify(body) : undefined, ...(init || {}) }),
  delete: <T = any>(endpoint: string, init?: RequestInit) =>
    apiRequest<T>(endpoint, { method: 'DELETE', ...(init || {}) }),
};

export default http;
