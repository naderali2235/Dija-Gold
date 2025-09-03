import React, { createContext, useContext, useState, useEffect } from 'react';
import { useLogin, useCurrentUser } from '../hooks/useApi';
import { getAuthToken } from '../services/api';
import { User as ApiUser } from '../services/api';

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
  error: string | null;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

function mapApiUserToUser(apiUser: ApiUser): User {
  console.log('Mapping API user to UI user:', apiUser);
  
  // Map API user to UI user format for backward compatibility
  const primaryRole: 'Cashier' | 'Manager' = apiUser.roles.includes('Manager') ? 'Manager' : 'Cashier';
  const mappedUser = {
    id: apiUser.id,
    username: apiUser.username,
    role: primaryRole,
    branchId: apiUser.branch?.code || 'default', // Keep as string for backward compatibility
    fullName: apiUser.fullName,
    email: apiUser.email,
    employeeCode: apiUser.employeeCode,
    roles: apiUser.roles,
    branch: apiUser.branch, // This contains the numeric ID
    lastLoginAt: apiUser.lastLoginAt,
  };
  
  console.log('Mapped user:', mappedUser);
  return mappedUser;
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [error, setError] = useState<string | null>(null);
  
  // Use the hooks from useApi.ts
  const { execute: loginExecute, loading: loginLoading, error: loginError } = useLogin();
  const { fetchUser, logout: logoutExecute, loading: userLoading, error: userError } = useCurrentUser();

  // Combined loading state
  const isLoading = loginLoading || userLoading;

  // Handle errors from hooks
  useEffect(() => {
    if (loginError) {
      setError(loginError);
    }
  }, [loginError]);

  useEffect(() => {
    if (userError) {
      setError(userError);
    }
  }, [userError]);

  useEffect(() => {
    // Check for stored token and validate with API
    const initializeAuth = async () => {
      console.log('Initializing auth...');
      
      try {
        // Guard: if no token is present, don't call /auth/me to avoid 401 loops
        const token = getAuthToken();
        const onLoginRoute = typeof window !== 'undefined' && window.location.pathname.toLowerCase() === '/login';
        if (!token) {
          console.log('No auth token found; skipping current user fetch.');
          setUser(null);
          setError(null);
          return;
        }
        if (onLoginRoute) {
          console.log('On login route; deferring current user fetch.');
          return;
        }
        console.log('Attempting to get current user from API...');
        const apiUser = await fetchUser();
        if (apiUser) {
          const mappedUser = mapApiUserToUser(apiUser);
          console.log('Successfully validated stored token for user:', mappedUser.username);
          console.log('User branch information:', mappedUser.branch);
          setUser(mappedUser);
        }
      } catch (error) {
        // Token invalid or expired, clear it
        console.warn('Failed to validate stored token:', error);
        setUser(null);
        setError(null); // Clear any previous errors
      }
    };

    initializeAuth();
  }, [fetchUser]);

  const login = async (username: string, password: string): Promise<boolean> => {
    try {
      setError(null); // Clear previous errors
      console.log('Starting login process for user:', username);
      
      const response = await loginExecute({
        username,
        password,
        rememberMe: true
      });
      
      console.log('Login API response received:', response);
      
      const mappedUser = mapApiUserToUser(response.user);
      console.log('Mapped user:', mappedUser);
      console.log('User branch information:', mappedUser.branch);
      
      setUser(mappedUser);
      console.log('User state set, login successful');
      
      return true;
    } catch (error) {
      console.error('Login failed:', error);
      setUser(null); // Clear any existing user state on failure
      
      // Re-throw the error - no redirect on failure, let the UI handle it
      throw error;
    }
  };

  const logout = async () => {
    try {
      await logoutExecute();
    } catch (error) {
      console.error('Logout failed:', error);
    } finally {
      setUser(null);
      setError(null);
    }
  };

  const isAuthenticated = !!user;
  const isManager = !!(user?.role === 'Manager' || user?.roles?.includes('Manager'));

  return (
    <AuthContext.Provider value={{ 
      user, 
      login, 
      logout, 
      isAuthenticated, 
      isManager, 
      isLoading,
      error 
    }}>
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