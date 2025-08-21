import React, { createContext, useContext, useState, useEffect } from 'react';
import api, { User as ApiUser, getAuthToken, testApiConnection } from '../services/api';

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
  console.log('Mapping API user to UI user:', apiUser);
  
  // Map API user to UI user format for backward compatibility
  const primaryRole: 'Cashier' | 'Manager' = apiUser.roles.includes('Manager') ? 'Manager' : 'Cashier';
  const mappedUser = {
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
  
  console.log('Mapped user:', mappedUser);
  return mappedUser;
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Check for stored token and validate with API
    const initializeAuth = async () => {
      console.log('Initializing auth...');
      
      // Test API connectivity first
      /*
      const isApiConnected = await testApiConnection();
      console.log('API connectivity test:', isApiConnected);
      
      if (!isApiConnected) {
        console.error('API is not accessible. Check if the backend is running on the configured URL.');
        setIsLoading(false);
        return;
      }
      */
      
      const token = getAuthToken();
      console.log('Found stored token:', !!token);
      
      if (token) {
        try {
          console.log('Attempting to get current user from API...');
          const apiUser = await api.auth.getCurrentUser();
          const mappedUser = mapApiUserToUser(apiUser);
          console.log('Successfully validated stored token for user:', mappedUser.username);
          console.log('User branch information:', mappedUser.branch);
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
      console.log('Starting login process for user:', username);
      
      const response = await api.auth.login({
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
  const isManager = !!(user?.role === 'Manager' || user?.roles?.includes('Manager'));

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