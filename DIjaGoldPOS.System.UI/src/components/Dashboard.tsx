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
  Loader2,
} from 'lucide-react';
import { useAuth } from './AuthContext';
import { formatCurrency, GOLD_RATES_EGP } from './utils/currency';
import { useGoldRates, useDailySalesSummary, useTransactionLogReport } from '../hooks/useApi';

interface DashboardProps {
  onNavigate?: (page: string) => void;
}

interface GoldRateData {
  rate: number;
  change: number;
  changePercent: number;
}

interface GoldRatesMap {
  [karat: string]: GoldRateData;
}

export default function Dashboard({ onNavigate }: DashboardProps) {
  const { user, isManager } = useAuth();
  const { data: goldRatesData, loading: goldRatesLoading, error: goldRatesError } = useGoldRates();
  
  // Dashboard data hooks
  const { execute: fetchDailySalesSummary, loading: dailySalesLoading } = useDailySalesSummary();
  const { execute: fetchTransactionLogReport, loading: transactionLogLoading } = useTransactionLogReport();
  
  // State for dashboard data
  const [todayStats, setTodayStats] = React.useState({
    sales: { count: 0, amount: 0 },
    returns: { count: 0, amount: 0 },
    repairs: { count: 0, amount: 0 },
    lowStock: 0,
  });
  
  const [recentActivities, setRecentActivities] = React.useState<any[]>([]);
  const [dashboardLoading, setDashboardLoading] = React.useState(true);

  // Transform API gold rates to dashboard format
  const goldRates: GoldRatesMap = React.useMemo(() => {
    if (!goldRatesData || goldRatesData.length === 0) {
      // Fallback to default rates if API data not available
      return {
        '24k': { rate: GOLD_RATES_EGP['24k'], change: 0, changePercent: 0 },
        '22k': { rate: GOLD_RATES_EGP['22k'], change: 0, changePercent: 0 },
        '18k': { rate: GOLD_RATES_EGP['18k'], change: 0, changePercent: 0 },
        '14k': { rate: GOLD_RATES_EGP['14k'], change: 0, changePercent: 0 },
      };
    }

    const rates: GoldRatesMap = {};
    goldRatesData.forEach((rate: any) => {
      // Skip if karatType is undefined or null
      if (!rate.karatType) {
        console.warn('Skipping gold rate with undefined karatType:', rate);
        return;
      }
      
      // Convert karatType number to string format (e.g., 18 -> "18k")
      const karatString = `${rate.karatType}k`;
      
      // For now, use a small fixed change percentage (in real app, this would come from historical data)
      const changePercent = 0.1; // 0.1% change
      const change = rate.ratePerGram * (changePercent / 100);
      
      rates[karatString] = {
        rate: rate.ratePerGram,
        change: change,
        changePercent: changePercent,
      };
    });
    
    return rates;
  }, [goldRatesData]);

  // Fetch dashboard data on component mount
  React.useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        setDashboardLoading(true);
        const today = new Date().toISOString().split('T')[0];
        const branchId = user?.branch?.id || 1; // Default to branch 1 if user branch not available
        
        // Fetch daily sales summary
        const dailySales = await fetchDailySalesSummary(branchId, today);
        setTodayStats({
          sales: { 
            count: dailySales.transactionCount, 
            amount: dailySales.totalSales 
          },
          returns: { 
            count: 0, // This would need to be calculated from transaction data
            amount: dailySales.totalReturns 
          },
          repairs: { 
            count: 0, // This would need to be calculated from transaction data
            amount: 0 
          },
          lowStock: 0, // This would need to be fetched from inventory API
        });
        
        // Fetch recent transactions
        const transactionLog = await fetchTransactionLogReport(branchId, today);
        const activities = transactionLog.transactions.slice(0, 5).map((transaction: any) => ({
          type: transaction.transactionType.toLowerCase(),
          id: transaction.transactionNumber,
          amount: transaction.totalAmount,
          time: new Date(transaction.transactionDate).toLocaleTimeString([], { 
            hour: '2-digit', 
            minute: '2-digit' 
          }),
        }));
        setRecentActivities(activities);
      } catch (error) {
        console.error('Failed to fetch dashboard data:', error);
        // Set fallback data on error
        setTodayStats({
          sales: { count: 0, amount: 0 },
          returns: { count: 0, amount: 0 },
          repairs: { count: 0, amount: 0 },
          lowStock: 0,
        });
        setRecentActivities([]);
      } finally {
        setDashboardLoading(false);
      }
    };

    if (user?.branch?.id) {
      fetchDashboardData();
    }
  }, [user?.branch?.id, fetchDailySalesSummary, fetchTransactionLogReport]);

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
          <Button 
            className="touch-target"
            variant="golden"
            onClick={() => onNavigate?.('sales')}
          >
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
        <Card 
          className="pos-card cursor-pointer hover:shadow-lg transition-shadow"
          onClick={() => onNavigate?.('sales')}
        >
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">Today's Sales</CardTitle>
            <ShoppingCart className="h-4 w-4 text-[#D4AF37]" />
          </CardHeader>
          <CardContent>
            {dashboardLoading ? (
              <div className="animate-pulse">
                <div className="h-8 bg-gray-300 rounded mb-2"></div>
                <div className="h-3 bg-gray-300 rounded w-3/4"></div>
              </div>
            ) : (
              <>
                <div className="text-2xl text-[#0D1B2A]">{todayStats.sales.count}</div>
                <p className="text-xs text-muted-foreground">
                  {formatCurrency(todayStats.sales.amount)} total
                </p>
              </>
            )}
          </CardContent>
        </Card>

        <Card 
          className="pos-card cursor-pointer hover:shadow-lg transition-shadow"
          onClick={() => onNavigate?.('returns')}
        >
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">Returns</CardTitle>
            <RotateCcw className="h-4 w-4 text-orange-600" />
          </CardHeader>
          <CardContent>
            {dashboardLoading ? (
              <div className="animate-pulse">
                <div className="h-8 bg-gray-300 rounded mb-2"></div>
                <div className="h-3 bg-gray-300 rounded w-3/4"></div>
              </div>
            ) : (
              <>
                <div className="text-2xl text-[#0D1B2A]">{todayStats.returns.count}</div>
                <p className="text-xs text-muted-foreground">
                  {formatCurrency(todayStats.returns.amount)} total
                </p>
              </>
            )}
          </CardContent>
        </Card>

        <Card 
          className="pos-card cursor-pointer hover:shadow-lg transition-shadow"
          onClick={() => onNavigate?.('repairs')}
        >
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">Repairs</CardTitle>
            <Wrench className="h-4 w-4 text-blue-600" />
          </CardHeader>
          <CardContent>
            {dashboardLoading ? (
              <div className="animate-pulse">
                <div className="h-8 bg-gray-300 rounded mb-2"></div>
                <div className="h-3 bg-gray-300 rounded w-3/4"></div>
              </div>
            ) : (
              <>
                <div className="text-2xl text-[#0D1B2A]">{todayStats.repairs.count}</div>
                <p className="text-xs text-muted-foreground">
                  {formatCurrency(todayStats.repairs.amount)} total
                </p>
              </>
            )}
          </CardContent>
        </Card>

        <Card 
          className="pos-card cursor-pointer hover:shadow-lg transition-shadow"
          onClick={() => onNavigate?.('inventory')}
        >
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">Low Stock</CardTitle>
            <AlertTriangle className="h-4 w-4 text-red-600" />
          </CardHeader>
          <CardContent>
            {dashboardLoading ? (
              <div className="animate-pulse">
                <div className="h-8 bg-gray-300 rounded mb-2"></div>
                <div className="h-3 bg-gray-300 rounded w-3/4"></div>
              </div>
            ) : (
              <>
                <div className="text-2xl text-[#0D1B2A]">{todayStats.lowStock}</div>
                <p className="text-xs text-muted-foreground">
                  Items need restocking
                </p>
              </>
            )}
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
              <Button 
                variant="outline" 
                className="touch-target h-20 flex-col gap-2"
                onClick={() => onNavigate?.('sales')}
              >
                <ShoppingCart className="h-6 w-6" />
                New Sale
              </Button>
              <Button 
                variant="outline" 
                className="touch-target h-20 flex-col gap-2"
                onClick={() => onNavigate?.('returns')}
              >
                <RotateCcw className="h-6 w-6" />
                Process Return
              </Button>
              <Button 
                variant="outline" 
                className="touch-target h-20 flex-col gap-2"
                onClick={() => onNavigate?.('inventory')}
              >
                <Package className="h-6 w-6" />
                Check Inventory
              </Button>
              <Button 
                variant="outline" 
                className="touch-target h-20 flex-col gap-2"
                onClick={() => onNavigate?.('customers')}
              >
                <Users className="h-6 w-6" />
                Find Customer
              </Button>
              {isManager && (
                <>
                  <Button 
                    variant="outline" 
                    className="touch-target h-20 flex-col gap-2"
                    onClick={() => onNavigate?.('reports')}
                  >
                    <TrendingUp className="h-6 w-6" />
                    Daily Report
                  </Button>
                  <Button 
                    variant="outline" 
                    className="touch-target h-20 flex-col gap-2"
                    onClick={() => onNavigate?.('settings')}
                  >
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