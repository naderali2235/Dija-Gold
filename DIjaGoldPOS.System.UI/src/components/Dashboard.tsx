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
  Wrench,
  Loader2,
} from 'lucide-react';
import { useAuth } from './AuthContext';
import { formatCurrency } from './utils/currency';
import { useGoldRates, useDailySalesSummary, useTransactionLogReport, useTransactionTypes, useLowStockItems } from '../hooks/useApi';
import { EnumLookupDto } from '../types/lookups';
import { productOwnershipApi, OwnershipAlertDto, ProductOwnershipDto } from '../services/api';
import { GoldRatesDisplay, GOLD_RATES_KEYS } from './shared/GoldRatesDisplay';

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
  const { user, isManager, isLoading } = useAuth();
  const { data: goldRatesData, loading: goldRatesLoading, error: goldRatesError, fetchRates } = useGoldRates();
  
  // Dashboard data hooks
  const { execute: fetchDailySalesSummary, loading: dailySalesLoading } = useDailySalesSummary();
  const { execute: fetchTransactionLogReport, loading: transactionLogLoading } = useTransactionLogReport();
  const { data: transactionTypesData, execute: fetchTransactionTypes } = useTransactionTypes();
  const { execute: fetchLowStockItems, loading: lowStockLoading } = useLowStockItems();
  
  // State for dashboard data
  const [todayStats, setTodayStats] = React.useState({
    sales: { count: 0, amount: 0 },
    repairs: { count: 0, amount: 0 },
    lowStock: 0,
  });
  
  const [recentActivities, setRecentActivities] = React.useState<any[]>([]);
  const [dashboardLoading, setDashboardLoading] = React.useState(true);
  
  // Ownership state
  const [ownershipAlerts, setOwnershipAlerts] = React.useState<OwnershipAlertDto[]>([]);
  const [lowOwnershipProducts, setLowOwnershipProducts] = React.useState<ProductOwnershipDto[]>([]);
  const [outstandingPayments, setOutstandingPayments] = React.useState<ProductOwnershipDto[]>([]);
  const [ownershipLoading, setOwnershipLoading] = React.useState(false);



  // Transform API gold rates to dashboard format using standardized keys
  const goldRates: GoldRatesMap = React.useMemo(() => {
    if (!goldRatesData || goldRatesData.length === 0) {
      // Return empty rates if API data not available
      return {};
    }

    const rates: GoldRatesMap = {};
    
    goldRatesData.forEach((rate: any) => {
      // Skip if karatTypeId is undefined or null
      if (!rate.karatTypeId) {
        console.warn('Skipping gold rate with undefined karatTypeId:', rate);
        return;
      }
      
      // Use the karat type name from the API response
      if (rate.karatType && rate.karatType.name) {
        const karatDisplayName = rate.karatType.name.toUpperCase();
        
        // For now, use a small fixed change percentage (in real app, this would come from historical data)
        const changePercent = 0.1; // 0.1% change
        const change = rate.ratePerGram * (changePercent / 100);
        
        rates[karatDisplayName] = {
          rate: rate.ratePerGram,
          change: change,
          changePercent: changePercent,
        };
      }
    });
    
    return rates;
  }, [goldRatesData]);

  // Function to get transaction type display name
  const getTransactionTypeName = (type: string | number): 'sale' | 'repair' => {
    // If it's already a string, it might be the display name
    if (typeof type === 'string') {
      // Check if it's a numeric string (enum value)
      const numericValue = parseInt(type);
      if (!isNaN(numericValue)) {
        const transactionType = transactionTypesData?.find(t => t.id === numericValue);
        const displayName = transactionType ? transactionType.name.toLowerCase() : type.toLowerCase();
        return (displayName === 'sale' || displayName === 'repair') 
          ? displayName as 'sale' | 'repair' 
          : 'sale';
      }
      // If it's already a display name, return as is if valid
      const lowerType = type.toLowerCase();
      return (lowerType === 'sale' || lowerType === 'repair') 
        ? lowerType as 'sale' | 'repair' 
        : 'sale';
    }
    
    // If it's a number, find the display name
    if (typeof type === 'number') {
      const transactionType = transactionTypesData?.find(t => t.id === type);
      const displayName = transactionType ? transactionType.name.toLowerCase() : 'sale';
      return (displayName === 'sale' || displayName === 'repair') 
        ? displayName as 'sale' | 'repair' 
        : 'sale';
    }
    
    return 'sale';
  };

  // Fetch dashboard data on component mount
  React.useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        setDashboardLoading(true);
        // Format date as YYYY-MM-DD in local timezone
        const today = new Date().getFullYear() + '-' + 
          String(new Date().getMonth() + 1).padStart(2, '0') + '-' + 
          String(new Date().getDate()).padStart(2, '0');
        const branchId = user?.branch?.id || 1; // Default to branch 1 if user branch not available
        
        console.log('Fetching dashboard data for branch:', branchId, 'date:', today);
        
        // Fetch transaction types lookup data
        await fetchTransactionTypes();
        
        // Fetch gold rates
        await fetchRates();
        
        // Fetch daily sales summary
        const dailySales = await fetchDailySalesSummary(branchId, today);
        console.log('Daily sales data received:', dailySales);
        
        // Fetch low stock items
        const lowStockItems = await fetchLowStockItems(branchId);
        console.log('Low stock items data received:', lowStockItems);
        
        // Ensure we have valid data with fallbacks
        const transactionCount = dailySales?.transactionCount ?? 0;
        const totalSales = dailySales?.totalSales ?? 0;
        const lowStockCount = lowStockItems?.length ?? 0;
        
        console.log('Processed sales data:', { transactionCount, totalSales });
        console.log('Low stock count:', lowStockCount);
        
        setTodayStats({
          sales: { 
            count: transactionCount, 
            amount: totalSales
          },
          repairs: { 
            count: 0, // This would need to be calculated from transaction data
            amount: 0 
          },
          lowStock: lowStockCount,
        });
        
        // Fetch ownership data
        try {
          setOwnershipLoading(true);
          const [alerts, lowOwnership, outstanding] = await Promise.all([
            productOwnershipApi.getOwnershipAlerts(branchId),
            productOwnershipApi.getLowOwnershipProducts(0.5),
            productOwnershipApi.getProductsWithOutstandingPayments()
          ]);
          
          setOwnershipAlerts(alerts);
          setLowOwnershipProducts(lowOwnership);
          setOutstandingPayments(outstanding);
        } catch (error) {
          console.error('Failed to load ownership data:', error);
        } finally {
          setOwnershipLoading(false);
        }
        
        // Fetch recent transactions
        const transactionLog = await fetchTransactionLogReport(branchId, today);
        console.log('Transaction log data received:', transactionLog);
        
        const transactions = transactionLog?.transactions || [];
        console.log('Transactions array:', transactions);
        console.log('Transaction count:', transactions.length);
        console.log('Transaction statuses:', transactions.map(t => ({ number: t.transactionNumber, status: t.status, type: typeof t.status })));
        
        const activities = transactions.slice(0, 5).map((transaction: any) => {
          // Handle transactionType using lookup data
          const transactionType = getTransactionTypeName(transaction.transactionType);
          
          const activity = {
            type: transactionType,
            id: transaction.transactionNumber || 'N/A',
            amount: transaction.totalAmount || 0,
            time: new Date(transaction.transactionDate).toLocaleTimeString([], { 
              hour: '2-digit', 
              minute: '2-digit' 
            }),
          };
          console.log('Processed activity:', activity);
          return activity;
        });
        
        console.log('Final activities array:', activities);
        setRecentActivities(activities);
      } catch (error) {
        console.error('Failed to fetch dashboard data:', error);
        // Set fallback data on error
        setTodayStats({
          sales: { count: 0, amount: 0 },
          repairs: { count: 0, amount: 0 },
          lowStock: 0,
        });
        setRecentActivities([]);
      } finally {
        setDashboardLoading(false);
      }
    };

    // Fetch data if user is authenticated, regardless of branch assignment
    if (user) {
      fetchDashboardData();
    } else if (!isLoading) {
      // If no user and auth is not loading, set loading to false and show empty state
      setDashboardLoading(false);
    }
    // If auth is still loading, keep dashboard loading state as true
  }, [user, isLoading, fetchDailySalesSummary, fetchTransactionLogReport, fetchLowStockItems]);

  const getActivityIcon = (type: string) => {
    switch (type) {
      case 'sale': return <ShoppingCart className="h-4 w-4 text-green-600" />;
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
          <p className="text-muted-foreground">
            Here's what's happening at your store today.
            {(dashboardLoading || lowStockLoading) && (
              <span className="ml-2 text-sm text-blue-600">
                <Loader2 className="inline h-4 w-4 animate-spin mr-1" />
                Loading data...
              </span>
            )}
          </p>
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
      <GoldRatesDisplay
        goldRates={goldRates}
        title="Today's Gold Rates"
        description={goldRatesError ? "Failed to load rates - showing cached data" : "Current market rates per gram in Egyptian Pounds"}
        loading={goldRatesLoading}
        error={goldRatesError}
        showInputs={false}
        showChanges={true}
        disabled={false}
      />

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
            {(dashboardLoading || lowStockLoading) ? (
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

      {/* Ownership Alerts */}
      {(ownershipAlerts.length > 0 || lowOwnershipProducts.length > 0 || outstandingPayments.length > 0) && (
        <Card className="pos-card">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 text-orange-500" />
              Ownership Alerts
            </CardTitle>
            <CardDescription>Products requiring attention due to ownership issues</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {/* Ownership Alerts */}
              {ownershipAlerts.slice(0, 3).map((alert, index) => (
                <div key={`alert-${index}`} className="flex items-center justify-between p-3 border rounded-lg bg-orange-50">
                  <div className="flex items-center space-x-3">
                    <AlertTriangle className="h-5 w-5 text-orange-500" />
                    <div>
                      <h4 className="font-medium text-sm">{alert.productName}</h4>
                      <p className="text-xs text-muted-foreground">{alert.message}</p>
                    </div>
                  </div>
                  <Badge 
                    variant="outline" 
                    className={
                      alert.severity.toLowerCase() === 'high' ? 'bg-red-100 text-red-800' :
                      alert.severity.toLowerCase() === 'medium' ? 'bg-yellow-100 text-yellow-800' :
                      'bg-blue-100 text-blue-800'
                    }
                  >
                    {alert.severity}
                  </Badge>
                </div>
              ))}

              {/* Low Ownership Products */}
              {lowOwnershipProducts.slice(0, 2).map((product, index) => (
                <div key={`low-${index}`} className="flex items-center justify-between p-3 border rounded-lg bg-red-50">
                  <div className="flex items-center space-x-3">
                    <Package className="h-5 w-5 text-red-500" />
                    <div>
                      <h4 className="font-medium text-sm">{product.productName}</h4>
                      <p className="text-xs text-muted-foreground">Low ownership: {product.ownershipPercentage.toFixed(1)}%</p>
                    </div>
                  </div>
                  <Badge variant="outline" className="bg-red-100 text-red-800">
                    Low Ownership
                  </Badge>
                </div>
              ))}

              {/* Outstanding Payments */}
              {outstandingPayments.slice(0, 2).map((product, index) => (
                <div key={`outstanding-${index}`} className="flex items-center justify-between p-3 border rounded-lg bg-yellow-50">
                  <div className="flex items-center space-x-3">
                    <DollarSign className="h-5 w-5 text-yellow-600" />
                    <div>
                      <h4 className="font-medium text-sm">{product.productName}</h4>
                      <p className="text-xs text-muted-foreground">Outstanding: {formatCurrency(product.outstandingAmount)}</p>
                    </div>
                  </div>
                  <Badge variant="outline" className="bg-yellow-100 text-yellow-800">
                    Payment Due
                  </Badge>
                </div>
              ))}

              {/* View All Button */}
              {(ownershipAlerts.length > 3 || lowOwnershipProducts.length > 2 || outstandingPayments.length > 2) && (
                <div className="pt-2">
                  <Button 
                    variant="outline" 
                    size="sm" 
                    className="w-full"
                    onClick={() => onNavigate?.('product-ownership')}
                  >
                    View All Ownership Issues
                  </Button>
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      )}

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