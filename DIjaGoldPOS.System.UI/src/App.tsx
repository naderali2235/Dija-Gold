import React, { useState } from 'react';
import { AuthProvider, useAuth } from './components/AuthContext';
import LoginScreen from './components/LoginScreen';
import Layout from './components/Layout';
import Dashboard from './components/Dashboard';
import Sales from './components/Sales';
import Orders from './components/Orders';
import Repairs from './components/Repairs';
import Technicians from './components/Technicians';
import Products from './components/Products';
import Inventory from './components/Inventory';
import ProductOwnership from './components/ProductOwnership';
import OwnershipConsolidation from './components/OwnershipConsolidation';
import ProductManufacture from './components/ProductManufacture';
import ManufacturingReports from './components/ManufacturingReports';
import Customers from './components/Customers';
import CustomerPurchase from './components/CustomerPurchase';
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
      case 'orders':
        return <Orders />;
      case 'repairs':
        return <Repairs />;
      case 'technicians':
        return <Technicians />;
      case 'products':
        return <Products />;
      case 'inventory':
        return <Inventory />;
      case 'product-ownership':
        return <ProductOwnership />;
      case 'ownership-consolidation':
        return <OwnershipConsolidation />;
      case 'product-manufacture':
        return <ProductManufacture />;
      case 'customers':
        return <Customers />;
      case 'customer-purchases':
        return <CustomerPurchase />;
      case 'suppliers':
        return <Suppliers />;
      case 'purchase-orders':
        return <PurchaseOrders />;
      case 'reports':
        return <Reports />;
      case 'manufacturing-reports':
        return <ManufacturingReports />;
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