import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Badge } from './ui/badge';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from './ui/table';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from './ui/dialog';
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
  Merge,
  TrendingUp,
  DollarSign,
  Package,
  AlertTriangle,
  CheckCircle,
  Loader2,
  Calculator,
  BarChart3,
  Zap,
} from 'lucide-react';
import { useAuth } from './AuthContext';
import { useBranches } from '../hooks/useApi';
import api from '../services/api';
import {
  ConsolidationOpportunity,
  ConsolidationResult,
  WeightedAverageCost,
  WeightedAverageCostResult,
  ProductCostAnalysis,
} from '../types/ownership';

const OwnershipConsolidation: React.FC = () => {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState('opportunities');
  const [loading, setLoading] = useState(false);
  const [selectedBranchId, setSelectedBranchId] = useState<number>(0);

  // State for consolidation opportunities
  const [opportunities, setOpportunities] = useState<ConsolidationOpportunity[]>([]);
  const [consolidationResults, setConsolidationResults] = useState<ConsolidationResult[]>([]);
  const [costAnalysis, setCostAnalysis] = useState<ProductCostAnalysis[]>([]);

  // Dialog states
  const [consolidationDialogOpen, setConsolidationDialogOpen] = useState(false);
  const [costAnalysisDialogOpen, setCostAnalysisDialogOpen] = useState(false);
  const [selectedOpportunity, setSelectedOpportunity] = useState<ConsolidationOpportunity | null>(null);
  const [selectedProductForAnalysis, setSelectedProductForAnalysis] = useState<number>(0);

  // API hooks
  const branchesApi = useBranches();

  // Fetch initial data on component mount
  useEffect(() => {
    const fetchInitialData = async () => {
      try {
        await branchesApi.execute();
      } catch (error) {
        console.error('Error fetching initial data:', error);
      }
    };

    fetchInitialData();
  }, []);

  // Fetch consolidation opportunities
  const fetchOpportunities = async () => {
    if (!selectedBranchId) return;

    setLoading(true);
    try {
      const data = await api.ownershipConsolidation.getConsolidationOpportunities(selectedBranchId);
      setOpportunities(data);
    } catch (error) {
      console.error('Error fetching consolidation opportunities:', error);
      setOpportunities([]);
    } finally {
      setLoading(false);
    }
  };

  // Handle consolidation
  const handleConsolidation = async (opportunity: ConsolidationOpportunity) => {
    setLoading(true);
    try {
      const result = await api.ownershipConsolidation.consolidateOwnership({
        productId: opportunity.productId,
        supplierId: opportunity.supplierId,
        branchId: opportunity.branchId,
      });
      
      setConsolidationResults(prev => [result, ...prev]);
      
      // Refresh opportunities
      await fetchOpportunities();
      
      setConsolidationDialogOpen(false);
      setSelectedOpportunity(null);
    } catch (error) {
      console.error('Error consolidating ownership:', error);
      alert('Failed to consolidate ownership');
    } finally {
      setLoading(false);
    }
  };

  // Handle supplier consolidation
  const handleSupplierConsolidation = async (supplierId: number) => {
    if (!selectedBranchId) return;

    setLoading(true);
    try {
      const results = await api.ownershipConsolidation.consolidateSupplierOwnership(supplierId, selectedBranchId);
      setConsolidationResults(prev => [...results, ...prev]);
      
      // Refresh opportunities
      await fetchOpportunities();
    } catch (error) {
      console.error('Error consolidating supplier ownership:', error);
      alert('Failed to consolidate supplier ownership');
    } finally {
      setLoading(false);
    }
  };

  // Fetch cost analysis
  const fetchCostAnalysis = async (productId: number) => {
    if (!selectedBranchId) return;

    setLoading(true);
    try {
      const analysis = await api.weightedAverageCosting.getProductCostAnalysis(productId, selectedBranchId);
      setCostAnalysis([analysis]);
      setCostAnalysisDialogOpen(true);
    } catch (error) {
      console.error('Error fetching cost analysis:', error);
      alert('Failed to fetch cost analysis');
    } finally {
      setLoading(false);
    }
  };

  // Update product cost with weighted average
  const updateProductCost = async (productId: number) => {
    if (!selectedBranchId) return;

    setLoading(true);
    try {
      await api.weightedAverageCosting.updateProductCostWithWeightedAverage(productId, selectedBranchId);
      alert('Product cost updated successfully');
    } catch (error) {
      console.error('Error updating product cost:', error);
      alert('Failed to update product cost');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (selectedBranchId) {
      fetchOpportunities();
    }
  }, [selectedBranchId]);

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Ownership Consolidation</h1>
          <p className="text-muted-foreground">
            Consolidate ownership records and analyze cost structures
          </p>
        </div>
        <div className="flex items-center gap-4">
          <div className="w-48">
            <Select
              value={selectedBranchId.toString()}
              onValueChange={(value) => setSelectedBranchId(parseInt(value))}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select branch" />
              </SelectTrigger>
              <SelectContent>
                {branchesApi.data?.items?.map((branch) => (
                  <SelectItem key={branch.id} value={branch.id.toString()}>
                    {branch.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Opportunities</CardTitle>
            <Merge className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{opportunities.length}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Potential Savings</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              ${opportunities.reduce((sum, opp) => sum + opp.potentialSavings, 0).toFixed(2)}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Completed</CardTitle>
            <CheckCircle className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{consolidationResults.length}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Value</CardTitle>
            <DollarSign className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              ${consolidationResults.reduce((sum, result) => sum + result.totalValue, 0).toFixed(2)}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Main Content */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="opportunities">Consolidation Opportunities</TabsTrigger>
          <TabsTrigger value="results">Consolidation Results</TabsTrigger>
          <TabsTrigger value="cost-analysis">Cost Analysis</TabsTrigger>
        </TabsList>

        <TabsContent value="opportunities" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Consolidation Opportunities</CardTitle>
              <CardDescription>
                Products with multiple ownership records that can be consolidated
              </CardDescription>
            </CardHeader>
            <CardContent>
              {loading ? (
                <div className="flex items-center justify-center py-8">
                  <Loader2 className="w-8 h-8 animate-spin" />
                </div>
              ) : opportunities.length === 0 ? (
                <div className="text-center py-8 text-muted-foreground">
                  <Package className="w-12 h-12 mx-auto mb-4 opacity-50" />
                  <p>No consolidation opportunities found</p>
                  <p className="text-sm mt-2">Select a branch to view opportunities</p>
                </div>
              ) : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Product</TableHead>
                      <TableHead>Supplier</TableHead>
                      <TableHead>Records</TableHead>
                      <TableHead>Total Quantity</TableHead>
                      <TableHead>Average Cost</TableHead>
                      <TableHead>Potential Savings</TableHead>
                      <TableHead className="text-right">Actions</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {opportunities.map((opportunity) => (
                      <TableRow key={`${opportunity.productId}-${opportunity.supplierId}`}>
                        <TableCell>
                          <div>
                            <p className="font-medium">{opportunity.productName}</p>
                          </div>
                        </TableCell>
                        <TableCell>{opportunity.supplierName}</TableCell>
                        <TableCell>
                          <Badge variant="outline">{opportunity.ownershipRecords.length}</Badge>
                        </TableCell>
                        <TableCell>{opportunity.totalQuantity}</TableCell>
                        <TableCell>${opportunity.averageCost.toFixed(2)}</TableCell>
                        <TableCell>
                          <Badge className="bg-green-100 text-green-800">
                            ${opportunity.potentialSavings.toFixed(2)}
                          </Badge>
                        </TableCell>
                        <TableCell className="text-right">
                          <div className="flex justify-end gap-2">
                            <Button
                              size="sm"
                              variant="outline"
                              onClick={() => fetchCostAnalysis(opportunity.productId)}
                            >
                              <BarChart3 className="w-4 h-4 mr-1" />
                              Analyze
                            </Button>
                            <Button
                              size="sm"
                              onClick={() => {
                                setSelectedOpportunity(opportunity);
                                setConsolidationDialogOpen(true);
                              }}
                            >
                              <Merge className="w-4 h-4 mr-1" />
                              Consolidate
                            </Button>
                          </div>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="results" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Consolidation Results</CardTitle>
              <CardDescription>
                Recently completed consolidation operations
              </CardDescription>
            </CardHeader>
            <CardContent>
              {consolidationResults.length === 0 ? (
                <div className="text-center py-8 text-muted-foreground">
                  <CheckCircle className="w-12 h-12 mx-auto mb-4 opacity-50" />
                  <p>No consolidation results yet</p>
                  <p className="text-sm mt-2">Complete consolidations to see results here</p>
                </div>
              ) : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Status</TableHead>
                      <TableHead>Original Records</TableHead>
                      <TableHead>Total Quantity</TableHead>
                      <TableHead>Weighted Avg Cost</TableHead>
                      <TableHead>Total Value</TableHead>
                      <TableHead>New Ownership ID</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {consolidationResults.map((result, index) => (
                      <TableRow key={index}>
                        <TableCell>
                          {result.success ? (
                            <Badge className="bg-green-100 text-green-800">
                              <CheckCircle className="w-3 h-3 mr-1" />
                              Success
                            </Badge>
                          ) : (
                            <Badge className="bg-red-100 text-red-800">
                              <AlertTriangle className="w-3 h-3 mr-1" />
                              Failed
                            </Badge>
                          )}
                        </TableCell>
                        <TableCell>
                          <Badge variant="outline">{result.originalOwnershipIds.length}</Badge>
                        </TableCell>
                        <TableCell>{result.totalQuantity}</TableCell>
                        <TableCell>${result.weightedAverageCost.toFixed(2)}</TableCell>
                        <TableCell>${result.totalValue.toFixed(2)}</TableCell>
                        <TableCell>#{result.consolidatedOwnershipId}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="cost-analysis" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Cost Analysis Tools</CardTitle>
              <CardDescription>
                Analyze and update product costs using different costing methods
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <div className="space-y-4">
                  <h4 className="text-sm font-medium">Quick Actions</h4>
                  <div className="space-y-2">
                    <Button
                      variant="outline"
                      className="w-full justify-start"
                      onClick={() => {
                        const productId = prompt('Enter Product ID for analysis:');
                        if (productId) {
                          fetchCostAnalysis(parseInt(productId));
                        }
                      }}
                    >
                      <Calculator className="w-4 h-4 mr-2" />
                      Analyze Product Cost
                    </Button>
                    <Button
                      variant="outline"
                      className="w-full justify-start"
                      onClick={() => {
                        const productId = prompt('Enter Product ID to update cost:');
                        if (productId) {
                          updateProductCost(parseInt(productId));
                        }
                      }}
                    >
                      <Zap className="w-4 h-4 mr-2" />
                      Update Product Cost
                    </Button>
                  </div>
                </div>
                <div className="space-y-4">
                  <h4 className="text-sm font-medium">Costing Methods</h4>
                  <div className="space-y-2 text-sm text-muted-foreground">
                    <p><strong>Weighted Average:</strong> Calculates cost based on all ownership records</p>
                    <p><strong>FIFO:</strong> First In, First Out costing method</p>
                    <p><strong>LIFO:</strong> Last In, First Out costing method</p>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Consolidation Confirmation Dialog */}
      <Dialog open={consolidationDialogOpen} onOpenChange={setConsolidationDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Confirm Consolidation</DialogTitle>
            <DialogDescription>
              Are you sure you want to consolidate ownership records for {selectedOpportunity?.productName}?
            </DialogDescription>
          </DialogHeader>
          {selectedOpportunity && (
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Product</Label>
                  <p className="text-sm">{selectedOpportunity.productName}</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Supplier</Label>
                  <p className="text-sm">{selectedOpportunity.supplierName}</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Records to Merge</Label>
                  <p className="text-sm">{selectedOpportunity.ownershipRecords.length}</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Total Quantity</Label>
                  <p className="text-sm">{selectedOpportunity.totalQuantity}</p>
                </div>
              </div>
              <div className="flex justify-end gap-2">
                <Button variant="outline" onClick={() => setConsolidationDialogOpen(false)}>
                  Cancel
                </Button>
                <Button 
                  onClick={() => handleConsolidation(selectedOpportunity)}
                  disabled={loading}
                >
                  {loading && <Loader2 className="w-4 h-4 mr-2 animate-spin" />}
                  Consolidate
                </Button>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>

      {/* Cost Analysis Dialog */}
      <Dialog open={costAnalysisDialogOpen} onOpenChange={setCostAnalysisDialogOpen}>
        <DialogContent className="max-w-4xl">
          <DialogHeader>
            <DialogTitle>Cost Analysis Results</DialogTitle>
            <DialogDescription>
              Detailed cost analysis using different costing methods
            </DialogDescription>
          </DialogHeader>
          {costAnalysis.length > 0 && (
            <div className="space-y-4">
              {costAnalysis.map((analysis, index) => (
                <div key={index} className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <Label className="text-sm font-medium text-muted-foreground">Product</Label>
                      <p className="text-sm font-medium">{analysis.productName}</p>
                    </div>
                    <div>
                      <Label className="text-sm font-medium text-muted-foreground">Branch</Label>
                      <p className="text-sm">{analysis.branchName}</p>
                    </div>
                  </div>
                  
                  <div className="grid grid-cols-4 gap-4">
                    <div>
                      <Label className="text-sm font-medium text-muted-foreground">Current Cost</Label>
                      <p className="text-sm">${analysis.currentCost.toFixed(2)}</p>
                    </div>
                    <div>
                      <Label className="text-sm font-medium text-muted-foreground">Weighted Average</Label>
                      <p className="text-sm">${analysis.weightedAverageCost.toFixed(2)}</p>
                    </div>
                    <div>
                      <Label className="text-sm font-medium text-muted-foreground">FIFO Cost</Label>
                      <p className="text-sm">${analysis.fifoCost.toFixed(2)}</p>
                    </div>
                    <div>
                      <Label className="text-sm font-medium text-muted-foreground">LIFO Cost</Label>
                      <p className="text-sm">${analysis.lifoCost.toFixed(2)}</p>
                    </div>
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <Label className="text-sm font-medium text-muted-foreground">Recommended Method</Label>
                      <Badge className="mt-1">{analysis.recommendedCostingMethod}</Badge>
                    </div>
                    <div>
                      <Label className="text-sm font-medium text-muted-foreground">Cost Variance</Label>
                      <p className="text-sm">
                        ${analysis.costVariance.toFixed(2)} ({analysis.costVariancePercentage.toFixed(1)}%)
                      </p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
};

export default OwnershipConsolidation;
