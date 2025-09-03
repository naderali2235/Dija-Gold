import React, { useState, useEffect } from 'react';
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
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from './ui/table';
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
  TrendingDown,
  DollarSign,
  Users,
  Package,
  BarChart3,
  PieChartIcon,
  FileSpreadsheet,
  Loader2,
  AlertTriangle,
  CheckCircle,
  Info,
} from 'lucide-react';
import { useAuth } from './AuthContext';
import { formatCurrency } from './utils/currency';
import { toast } from 'sonner';
import { cashDrawerApi } from '../services/api';
import {
  useDailySalesSummary,
  useCashReconciliation,
  useInventoryMovementReport,
  useProfitAnalysisReport,
  useCustomerAnalysisReport,
  useSupplierBalanceReport,
  useTaxReport,
  useTransactionLogReport,
  useReportTypes,
  useExportToExcel,
  useExportToPdf,
  useBranches,
  useTransactionTypes,
  useGetCashDrawerBalances,
  useGetOwnershipAlerts,
  useGetLowOwnershipProducts,
  useGetProductsWithOutstandingPayments
} from '../hooks/useApi';
import {
  DailySalesSummaryReport,
  CashReconciliationReport,
  InventoryMovementReport,
  ProfitAnalysisReport,
  CustomerAnalysisReport,
  SupplierBalanceReport,
  TaxReport,
  TransactionLogReport,
  ReportTypeDto,
  BranchDto,
  ExportReportRequest,
  CashDrawerBalance,
  ProductOwnershipDto,
  OwnershipAlertDto,
  FinancialTransactionTypeLookupDto
} from '../services/api';
import { EnumLookupDto } from '../types/lookups';
import { LookupHelper } from '../types/lookups';

const COLORS = ['#D4AF37', '#B8941F', '#0D1B2A', '#17A2B8', '#28A745', '#DC3545', '#6C757D'];

/**
 * API Parameter Conversion Utilities
 * 
 * The backend API functions have inconsistent parameter types:
 * - Some expect branchId as number, others as string
 * - Some expect dates as strings, others as timestamps
 * 
 * These utilities ensure proper type conversion for each API call.
 */
const convertBranchIdToNumber = (branchId: number): number => branchId;
const convertBranchIdToString = (branchId: number): string => branchId.toString();
const convertDateToString = (date: string): string => date;
const convertDateToNumber = (date: string): number => new Date(date).getTime();

// API parameter conversion helpers
const apiParams = {
  // Functions expecting number branchId and string dates
  dailySales: (branchId: number, date: string) => [convertBranchIdToNumber(branchId), convertDateToString(date)],
  cashReconciliation: (branchId: number, date: string) => [convertBranchIdToNumber(branchId), convertDateToString(date)],
  inventoryMovement: (branchId: number, fromDate: string, toDate: string) => [convertBranchIdToNumber(branchId), convertDateToString(fromDate), convertDateToString(toDate)],
  inventoryValuation: (branchId: number) => [convertBranchIdToNumber(branchId)],
  transactionLog: (branchId: number, date: string) => [convertBranchIdToNumber(branchId), convertDateToString(date)],
  
  // Functions expecting dates first, then branchId
  profitAnalysis: (branchId: number, fromDate: string, toDate: string) => [convertDateToString(fromDate), convertDateToString(toDate), convertBranchIdToNumber(branchId)],
  customerAnalysis: (branchId: number, fromDate: string, toDate: string) => [convertDateToString(fromDate), convertDateToString(toDate), convertBranchIdToNumber(branchId)],
  taxReport: (branchId: number, fromDate: string, toDate: string) => [convertDateToString(fromDate), convertDateToString(toDate), convertBranchIdToNumber(branchId)],
  
  // Functions with no branchId parameter
  supplierBalance: () => []
} as const;

export default function Reports() {
  const { user: currentUser, isManager } = useAuth();
  
  // State management
  const [activeTab, setActiveTab] = useState('sales');
  const [selectedBranchId, setSelectedBranchId] = useState<number>(1);
  // Helper function to format date as YYYY-MM-DD in local timezone
  const formatDateLocal = (date: Date) => {
    return date.getFullYear() + '-' + 
      String(date.getMonth() + 1).padStart(2, '0') + '-' + 
      String(date.getDate()).padStart(2, '0');
  };

  const [selectedDate, setSelectedDate] = useState(formatDateLocal(new Date()));
  const [dateRange, setDateRange] = useState({
    fromDate: formatDateLocal(new Date(Date.now() - 30 * 24 * 60 * 60 * 1000)),
    toDate: formatDateLocal(new Date()),
  });

  // API Hooks
  const { execute: fetchDailySalesSummary, loading: dailySalesLoading } = useDailySalesSummary();
  const { execute: fetchCashReconciliation, loading: cashRecLoading } = useCashReconciliation();
  const { execute: fetchInventoryMovementReport, loading: inventoryMovementLoading } = useInventoryMovementReport();
  const { execute: fetchProfitAnalysisReport, loading: profitAnalysisLoading } = useProfitAnalysisReport();
  const { execute: fetchCustomerAnalysisReport, loading: customerAnalysisLoading } = useCustomerAnalysisReport();
  const { execute: fetchSupplierBalanceReport, loading: supplierBalanceLoading } = useSupplierBalanceReport();

  const { execute: fetchTaxReport, loading: taxReportLoading } = useTaxReport();
  const { execute: fetchTransactionLogReport, loading: transactionLogLoading } = useTransactionLogReport();
  const { execute: fetchReportTypes, loading: reportTypesLoading } = useReportTypes();
  const { execute: exportToExcel, loading: excelExportLoading } = useExportToExcel();
  const { execute: exportToPdf, loading: pdfExportLoading } = useExportToPdf();
  const { execute: fetchBranches } = useBranches();
  const { execute: fetchTransactionTypes, loading: transactionTypesLoading } = useTransactionTypes();
  const { execute: fetchCashDrawerBalances, loading: cashDrawerBalancesLoading } = useGetCashDrawerBalances();
  const { execute: fetchOwnershipAlerts } = useGetOwnershipAlerts();
  const { execute: fetchLowOwnershipProducts } = useGetLowOwnershipProducts();
  const { execute: fetchOutstandingPayments } = useGetProductsWithOutstandingPayments();

  // Local state for report data
  const [dailySalesReport, setDailySalesReport] = useState<DailySalesSummaryReport | null>(null);
  const [cashReconciliationReport, setCashReconciliationReport] = useState<CashReconciliationReport | null>(null);
  const [inventoryMovementReport, setInventoryMovementReport] = useState<InventoryMovementReport | null>(null);
  const [profitAnalysisReport, setProfitAnalysisReport] = useState<ProfitAnalysisReport | null>(null);
  const [customerAnalysisReport, setCustomerAnalysisReport] = useState<CustomerAnalysisReport | null>(null);
  const [supplierBalanceReport, setSupplierBalanceReport] = useState<SupplierBalanceReport | null>(null);

  const [taxReport, setTaxReport] = useState<TaxReport | null>(null);
  const [transactionLogReport, setTransactionLogReport] = useState<TransactionLogReport | null>(null);
  const [cashDrawerBalances, setCashDrawerBalances] = useState<CashDrawerBalance[]>([]);
  const [reportTypes, setReportTypes] = useState<ReportTypeDto[]>([]);
  const [branches, setBranches] = useState<BranchDto[]>([]);
  const [transactionTypes, setTransactionTypes] = useState<FinancialTransactionTypeLookupDto[]>([]);
  
  // Ownership state
  const [ownershipData, setOwnershipData] = useState<ProductOwnershipDto[]>([]);
  const [ownershipAlerts, setOwnershipAlerts] = useState<OwnershipAlertDto[]>([]);
  const [lowOwnershipProducts, setLowOwnershipProducts] = useState<ProductOwnershipDto[]>([]);
  const [outstandingPayments, setOutstandingPayments] = useState<ProductOwnershipDto[]>([]);
  const [ownershipLoading, setOwnershipLoading] = useState(false);

  // Load initial data
  useEffect(() => {
    const loadInitialData = async () => {
      try {
        const [branchesResult, reportTypesResult, transactionTypesResult] = await Promise.all([
          fetchBranches(),
          fetchReportTypes(),
          fetchTransactionTypes()
        ]);
        setBranches(branchesResult.items || branchesResult);
        setReportTypes(reportTypesResult);
        setTransactionTypes(transactionTypesResult);
      } catch (error) {
        console.error('Failed to load initial data:', error);
      }
    };
    
    loadInitialData();
  }, [fetchBranches, fetchReportTypes, fetchTransactionTypes]);

  // Load ownership data
  useEffect(() => {
    const loadOwnershipData = async () => {
      if (!selectedBranchId) return;
      
      try {
        setOwnershipLoading(true);
        const [alerts, lowOwnership, outstanding] = await Promise.all([
          fetchOwnershipAlerts(selectedBranchId),
          fetchLowOwnershipProducts(0.5),
          fetchOutstandingPayments()
        ]);
        
        setOwnershipAlerts(alerts);
        setLowOwnershipProducts(lowOwnership);
        setOutstandingPayments(outstanding);
      } catch (error) {
        console.error('Failed to load ownership data:', error);
        toast.error('Failed to load ownership data');
      } finally {
        setOwnershipLoading(false);
      }
    };
    
    loadOwnershipData();
  }, [selectedBranchId]);

  // Ownership statistics helper functions
  const getOwnershipStatistics = () => {
    const totalProducts = ownershipData.length;
    const highOwnership = ownershipData.filter(item => item.ownershipPercentage >= 80).length;
    const mediumOwnership = ownershipData.filter(item => item.ownershipPercentage >= 50 && item.ownershipPercentage < 80).length;
    const lowOwnership = ownershipData.filter(item => item.ownershipPercentage < 50).length;
    const totalOutstanding = outstandingPayments.reduce((sum, item) => sum + item.outstandingAmount, 0);
    
    return {
      totalProducts,
      highOwnership,
      mediumOwnership,
      lowOwnership,
      totalOutstanding,
      averageOwnership: totalProducts > 0 ? ownershipData.reduce((sum, item) => sum + item.ownershipPercentage, 0) / totalProducts : 0
    };
  };

  const getOwnershipChartData = () => {
    const stats = getOwnershipStatistics();
    return [
      { name: 'High (≥80%)', value: stats.highOwnership, color: '#28A745' },
      { name: 'Medium (50-79%)', value: stats.mediumOwnership, color: '#FFC107' },
      { name: 'Low (<50%)', value: stats.lowOwnership, color: '#DC3545' }
    ];
  };

  const getOwnershipTrendData = () => {
    // Mock trend data - in real implementation, this would come from historical data
    return [
      { date: '2024-01', ownership: 75 },
      { date: '2024-02', ownership: 78 },
      { date: '2024-03', ownership: 82 },
      { date: '2024-04', ownership: 79 },
      { date: '2024-05', ownership: 85 },
      { date: '2024-06', ownership: 88 }
    ];
  };

  // Load reports based on active tab
  useEffect(() => {
    const loadReportData = async () => {
      try {
        switch (activeTab) {
          case 'sales':
            const dailySales = await fetchDailySalesSummary(...apiParams.dailySales(selectedBranchId, selectedDate) as [number, string]);
            setDailySalesReport(dailySales);
            break;
          case 'cash':
            const cashRec = await fetchCashReconciliation(...apiParams.cashReconciliation(selectedBranchId, selectedDate) as [number, string]);
            setCashReconciliationReport(cashRec);
            break;
          case 'cashdrawer':
            try {
              const balances = await fetchCashDrawerBalances(selectedBranchId, dateRange.fromDate, dateRange.toDate);
              setCashDrawerBalances(balances);
            } catch (error) {
              console.error('Failed to load cash drawer balances:', error);
              toast.error('Failed to load cash drawer balances');
            }
            break;
          case 'inventory':
            const invMovement = await fetchInventoryMovementReport(selectedBranchId, dateRange.fromDate, dateRange.toDate);
            setInventoryMovementReport(invMovement);
            break;
          case 'profit':
            const profit = await fetchProfitAnalysisReport(...apiParams.profitAnalysis(selectedBranchId, dateRange.fromDate, dateRange.toDate) as [string, string, number]);
            setProfitAnalysisReport(profit);
            break;
          case 'customers':
            const customers = await fetchCustomerAnalysisReport(...apiParams.customerAnalysis(selectedBranchId, dateRange.fromDate, dateRange.toDate) as [string, string, number]);
            setCustomerAnalysisReport(customers);
            break;
          case 'suppliers':
            const suppliers = await fetchSupplierBalanceReport();
            setSupplierBalanceReport(suppliers);
            break;
          case 'tax':
            const tax = await fetchTaxReport(...apiParams.taxReport(selectedBranchId, dateRange.fromDate, dateRange.toDate) as [string, string, number]);
            setTaxReport(tax);
            break;
          case 'transactions':
            const transactions = await fetchTransactionLogReport(...apiParams.transactionLog(selectedBranchId, selectedDate) as [number, string]);
            setTransactionLogReport(transactions);
            break;
        }
      } catch (error) {
        console.error('Failed to load report data:', error);
        toast.error('Failed to load report data');
      }
    };

    loadReportData();
  }, [activeTab, selectedBranchId, selectedDate, dateRange]);

  // Handlers
  const handleExportToExcel = async (reportData: any, reportName: string, reportType: string) => {
    try {
      if (!reportData) {
        toast.error('No report data available for export');
        return;
      }

      const request: ExportReportRequest = {
        reportType: reportType,
        reportName: reportName,
        reportDataJson: JSON.stringify(reportData),
      };
      
      const blob = await exportToExcel(request);
      
      if (!blob || blob.size === 0) {
        toast.error('Received empty file from server');
        return;
      }
      
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.style.display = 'none';
      a.href = url;
      a.download = `${reportName}.xlsx`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
      
      toast.success('Report exported to Excel successfully');
    } catch (error) {
      console.error('Excel export error:', error);
      const errorMessage = error instanceof Error ? error.message : 'Failed to export to Excel';
      toast.error(`Failed to export to Excel: ${errorMessage}`);
    }
  };

  const handleExportToPdf = async (reportData: any, reportName: string, reportType: string) => {
    try {
      if (!reportData) {
        toast.error('No report data available for export');
        return;
      }

      const request: ExportReportRequest = {
        reportType: reportType,
        reportName: reportName,
        reportDataJson: JSON.stringify(reportData),
      };
      
      const blob = await exportToPdf(request);
      
      if (!blob || blob.size === 0) {
        toast.error('Received empty file from server');
        return;
      }
      
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.style.display = 'none';
      a.href = url;
      a.download = `${reportName}.pdf`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
      
      toast.success('Report exported to PDF successfully');
    } catch (error) {
      console.error('PDF export error:', error);
      const errorMessage = error instanceof Error ? error.message : 'Failed to export to PDF';
      toast.error(`Failed to export to PDF: ${errorMessage}`);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const getStatusColor = (status: string | number) => {
    // Convert status to string and handle both enum values and string values
    const statusString = typeof status === 'string' ? status : String(status);
    
    switch (statusString.toLowerCase()) {
      case 'completed':
      case '2': // TransactionStatus.Completed enum value
        return 'text-green-600';
      case 'pending':
      case '1': // TransactionStatus.Pending enum value
        return 'text-yellow-600';
      case 'cancelled':
      case '3': // TransactionStatus.Cancelled enum value
        return 'text-red-600';
      case 'refunded':
      case '4': // TransactionStatus.Refunded enum value
        return 'text-orange-600';
      case 'voided':
      case '5': // TransactionStatus.Voided enum value
        return 'text-gray-600';
      default: 
        return 'text-gray-600';
    }
  };

  const getTransactionTypeName = (type: string | number) => {
    // If it's already a string, it might be the display name
    if (typeof type === 'string') {
      // Check if it's a numeric string (enum value)
      const numericValue = parseInt(type);
      if (!isNaN(numericValue)) {
        const transactionType = transactionTypes.find(t => t.id === numericValue);
        return transactionType ? transactionType.name : type;
      }
      // If it's already a display name, return as is
      return type;
    }
    
    // If it's a number, find the display name
    if (typeof type === 'number') {
      const transactionType = transactionTypes.find(t => t.id === type);
      return transactionType ? transactionType.name : String(type);
    }
    
    return String(type);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <h1 className="text-3xl text-[#0D1B2A]">Business Reports</h1>
        <div className="flex space-x-2">
          <Select value={selectedBranchId.toString()} onValueChange={(value: string) => setSelectedBranchId(parseInt(value))}>
            <SelectTrigger className="w-48">
              <SelectValue placeholder="Select Branch" />
            </SelectTrigger>
            <SelectContent className="bg-white border-gray-200 shadow-lg">
              {branches.map((branch) => (
                <SelectItem key={branch.id} value={branch.id.toString()} className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">
                  {branch.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      {/* Date Controls */}
      <Card className="pos-card">
        <CardContent className="pt-6">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div className="space-y-2">
              <Label>Single Date</Label>
              <Input
                type="date"
                value={selectedDate}
                onChange={(e) => setSelectedDate(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label>From Date</Label>
              <Input
                type="date"
                value={dateRange.fromDate}
                onChange={(e) => setDateRange({...dateRange, fromDate: e.target.value})}
              />
            </div>
            <div className="space-y-2">
              <Label>To Date</Label>
              <Input
                type="date"
                value={dateRange.toDate}
                onChange={(e) => setDateRange({...dateRange, toDate: e.target.value})}
              />
            </div>
            <div className="flex items-end">
              <Button 
                variant="outline" 
                onClick={() => {
                          const today = formatDateLocal(new Date());
        const thirtyDaysAgo = formatDateLocal(new Date(Date.now() - 30 * 24 * 60 * 60 * 1000));
                  setSelectedDate(today);
                  setDateRange({ fromDate: thirtyDaysAgo, toDate: today });
                }}
              >
                Reset to Last 30 Days
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
        <TabsList className="grid w-full grid-cols-4 lg:grid-cols-10">
          <TabsTrigger value="sales">Sales</TabsTrigger>
          <TabsTrigger value="cash">Cash</TabsTrigger>
          <TabsTrigger value="cashdrawer">Cash Drawer</TabsTrigger>
          <TabsTrigger value="inventory">Inventory</TabsTrigger>
          <TabsTrigger value="profit">Profit</TabsTrigger>
          <TabsTrigger value="customers">Customers</TabsTrigger>
          <TabsTrigger value="suppliers">Suppliers</TabsTrigger>
          <TabsTrigger value="tax">Tax</TabsTrigger>
          <TabsTrigger value="transactions">Transactions</TabsTrigger>
          <TabsTrigger value="ownership">Ownership</TabsTrigger>
        </TabsList>

        {/* Daily Sales Summary Tab */}
        <TabsContent value="sales" className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-2xl font-semibold">Daily Sales Summary</h2>
            {dailySalesReport && (
              <div className="flex space-x-2">
                <Button
                  onClick={() => handleExportToExcel(dailySalesReport, `Daily_Sales_${selectedDate}`, 'daily-sales-summary')}
                  disabled={excelExportLoading}
                  variant="outline"
                  size="sm"
                  className="touch-target hover:bg-[#F4E9B1] transition-colors"
                >
                  <FileSpreadsheet className="mr-2 h-4 w-4" />
                  Export Excel
                </Button>
                <Button
                  onClick={() => handleExportToPdf(dailySalesReport, `Daily_Sales_${selectedDate}`, 'daily-sales-summary')}
                  disabled={pdfExportLoading}
                  variant="outline"
                  size="sm"
                  className="touch-target hover:bg-[#F4E9B1] transition-colors"
                >
                  <FileText className="mr-2 h-4 w-4" />
                  Export PDF
                </Button>
              </div>
            )}
          </div>

          {dailySalesLoading && (
            <div className="flex justify-center py-12">
              <Loader2 className="h-8 w-8 animate-spin" />
            </div>
          )}

          {dailySalesReport && !dailySalesLoading && (
            <>
              {/* Summary Cards */}
              <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Total Sales</p>
                        <p className="text-3xl font-bold text-green-600">
                          {formatCurrency(dailySalesReport.totalSales)}
                        </p>
                      </div>
                      <DollarSign className="h-8 w-8 text-green-500" />
                    </div>
                  </CardContent>
                </Card>



                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Net Sales</p>
                        <p className="text-3xl font-bold text-blue-600">
                          {formatCurrency(dailySalesReport.netSales)}
                        </p>
                      </div>
                      <BarChart3 className="h-8 w-8 text-blue-500" />
                    </div>
                  </CardContent>
                </Card>

                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Transactions</p>
                        <p className="text-3xl font-bold">{dailySalesReport.transactionCount}</p>
                      </div>
                      <FileText className="h-8 w-8 text-muted-foreground" />
                    </div>
                  </CardContent>
                </Card>
              </div>

              {/* Charts */}
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* Category Breakdown Chart */}
                {dailySalesReport.categoryBreakdown.length > 0 && (
                  <Card className="pos-card">
                    <CardHeader>
                      <CardTitle>Sales by Category</CardTitle>
                    </CardHeader>
                    <CardContent>
                      <ResponsiveContainer width="100%" height={300}>
                        <PieChart>
                          <Pie
                            data={dailySalesReport.categoryBreakdown.map((item, index) => ({
                              name: item.category,
                              value: item.totalSales,
                              fill: COLORS[index % COLORS.length]
                            }))}
                            cx="50%"
                            cy="50%"
                            labelLine={false}
                            label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                            outerRadius={80}
                            fill="#8884d8"
                            dataKey="value"
                          >
                            {dailySalesReport.categoryBreakdown.map((entry, index) => (
                              <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                            ))}
                          </Pie>
                          <Tooltip formatter={(value: any) => formatCurrency(value)} />
                        </PieChart>
                      </ResponsiveContainer>
                    </CardContent>
                  </Card>
                )}

                {/* Payment Method Breakdown */}
                {dailySalesReport.paymentMethodBreakdown.length > 0 && (
                  <Card className="pos-card">
                    <CardHeader>
                      <CardTitle>Payment Methods</CardTitle>
                    </CardHeader>
                    <CardContent>
                      <div className="space-y-4">
                        {dailySalesReport.paymentMethodBreakdown.map((payment, index) => (
                          <div key={payment.paymentMethod} className="flex justify-between items-center">
                            <div className="flex items-center space-x-3">
                              <div 
                                className="w-4 h-4 rounded"
                                style={{ backgroundColor: COLORS[index % COLORS.length] }}
                              />
                              <span className="font-medium">{payment.paymentMethod}</span>
                            </div>
                            <div className="text-right">
                              <p className="font-semibold">{formatCurrency(payment.amount)}</p>
                              <p className="text-sm text-muted-foreground">
                                {payment.transactionCount} transactions
                              </p>
                            </div>
                          </div>
                        ))}
                      </div>
                    </CardContent>
                  </Card>
                )}
              </div>
            </>
          )}
        </TabsContent>

        {/* Cash Reconciliation Tab */}
        <TabsContent value="cash" className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-2xl font-semibold">Cash Reconciliation</h2>
            {cashReconciliationReport && (
              <div className="flex space-x-2">
                <Button
                  onClick={() => handleExportToExcel(cashReconciliationReport, `Cash_Reconciliation_${selectedDate}`, 'cash-reconciliation')}
                  disabled={excelExportLoading}
                  variant="outline"
                  size="sm"
                  className="touch-target hover:bg-[#F4E9B1] transition-colors"
                >
                  <FileSpreadsheet className="mr-2 h-4 w-4" />
                  Export Excel
                </Button>
                <Button
                  onClick={() => handleExportToPdf(cashReconciliationReport, `Cash_Reconciliation_${selectedDate}`, 'cash-reconciliation')}
                  disabled={pdfExportLoading}
                  variant="outline"
                  size="sm"
                  className="touch-target hover:bg-[#F4E9B1] transition-colors"
                >
                  <FileText className="mr-2 h-4 w-4" />
                  Export PDF
                </Button>
              </div>
            )}
          </div>

          {cashRecLoading && (
            <div className="flex justify-center py-12">
              <Loader2 className="h-8 w-8 animate-spin" />
            </div>
          )}

          {cashReconciliationReport && !cashRecLoading && (
            <Card className="pos-card">
              <CardHeader>
                <CardTitle>Cash Flow Summary</CardTitle>
                <CardDescription>
                  {branches.find(b => b.id === cashReconciliationReport.branchId)?.name || 'Unknown Branch'} - {formatDate(cashReconciliationReport.reportDate)}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-2 gap-6">
                  <div className="space-y-4">
                    <div className="flex justify-between">
                      <span>Opening Balance:</span>
                      <span className="font-semibold">{formatCurrency(cashReconciliationReport.openingBalance)}</span>
                    </div>
                    <div className="flex justify-between">
                      <span>Cash Sales:</span>
                      <span className="font-semibold text-green-600">{formatCurrency(cashReconciliationReport.cashSales)}</span>
                    </div>
                    <div className="flex justify-between">
                      <span>Cash Repairs:</span>
                      <span className="font-semibold text-blue-600">{formatCurrency(cashReconciliationReport.cashRepairs)}</span>
                    </div>

                    <div className="flex justify-between border-t pt-2">
                      <span className="font-semibold">Expected Closing Balance:</span>
                      <span className="font-semibold">{formatCurrency(cashReconciliationReport.expectedClosingBalance)}</span>
                    </div>
                  </div>
                  <div className="space-y-4">
                    <div className="flex justify-between">
                      <span className="font-semibold">Actual Closing Balance:</span>
                      <span className="font-semibold">{formatCurrency(cashReconciliationReport.actualClosingBalance)}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="font-semibold">Cash Over/Short:</span>
                      <span className={`font-semibold ${cashReconciliationReport.cashOverShort >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                        {formatCurrency(cashReconciliationReport.cashOverShort)}
                      </span>
                    </div>
                    {cashReconciliationReport.cashOverShort !== 0 && (
                      <div className="p-3 rounded bg-yellow-50 border border-yellow-200">
                        <div className="flex items-center">
                          <AlertTriangle className="h-4 w-4 text-yellow-500 mr-2" />
                          <span className="text-sm">
                            {cashReconciliationReport.cashOverShort > 0 
                              ? 'Cash over - investigate excess' 
                              : 'Cash short - investigate shortage'}
                          </span>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          )}
        </TabsContent>

        {/* Cash Drawer Balances Tab */}
        <TabsContent value="cashdrawer" className="space-y-4 max-w-full overflow-hidden">
          <div className="flex justify-between items-center">
            <h2 className="text-2xl font-semibold">Cash Drawer Balances</h2>
            {cashDrawerBalances.length > 0 && (
              <div className="flex space-x-2">
                <Button
                  onClick={() => handleExportToExcel(cashDrawerBalances, `Cash_Drawer_Balances_${dateRange.fromDate}_to_${dateRange.toDate}`, 'cash-drawer-balances')}
                  disabled={excelExportLoading}
                  variant="outline"
                  size="sm"
                  className="touch-target hover:bg-[#F4E9B1] transition-colors"
                >
                  <FileSpreadsheet className="mr-2 h-4 w-4" />
                  Export Excel
                </Button>
                <Button
                  onClick={() => handleExportToPdf(cashDrawerBalances, `Cash_Drawer_Balances_${dateRange.fromDate}_to_${dateRange.toDate}`, 'cash-drawer-balances')}
                  disabled={pdfExportLoading}
                  variant="outline"
                  size="sm"
                  className="touch-target hover:bg-[#F4E9B1] transition-colors"
                >
                  <FileText className="mr-2 h-4 w-4" />
                  Export PDF
                </Button>
              </div>
            )}
          </div>

          {cashDrawerBalancesLoading && (
            <div className="flex justify-center py-12">
              <Loader2 className="h-8 w-8 animate-spin" />
            </div>
          )}

          {cashDrawerBalances.length > 0 && !cashDrawerBalancesLoading && (
            <>
              {/* Summary Cards */}
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 lg:gap-6 max-w-full">
                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Total Days</p>
                        <p className="text-3xl font-bold">
                          {cashDrawerBalances.length}
                        </p>
                      </div>
                      <Calendar className="h-8 w-8 text-blue-500" />
                    </div>
                  </CardContent>
                </Card>

                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Total Settled</p>
                        <p className="text-3xl font-bold text-green-600">
                          {formatCurrency(cashDrawerBalances.reduce((sum, balance) => sum + (balance.settledAmount || 0), 0))}
                        </p>
                      </div>
                      <CheckCircle className="h-8 w-8 text-green-500" />
                    </div>
                  </CardContent>
                </Card>

                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Total Carried Forward</p>
                        <p className="text-3xl font-bold text-blue-600">
                          {formatCurrency(cashDrawerBalances.reduce((sum, balance) => sum + (balance.carriedForwardAmount || 0), 0))}
                        </p>
                      </div>
                      <TrendingUp className="h-8 w-8 text-blue-500" />
                    </div>
                  </CardContent>
                </Card>

                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Total Over/Short</p>
                        <p className="text-3xl font-bold text-orange-600">
                          {formatCurrency(cashDrawerBalances.reduce((sum, balance) => sum + (balance.cashOverShort || 0), 0))}
                        </p>
                      </div>
                      <AlertTriangle className="h-8 w-8 text-orange-500" />
                    </div>
                  </CardContent>
                </Card>
              </div>

              {/* Cash Drawer Balances Table */}
              <Card className="pos-card">
                <CardHeader>
                  <CardTitle>Cash Drawer Balance History</CardTitle>
                  <CardDescription>
                    Daily cash drawer balances from {formatDate(dateRange.fromDate)} to {formatDate(dateRange.toDate)}
                  </CardDescription>
                </CardHeader>
                <CardContent className="overflow-x-auto">
                  <div className="min-w-full">
                    <Table>
                      <TableHeader>
                        <TableRow>
                          <TableHead className="whitespace-nowrap">Date</TableHead>
                          <TableHead className="whitespace-nowrap">Status</TableHead>
                          <TableHead className="whitespace-nowrap">Opening</TableHead>
                          <TableHead className="whitespace-nowrap">Expected</TableHead>
                          <TableHead className="whitespace-nowrap">Actual</TableHead>
                          <TableHead className="whitespace-nowrap">Over/Short</TableHead>
                          <TableHead className="whitespace-nowrap">Settled</TableHead>
                          <TableHead className="whitespace-nowrap">Carried</TableHead>
                          <TableHead className="whitespace-nowrap">Notes</TableHead>
                        </TableRow>
                      </TableHeader>
                      <TableBody>
                        {cashDrawerBalances.map((balance) => (
                          <TableRow key={balance.id}>
                            <TableCell className="font-medium whitespace-nowrap">
                              {formatDate(balance.balanceDate)}
                            </TableCell>
                            <TableCell className="whitespace-nowrap">
                              <Badge variant={
                                balance.status === 1 ? "default" : 
                                balance.status === 2 ? "secondary" : 
                                balance.status === 3 ? "destructive" : "outline"
                              }>
                                {balance.status === 1 ? 'Open' : 
                                 balance.status === 2 ? 'Closed' : 
                                 balance.status === 3 ? 'Pending' : 'Unknown'}
                              </Badge>
                            </TableCell>
                            <TableCell className="font-semibold whitespace-nowrap">
                              {formatCurrency(balance.openingBalance)}
                            </TableCell>
                            <TableCell className="font-semibold whitespace-nowrap">
                              {formatCurrency(balance.expectedClosingBalance)}
                            </TableCell>
                            <TableCell className="font-semibold whitespace-nowrap">
                              {balance.actualClosingBalance ? formatCurrency(balance.actualClosingBalance) : '-'}
                            </TableCell>
                            <TableCell className={`font-semibold whitespace-nowrap ${balance.cashOverShort && balance.cashOverShort !== 0 ? 'text-red-600' : 'text-green-600'}`}>
                              {balance.cashOverShort ? formatCurrency(balance.cashOverShort) : '-'}
                            </TableCell>
                            <TableCell className="font-semibold text-orange-600 whitespace-nowrap">
                              {balance.settledAmount ? formatCurrency(balance.settledAmount) : '-'}
                            </TableCell>
                            <TableCell className="font-semibold text-blue-600 whitespace-nowrap">
                              {balance.carriedForwardAmount ? formatCurrency(balance.carriedForwardAmount) : '-'}
                            </TableCell>
                            <TableCell className="max-w-32">
                              <div className="truncate" title={balance.notes || ''}>
                                {balance.notes || '-'}
                              </div>
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </div>
                </CardContent>
              </Card>

              {/* Cash Flow Chart */}
              {cashDrawerBalances.length > 0 && (
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 max-w-full">
                  <Card className="pos-card">
                    <CardHeader>
                      <CardTitle>Cash Flow Over Time</CardTitle>
                    </CardHeader>
                    <CardContent>
                      <ResponsiveContainer width="100%" height={300}>
                        <LineChart data={cashDrawerBalances.map(balance => ({
                          date: formatDate(balance.balanceDate),
                          opening: balance.openingBalance,
                          expected: balance.expectedClosingBalance,
                          actual: balance.actualClosingBalance || 0
                        }))}>
                          <CartesianGrid strokeDasharray="3 3" />
                          <XAxis dataKey="date" angle={-45} textAnchor="end" height={80} />
                          <YAxis />
                          <Tooltip formatter={(value: any) => formatCurrency(value)} />
                          <Line type="monotone" dataKey="opening" stroke="#3B82F6" name="Opening Balance" />
                          <Line type="monotone" dataKey="expected" stroke="#10B981" name="Expected Closing" />
                          <Line type="monotone" dataKey="actual" stroke="#EF4444" name="Actual Closing" />
                        </LineChart>
                      </ResponsiveContainer>
                    </CardContent>
                  </Card>

                  <Card className="pos-card">
                    <CardHeader>
                      <CardTitle>Settlement vs Carry Forward</CardTitle>
                    </CardHeader>
                    <CardContent>
                      <ResponsiveContainer width="100%" height={300}>
                        <BarChart data={cashDrawerBalances.map(balance => ({
                          date: formatDate(balance.balanceDate),
                          settled: balance.settledAmount || 0,
                          carriedForward: balance.carriedForwardAmount || 0
                        }))}>
                          <CartesianGrid strokeDasharray="3 3" />
                          <XAxis dataKey="date" angle={-45} textAnchor="end" height={80} />
                          <YAxis />
                          <Tooltip formatter={(value: any) => formatCurrency(value)} />
                          <Bar dataKey="settled" fill="#F59E0B" name="Settled Amount" />
                          <Bar dataKey="carriedForward" fill="#3B82F6" name="Carried Forward" />
                        </BarChart>
                      </ResponsiveContainer>
                    </CardContent>
                  </Card>
                </div>
              )}
            </>
          )}

          {cashDrawerBalances.length === 0 && !cashDrawerBalancesLoading && (
            <Card className="pos-card">
              <CardContent className="pt-6">
                <div className="text-center py-8 text-muted-foreground">
                  <Info className="h-12 w-12 mx-auto mb-4 text-gray-400" />
                  <p>No cash drawer balance data found for the selected period</p>
                  <p className="text-sm mt-2">Try adjusting the date range or check if cash drawer operations were performed</p>
                </div>
              </CardContent>
            </Card>
          )}
        </TabsContent>

        {/* Customer Analysis Tab */}
        <TabsContent value="customers" className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-2xl font-semibold">Customer Analysis</h2>
            {customerAnalysisReport && (
              <div className="flex space-x-2">
                <Button
                  onClick={() => handleExportToExcel(customerAnalysisReport, `Customer_Analysis_${dateRange.fromDate}_to_${dateRange.toDate}`, 'customer-analysis')}
                  disabled={excelExportLoading}
                  variant="outline"
                  size="sm"
                  className="touch-target hover:bg-[#F4E9B1] transition-colors"
                >
                  <FileSpreadsheet className="mr-2 h-4 w-4" />
                  Excel
                </Button>
                <Button
                  onClick={() => handleExportToPdf(customerAnalysisReport, `Customer_Analysis_${dateRange.fromDate}_to_${dateRange.toDate}`, 'customer-analysis')}
                  disabled={pdfExportLoading}
                  variant="outline"
                  size="sm"
                >
                  <FileText className="mr-2 h-4 w-4" />
                  PDF
                </Button>
              </div>
            )}
          </div>

          {customerAnalysisLoading && (
            <div className="flex justify-center py-12">
              <Loader2 className="h-8 w-8 animate-spin" />
            </div>
          )}

          {customerAnalysisReport && !customerAnalysisLoading && (
            <>
              {/* Summary Cards */}
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Total Customer Sales</p>
                        <p className="text-3xl font-bold text-green-600">
                          {formatCurrency(customerAnalysisReport.totalCustomerSales)}
                        </p>
                      </div>
                      <DollarSign className="h-8 w-8 text-green-500" />
                    </div>
                  </CardContent>
                </Card>

                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Unique Customers</p>
                        <p className="text-3xl font-bold">{customerAnalysisReport.totalUniqueCustomers}</p>
                      </div>
                      <Users className="h-8 w-8 text-blue-500" />
                    </div>
                  </CardContent>
                </Card>

                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Average per Customer</p>
                        <p className="text-3xl font-bold text-purple-600">
                          {formatCurrency(customerAnalysisReport.totalCustomerSales / Math.max(customerAnalysisReport.totalUniqueCustomers, 1))}
                        </p>
                      </div>
                      <TrendingUp className="h-8 w-8 text-purple-500" />
                    </div>
                  </CardContent>
                </Card>
              </div>

              {/* Top Customers Table */}
              <Card className="pos-card">
                <CardHeader>
                  <CardTitle>Top Customers</CardTitle>
                </CardHeader>
                <CardContent>
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Customer Name</TableHead>
                        <TableHead>Total Purchases</TableHead>
                        <TableHead>Transactions</TableHead>
                        <TableHead>Avg. Transaction</TableHead>
                        <TableHead>Last Purchase</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {customerAnalysisReport.topCustomers.map((customer) => (
                        <TableRow key={customer.customerId}>
                          <TableCell className="font-medium">{customer.customerName}</TableCell>
                          <TableCell>{formatCurrency(customer.totalPurchases)}</TableCell>
                          <TableCell>{customer.transactionCount}</TableCell>
                          <TableCell>{formatCurrency(customer.averageTransactionValue)}</TableCell>
                          <TableCell>{formatDate(customer.lastPurchaseDate)}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </CardContent>
              </Card>
            </>
          )}
        </TabsContent>

        {/* Transaction Log Tab */}
        <TabsContent value="transactions" className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-2xl font-semibold">Transaction Log</h2>
            {transactionLogReport && (
              <div className="flex space-x-2">
                <Button
                  onClick={() => handleExportToExcel(transactionLogReport, `Transaction_Log_${selectedDate}`, 'transaction-log')}
                  disabled={excelExportLoading}
                  variant="outline"
                  size="sm"
                  className="touch-target hover:bg-[#F4E9B1] transition-colors"
                >
                  <FileSpreadsheet className="mr-2 h-4 w-4" />
                  Export Excel
                </Button>
                <Button
                  onClick={() => handleExportToPdf(transactionLogReport, `Transaction_Log_${selectedDate}`, 'transaction-log')}
                  disabled={pdfExportLoading}
                  variant="outline"
                  size="sm"
                  className="touch-target hover:bg-[#F4E9B1] transition-colors"
                >
                  <FileText className="mr-2 h-4 w-4" />
                  Export PDF
                </Button>
              </div>
            )}
          </div>

          {transactionLogLoading && (
            <div className="flex justify-center py-12">
              <Loader2 className="h-8 w-8 animate-spin" />
            </div>
          )}

          {transactionLogReport && !transactionLogLoading && (
            <Card className="pos-card">
              <CardHeader>
                <CardTitle>
                  {branches.find(b => b.id === transactionLogReport.branchId)?.name || 'Unknown Branch'} - {formatDate(transactionLogReport.reportDate)}
                </CardTitle>
                <CardDescription>
                  All transactions for the selected date
                </CardDescription>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Transaction #</TableHead>
                      <TableHead>Date & Time</TableHead>
                      <TableHead>Type</TableHead>
                      <TableHead>Customer</TableHead>
                      <TableHead>Cashier</TableHead>
                      <TableHead>Amount</TableHead>
                      <TableHead>Status</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {transactionLogReport.transactions.map((transaction) => (
                      <TableRow key={transaction.transactionNumber}>
                        <TableCell className="font-medium">{transaction.transactionNumber}</TableCell>
                        <TableCell>{new Date(transaction.transactionDate).toLocaleString()}</TableCell>
                        <TableCell>
                          <Badge variant="outline">
                            {getTransactionTypeName(transaction.transactionType)}
                          </Badge>
                        </TableCell>
                        <TableCell>{transaction.customerName || 'Walk-in'}</TableCell>
                        <TableCell>{transaction.cashierName}</TableCell>
                        <TableCell>{formatCurrency(transaction.totalAmount)}</TableCell>
                        <TableCell>
                          <span className={getStatusColor(transaction.status)}>
                            {typeof transaction.status === 'string' ? transaction.status : 
                             transaction.status === 1 ? 'Pending' :
                             transaction.status === 2 ? 'Completed' :
                             transaction.status === 3 ? 'Cancelled' :
                             transaction.status === 4 ? 'Refunded' :
                             transaction.status === 5 ? 'Voided' : 
                             String(transaction.status)}
                          </span>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
                {transactionLogReport.transactions.length === 0 && (
                  <div className="text-center py-8 text-muted-foreground">
                    No transactions found for the selected date
                  </div>
                )}
              </CardContent>
            </Card>
          )}
        </TabsContent>

        {/* Inventory Movement Tab */}
        <TabsContent value="inventory" className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-2xl font-semibold">Inventory Movement Report</h2>
            {inventoryMovementReport && (
              <div className="flex space-x-2">
                <Button
                  onClick={() => handleExportToExcel(inventoryMovementReport, `Inventory_Movement_${dateRange.fromDate}_to_${dateRange.toDate}`, 'inventory-movement')}
                  disabled={excelExportLoading}
                  variant="outline"
                  size="sm"
                  className="touch-target hover:bg-[#F4E9B1] transition-colors"
                >
                  <FileSpreadsheet className="mr-2 h-4 w-4" />
                  Export Excel
                </Button>
                <Button
                  onClick={() => handleExportToPdf(inventoryMovementReport, `Inventory_Movement_${dateRange.fromDate}_to_${dateRange.toDate}`, 'inventory-movement')}
                  disabled={pdfExportLoading}
                  variant="outline"
                  size="sm"
                  className="touch-target hover:bg-[#F4E9B1] transition-colors"
                >
                  <FileText className="mr-2 h-4 w-4" />
                  Export PDF
                </Button>
              </div>
            )}
          </div>

          {inventoryMovementLoading && (
            <div className="flex justify-center py-12">
              <Loader2 className="h-8 w-8 animate-spin" />
            </div>
          )}

          {inventoryMovementReport && !inventoryMovementLoading && (
            <>
              {/* Report Header */}
              <Card className="pos-card">
                <CardHeader>
                  <CardTitle>
                    {branches.find(b => b.id === inventoryMovementReport.branchId)?.name || 'Unknown Branch'} - Inventory Movement
                  </CardTitle>
                  <CardDescription>
                    From {new Date(inventoryMovementReport.fromDate).toLocaleDateString()} to {new Date(inventoryMovementReport.toDate).toLocaleDateString()}
                  </CardDescription>
                </CardHeader>
              </Card>

              {/* Summary Cards */}
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Total Items</p>
                        <p className="text-3xl font-bold">
                          {inventoryMovementReport.movements?.length || 0}
                        </p>
                      </div>
                      <Package className="h-8 w-8 text-blue-500" />
                    </div>
                  </CardContent>
                </Card>
                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Total Weight</p>
                        <p className="text-3xl font-bold">
                          {(inventoryMovementReport.movements?.reduce((sum, movement) => {
                            const weight = movement?.closingWeight || 0;
                            return sum + weight;
                          }, 0) || 0).toFixed(2)}g
                        </p>
                      </div>
                      <Package className="h-8 w-8 text-green-500" />
                    </div>
                  </CardContent>
                </Card>
                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Total Sales</p>
                        <p className="text-3xl font-bold text-red-600">
                          {inventoryMovementReport.movements?.reduce((sum, movement) => {
                            const sales = movement?.sales || 0;
                            return sum + sales;
                          }, 0) || 0}
                        </p>
                      </div>
                      <TrendingDown className="h-8 w-8 text-red-500" />
                    </div>
                  </CardContent>
                </Card>
              </div>

              {/* Inventory Movement Table */}
              <Card className="pos-card">
                <CardHeader>
                  <CardTitle>Product Movement Summary</CardTitle>
                </CardHeader>
                <CardContent>
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Product</TableHead>
                        <TableHead>Category</TableHead>
                        <TableHead>Opening</TableHead>
                        <TableHead>Purchases</TableHead>
                        <TableHead>Sales</TableHead>

                        <TableHead>Adjustments</TableHead>
                        <TableHead>Transfers</TableHead>
                        <TableHead>Closing</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {inventoryMovementReport.movements?.map((movement) => (
                        <TableRow key={movement?.productId || Math.random()}>
                          <TableCell className="font-medium">
                            <div>
                              <div>{movement?.productName || 'Unknown Product'}</div>
                              <div className="text-sm text-muted-foreground">ID: {movement?.productId || 'N/A'}</div>
                            </div>
                          </TableCell>
                          <TableCell>
                            <Badge variant="secondary">
                              {movement?.category === 1 ? 'Jewelry' : 
                               movement?.category === 2 ? 'Bullion' : 
                               movement?.category === 3 ? 'Coins' : 
                               `Category ${movement?.category || 'Unknown'}`}
                            </Badge>
                          </TableCell>
                          <TableCell>
                            <div>
                              <div>Qty: {movement?.openingQuantity || 0}</div>
                              <div className="text-sm text-muted-foreground">
                                Weight: {(movement?.openingWeight || 0).toFixed(2)}g
                              </div>
                            </div>
                          </TableCell>
                          <TableCell>
                            <div className={(movement?.purchases || 0) > 0 ? 'text-green-600' : 'text-muted-foreground'}>
                              {movement?.purchases || 0}
                            </div>
                          </TableCell>
                          <TableCell>
                            <div className={(movement?.sales || 0) > 0 ? 'text-red-600' : 'text-muted-foreground'}>
                              {movement?.sales || 0}
                            </div>
                          </TableCell>

                          <TableCell>
                            <div className={(movement?.adjustments || 0) !== 0 ? ((movement?.adjustments || 0) > 0 ? 'text-green-600' : 'text-red-600') : 'text-muted-foreground'}>
                              {(movement?.adjustments || 0) > 0 ? '+' : ''}{movement?.adjustments || 0}
                            </div>
                          </TableCell>
                          <TableCell>
                            <div className={(movement?.transfers || 0) !== 0 ? ((movement?.transfers || 0) > 0 ? 'text-green-600' : 'text-red-600') : 'text-muted-foreground'}>
                              {(movement?.transfers || 0) > 0 ? '+' : ''}{movement?.transfers || 0}
                            </div>
                          </TableCell>
                          <TableCell>
                            <div>
                              <div className="font-semibold">Qty: {movement?.closingQuantity || 0}</div>
                              <div className="text-sm text-muted-foreground">
                                Weight: {(movement?.closingWeight || 0).toFixed(2)}g
                              </div>
                            </div>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                  {(!inventoryMovementReport.movements || inventoryMovementReport.movements.length === 0) && (
                    <div className="text-center py-8 text-muted-foreground">
                      No inventory movements found for the selected period
                    </div>
                  )}
                </CardContent>
              </Card>


            </>
          )}
        </TabsContent>

        {/* Profit Analysis Tab */}
        <TabsContent value="profit" className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-2xl font-semibold">Profit Analysis</h2>
            {profitAnalysisReport && (
              <div className="flex space-x-2">
                <Button
                  onClick={() => handleExportToExcel(profitAnalysisReport, `Profit_Analysis_${dateRange.fromDate}_to_${dateRange.toDate}`, 'profit-analysis')}
                  disabled={excelExportLoading}
                  variant="outline"
                  size="sm"
                  className="touch-target hover:bg-[#F4E9B1] transition-colors"
                >
                  <FileSpreadsheet className="mr-2 h-4 w-4" />
                  Export Excel
                </Button>
                <Button
                  onClick={() => handleExportToPdf(profitAnalysisReport, `Profit_Analysis_${dateRange.fromDate}_to_${dateRange.toDate}`, 'profit-analysis')}
                  disabled={pdfExportLoading}
                  variant="outline"
                  size="sm"
                  className="touch-target hover:bg-[#F4E9B1] transition-colors"
                >
                  <FileText className="mr-2 h-4 w-4" />
                  Export PDF
                </Button>
              </div>
            )}
          </div>

          {profitAnalysisLoading && (
            <div className="flex justify-center py-12">
              <Loader2 className="h-8 w-8 animate-spin" />
            </div>
          )}

          {profitAnalysisReport && !profitAnalysisLoading && (
            <>
              {/* Summary Cards */}
              <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Total Revenue</p>
                        <p className="text-3xl font-bold text-green-600">
                          {formatCurrency(profitAnalysisReport.totalRevenue)}
                        </p>
                      </div>
                      <DollarSign className="h-8 w-8 text-green-500" />
                    </div>
                  </CardContent>
                </Card>

                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Cost of Goods Sold</p>
                        <p className="text-3xl font-bold text-red-600">
                          {formatCurrency(profitAnalysisReport.totalCostOfGoodsSold)}
                        </p>
                      </div>
                      <TrendingDown className="h-8 w-8 text-red-500" />
                    </div>
                  </CardContent>
                </Card>

                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Gross Profit</p>
                        <p className="text-3xl font-bold text-blue-600">
                          {formatCurrency(profitAnalysisReport.grossProfit)}
                        </p>
                      </div>
                      <TrendingUp className="h-8 w-8 text-blue-500" />
                    </div>
                  </CardContent>
                </Card>

                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Profit Margin</p>
                        <p className="text-3xl font-bold text-purple-600">
                          {profitAnalysisReport.grossProfitMargin.toFixed(2)}%
                        </p>
                      </div>
                      <BarChart3 className="h-8 w-8 text-purple-500" />
                    </div>
                  </CardContent>
                </Card>
              </div>

              {/* Report Header */}
              <Card className="pos-card">
                <CardHeader>
                  <CardTitle>
                    {profitAnalysisReport.branchId ? branches.find(b => b.id === profitAnalysisReport.branchId)?.name || 'Unknown Branch' : 'All Branches'} - Profit Analysis
                  </CardTitle>
                  <CardDescription>
                    From {formatDate(profitAnalysisReport.fromDate)} to {formatDate(profitAnalysisReport.toDate)}
                  </CardDescription>
                </CardHeader>
              </Card>

              {/* Product Analysis Table */}
              <Card className="pos-card">
                <CardHeader>
                  <CardTitle>Product Profit Analysis</CardTitle>
                </CardHeader>
                <CardContent>
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Product</TableHead>
                        <TableHead>Category</TableHead>
                        <TableHead>Revenue</TableHead>
                        <TableHead>Cost of Goods Sold</TableHead>
                        <TableHead>Gross Profit</TableHead>
                        <TableHead>Profit Margin</TableHead>
                        <TableHead>Quantity Sold</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {profitAnalysisReport.productAnalysis.map((product) => (
                        <TableRow key={product.productId}>
                          <TableCell className="font-medium">
                            <div>
                              <div>{product.productName}</div>
                              <div className="text-sm text-muted-foreground">ID: {product.productId}</div>
                            </div>
                          </TableCell>
                          <TableCell>
                            <Badge variant="secondary">
                              {product.category}
                            </Badge>
                          </TableCell>
                          <TableCell className="text-green-600 font-semibold">
                            {formatCurrency(product.revenue)}
                          </TableCell>
                          <TableCell className="text-red-600 font-semibold">
                            {formatCurrency(product.costOfGoodsSold)}
                          </TableCell>
                          <TableCell className={`font-semibold ${product.grossProfit >= 0 ? 'text-blue-600' : 'text-red-600'}`}>
                            {formatCurrency(product.grossProfit)}
                          </TableCell>
                          <TableCell>
                            <Badge variant={product.grossProfitMargin >= 0 ? "default" : "destructive"}>
                              {product.grossProfitMargin.toFixed(2)}%
                            </Badge>
                          </TableCell>
                          <TableCell className="font-semibold">
                            {product.quantitySold}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                  {profitAnalysisReport.productAnalysis.length === 0 && (
                    <div className="text-center py-8 text-muted-foreground">
                      No product profit data found for the selected period
                    </div>
                  )}
                </CardContent>
              </Card>

              {/* Profit Margin Chart */}
              {profitAnalysisReport.productAnalysis.length > 0 && (
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                  <Card className="pos-card">
                    <CardHeader>
                      <CardTitle>Profit by Category</CardTitle>
                    </CardHeader>
                    <CardContent>
                      <ResponsiveContainer width="100%" height={300}>
                        <BarChart data={profitAnalysisReport.productAnalysis.reduce((acc, product) => {
                          const existing = acc.find(item => item.category === product.category);
                          if (existing) {
                            existing.profit += product.grossProfit;
                            existing.revenue += product.revenue;
                          } else {
                            acc.push({
                              category: product.category,
                              profit: product.grossProfit,
                              revenue: product.revenue
                            });
                          }
                          return acc;
                        }, [] as Array<{category: string, profit: number, revenue: number}>)}>
                          <CartesianGrid strokeDasharray="3 3" />
                          <XAxis dataKey="category" />
                          <YAxis />
                          <Tooltip formatter={(value: any) => formatCurrency(value)} />
                          <Bar dataKey="profit" fill="#3B82F6" name="Gross Profit" />
                        </BarChart>
                      </ResponsiveContainer>
                    </CardContent>
                  </Card>

                  <Card className="pos-card">
                    <CardHeader>
                      <CardTitle>Revenue vs Profit Margin</CardTitle>
                    </CardHeader>
                    <CardContent>
                      <ResponsiveContainer width="100%" height={300}>
                        <LineChart data={profitAnalysisReport.productAnalysis.map(product => ({
                          name: product.productName,
                          revenue: product.revenue,
                          margin: product.grossProfitMargin
                        }))}>
                          <CartesianGrid strokeDasharray="3 3" />
                          <XAxis dataKey="name" angle={-45} textAnchor="end" height={80} />
                          <YAxis yAxisId="left" />
                          <YAxis yAxisId="right" orientation="right" />
                          <Tooltip 
                            formatter={(value: any, name: string) => [
                              name === 'revenue' ? formatCurrency(value) : `${value.toFixed(2)}%`,
                              name === 'revenue' ? 'Revenue' : 'Profit Margin %'
                            ]}
                          />
                          <Line yAxisId="left" type="monotone" dataKey="revenue" stroke="#10B981" name="Revenue" />
                          <Line yAxisId="right" type="monotone" dataKey="margin" stroke="#8B5CF6" name="Profit Margin %" />
                        </LineChart>
                      </ResponsiveContainer>
                    </CardContent>
                  </Card>
                </div>
              )}
            </>
          )}
        </TabsContent>

        {/* Suppliers Tab */}
        <TabsContent value="suppliers" className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-2xl font-semibold">Supplier Balance Report</h2>
            {supplierBalanceReport && (
              <div className="flex space-x-2">
                <Button
                  onClick={() => handleExportToExcel(supplierBalanceReport, `Supplier_Balance_${formatDateLocal(new Date())}`, 'supplier-balance')}
                  disabled={excelExportLoading}
                  variant="outline"
                  size="sm"
                  className="touch-target hover:bg-[#F4E9B1] transition-colors"
                >
                  <FileSpreadsheet className="mr-2 h-4 w-4" />
                  Export Excel
                </Button>
                <Button
                  onClick={() => handleExportToPdf(supplierBalanceReport, `Supplier_Balance_${formatDateLocal(new Date())}`, 'supplier-balance')}
                  disabled={pdfExportLoading}
                  variant="outline"
                  size="sm"
                  className="touch-target hover:bg-[#F4E9B1] transition-colors"
                >
                  <FileText className="mr-2 h-4 w-4" />
                  Export PDF
                </Button>
              </div>
            )}
          </div>

          {supplierBalanceLoading && (
            <div className="flex justify-center py-12">
              <Loader2 className="h-8 w-8 animate-spin" />
            </div>
          )}

          {supplierBalanceReport && !supplierBalanceLoading && (
            <>
              {/* Summary Cards */}
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Total Suppliers</p>
                        <p className="text-3xl font-bold">
                          {supplierBalanceReport.supplierBalances?.length || 0}
                        </p>
                      </div>
                      <Users className="h-8 w-8 text-blue-500" />
                    </div>
                  </CardContent>
                </Card>

                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Total Outstanding (Current Balance)</p>
                        <p className="text-3xl font-bold text-red-600">
                          {formatCurrency(supplierBalanceReport.supplierBalances?.reduce((sum: number, supplier: any) => sum + (supplier?.currentBalance || 0), 0) || 0)}
                        </p>
                      </div>
                      <DollarSign className="h-8 w-8 text-red-500" />
                    </div>
                  </CardContent>
                </Card>

                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Total Payables</p>
                        <p className="text-3xl font-bold text-green-600">
                          {formatCurrency(supplierBalanceReport.totalPayables || 0)}
                        </p>
                      </div>
                      <TrendingUp className="h-8 w-8 text-green-500" />
                    </div>
                  </CardContent>
                </Card>
              </div>

              {/* Supplier Balance Table */}
              <Card className="pos-card">
                <CardHeader>
                  <CardTitle>Supplier Balance Details</CardTitle>
                  <CardDescription>
                    Current balances and overdue status for all suppliers
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Supplier ID</TableHead>
                        <TableHead>Current Balance</TableHead>
                        <TableHead>Overdue Amount</TableHead>
                        <TableHead>Days Overdue</TableHead>
                        <TableHead>Last Payment Date</TableHead>
                        <TableHead>Status</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {supplierBalanceReport.supplierBalances?.map((supplier: any) => (
                        <TableRow key={supplier?.supplierId || Math.random()}>
                          <TableCell className="font-medium">{supplier?.supplierId ?? 'N/A'}</TableCell>
                          <TableCell className={`font-semibold ${(supplier?.currentBalance || 0) > 0 ? 'text-red-600' : 'text-green-600'}`}>
                            {formatCurrency(supplier?.currentBalance || 0)}
                          </TableCell>
                          <TableCell className={`${(supplier?.overdueAmount || 0) > 0 ? 'text-orange-600' : ''} font-semibold`}>
                            {formatCurrency(supplier?.overdueAmount || 0)}
                          </TableCell>
                          <TableCell>{supplier?.daysOverdue ?? 0}</TableCell>
                          <TableCell>
                            {supplier?.lastPaymentDate ? formatDate(supplier.lastPaymentDate) : 'N/A'}
                          </TableCell>
                          <TableCell>
                            <Badge variant={(supplier?.overdueAmount || 0) > 0 ? 'destructive' : 'default'}>
                              {(supplier?.overdueAmount || 0) > 0 ? 'Overdue' : 'Current'}
                            </Badge>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                  {(!supplierBalanceReport.supplierBalances || supplierBalanceReport.supplierBalances.length === 0) && (
                    <div className="text-center py-8 text-muted-foreground">
                      No supplier balance data found
                    </div>
                  )}
                </CardContent>
              </Card>

              {/* Outstanding Balances Chart */}
              {supplierBalanceReport.supplierBalances && supplierBalanceReport.supplierBalances.length > 0 && (
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                  <Card className="pos-card">
                    <CardHeader>
                      <CardTitle>Outstanding Balances by Supplier</CardTitle>
                    </CardHeader>
                    <CardContent>
                      <ResponsiveContainer width="100%" height={300}>
                        <BarChart data={supplierBalanceReport.supplierBalances
                          .filter((supplier: any) => (supplier?.currentBalance || 0) > 0)
                          .map((supplier: any, index: number) => ({
                            name: `Supplier ${supplier?.supplierId ?? ''}`.trim(),
                            outstanding: supplier?.currentBalance || 0,
                            fill: COLORS[index % COLORS.length]
                          }))}>
                          <CartesianGrid strokeDasharray="3 3" />
                          <XAxis dataKey="name" angle={-45} textAnchor="end" height={80} />
                          <YAxis />
                          <Tooltip formatter={(value: any) => formatCurrency(value)} />
                          <Bar dataKey="outstanding" fill="#EF4444" name="Outstanding Balance" />
                        </BarChart>
                      </ResponsiveContainer>
                    </CardContent>
                  </Card>

                  <Card className="pos-card">
                    <CardHeader>
                      <CardTitle>Current vs Overdue Distribution</CardTitle>
                    </CardHeader>
                    <CardContent>
                      <ResponsiveContainer width="100%" height={300}>
                        <PieChart>
                          <Pie
                            data={[
                              {
                                name: 'Current Balance',
                                value: supplierBalanceReport.supplierBalances.reduce((sum: number, supplier: any) => sum + (supplier?.currentBalance || 0), 0),
                                fill: '#10B981'
                              },
                              {
                                name: 'Overdue Amount',
                                value: supplierBalanceReport.supplierBalances.reduce((sum: number, supplier: any) => sum + (supplier?.overdueAmount || 0), 0),
                                fill: '#EF4444'
                              }
                            ]}
                            cx="50%"
                            cy="50%"
                            labelLine={false}
                            label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                            outerRadius={80}
                            fill="#8884d8"
                            dataKey="value"
                          />
                          <Tooltip formatter={(value: any) => formatCurrency(value)} />
                        </PieChart>
                      </ResponsiveContainer>
                    </CardContent>
                  </Card>
                </div>
              )}
            </>
          )}
        </TabsContent>

        {/* Tax Report Tab */}
        <TabsContent value="tax" className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-2xl font-semibold">Tax Report</h2>
            {taxReport && (
              <div className="flex space-x-2">
                <Button
                  onClick={() => handleExportToExcel(taxReport, `Tax_Report_${dateRange.fromDate}_to_${dateRange.toDate}`, 'tax-report')}
                  disabled={excelExportLoading}
                  variant="outline"
                  size="sm"
                  className="touch-target hover:bg-[#F4E9B1] transition-colors"
                >
                  <FileSpreadsheet className="mr-2 h-4 w-4" />
                  Export Excel
                </Button>
                <Button
                  onClick={() => handleExportToPdf(taxReport, `Tax_Report_${dateRange.fromDate}_to_${dateRange.toDate}`, 'tax-report')}
                  disabled={pdfExportLoading}
                  variant="outline"
                  size="sm"
                  className="touch-target hover:bg-[#F4E9B1] transition-colors"
                >
                  <FileText className="mr-2 h-4 w-4" />
                  Export PDF
                </Button>
              </div>
            )}
          </div>

          {taxReportLoading && (
            <div className="flex justify-center py-12">
              <Loader2 className="h-8 w-8 animate-spin" />
            </div>
          )}

          {taxReport && !taxReportLoading && (
            <>
              {/* Summary Cards */}
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Total Tax Collected</p>
                        <p className="text-3xl font-bold text-green-600">
                          {formatCurrency(taxReport.totalTaxCollected)}
                        </p>
                      </div>
                      <DollarSign className="h-8 w-8 text-green-500" />
                    </div>
                  </CardContent>
                </Card>

                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Total Sales</p>
                        <p className="text-3xl font-bold text-blue-600">
                          {formatCurrency(taxReport.totalTaxCollected || 0)}
                        </p>
                      </div>
                      <TrendingUp className="h-8 w-8 text-blue-500" />
                    </div>
                  </CardContent>
                </Card>

                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Tax Rate</p>
                        <p className="text-3xl font-bold text-purple-600">
                          {taxReport.taxSummaries && taxReport.taxSummaries.length > 0 
                            ? (taxReport.taxSummaries.reduce((sum: number, tax: any) => sum + (tax?.taxRate || 0), 0) / taxReport.taxSummaries.length).toFixed(2) 
                            : '0.00'}%
                        </p>
                      </div>
                      <BarChart3 className="h-8 w-8 text-purple-500" />
                    </div>
                  </CardContent>
                </Card>
              </div>

              {/* Tax Report Table */}
              <Card className="pos-card">
                <CardHeader>
                  <CardTitle>Tax Summary</CardTitle>
                  <CardDescription>
                    Tax breakdown from {taxReport.fromDate ? new Date(taxReport.fromDate).toLocaleDateString() : 'N/A'} to {taxReport.toDate ? new Date(taxReport.toDate).toLocaleDateString() : 'N/A'}
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Tax Name</TableHead>
                        <TableHead>Tax Code</TableHead>
                        <TableHead>Tax Rate</TableHead>
                        <TableHead>Taxable Amount</TableHead>
                        <TableHead>Tax Amount</TableHead>
                        <TableHead>Total</TableHead>
                        <TableHead>Effective Rate</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {taxReport.taxSummaries?.map((tax: any) => (
                        <TableRow key={`${tax?.taxCode}-${tax?.taxName}` || Math.random()}>
                          <TableCell className="font-medium">
                            {tax?.taxName || 'N/A'}
                          </TableCell>
                          <TableCell>
                            <Badge variant="outline">{tax?.taxCode || 'N/A'}</Badge>
                          </TableCell>
                          <TableCell>
                            <span className="font-semibold">{tax?.taxRate || 0}%</span>
                          </TableCell>
                          <TableCell>
                            {formatCurrency(tax?.taxableAmount || 0)}
                          </TableCell>
                          <TableCell className="text-green-600 font-semibold">
                            {formatCurrency(tax?.taxAmount || 0)}
                          </TableCell>
                          <TableCell className="font-semibold">
                            {formatCurrency((tax?.taxableAmount || 0) + (tax?.taxAmount || 0))}
                          </TableCell>
                          <TableCell>
                            <Badge variant="secondary">
                              {tax?.taxRate || 0}%
                            </Badge>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                  {(!taxReport.taxSummaries || taxReport.taxSummaries.length === 0) && (
                    <div className="text-center py-8 text-muted-foreground">
                      No tax data found for the selected period
                    </div>
                  )}
                </CardContent>
              </Card>
            </>
          )}
        </TabsContent>

        {/* Ownership Reports Tab */}
        <TabsContent value="ownership" className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-2xl font-semibold">Product Ownership Reports</h2>
            <div className="flex space-x-2">
              <Button
                onClick={() => handleExportToExcel(ownershipData, `Ownership_Report_${selectedDate}`, 'ownership-report')}
                disabled={excelExportLoading || ownershipLoading}
                variant="outline"
                size="sm"
                className="touch-target hover:bg-[#F4E9B1] transition-colors"
              >
                <FileSpreadsheet className="mr-2 h-4 w-4" />
                Export Excel
              </Button>
              <Button
                onClick={() => handleExportToPdf(ownershipData, `Ownership_Report_${selectedDate}`, 'ownership-report')}
                disabled={pdfExportLoading || ownershipLoading}
                variant="outline"
                size="sm"
                className="touch-target hover:bg-[#F4E9B1] transition-colors"
              >
                <FileText className="mr-2 h-4 w-4" />
                Export PDF
              </Button>
            </div>
          </div>

          {ownershipLoading && (
            <div className="flex justify-center py-12">
              <Loader2 className="h-8 w-8 animate-spin" />
            </div>
          )}

          {!ownershipLoading && (
            <>
              {/* Ownership Summary Cards */}
              <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Total Products</p>
                        <p className="text-3xl font-bold text-blue-600">
                          {getOwnershipStatistics().totalProducts}
                        </p>
                      </div>
                      <Package className="h-8 w-8 text-blue-500" />
                    </div>
                  </CardContent>
                </Card>

                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">High Ownership</p>
                        <p className="text-3xl font-bold text-green-600">
                          {getOwnershipStatistics().highOwnership}
                        </p>
                      </div>
                      <CheckCircle className="h-8 w-8 text-green-500" />
                    </div>
                  </CardContent>
                </Card>

                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Low Ownership</p>
                        <p className="text-3xl font-bold text-red-600">
                          {getOwnershipStatistics().lowOwnership}
                        </p>
                      </div>
                      <AlertTriangle className="h-8 w-8 text-red-500" />
                    </div>
                  </CardContent>
                </Card>

                <Card className="pos-card">
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Outstanding Amount</p>
                        <p className="text-3xl font-bold text-orange-600">
                          {formatCurrency(getOwnershipStatistics().totalOutstanding)}
                        </p>
                      </div>
                      <DollarSign className="h-8 w-8 text-orange-500" />
                    </div>
                  </CardContent>
                </Card>
              </div>

              {/* Ownership Charts */}
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* Ownership Distribution Pie Chart */}
                <Card className="pos-card">
                  <CardHeader>
                    <CardTitle>Ownership Distribution</CardTitle>
                    <CardDescription>
                      Distribution of products by ownership percentage
                    </CardDescription>
                  </CardHeader>
                  <CardContent>
                    <ResponsiveContainer width="100%" height={300}>
                      <PieChart>
                        <Pie
                          data={getOwnershipChartData()}
                          cx="50%"
                          cy="50%"
                          labelLine={false}
                          label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                          outerRadius={80}
                          fill="#8884d8"
                          dataKey="value"
                        >
                          {getOwnershipChartData().map((entry, index) => (
                            <Cell key={`cell-${index}`} fill={entry.color} />
                          ))}
                        </Pie>
                        <Tooltip />
                      </PieChart>
                    </ResponsiveContainer>
                  </CardContent>
                </Card>

                {/* Ownership Trend Line Chart */}
                <Card className="pos-card">
                  <CardHeader>
                    <CardTitle>Ownership Trend</CardTitle>
                    <CardDescription>
                      Average ownership percentage over time
                    </CardDescription>
                  </CardHeader>
                  <CardContent>
                    <ResponsiveContainer width="100%" height={300}>
                      <LineChart data={getOwnershipTrendData()}>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="date" />
                        <YAxis />
                        <Tooltip />
                        <Line type="monotone" dataKey="ownership" stroke="#D4AF37" strokeWidth={2} />
                      </LineChart>
                    </ResponsiveContainer>
                  </CardContent>
                </Card>
              </div>

              {/* Ownership Alerts */}
              {ownershipAlerts.length > 0 && (
                <Card className="pos-card">
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <AlertTriangle className="h-5 w-5 text-orange-500" />
                      Ownership Alerts ({ownershipAlerts.length})
                    </CardTitle>
                    <CardDescription>
                      Products requiring attention due to ownership issues
                    </CardDescription>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-4">
                      {ownershipAlerts.map((alert, index) => (
                        <div key={index} className="flex items-center justify-between p-4 border rounded-lg">
                          <div className="flex items-center space-x-4">
                            <AlertTriangle className="h-6 w-6 text-orange-500" />
                            <div>
                              <h3 className="font-medium">{alert.productName}</h3>
                              <p className="text-sm text-muted-foreground">{alert.message}</p>
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
                    </div>
                  </CardContent>
                </Card>
              )}

              {/* Low Ownership Products */}
              {lowOwnershipProducts.length > 0 && (
                <Card className="pos-card">
                  <CardHeader>
                    <CardTitle>Low Ownership Products</CardTitle>
                    <CardDescription>
                      Products with ownership percentage below 50%
                    </CardDescription>
                  </CardHeader>
                  <CardContent>
                    <Table>
                      <TableHeader>
                        <TableRow>
                          <TableHead>Product</TableHead>
                          <TableHead>Ownership %</TableHead>
                          <TableHead>Outstanding Amount</TableHead>
                          <TableHead>Status</TableHead>
                        </TableRow>
                      </TableHeader>
                      <TableBody>
                        {lowOwnershipProducts.map((product) => (
                          <TableRow key={product.id}>
                            <TableCell>
                              <div>
                                <div className="font-medium">{product.productName}</div>
                                <div className="text-sm text-muted-foreground">{product.productCode}</div>
                              </div>
                            </TableCell>
                            <TableCell>
                              <Badge className="bg-red-100 text-red-800">
                                {product.ownershipPercentage.toFixed(1)}%
                              </Badge>
                            </TableCell>
                            <TableCell className="text-red-600 font-medium">
                              {formatCurrency(product.outstandingAmount)}
                            </TableCell>
                            <TableCell>
                              <Badge variant="outline" className="bg-red-50 text-red-700">
                                Low Ownership
                              </Badge>
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </CardContent>
                </Card>
              )}

              {/* Outstanding Payments */}
              {outstandingPayments.length > 0 && (
                <Card className="pos-card">
                  <CardHeader>
                    <CardTitle>Outstanding Payments</CardTitle>
                    <CardDescription>
                      Products with pending payments
                    </CardDescription>
                  </CardHeader>
                  <CardContent>
                    <Table>
                      <TableHeader>
                        <TableRow>
                          <TableHead>Product</TableHead>
                          <TableHead>Supplier</TableHead>
                          <TableHead>Outstanding Amount</TableHead>
                          <TableHead>Ownership %</TableHead>
                        </TableRow>
                      </TableHeader>
                      <TableBody>
                        {outstandingPayments.map((product) => (
                          <TableRow key={product.id}>
                            <TableCell>
                              <div>
                                <div className="font-medium">{product.productName}</div>
                                <div className="text-sm text-muted-foreground">{product.productCode}</div>
                              </div>
                            </TableCell>
                            <TableCell>{product.supplierId || 'N/A'}</TableCell>
                            <TableCell className="text-red-600 font-medium">
                              {formatCurrency(product.outstandingAmount)}
                            </TableCell>
                            <TableCell>
                              <Badge 
                                variant="outline" 
                                className={
                                  product.ownershipPercentage >= 80 ? 'bg-green-100 text-green-800' :
                                  product.ownershipPercentage >= 50 ? 'bg-yellow-100 text-yellow-800' :
                                  'bg-red-100 text-red-800'
                                }
                              >
                                {product.ownershipPercentage.toFixed(1)}%
                              </Badge>
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </CardContent>
                </Card>
              )}

              {ownershipData.length === 0 && ownershipAlerts.length === 0 && lowOwnershipProducts.length === 0 && outstandingPayments.length === 0 && (
                <Card className="pos-card">
                  <CardContent className="text-center py-12">
                    <CheckCircle className="h-12 w-12 mx-auto mb-4 text-green-500" />
                    <p className="text-lg font-medium">No ownership data available</p>
                    <p className="text-muted-foreground">Start managing product ownership to see reports here</p>
                  </CardContent>
                </Card>
              )}
            </>
          )}
        </TabsContent>

      </Tabs>
    </div>
  );
}
