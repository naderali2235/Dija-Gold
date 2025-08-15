import React, { createContext, useContext, useState, useEffect } from 'react';
import api, { User as ApiUser, getAuthToken } from '../services/api';

interface User {
  id: string;
  username: string;
  role: 'Cashier' | 'Manager';
  branchId: string;
  fullName: string;
  email: string;
  employeeCode?: string;
  roles: string[];
  branch?: {
    id: number;
    name: string;
    code: string;
    isHeadquarters: boolean;
  };
  lastLoginAt?: string;
}

interface AuthContextType {
  user: User | null;
  login: (username: string, password: string) => Promise<boolean>;
  logout: () => void;
  isAuthenticated: boolean;
  isManager: boolean;
  isLoading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

function mapApiUserToUser(apiUser: ApiUser): User {
  // Map API user to UI user format for backward compatibility
  const primaryRole = apiUser.roles.includes('Manager') ? 'Manager' : 'Cashier';
  return {
    id: apiUser.id,
    username: apiUser.username,
    role: primaryRole,
    branchId: apiUser.branch?.code || 'default',
    fullName: apiUser.fullName,
    email: apiUser.email,
    employeeCode: apiUser.employeeCode,
    roles: apiUser.roles,
    branch: apiUser.branch,
    lastLoginAt: apiUser.lastLoginAt,
  };
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Check for stored token and validate with API
    const initializeAuth = async () => {
      const token = getAuthToken();
      if (token) {
        try {
          const apiUser = await api.auth.getCurrentUser();
          const mappedUser = mapApiUserToUser(apiUser);
          setUser(mappedUser);
        } catch (error) {
          // Token invalid or expired, clear it
          console.warn('Failed to validate stored token:', error);
          api.auth.logout(); // This will clear the token
        }
      }
      setIsLoading(false);
    };

    initializeAuth();
  }, []);

  const login = async (username: string, password: string): Promise<boolean> => {
    try {
      setIsLoading(true);
      const response = await api.auth.login({
        username,
        password,
        rememberMe: true
      });
      
      const mappedUser = mapApiUserToUser(response.user);
      setUser(mappedUser);
      
      return true;
    } catch (error) {
      console.error('Login failed:', error);
      return false;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = async () => {
    try {
      setIsLoading(true);
      await api.auth.logout();
    } catch (error) {
      console.error('Logout failed:', error);
    } finally {
      setUser(null);
      setIsLoading(false);
    }
  };

  const isAuthenticated = !!user;
  const isManager = user?.role === 'Manager' || user?.roles?.includes('Manager');

  return (
    <AuthContext.Provider value={{ user, login, logout, isAuthenticated, isManager, isLoading }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}