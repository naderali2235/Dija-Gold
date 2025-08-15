import React from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Badge } from './ui/badge';
import { Button } from './ui/button';
import {
  ShoppingCart,
  Package,
  Users,
  TrendingUp,
  TrendingDown,
  AlertTriangle,
  DollarSign,
  RotateCcw,
  Wrench,
} from 'lucide-react';
import { useAuth } from './AuthContext';
import { formatCurrency, GOLD_RATES_EGP } from './utils/currency';
import { useGoldRates } from '../hooks/useApi';

export default function Dashboard() {
  const { user, isManager } = useAuth();
  const { data: goldRatesData, loading: goldRatesLoading, error: goldRatesError } = useGoldRates();

  // Transform API gold rates to dashboard format
  const goldRates = React.useMemo(() => {
    if (!goldRatesData || goldRatesData.length === 0) {
      // Fallback to default rates if API data not available
      return {
        '24k': { rate: GOLD_RATES_EGP['24k'], change: 0, changePercent: 0 },
        '22k': { rate: GOLD_RATES_EGP['22k'], change: 0, changePercent: 0 },
        '18k': { rate: GOLD_RATES_EGP['18k'], change: 0, changePercent: 0 },
        '14k': { rate: GOLD_RATES_EGP['14k'], change: 0, changePercent: 0 },
      };
    }

    const rates: any = {};
    goldRatesData.forEach((rate: any) => {
      // Calculate mock change percentage for now (in real app, this would come from historical data)
      const mockChangePercent = Math.random() * 0.5 - 0.25; // Random between -0.25% and +0.25%
      const mockChange = rate.sellRate * (mockChangePercent / 100);
      
      rates[rate.karat.toLowerCase()] = {
        rate: rate.sellRate,
        change: mockChange,
        changePercent: mockChangePercent,
      };
    });
    
    return rates;
  }, [goldRatesData]);

  const todayStats = {
    sales: { count: 23, amount: 145780 },
    returns: { count: 2, amount: 8560 },
    repairs: { count: 5, amount: 3200 },
    lowStock: 12,
  };

  const recentActivities = [
    { type: 'sale', id: 'INV-2024-001', amount: 12500, time: '10:30 AM' },
    { type: 'return', id: 'RET-2024-001', amount: 8560, time: '11:15 AM' },
    { type: 'repair', id: 'REP-2024-003', amount: 800, time: '12:00 PM' },
    { type: 'sale', id: 'INV-2024-002', amount: 25600, time: '1:45 PM' },
  ];

  const getActivityIcon = (type: string) => {
    switch (type) {
      case 'sale': return <ShoppingCart className="h-4 w-4 text-green-600" />;
      case 'return': return <RotateCcw className="h-4 w-4 text-orange-600" />;
      case 'repair': return <Wrench className="h-4 w-4 text-blue-600" />;
      default: return <Package className="h-4 w-4" />;
    }
  };

  return (
    <div className="space-y-6">
      {/* Welcome Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl text-[#0D1B2A]">
            Good {new Date().getHours() < 12 ? 'Morning' : new Date().getHours() < 17 ? 'Afternoon' : 'Evening'}, {user?.fullName}
          </h1>
          <p className="text-muted-foreground">Here's what's happening at your store today.</p>
        </div>
        <div className="flex gap-3">
          <Button className="touch-target pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
            <ShoppingCart className="mr-2 h-4 w-4" />
            New Sale
          </Button>
        </div>
      </div>

      {/* Gold Rates */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <TrendingUp className="h-5 w-5 text-[#D4AF37]" />
            Today's Gold Rates
            {goldRatesLoading && (
              <div className="animate-pulse h-4 w-4 bg-[#D4AF37] rounded-full"></div>
            )}
          </CardTitle>
          <CardDescription>
            {goldRatesError ? (
              <span className="text-red-600">Failed to load rates - showing cached data</span>
            ) : (
              "Current market rates per gram in Egyptian Pounds"
            )}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {goldRatesLoading && !goldRatesData ? (
            <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
              {[1, 2, 3, 4].map((i) => (
                <div key={i} className="p-4 bg-gradient-to-br from-[#D4AF37]/10 to-[#D4AF37]/5 rounded-lg border border-[#D4AF37]/20">
                  <div className="animate-pulse">
                    <div className="h-4 bg-gray-300 rounded mb-2"></div>
                    <div className="h-8 bg-gray-300 rounded mb-2"></div>
                    <div className="h-3 bg-gray-300 rounded w-3/4"></div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
              {Object.entries(goldRates).map(([karat, data]) => (
                <div key={karat} className="p-4 bg-gradient-to-br from-[#D4AF37]/10 to-[#D4AF37]/5 rounded-lg border border-[#D4AF37]/20">
                  <div className="flex items-center justify-between mb-2">
                    <h3 className="font-semibold text-[#0D1B2A]">{karat.toUpperCase()}</h3>
                    <Badge variant={data.change >= 0 ? "default" : "destructive"} className="text-xs">
                      {data.change >= 0 ? '+' : ''}{data.changePercent.toFixed(2)}%
                    </Badge>
                  </div>
                  <p className="text-2xl text-[#0D1B2A]">{formatCurrency(data.rate)}</p>
                  <p className={`text-sm ${data.change >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                    {data.change >= 0 ? '+' : ''}{formatCurrency(Math.abs(data.change))}
                  </p>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <Card className="pos-card">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">Today's Sales</CardTitle>
            <ShoppingCart className="h-4 w-4 text-[#D4AF37]" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl text-[#0D1B2A]">{todayStats.sales.count}</div>
            <p className="text-xs text-muted-foreground">
              {formatCurrency(todayStats.sales.amount)} total
            </p>
          </CardContent>
        </Card>

        <Card className="pos-card">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">Returns</CardTitle>
            <RotateCcw className="h-4 w-4 text-orange-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl text-[#0D1B2A]">{todayStats.returns.count}</div>
            <p className="text-xs text-muted-foreground">
              {formatCurrency(todayStats.returns.amount)} total
            </p>
          </CardContent>
        </Card>

        <Card className="pos-card">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">Repairs</CardTitle>
            <Wrench className="h-4 w-4 text-blue-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl text-[#0D1B2A]">{todayStats.repairs.count}</div>
            <p className="text-xs text-muted-foreground">
              {formatCurrency(todayStats.repairs.amount)} total
            </p>
          </CardContent>
        </Card>

        <Card className="pos-card">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">Low Stock</CardTitle>
            <AlertTriangle className="h-4 w-4 text-red-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl text-[#0D1B2A]">{todayStats.lowStock}</div>
            <p className="text-xs text-muted-foreground">
              Items need restocking
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Recent Activities & Quick Actions */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Recent Activities */}
        <Card className="pos-card">
          <CardHeader>
            <CardTitle>Recent Activities</CardTitle>
            <CardDescription>Latest transactions and updates</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {recentActivities.map((activity, index) => (
                <div key={index} className="flex items-center gap-4 p-3 bg-muted/50 rounded-lg">
                  {getActivityIcon(activity.type)}
                  <div className="flex-1">
                    <p className="text-sm">{activity.id}</p>
                    <p className="text-xs text-muted-foreground">{activity.time}</p>
                  </div>
                  <div className="text-right">
                    <p className="text-sm">{formatCurrency(activity.amount)}</p>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Quick Actions */}
        <Card className="pos-card">
          <CardHeader>
            <CardTitle>Quick Actions</CardTitle>
            <CardDescription>Frequently used operations</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 gap-4">
              <Button variant="outline" className="touch-target h-20 flex-col gap-2">
                <ShoppingCart className="h-6 w-6" />
                New Sale
              </Button>
              <Button variant="outline" className="touch-target h-20 flex-col gap-2">
                <RotateCcw className="h-6 w-6" />
                Process Return
              </Button>
              <Button variant="outline" className="touch-target h-20 flex-col gap-2">
                <Package className="h-6 w-6" />
                Check Inventory
              </Button>
              <Button variant="outline" className="touch-target h-20 flex-col gap-2">
                <Users className="h-6 w-6" />
                Find Customer
              </Button>
              {isManager && (
                <>
                  <Button variant="outline" className="touch-target h-20 flex-col gap-2">
                    <TrendingUp className="h-6 w-6" />
                    Daily Report
                  </Button>
                  <Button variant="outline" className="touch-target h-20 flex-col gap-2">
                    <DollarSign className="h-6 w-6" />
                    Update Rates
                  </Button>
                </>
              )}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}