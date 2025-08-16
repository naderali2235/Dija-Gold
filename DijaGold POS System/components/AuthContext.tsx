import React, { createContext, useContext, useState, useEffect } from 'react';

interface User {
  id: string;
  username: string;
  role: 'Cashier' | 'Manager';
  branchId: string;
  fullName: string;
}

interface AuthContextType {
  user: User | null;
  login: (username: string, password: string) => Promise<boolean>;
  logout: () => void;
  isAuthenticated: boolean;
  isManager: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);

  useEffect(() => {
    // Check for stored user session
    const storedUser = localStorage.getItem('dijapos_user');
    if (storedUser) {
      setUser(JSON.parse(storedUser));
    }
  }, []);

  const login = async (username: string, password: string): Promise<boolean> => {
    // Mock authentication - in real app, this would call API
    const mockUsers = {
      'manager': { id: '1', username: 'manager', role: 'Manager' as const, branchId: 'branch1', fullName: 'Store Manager' },
      'cashier': { id: '2', username: 'cashier', role: 'Cashier' as const, branchId: 'branch1', fullName: 'John Cashier' }
    };

    if (mockUsers[username as keyof typeof mockUsers] && password === 'password') {
      const userData = mockUsers[username as keyof typeof mockUsers];
      setUser(userData);
      localStorage.setItem('dijapos_user', JSON.stringify(userData));
      return true;
    }
    return false;
  };

  const logout = () => {
    setUser(null);
    localStorage.removeItem('dijapos_user');
  };

  const isAuthenticated = !!user;
  const isManager = user?.role === 'Manager';

  return (
    <AuthContext.Provider value={{ user, login, logout, isAuthenticated, isManager }}>
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