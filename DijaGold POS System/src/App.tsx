import React, { useState } from 'react';
import { AuthProvider, useAuth } from './components/AuthContext';
import LoginScreen from './components/LoginScreen';
import Layout from './components/Layout';
import Dashboard from './components/Dashboard';
import Sales from './components/Sales';
import Returns from './components/Returns';
import Repairs from './components/Repairs';
import Products from './components/Products';
import Inventory from './components/Inventory';
import Customers from './components/Customers';
import Suppliers from './components/Suppliers';
import Reports from './components/Reports';
import Users from './components/Users';
import Settings from './components/Settings';
import { Toaster } from './components/ui/sonner';

function AppContent() {
  const { isAuthenticated, isLoading } = useAuth();
  const [currentPage, setCurrentPage] = useState('dashboard');

  if (isLoading) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-center">
          <div className="animate-pulse flex justify-center mb-4">
            <div className="h-16 w-16 bg-gradient-to-br from-[#D4AF37] to-[#B8941F] rounded-full"></div>
          </div>
          <p className="text-muted-foreground">Loading DijaGold POS...</p>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <LoginScreen />;
  }

  const renderPage = () => {
    switch (currentPage) {
      case 'dashboard':
        return <Dashboard />;
      case 'sales':
        return <Sales />;
      case 'returns':
        return <Returns />;
      case 'repairs':
        return <Repairs />;
      case 'products':
        return <Products />;
      case 'inventory':
        return <Inventory />;
      case 'customers':
        return <Customers />;
      case 'suppliers':
        return <Suppliers />;
      case 'reports':
        return <Reports />;
      case 'users':
        return <Users />;
      case 'settings':
        return <Settings />;
      default:
        return <Dashboard />;
    }
  };

  return (
    <Layout currentPage={currentPage} onPageChange={setCurrentPage}>
      {renderPage()}
    </Layout>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <div className="min-h-screen bg-background">
        <AppContent />
        <Toaster />
      </div>
    </AuthProvider>
  );
}