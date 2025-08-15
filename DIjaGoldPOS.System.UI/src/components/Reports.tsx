import React, { useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Badge } from './ui/badge';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from './ui/select';
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from './ui/tabs';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  LineChart,
  Line,
  PieChart,
  Pie,
  Cell,
} from 'recharts';
import {
  FileText,
  Download,
  Calendar,
  TrendingUp,
  DollarSign,
  Users,
  Package,
  BarChart3,
  PieChart as PieChartIcon,
  FileSpreadsheet,
} from 'lucide-react';
import { useAuth } from './AuthContext';
import { formatCurrency, formatCurrencyShort } from './utils/currency';

export default function Reports() {
  const { isManager } = useAuth();
  const [dateRange, setDateRange] = useState('last_30_days');
  const [reportType, setReportType] = useState('daily_sales');
  const [selectedBranch, setSelectedBranch] = useState('all');

  // Mock data for charts
  const dailySalesData = [
    { date: '2024-01-01', sales: 45000, transactions: 12 },
    { date: '2024-01-02', sales: 52000, transactions: 15 },
    { date: '2024-01-03', sales: 38000, transactions: 9 },
    { date: '2024-01-04', sales: 61000, transactions: 18 },
    { date: '2024-01-05', sales: 49000, transactions: 14 },
    { date: '2024-01-06', sales: 71000, transactions: 22 },
    { date: '2024-01-07', sales: 55000, transactions: 16 },
  ];

  const categoryData = [
    { name: 'Rings', value: 125000, transactions: 45, color: '#D4AF37' },
    { name: 'Chains', value: 98000, transactions: 32, color: '#B8941F' },
    { name: 'Earrings', value: 67000, transactions: 28, color: '#0D1B2A' },
    { name: 'Bangles', value: 89000, transactions: 21, color: '#17A2B8' },
    { name: 'Necklaces', value: 76000, transactions: 19, color: '#28A745' },
  ];

  const monthlyProfitData = [
    { month: 'Aug', revenue: 567000, cost: 412000, profit: 155000 },
    { month: 'Sep', revenue: 623000, cost: 445000, profit: 178000 },
    { month: 'Oct', revenue: 701000, cost: 498000, profit: 203000 },
    { month: 'Nov', revenue: 789000, cost: 556000, profit: 233000 },
    { month: 'Dec', revenue: 856000, cost: 599000, profit: 257000 },
    { month: 'Jan', revenue: 923000, cost: 645000, profit: 278000 },
  ];

  const topCustomersData = [
    { name: 'Ahmed Hassan', purchases: 125000, visits: 8, lastVisit: '2024-01-15' },
    { name: 'Fatima El-Sayed', purchases: 89000, visits: 5, lastVisit: '2024-01-14' },
    { name: 'Mohamed Ali', purchases: 67000, visits: 4, lastVisit: '2024-01-12' },
    { name: 'Nour Abdel Rahman', purchases: 54000, visits: 6, lastVisit: '2024-01-10' },
    { name: 'Hassan Ibrahim', purchases: 43000, visits: 3, lastVisit: '2024-01-08' },
  ];

  const inventoryData = [
    { category: 'Rings', inStock: 245, lowStock: 12, outOfStock: 3, value: 2450000 },
    { category: 'Chains', inStock: 189, lowStock: 8, outOfStock: 2, value: 1890000 },
    { category: 'Earrings', inStock: 156, lowStock: 15, outOfStock: 5, value: 1560000 },
    { category: 'Bangles', inStock: 98, lowStock: 6, outOfStock: 1, value: 980000 },
    { category: 'Necklaces', inStock: 134, lowStock: 9, outOfStock: 4, value: 1340000 },
  ];

  const reportSummary = {
    totalSales: 455000,
    totalTransactions: 127,
    averageTransaction: 3583,
    topSellingCategory: 'Rings',
    profitMargin: 28.5,
    returnRate: 1.2,
  };

  const handleExportReport = (format: 'excel' | 'pdf') => {
    if (!isManager) {
      alert('Only managers can export reports');
      return;
    }
    
    // Mock export functionality
    console.log(`Exporting ${reportType} report as ${format.toUpperCase()}`);
    alert(`${reportType.replace('_', ' ').toUpperCase()} report exported as ${format.toUpperCase()}`);
  };

  const CustomTooltip = ({ active, payload, label }: any) => {
    if (active && payload && payload.length) {
      return (
        <div className="bg-white p-3 border rounded-lg shadow-lg">
          <p className="font-medium">{label}</p>
          {payload.map((pld: any, index: number) => (
            <div key={index} className="text-sm">
              <span style={{ color: pld.color }}>
                {pld.dataKey === 'sales' || pld.dataKey === 'revenue' || pld.dataKey === 'cost' || pld.dataKey === 'profit' 
                  ? formatCurrency(pld.value) 
                  : pld.value}
              </span>
            </div>
          ))}
        </div>
      );
    }
    return null;
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl text-[#0D1B2A]">Reports & Analytics</h1>
          <p className="text-muted-foreground">Business insights and performance metrics</p>
        </div>
        {isManager && (
          <div className="flex gap-3">
            <Button
              variant="outline"
              onClick={() => handleExportReport('excel')}
              className="touch-target"
            >
              <FileSpreadsheet className="mr-2 h-4 w-4" />
              Export Excel
            </Button>
            <Button
              variant="outline"
              onClick={() => handleExportReport('pdf')}
              className="touch-target"
            >
              <FileText className="mr-2 h-4 w-4" />
              Export PDF
            </Button>
          </div>
        )}
      </div>

      {/* Report Filters */}
      <Card className="pos-card">
        <CardContent className="pt-6">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div className="space-y-2">
              <Label>Report Type</Label>
              <Select value={reportType} onValueChange={setReportType}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="daily_sales">Daily Sales</SelectItem>
                  <SelectItem value="monthly_profit">Monthly Profit</SelectItem>
                  <SelectItem value="category_analysis">Category Analysis</SelectItem>
                  <SelectItem value="customer_analysis">Customer Analysis</SelectItem>
                  <SelectItem value="inventory_report">Inventory Report</SelectItem>
                </SelectContent>
              </Select>
            </div>
            
            <div className="space-y-2">
              <Label>Date Range</Label>
              <Select value={dateRange} onValueChange={setDateRange}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="today">Today</SelectItem>
                  <SelectItem value="yesterday">Yesterday</SelectItem>
                  <SelectItem value="last_7_days">Last 7 Days</SelectItem>
                  <SelectItem value="last_30_days">Last 30 Days</SelectItem>
                  <SelectItem value="this_month">This Month</SelectItem>
                  <SelectItem value="last_month">Last Month</SelectItem>
                  <SelectItem value="this_year">This Year</SelectItem>
                  <SelectItem value="custom">Custom Range</SelectItem>
                </SelectContent>
              </Select>
            </div>
            
            <div className="space-y-2">
              <Label>Branch</Label>
              <Select value={selectedBranch} onValueChange={setSelectedBranch}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Branches</SelectItem>
                  <SelectItem value="main">Main Branch</SelectItem>
                  <SelectItem value="mall">Mall Branch</SelectItem>
                  <SelectItem value="downtown">Downtown Branch</SelectItem>
                </SelectContent>
              </Select>
            </div>
            
            <div className="space-y-2">
              <Label>Actions</Label>
              <Button variant="outline" className="w-full touch-target">
                <BarChart3 className="mr-2 h-4 w-4" />
                Generate Report
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-6 gap-4">
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="text-center">
              <DollarSign className="h-8 w-8 text-[#D4AF37] mx-auto mb-2" />
              <p className="text-2xl text-[#0D1B2A]">{formatCurrencyShort(reportSummary.totalSales)}</p>
              <p className="text-sm text-muted-foreground">Total Sales</p>
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="text-center">
              <FileText className="h-8 w-8 text-blue-600 mx-auto mb-2" />
              <p className="text-2xl text-[#0D1B2A]">{reportSummary.totalTransactions}</p>
              <p className="text-sm text-muted-foreground">Transactions</p>
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="text-center">
              <TrendingUp className="h-8 w-8 text-green-600 mx-auto mb-2" />
              <p className="text-2xl text-[#0D1B2A]">{formatCurrencyShort(reportSummary.averageTransaction)}</p>
              <p className="text-sm text-muted-foreground">Avg Transaction</p>
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="text-center">
              <Package className="h-8 w-8 text-purple-600 mx-auto mb-2" />
              <p className="text-2xl text-[#0D1B2A]">{reportSummary.topSellingCategory}</p>
              <p className="text-sm text-muted-foreground">Top Category</p>
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="text-center">
              <BarChart3 className="h-8 w-8 text-orange-600 mx-auto mb-2" />
              <p className="text-2xl text-[#0D1B2A]">{reportSummary.profitMargin}%</p>
              <p className="text-sm text-muted-foreground">Profit Margin</p>
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="text-center">
              <Users className="h-8 w-8 text-red-600 mx-auto mb-2" />
              <p className="text-2xl text-[#0D1B2A]">{reportSummary.returnRate}%</p>
              <p className="text-sm text-muted-foreground">Return Rate</p>
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs value={reportType} onValueChange={setReportType}>
        <TabsList className="grid w-full grid-cols-5">
          <TabsTrigger value="daily_sales">Daily Sales</TabsTrigger>
          <TabsTrigger value="monthly_profit">Monthly Profit</TabsTrigger>
          <TabsTrigger value="category_analysis">Categories</TabsTrigger>
          <TabsTrigger value="customer_analysis">Customers</TabsTrigger>
          <TabsTrigger value="inventory_report">Inventory</TabsTrigger>
        </TabsList>
        
        <TabsContent value="daily_sales" className="space-y-4">
          <Card className="pos-card">
            <CardHeader>
              <CardTitle>Daily Sales Trend</CardTitle>
              <CardDescription>Sales performance over the selected period</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="h-80">
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={dailySalesData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="date" tickFormatter={(value) => new Date(value).toLocaleDateString()} />
                    <YAxis tickFormatter={(value) => formatCurrencyShort(value)} />
                    <Tooltip content={<CustomTooltip />} />
                    <Bar dataKey="sales" fill="#D4AF37" />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
        
        <TabsContent value="monthly_profit" className="space-y-4">
          <Card className="pos-card">
            <CardHeader>
              <CardTitle>Monthly Profit Analysis</CardTitle>
              <CardDescription>Revenue, cost, and profit trends</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="h-80">
                <ResponsiveContainer width="100%" height="100%">
                  <LineChart data={monthlyProfitData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="month" />
                    <YAxis tickFormatter={(value) => formatCurrencyShort(value)} />
                    <Tooltip content={<CustomTooltip />} />
                    <Line type="monotone" dataKey="revenue" stroke="#0D1B2A" strokeWidth={2} />
                    <Line type="monotone" dataKey="cost" stroke="#DC3545" strokeWidth={2} />
                    <Line type="monotone" dataKey="profit" stroke="#28A745" strokeWidth={2} />
                  </LineChart>
                </ResponsiveContainer>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
        
        <TabsContent value="category_analysis" className="space-y-4">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card className="pos-card">
              <CardHeader>
                <CardTitle>Sales by Category</CardTitle>
                <CardDescription>Revenue distribution across product categories</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="h-80">
                  <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                      <Pie
                        data={categoryData}
                        cx="50%"
                        cy="50%"
                        labelLine={false}
                        label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                        outerRadius={80}
                        fill="#8884d8"
                        dataKey="value"
                      >
                        {categoryData.map((entry, index) => (
                          <Cell key={`cell-${index}`} fill={entry.color} />
                        ))}
                      </Pie>
                      <Tooltip formatter={(value) => formatCurrency(Number(value))} />
                    </PieChart>
                  </ResponsiveContainer>
                </div>
              </CardContent>
            </Card>
            
            <Card className="pos-card">
              <CardHeader>
                <CardTitle>Category Performance</CardTitle>
                <CardDescription>Detailed breakdown by category</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {categoryData.map((category, index) => (
                    <div key={index} className="flex items-center justify-between p-3 bg-muted/50 rounded-lg">
                      <div className="flex items-center gap-3">
                        <div 
                          className="w-4 h-4 rounded" 
                          style={{ backgroundColor: category.color }}
                        />
                        <div>
                          <p className="font-medium">{category.name}</p>
                          <p className="text-sm text-muted-foreground">{category.transactions} transactions</p>
                        </div>
                      </div>
                      <div className="text-right">
                        <p className="font-medium">{formatCurrency(category.value)}</p>
                        <p className="text-sm text-muted-foreground">
                          {((category.value / categoryData.reduce((sum, c) => sum + c.value, 0)) * 100).toFixed(1)}%
                        </p>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>
        
        <TabsContent value="customer_analysis" className="space-y-4">
          <Card className="pos-card">
            <CardHeader>
              <CardTitle>Top Customers</CardTitle>
              <CardDescription>Highest value customers by total purchases</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {topCustomersData.map((customer, index) => (
                  <div key={index} className="flex items-center justify-between p-4 border rounded-lg">
                    <div className="flex items-center gap-4">
                      <div className="w-10 h-10 bg-[#D4AF37] rounded-full flex items-center justify-center text-[#0D1B2A] font-semibold">
                        {index + 1}
                      </div>
                      <div>
                        <p className="font-medium">{customer.name}</p>
                        <p className="text-sm text-muted-foreground">{customer.visits} visits</p>
                      </div>
                    </div>
                    <div className="text-right">
                      <p className="font-medium">{formatCurrency(customer.purchases)}</p>
                      <p className="text-sm text-muted-foreground">
                        Last: {new Date(customer.lastVisit).toLocaleDateString()}
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
        
        <TabsContent value="inventory_report" className="space-y-4">
          <Card className="pos-card">
            <CardHeader>
              <CardTitle>Inventory Overview</CardTitle>
              <CardDescription>Stock levels and value by category</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {inventoryData.map((item, index) => (
                  <div key={index} className="p-4 border rounded-lg">
                    <div className="flex items-center justify-between mb-3">
                      <h3 className="font-medium">{item.category}</h3>
                      <p className="font-semibold">{formatCurrency(item.value)}</p>
                    </div>
                    <div className="grid grid-cols-3 gap-4 text-sm">
                      <div className="text-center">
                        <p className="text-green-600 font-medium">{item.inStock}</p>
                        <p className="text-muted-foreground">In Stock</p>
                      </div>
                      <div className="text-center">
                        <p className="text-yellow-600 font-medium">{item.lowStock}</p>
                        <p className="text-muted-foreground">Low Stock</p>
                      </div>
                      <div className="text-center">
                        <p className="text-red-600 font-medium">{item.outOfStock}</p>
                        <p className="text-muted-foreground">Out of Stock</p>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}