import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Badge } from './ui/badge';
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from './ui/tabs';
import {
  BarChart3,
  TrendingUp,
  DollarSign,
  Scale,
  Clock,
  CheckCircle,
  AlertTriangle,
  Download,
  Calendar,
  Filter,
  RefreshCw,
  Loader2,
} from 'lucide-react';
import api from '../services/api';

const ManufacturingReports: React.FC = () => {
  const [activeTab, setActiveTab] = useState('dashboard');
  const [dateRange, setDateRange] = useState({
    startDate: '',
    endDate: ''
  });
  const [loading, setLoading] = useState(false);
  const [reportsData, setReportsData] = useState<any>(null);


  // Initialize with current month
  useEffect(() => {
    const today = new Date();
    const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
    const lastDayOfMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0);

    setDateRange({
      startDate: firstDayOfMonth.toISOString().split('T')[0],
      endDate: lastDayOfMonth.toISOString().split('T')[0]
    });
  }, []);


  // Load reports data
  const loadReports = async () => {
    setLoading(true);
    try {
      const startDateStr = dateRange.startDate || undefined;
      const endDateStr = dateRange.endDate || undefined;

      // Load all reports in parallel
      const [dashboardData, rawGoldData, efficiencyData, costData, workflowData] = await Promise.all([
        api.manufacturingReports.getManufacturingDashboard(startDateStr, endDateStr),
        api.manufacturingReports.getRawGoldUtilizationReport(startDateStr, endDateStr),
        api.manufacturingReports.getEfficiencyReport(startDateStr, endDateStr),
        api.manufacturingReports.getCostAnalysisReport(startDateStr, endDateStr),
        api.manufacturingReports.getWorkflowPerformanceReport(startDateStr, endDateStr),
      ]);

      // Debug: Log the API responses
      console.log('API Responses:', {
        dashboardData,
        rawGoldData,
        efficiencyData,
        costData,
        workflowData
      });

      // Structure the data to match the expected format
      setReportsData({
        dashboard: dashboardData,
        rawGoldUtilization: rawGoldData,
        efficiency: efficiencyData,
        costAnalysis: costData,
        workflowPerformance: workflowData
      });
    } catch (error) {
      console.error('Error loading reports:', error);
      // Set empty data structure on error
      setReportsData({
        dashboard: null,
        rawGoldUtilization: null,
        efficiency: null,
        costAnalysis: null,
        workflowPerformance: null
      });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (dateRange.startDate && dateRange.endDate) {
      loadReports();
    }
  }, [dateRange]);

  // Handle export
  const handleExport = async (format: 'excel' | 'pdf') => {
    try {
      // In real app, this would call the export API
      console.log(`Exporting to ${format}:`, reportsData);
      alert(`Export to ${format.toUpperCase()} - Feature coming soon!`);
    } catch (error) {
      console.error('Error exporting:', error);
      alert('Failed to export report');
    }
  };

  // Format currency
  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  // Format percentage
  const formatPercentage = (value: number) => {
    return `${value.toFixed(1)}%`;
  };

  // Format weight
  const formatWeight = (weight: number) => {
    return `${weight.toFixed(2)}g`;
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Manufacturing Reports</h1>
          <p className="text-muted-foreground">
            Comprehensive analytics and insights for manufacturing operations
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => loadReports()} disabled={loading}>
            {loading ? <Loader2 className="w-4 h-4 mr-2 animate-spin" /> : <RefreshCw className="w-4 h-4 mr-2" />}
            Refresh
          </Button>
          <Button variant="outline" onClick={() => handleExport('excel')}>
            <Download className="w-4 h-4 mr-2" />
            Export Excel
          </Button>
          <Button variant="outline" onClick={() => handleExport('pdf')}>
            <Download className="w-4 h-4 mr-2" />
            Export PDF
          </Button>
        </div>
      </div>

      {/* Date Range Filter */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Report Period</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="space-y-2">
              <Label htmlFor="startDate">Start Date</Label>
              <Input
                id="startDate"
                type="date"
                value={dateRange.startDate}
                onChange={(e) => setDateRange(prev => ({ ...prev, startDate: e.target.value }))}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="endDate">End Date</Label>
              <Input
                id="endDate"
                type="date"
                value={dateRange.endDate}
                onChange={(e) => setDateRange(prev => ({ ...prev, endDate: e.target.value }))}
              />
            </div>
            <div className="flex items-end">
              <Button onClick={loadReports} disabled={loading}>
                <Filter className="w-4 h-4 mr-2" />
                Apply Filter
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Debug: Show current data structure */}
      {reportsData && (
        <div className="mb-4 p-4 bg-gray-100 rounded">
          <h3 className="font-bold">Debug - Data Structure:</h3>
          <pre className="text-xs overflow-auto max-h-32">
            {JSON.stringify(reportsData, null, 2)}
          </pre>
        </div>
      )}

      {/* Summary Cards */}
      {reportsData?.dashboard?.Summary && (
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Raw Gold Utilized</CardTitle>
              <Scale className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{formatPercentage(reportsData.dashboard.Summary.RawGoldUtilizationRate)}</div>
              <p className="text-xs text-muted-foreground">
                {formatWeight(reportsData.dashboard.Summary.TotalRawGoldConsumed)} consumed
              </p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Completion Rate</CardTitle>
              <CheckCircle className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{formatPercentage(reportsData.dashboard.Summary.OverallCompletionRate)}</div>
              <p className="text-xs text-muted-foreground">
                {reportsData.dashboard.Summary.TotalProductsManufactured} products
              </p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Total Cost</CardTitle>
              <DollarSign className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{formatCurrency(reportsData.dashboard.Summary.TotalManufacturingCost)}</div>
              <p className="text-xs text-muted-foreground">
                Avg: {formatCurrency(reportsData.dashboard.Summary.AverageCostPerGram)}/g
              </p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Quality Pass Rate</CardTitle>
              <TrendingUp className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{formatPercentage(reportsData.dashboard.Summary.QualityPassRate)}</div>
              <p className="text-xs text-muted-foreground">
                Approval: {formatPercentage(reportsData.dashboard.Summary.ApprovalRate)}
              </p>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Main Content */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="dashboard">Dashboard</TabsTrigger>
          <TabsTrigger value="raw-gold">Raw Gold Utilization</TabsTrigger>
          <TabsTrigger value="efficiency">Efficiency</TabsTrigger>
          <TabsTrigger value="cost-analysis">Cost Analysis</TabsTrigger>
          <TabsTrigger value="workflow">Workflow Performance</TabsTrigger>
        </TabsList>

        <TabsContent value="dashboard" className="space-y-4">
          {/* Manufacturing Dashboard */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Raw Gold Summary</CardTitle>
                <CardDescription>
                  Raw gold utilization and efficiency metrics
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                {reportsData?.rawGoldUtilization && (
                  <>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">Total Purchased:</span>
                      <span className="font-medium">{formatWeight(reportsData.rawGoldUtilization.totalRawGoldPurchased)}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">Total Consumed:</span>
                      <span className="font-medium">{formatWeight(reportsData.rawGoldUtilization.totalRawGoldConsumed)}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">Total Wastage:</span>
                      <span className="font-medium text-red-600">{formatWeight(reportsData.rawGoldUtilization.totalWastage)}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">Utilization Rate:</span>
                      <span className="font-medium">{formatPercentage(reportsData.rawGoldUtilization.rawGoldUtilizationRate)}</span>
                    </div>
                  </>
                )}
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Cost Breakdown</CardTitle>
                <CardDescription>
                  Manufacturing cost distribution
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                {reportsData?.costAnalysis?.costBreakdownByProductType.map((item: any, index: number) => (
                  <div key={index} className="flex justify-between">
                    <span className="text-sm text-muted-foreground">{item.productType}:</span>
                    <span className="font-medium">{formatCurrency(item.totalCost)}</span>
                  </div>
                ))}
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Technician Performance</CardTitle>
                <CardDescription>
                  Top performing technicians this period
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  {reportsData?.efficiency?.byTechnician.slice(0, 3).map((tech: any, index: number) => (
                    <div key={index} className="flex justify-between items-center">
                      <div>
                        <p className="text-sm font-medium">{tech.technicianName}</p>
                        <p className="text-xs text-muted-foreground">{tech.completedRecords} items completed</p>
                      </div>
                      <Badge variant="outline">{formatPercentage(tech.completionRate)}</Badge>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Workflow Status</CardTitle>
                <CardDescription>
                  Current workflow performance metrics
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                {reportsData?.workflowPerformance && (
                  <>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">Approval Rate:</span>
                      <span className="font-medium">{formatPercentage(reportsData.workflowPerformance.approvalRate)}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">Quality Pass Rate:</span>
                      <span className="font-medium">{formatPercentage(reportsData.workflowPerformance.qualityPassRate)}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">Avg Processing Time:</span>
                      <span className="font-medium">{reportsData.workflowPerformance.averageTimeInProgress} hours</span>
                    </div>
                  </>
                )}
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="raw-gold" className="space-y-4">
          {/* Raw Gold Utilization Report */}
          <Card>
            <CardHeader>
              <CardTitle>Raw Gold Utilization Report</CardTitle>
              <CardDescription>
                Detailed breakdown of raw gold usage by supplier
              </CardDescription>
            </CardHeader>
            <CardContent>
              {reportsData?.rawGoldUtilization?.bySupplier && (
                <div className="space-y-4">
                  {reportsData.rawGoldUtilization.bySupplier.map((supplier: any, index: number) => (
                    <div key={index} className="border rounded-lg p-4">
                      <div className="flex justify-between items-start mb-2">
                        <h4 className="font-medium">{supplier.supplierName}</h4>
                        <Badge variant="outline">
                          {formatPercentage(supplier.utilizationRate)}
                        </Badge>
                      </div>
                      <div className="grid grid-cols-3 gap-4 text-sm">
                        <div>
                          <span className="text-muted-foreground">Total:</span>
                          <span className="ml-2 font-medium">{formatWeight(supplier.rawGoldPurchased)}</span>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Consumed:</span>
                          <span className="ml-2 font-medium">{formatWeight(supplier.rawGoldConsumed)}</span>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Wastage:</span>
                          <span className="ml-2 font-medium text-red-600">{formatWeight(supplier.wastage)}</span>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="efficiency" className="space-y-4">
          {/* Manufacturing Efficiency Report */}
          <Card>
            <CardHeader>
              <CardTitle>Technician Performance</CardTitle>
              <CardDescription>
                Individual technician efficiency metrics
              </CardDescription>
            </CardHeader>
            <CardContent>
              {reportsData?.efficiency?.byTechnician && (
                <div className="space-y-4">
                  {reportsData.efficiency.byTechnician.map((tech: any, index: number) => (
                    <div key={index} className="border rounded-lg p-4">
                      <div className="flex justify-between items-start mb-2">
                        <h4 className="font-medium">{tech.technicianName}</h4>
                        <Badge variant={tech.completionRate >= 90 ? "default" : "secondary"}>
                          {formatPercentage(tech.completionRate)}
                        </Badge>
                      </div>
                      <div className="grid grid-cols-2 gap-4 text-sm">
                        <div>
                          <span className="text-muted-foreground">Completed Items:</span>
                          <span className="ml-2 font-medium">{tech.completedRecords}</span>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Avg Time:</span>
                          <span className="ml-2 font-medium">{tech.averageEfficiencyRating}%</span>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="cost-analysis" className="space-y-4">
          {/* Cost Analysis Report */}
          <Card>
            <CardHeader>
              <CardTitle>Product Cost Analysis</CardTitle>
              <CardDescription>
                Cost breakdown by product type
              </CardDescription>
            </CardHeader>
            <CardContent>
              {reportsData?.costAnalysis?.costBreakdownByProductType && (
                <div className="space-y-4">
                  {reportsData.costAnalysis.costBreakdownByProductType.map((product: any, index: number) => (
                    <div key={index} className="border rounded-lg p-4">
                      <div className="flex justify-between items-start mb-2">
                        <h4 className="font-medium">{product.productType}</h4>
                        <Badge variant="outline">{product.productsManufactured} units</Badge>
                      </div>
                      <div className="grid grid-cols-2 gap-4 text-sm">
                        <div>
                          <span className="text-muted-foreground">Total Cost:</span>
                          <span className="ml-2 font-medium">{formatCurrency(product.totalCost)}</span>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Cost per Unit:</span>
                          <span className="ml-2 font-medium">{formatCurrency(product.averageCostPerProduct)}</span>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="workflow" className="space-y-4">
          {/* Workflow Performance Report */}
          <Card>
            <CardHeader>
              <CardTitle>Workflow Stages Performance</CardTitle>
              <CardDescription>
                Performance metrics for each workflow stage
              </CardDescription>
            </CardHeader>
            <CardContent>
              {reportsData?.workflowPerformance?.workflowStepAnalysis && (
                <div className="space-y-4">
                  {reportsData.workflowPerformance.workflowStepAnalysis.map((stage: any, index: number) => (
                    <div key={index} className="border rounded-lg p-4">
                      <div className="flex justify-between items-start mb-2">
                        <h4 className="font-medium">{stage.stepName}</h4>
                        <Badge variant="outline">{stage.totalRecords} items</Badge>
                      </div>
                      <div className="text-sm">
                        <span className="text-muted-foreground">Average Time:</span>
                        <span className="ml-2 font-medium">{formatPercentage(stage.completionRate)}</span>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Potential Bottlenecks</CardTitle>
              <CardDescription>
                Workflow stages that may need attention
              </CardDescription>
            </CardHeader>
            <CardContent>
              {reportsData?.workflowPerformance?.transitionAnalysis && (
                <div className="space-y-4">
                  {reportsData.workflowPerformance.transitionAnalysis.map((transition: any, index: number) => (
                    <div key={index} className="border rounded-lg p-4 border-yellow-200 bg-yellow-50">
                      <div className="flex justify-between items-start mb-2">
                        <h4 className="font-medium flex items-center">
                          <AlertTriangle className="w-4 h-4 mr-2 text-yellow-600" />
                          {transition.fromStatus} → {transition.toStatus}
                        </h4>
                        <Badge variant="outline" className="text-yellow-700">
                          {formatPercentage(transition.successRate)} Success
                        </Badge>
                      </div>
                      <div className="text-sm">
                        <span className="text-muted-foreground">Delay:</span>
                        <span className="ml-2 font-medium">{transition.transitionCount} transitions</span>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
};

export default ManufacturingReports;
