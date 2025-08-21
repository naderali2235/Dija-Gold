import React, { useState } from 'react';
import { AuthProvider, useAuth } from './components/AuthContext';
import LoginScreen from './components/LoginScreen';
import Layout from './components/Layout';
import Dashboard from './components/Dashboard';
import Sales from './components/Sales';
// import Returns from './components/Returns'; // Hidden per user request
import Repairs from './components/Repairs';
import Products from './components/Products';
import Inventory from './components/Inventory';
import Customers from './components/Customers';
import Suppliers from './components/Suppliers';
import PurchaseOrders from './components/PurchaseOrders';
import Reports from './components/Reports';
import CashDrawer from './components/CashDrawer';
import Users from './components/Users';
import Settings from './components/Settings';
import { Toaster } from './components/ui/sonner';

function AppContent() {
  const { isAuthenticated, isLoading, user } = useAuth();
  const [currentPage, setCurrentPage] = useState('dashboard');

  console.log('AppContent render - isAuthenticated:', isAuthenticated, 'isLoading:', isLoading, 'user:', user);

  if (isLoading) {
    console.log('Rendering loading screen');
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
    console.log('Rendering login screen');
    return <LoginScreen />;
  }

  console.log('Rendering main app');

  const renderPage = () => {
    switch (currentPage) {
      case 'dashboard':
        return <Dashboard onNavigate={setCurrentPage} />;
      case 'sales':
        return <Sales />;
      // case 'returns': // Hidden per user request
      //   return <Returns />;
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
      case 'purchase-orders':
        return <PurchaseOrders />;
      case 'reports':
        return <Reports />;
      case 'cash-drawer':
        return <CashDrawer />;
      case 'users':
        return <Users />;
      case 'settings':
        return <Settings />;
      default:
        return <Dashboard onNavigate={setCurrentPage} />;
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