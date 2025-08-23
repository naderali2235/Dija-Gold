import React, { useState } from 'react';
import { Button } from './ui/button';
import { Badge } from './ui/badge';
import { Separator } from './ui/separator';
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
  SidebarTrigger,
  useSidebar,
} from './ui/sidebar';

import {
  BarChart3,
  Package,
  ShoppingCart,
  Wrench,
  Users,
  Truck,
  FileText,
  UserCog,
  Settings,
  LogOut,
  Bell,
  Wallet,
  Shield,
} from 'lucide-react';
import { useAuth } from './AuthContext';
import DijaLogo, { DijaLogoWithText } from './DijaLogo';

interface LayoutProps {
  children: React.ReactNode;
  currentPage: string;
  onPageChange: (page: string) => void;
}

function LayoutContent({ children, currentPage, onPageChange }: LayoutProps) {
  const { user, logout, isManager } = useAuth();
  const { open } = useSidebar();

  const menuItems = [
    {
      title: 'Dashboard',
      icon: BarChart3,
      id: 'dashboard',
      badge: null,
    },
    {
      title: 'Sales',
      icon: ShoppingCart,
      id: 'sales',
      badge: null,
    },
    {
      title: 'Orders',
      icon: FileText,
      id: 'orders',
      badge: null,
    },
    {
      title: 'Repairs',
      icon: Wrench,
      id: 'repairs',
      badge: null,
    },
  ];

  const inventoryItems = [
    {
      title: 'Products',
      icon: Package,
      id: 'products',
      badge: null,
    },
    {
      title: 'Inventory',
      icon: Package,
      id: 'inventory',
      badge: null,
    },
    {
      title: 'Product Ownership',
      icon: Shield,
      id: 'product-ownership',
      badge: null,
      managerOnly: true,
    },
  ];

  const managementItems = [
    {
      title: 'Customers',
      icon: Users,
      id: 'customers',
      badge: null,
    },
    {
      title: 'Technicians',
      icon: Wrench,
      id: 'technicians',
      badge: null,
    },
    {
      title: 'Suppliers',
      icon: Truck,
      id: 'suppliers',
      badge: null,
    },
    {
      title: 'Purchase Orders',
      icon: FileText,
      id: 'purchase-orders',
      badge: null,
      managerOnly: true,
    },
  ];

  const adminItems = [
    {
      title: 'Reports',
      icon: FileText,
      id: 'reports',
      managerOnly: true,
    },
    {
      title: 'Cash Drawer',
      icon: Wallet,
      id: 'cash-drawer',
      managerOnly: true,
    },
    {
      title: 'Users',
      icon: UserCog,
      id: 'users',
      managerOnly: true,
    },
    {
      title: 'Settings',
      icon: Settings,
      id: 'settings',
      managerOnly: true,
    },
  ];

  const renderMenuItems = (items: any[], groupName: string) => (
    <SidebarGroup>
      <SidebarGroupLabel className="text-sidebar-foreground/80">
        {groupName}
      </SidebarGroupLabel>
      <SidebarGroupContent>
        <SidebarMenu>
          {items
            .filter(item => !item.managerOnly || isManager)
            .map((item) => (
              <SidebarMenuItem key={item.id}>
                <SidebarMenuButton
                  onClick={() => onPageChange(item.id)}
                  isActive={currentPage === item.id}
                  className="w-full justify-start gap-3 text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground touch-target"
                >
                  <item.icon className="h-5 w-5" />
                  <span>{item.title}</span>
                  {item.badge && (
                    <Badge variant={item.badge.variant} className="ml-auto text-xs">
                      {item.badge.text}
                    </Badge>
                  )}
                </SidebarMenuButton>
              </SidebarMenuItem>
            ))}
        </SidebarMenu>
      </SidebarGroupContent>
    </SidebarGroup>
  );

  return (
    <div className="flex min-h-screen w-full bg-background">
        {/* Sidebar */}
        <Sidebar className="border-r border-sidebar-border bg-sidebar">
          <SidebarHeader className="border-b border-sidebar-border p-6">
            <DijaLogoWithText size="md" className="text-sidebar-foreground" />
          </SidebarHeader>
          
          <SidebarContent className="p-4">
            {renderMenuItems(menuItems, 'Operations')}
            {renderMenuItems(inventoryItems, 'Inventory')}
            {renderMenuItems(managementItems, 'Management')}
            {renderMenuItems(adminItems, 'Administration')}
            
            <Separator className="my-4 bg-sidebar-border" />
            
            {/* User section */}
            <SidebarGroup>
              <SidebarGroupContent>
                <div className="p-4 bg-sidebar-accent/50 rounded-lg">
                  <div className="flex items-center gap-3 mb-3">
                    <div className="w-8 h-8 bg-sidebar-primary rounded-full flex items-center justify-center">
                      <span className="text-sm font-semibold text-sidebar-primary-foreground">
                        {user?.fullName?.charAt(0) || 'U'}
                      </span>
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium text-sidebar-foreground truncate">
                        {user?.fullName || 'User'}
                      </p>
                      <p className="text-xs text-sidebar-foreground/60">
                        {user?.role || 'Role'}
                      </p>
                    </div>
                  </div>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={logout}
                    className="w-full justify-start gap-2 text-sidebar-foreground hover:bg-sidebar-accent touch-target"
                  >
                    <LogOut className="h-4 w-4" />
                    Sign Out
                  </Button>
                </div>
              </SidebarGroupContent>
            </SidebarGroup>
          </SidebarContent>
        </Sidebar>

        {/* Main content */}
        <main className={`flex-1 flex flex-col transition-all duration-300 ease-in-out ${
          open ? 'md:ml-[var(--sidebar-width)]' : 'md:ml-0'
        }`}>
          {/* Top bar */}
          <header className="flex items-center justify-between px-6 py-4 bg-card border-b border-border">
            <div className="flex items-center gap-4">
              <SidebarTrigger className="touch-target" />
              <div className="flex items-center gap-2">
                <DijaLogo size="sm" />
                <span className="font-semibold text-foreground">
                  DijaGold POS
                </span>
              </div>
            </div>
            
            <div className="flex items-center gap-3">
              {/* Notification Bell */}
              <Button variant="ghost" size="sm" className="touch-target relative">
                <Bell className="h-5 w-5" />
                <span className="absolute -top-1 -right-1 w-5 h-5 bg-destructive text-destructive-foreground text-xs rounded-full flex items-center justify-center">
                  3
                </span>
              </Button>
              
              {/* Current time */}
              <div className="text-sm text-muted-foreground hidden sm:block">
                {new Date().toLocaleTimeString('en-US', { 
                  hour: '2-digit', 
                  minute: '2-digit' 
                })}
              </div>
              
              {/* User role badge */}
              <Badge 
                variant={isManager ? "default" : "secondary"}
                className={isManager ? "bg-primary text-primary-foreground" : ""}
              >
                {user?.role}
              </Badge>
            </div>
          </header>

          {/* Page content */}
          <div className="flex-1 p-6 overflow-auto">
            {children}
          </div>
        </main>
      </div>
  );
}

export default function Layout(props: LayoutProps) {
  return (
    <SidebarProvider>
      <LayoutContent {...props} />
    </SidebarProvider>
  );
}