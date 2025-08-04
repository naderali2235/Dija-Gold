import { UserRole } from './enums';

export interface LoginRequest {
  username: string;
  password: string;
  rememberMe?: boolean;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  user: UserInfo;
}

export interface UserInfo {
  id: string;
  username: string;
  fullName: string;
  email: string;
  employeeCode?: string;
  roles: string[];
  branch?: BranchInfo;
  lastLoginAt?: string;
}

export interface BranchInfo {
  id: number;
  name: string;
  code: string;
  isHeadquarters: boolean;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ApiResponse<T = any> {
  success: boolean;
  message: string;
  data?: T;
  errors?: any;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface AuthState {
  isAuthenticated: boolean;
  user: UserInfo | null;
  token: string | null;
  loading: boolean;
  error: string | null;
}

export interface NavigationItem {
  label: string;
  route?: string;
  icon: string;
  roles?: UserRole[];
  children?: NavigationItem[];
  badge?: string | number;
  badgeColor?: string;
}