import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Badge } from './ui/badge';
import { Separator } from './ui/separator';
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
import { formatCurrency } from './utils/currency';
import {
  TrendingUp,
  TrendingDown,
  Package,
  AlertTriangle,
  Plus,
  Minus,
  ArrowUpDown,
  Search,
  Filter,
  History,
  Eye,
  Loader2,
  CheckCircle,
  XCircle,
  Building,
  Calendar,
  Weight,
  Edit,
} from 'lucide-react';
import { useAuth } from './AuthContext';
import { toast } from 'sonner';
import {
  useBranchInventory,
  useLowStockItems,
  useAddInventory,
  useAdjustInventory,
  useTransferInventory,
  usePaginatedInventoryMovements,
  useBranches,
  useProducts,
} from '../hooks/useApi';
import {
  InventoryDto,
  AddInventoryRequest,
  AdjustInventoryRequest,
  TransferInventoryRequest,
  InventoryMovementDto,
  BranchDto,
  Product,
  productOwnershipApi,
  ProductOwnershipDto,
} from '../services/api';

interface InventoryFormData {
  productId: number;
  branchId: number;
  quantity: number;
  weight: number;
  movementType: string;
  referenceNumber: string;
  unitCost: number;
  notes: string;
}

interface AdjustmentFormData {
  productId: number;
  branchId: number;
  newQuantity: number;
  newWeight: number;
  reason: string;
}

interface TransferFormData {
  productId: number;
  fromBranchId: number;
  toBranchId: number;
  quantity: number;
  weight: number;
  transferNumber: string;
  notes: string;
}

const defaultInventoryForm: InventoryFormData = {
  productId: 0,
  branchId: 1,
  quantity: 0,
  weight: 0,
  movementType: 'Purchase',
  referenceNumber: '',
  unitCost: 0,
  notes: '',
};

const defaultAdjustmentForm: AdjustmentFormData = {
  productId: 0,
  branchId: 1,
  newQuantity: 0,
  newWeight: 0,
  reason: '',
};

const defaultTransferForm: TransferFormData = {
  productId: 0,
  fromBranchId: 1,
  toBranchId: 1,
  quantity: 0,
  weight: 0,
  transferNumber: '',
  notes: '',
};

export default function Inventory() {
  const { user: currentUser, isManager } = useAuth();
  
  // State management
  const [activeTab, setActiveTab] = useState('inventory');
  const [selectedBranchId, setSelectedBranchId] = useState<number | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [includeZeroStock, setIncludeZeroStock] = useState(false);
  
  // Dialog states
  const [isAddInventoryOpen, setIsAddInventoryOpen] = useState(false);
  const [isAdjustInventoryOpen, setIsAdjustInventoryOpen] = useState(false);
  const [isTransferInventoryOpen, setIsTransferInventoryOpen] = useState(false);
  const [selectedInventoryItem, setSelectedInventoryItem] = useState<InventoryDto | null>(null);
  
  // Form states
  const [inventoryForm, setInventoryForm] = useState<InventoryFormData>(defaultInventoryForm);
  const [adjustmentForm, setAdjustmentForm] = useState<AdjustmentFormData>(defaultAdjustmentForm);
  const [transferForm, setTransferForm] = useState<TransferFormData>(defaultTransferForm);

  // API Hooks
  const { execute: fetchBranchInventory, loading: inventoryLoading, error: inventoryError } = useBranchInventory();
  const { execute: fetchLowStockItems, loading: lowStockLoading } = useLowStockItems();
  const { execute: addInventory, loading: addLoading } = useAddInventory();
  const { execute: adjustInventory, loading: adjustLoading } = useAdjustInventory();
  const { execute: transferInventory, loading: transferLoading } = useTransferInventory();
  const { execute: fetchBranches, loading: branchesLoading } = useBranches();
  const { execute: fetchProducts, loading: productsLoading } = useProducts();

  const {
    data: movementsData,
    loading: movementsLoading,
    error: movementsError,
    fetchData: refetchMovements,
    updateParams: updateMovementsParams,
    hasNextPage,
    hasPrevPage,
    nextPage,
    prevPage,
  } = usePaginatedInventoryMovements(
    selectedBranchId ? { branchId: selectedBranchId } : {}
  ) as {
    data: { items: InventoryMovementDto[]; totalCount: number; pageNumber: number; pageSize: number } | null;
    loading: boolean;
    error: string | null;
    fetchData: () => Promise<any>;
    updateParams: (params: any) => void;
    hasNextPage: boolean;
    hasPrevPage: boolean;
    nextPage: () => void;
    prevPage: () => void;
  };

  // Local state for inventory data
  const [inventoryItems, setInventoryItems] = useState<InventoryDto[]>([]);
  const [lowStockItems, setLowStockItems] = useState<InventoryDto[]>([]);
  const [branches, setBranches] = useState<BranchDto[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  
  // Ownership state
  const [productOwnership, setProductOwnership] = useState<{[key: number]: ProductOwnershipDto[]}>({});
  const [ownershipLoading, setOwnershipLoading] = useState(false);
  const [ownershipValidationErrors, setOwnershipValidationErrors] = useState<{[key: number]: string}>({});

  // Load branches and products on mount
  useEffect(() => {
    const loadInitialData = async () => {
      try {
        const [branchesResult, productsResult] = await Promise.all([
          fetchBranches(),
          fetchProducts({ isActive: true, pageSize: 1000 }), // Get all active products
        ]);
        setBranches(branchesResult.items);
        setProducts(productsResult.items);
        
        // Set default branch if not already set
        if (selectedBranchId === null && branchesResult.items.length > 0) {
          setSelectedBranchId(branchesResult.items[0].id);
        }
      } catch (error) {
        console.error('Failed to load initial data:', error);
      }
    };
    loadInitialData();
  }, [fetchBranches, fetchProducts, selectedBranchId]);

  // Load inventory data when branch changes
  useEffect(() => {
    const loadInventoryData = async () => {
      if (!selectedBranchId) return;
      
      try {
        const [inventory, lowStock] = await Promise.all([
          fetchBranchInventory(selectedBranchId, includeZeroStock),
          fetchLowStockItems(selectedBranchId),
        ]);
        setInventoryItems(inventory);
        setLowStockItems(lowStock);
        
        // Fetch ownership data for all products
        if (inventory.length > 0) {
          const ownershipPromises = inventory.map(item => 
            fetchProductOwnership(item.productId, selectedBranchId)
          );
          await Promise.all(ownershipPromises);
        }
      } catch (error) {
        console.error('Failed to load inventory data:', error);
        toast.error('Failed to load inventory data');
      }
    };
    
    loadInventoryData();
  }, [selectedBranchId, includeZeroStock, fetchBranchInventory, fetchLowStockItems]);

  // Update movements when branch changes
  useEffect(() => {
    if (selectedBranchId) {
      updateMovementsParams({
        branchId: selectedBranchId,
        pageNumber: 1,
      });
    } else {
      // Clear movements when no branch is selected
      updateMovementsParams({
        branchId: undefined,
        pageNumber: 1,
      });
    }
  }, [selectedBranchId, updateMovementsParams]);

  // Ownership validation functions
  const validateOwnershipForInventory = async (productId: number, branchId: number, quantity: number): Promise<boolean> => {
    if (!currentUser?.branch?.id) return true; // Skip validation if no branch info
    
    try {
      setOwnershipLoading(true);
      const validation = await productOwnershipApi.validateOwnership({
        productId,
        branchId,
        requestedQuantity: quantity
      });
      
      if (!validation.canSell) {
        setOwnershipValidationErrors(prev => ({
          ...prev,
          [productId]: validation.message
        }));
        return false;
      }
      
      // Clear any previous errors for this product
      setOwnershipValidationErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors[productId];
        return newErrors;
      });
      
      return true;
    } catch (error) {
      console.error('Ownership validation failed:', error);
      setOwnershipValidationErrors(prev => ({
        ...prev,
        [productId]: 'Failed to validate ownership'
      }));
      return false;
    } finally {
      setOwnershipLoading(false);
    }
  };

  const fetchProductOwnership = async (productId: number, branchId: number) => {
    try {
      const ownership = await productOwnershipApi.getProductOwnership(productId, branchId);
      setProductOwnership(prev => ({
        ...prev,
        [productId]: ownership
      }));
    } catch (error) {
      console.error('Failed to fetch product ownership:', error);
    }
  };

  // Filter inventory items
  const filteredInventoryItems = inventoryItems.filter(item => {
    const matchesSearch = searchQuery === '' || 
      item.productName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      item.productCode.toLowerCase().includes(searchQuery.toLowerCase());
    
    const matchesStatus = statusFilter === '' || statusFilter === 'all' ||
      (statusFilter === 'low_stock' && item.isLowStock) ||
      (statusFilter === 'normal_stock' && !item.isLowStock);
    
    return matchesSearch && matchesStatus;
  });

  // Handlers
  const handleAddInventory = async () => {
    if (!inventoryForm.productId || !inventoryForm.quantity || !inventoryForm.weight) {
      toast.error('Please fill in all required fields');
      return;
    }

    // Validate ownership for inventory addition
    const isValid = await validateOwnershipForInventory(
      inventoryForm.productId, 
      inventoryForm.branchId, 
      inventoryForm.quantity
    );
    
    if (!isValid) {
      toast.error('Cannot add inventory due to ownership restrictions');
      return;
    }

    try {
      const addInventoryData: AddInventoryRequest = {
        productId: inventoryForm.productId,
        branchId: inventoryForm.branchId,
        quantity: inventoryForm.quantity,
        weight: inventoryForm.weight,
        movementType: inventoryForm.movementType,
        referenceNumber: inventoryForm.referenceNumber || undefined,
        unitCost: inventoryForm.unitCost > 0 ? inventoryForm.unitCost : undefined,
        notes: inventoryForm.notes || undefined,
      };

      await addInventory(addInventoryData);
      toast.success('Inventory added successfully');
      setIsAddInventoryOpen(false);
      resetInventoryForm();
      refreshInventoryData();
      setOwnershipValidationErrors({}); // Clear ownership errors
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to add inventory');
    }
  };

  const handleAdjustInventory = async () => {
    if (!adjustmentForm.productId || !adjustmentForm.reason) {
      toast.error('Please fill in all required fields');
      return;
    }

    // Validate ownership for inventory adjustment
    const isValid = await validateOwnershipForInventory(
      adjustmentForm.productId, 
      adjustmentForm.branchId, 
      adjustmentForm.newQuantity
    );
    
    if (!isValid) {
      toast.error('Cannot adjust inventory due to ownership restrictions');
      return;
    }

    try {
      const adjustInventoryData: AdjustInventoryRequest = {
        productId: adjustmentForm.productId,
        branchId: adjustmentForm.branchId,
        newQuantity: adjustmentForm.newQuantity,
        newWeight: adjustmentForm.newWeight,
        reason: adjustmentForm.reason,
      };

      await adjustInventory(adjustInventoryData);
      toast.success('Inventory adjusted successfully');
      setIsAdjustInventoryOpen(false);
      resetAdjustmentForm();
      refreshInventoryData();
      setOwnershipValidationErrors({}); // Clear ownership errors
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to adjust inventory');
    }
  };

  const handleTransferInventory = async () => {
    if (!transferForm.productId || !transferForm.quantity || !transferForm.weight || !transferForm.transferNumber) {
      toast.error('Please fill in all required fields');
      return;
    }

    if (transferForm.fromBranchId === transferForm.toBranchId) {
      toast.error('From and To branches cannot be the same');
      return;
    }

    // Validate ownership for inventory transfer (from branch)
    const isValid = await validateOwnershipForInventory(
      transferForm.productId, 
      transferForm.fromBranchId, 
      transferForm.quantity
    );
    
    if (!isValid) {
      toast.error('Cannot transfer inventory due to ownership restrictions');
      return;
    }

    try {
      const transferInventoryData: TransferInventoryRequest = {
        productId: transferForm.productId,
        fromBranchId: transferForm.fromBranchId,
        toBranchId: transferForm.toBranchId,
        quantity: transferForm.quantity,
        weight: transferForm.weight,
        transferNumber: transferForm.transferNumber,
        notes: transferForm.notes || undefined,
      };

      await transferInventory(transferInventoryData);
      toast.success('Inventory transferred successfully');
      setIsTransferInventoryOpen(false);
      resetTransferForm();
      refreshInventoryData();
      setOwnershipValidationErrors({}); // Clear ownership errors
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to transfer inventory');
    }
  };

  const refreshInventoryData = async () => {
    if (!selectedBranchId) return;
    
    try {
      const [inventory, lowStock] = await Promise.all([
        fetchBranchInventory(selectedBranchId, includeZeroStock),
        fetchLowStockItems(selectedBranchId),
      ]);
      setInventoryItems(inventory);
      setLowStockItems(lowStock);
      refetchMovements();
    } catch (error) {
      console.error('Failed to refresh inventory data:', error);
    }
  };

  const resetInventoryForm = () => {
    setInventoryForm({ ...defaultInventoryForm, branchId: selectedBranchId || 1 });
  };

  const resetAdjustmentForm = () => {
    setAdjustmentForm({ ...defaultAdjustmentForm, branchId: selectedBranchId || 1 });
  };

  const resetTransferForm = () => {
    setTransferForm({ ...defaultTransferForm, fromBranchId: selectedBranchId || 1 });
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const getStockStatusColor = (item: InventoryDto) => {
    if (item.quantityOnHand === 0) return 'bg-red-100 text-red-800';
    if (item.isLowStock) return 'bg-yellow-100 text-yellow-800';
    return 'bg-green-100 text-green-800';
  };

  const getStockStatusText = (item: InventoryDto) => {
    if (item.quantityOnHand === 0) return 'Out of Stock';
    if (item.isLowStock) return 'Low Stock';
    return 'In Stock';
  };

  const getMovementTypeColor = (movementType: string) => {
    switch (movementType.toLowerCase()) {
      case 'purchase': return 'bg-green-100 text-green-800';
      case 'sale': return 'bg-blue-100 text-blue-800';
      case 'adjustment': return 'bg-yellow-100 text-yellow-800';
      case 'transfer': return 'bg-purple-100 text-purple-800';
      case 'return': return 'bg-orange-100 text-orange-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <h1 className="text-3xl text-[#0D1B2A]">Inventory Management</h1>
        <div className="flex space-x-2">
          <Select value={selectedBranchId?.toString() || ''} onValueChange={(value: string) => setSelectedBranchId(parseInt(value))}>
            <SelectTrigger className="w-48">
              <SelectValue placeholder={branchesLoading ? "Loading branches..." : "Select Branch"} />
            </SelectTrigger>
                            <SelectContent className="bg-white border-gray-200 shadow-lg">
                  {branchesLoading ? (
                    <SelectItem value="loading" disabled>Loading branches...</SelectItem>
                  ) : branches.length === 0 ? (
                    <SelectItem value="no-branches" disabled>No branches available</SelectItem>
                  ) : (
                    branches.map((branch) => (
                      <SelectItem key={branch.id} value={branch.id.toString()} className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">
                        {branch.name}
                      </SelectItem>
                    ))
                  )}
                </SelectContent>
          </Select>
          {isManager && (
            <>
              <Button 
                onClick={() => setIsAddInventoryOpen(true)}
                variant="golden"
              >
                <Plus className="mr-2 h-4 w-4" />
                Add Stock
              </Button>
              <Button 
                onClick={() => setIsAdjustInventoryOpen(true)}
                variant="outline"
              >
                <Edit className="mr-2 h-4 w-4" />
                Adjust Stock
              </Button>
              <Button 
                onClick={() => setIsTransferInventoryOpen(true)}
                variant="outline"
              >
                <ArrowUpDown className="mr-2 h-4 w-4" />
                Transfer Stock
              </Button>
            </>
          )}
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Total Items</p>
                <p className="text-3xl font-bold">{inventoryItems.length}</p>
              </div>
              <Package className="h-8 w-8 text-muted-foreground" />
            </div>
          </CardContent>
        </Card>

        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Low Stock Items</p>
                <p className="text-3xl font-bold text-orange-600">{lowStockItems.length}</p>
              </div>
              <AlertTriangle className="h-8 w-8 text-orange-500" />
            </div>
          </CardContent>
        </Card>

        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Total Quantity</p>
                <p className="text-3xl font-bold">
                  {inventoryItems.reduce((sum, item) => sum + item.quantityOnHand, 0)}
                </p>
              </div>
              <TrendingUp className="h-8 w-8 text-green-500" />
            </div>
          </CardContent>
        </Card>

        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Total Weight</p>
                <p className="text-3xl font-bold">
                  {inventoryItems.reduce((sum, item) => sum + item.weightOnHand, 0).toFixed(2)}g
                </p>
              </div>
              <Weight className="h-8 w-8 text-purple-500" />
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card className="pos-card">
        <CardContent className="pt-6">
          <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
            <div className="relative">
              <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search products..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-10"
              />
            </div>
            <Select value={statusFilter} onValueChange={setStatusFilter}>
              <SelectTrigger>
                <SelectValue placeholder="Filter by status" />
              </SelectTrigger>
              <SelectContent className="bg-white border-gray-200 shadow-lg">
                <SelectItem value="all" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">All Items</SelectItem>
                <SelectItem value="normal_stock" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Normal Stock</SelectItem>
                <SelectItem value="low_stock" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Low Stock</SelectItem>
              </SelectContent>
            </Select>
            <div className="flex items-center space-x-2">
              <input
                type="checkbox"
                id="includeZeroStock"
                checked={includeZeroStock}
                onChange={(e) => setIncludeZeroStock(e.target.checked)}
                className="rounded border-gray-300"
              />
              <Label htmlFor="includeZeroStock">Include Zero Stock</Label>
            </div>
            <div />
            <Button 
              variant="outline" 
              onClick={() => {
                setSearchQuery('');
                setStatusFilter('all');
                setIncludeZeroStock(false);
              }}
            >
              Clear Filters
            </Button>
          </div>
        </CardContent>
      </Card>

      <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="inventory">Current Inventory</TabsTrigger>
          <TabsTrigger value="movements">Stock Movements</TabsTrigger>
          <TabsTrigger value="alerts">Low Stock Alerts</TabsTrigger>
        </TabsList>

        <TabsContent value="inventory" className="space-y-4">
          <Card className="pos-card">
            <CardContent className="p-0">
              {!selectedBranchId && (
                <div className="text-center py-12 text-muted-foreground">
                  <Building className="h-12 w-12 mx-auto mb-4 text-muted-foreground" />
                  <p>Please select a branch to view inventory</p>
                </div>
              )}

              {selectedBranchId && inventoryLoading && (
                <div className="flex justify-center items-center py-12">
                  <Loader2 className="h-8 w-8 animate-spin" />
                </div>
              )}

              {selectedBranchId && inventoryError && (
                <div className="text-center py-12 text-red-600">
                  <p>Error loading inventory: {inventoryError}</p>
                  <Button onClick={refreshInventoryData} className="mt-4">
                    Try Again
                  </Button>
                </div>
              )}

              {selectedBranchId && !inventoryLoading && !inventoryError && (
                <>
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Product</TableHead>
                        <TableHead>Current Stock</TableHead>
                        <TableHead>Weight</TableHead>
                        <TableHead>Stock Levels</TableHead>
                        <TableHead>Status</TableHead>
                        <TableHead>Ownership</TableHead>
                        <TableHead>Last Updated</TableHead>
                        <TableHead>Actions</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {filteredInventoryItems.map((item) => (
                        <TableRow key={item.id}>
                          <TableCell>
                            <div className="flex items-center space-x-3">
                              <Package className="h-8 w-8 text-muted-foreground" />
                              <div>
                                <div className="font-medium">{item.productName}</div>
                                <div className="text-sm text-muted-foreground">
                                  {item.productCode}
                                </div>
                              </div>
                            </div>
                          </TableCell>
                          <TableCell>
                            <div className="font-medium">{item.quantityOnHand}</div>
                          </TableCell>
                          <TableCell>
                            <div className="font-medium">{item.weightOnHand.toFixed(2)}g</div>
                          </TableCell>
                          <TableCell>
                            <div className="space-y-1 text-sm">
                              <div>Min: {item.minimumStockLevel}</div>
                              <div>Max: {item.maximumStockLevel}</div>
                              <div>Reorder: {item.reorderPoint}</div>
                            </div>
                          </TableCell>
                          <TableCell>
                            <Badge 
                              variant="secondary" 
                              className={getStockStatusColor(item)}
                            >
                              {getStockStatusText(item)}
                            </Badge>
                          </TableCell>
                          <TableCell>
                            <div className="text-sm">
                              {ownershipLoading ? (
                                <Loader2 className="h-4 w-4 animate-spin" />
                              ) : productOwnership[item.productId]?.length > 0 ? (
                                <div className="space-y-1">
                                  {productOwnership[item.productId].map((ownership, index) => (
                                    <Badge 
                                      key={index}
                                      variant="outline" 
                                      className={
                                        ownership.ownershipPercentage >= 80 ? 'bg-green-100 text-green-800' :
                                        ownership.ownershipPercentage >= 50 ? 'bg-yellow-100 text-yellow-800' :
                                        'bg-red-100 text-red-800'
                                      }
                                    >
                                      {ownership.ownershipPercentage.toFixed(1)}%
                                    </Badge>
                                  ))}
                                </div>
                              ) : (
                                <span className="text-muted-foreground">No ownership</span>
                              )}
                            </div>
                          </TableCell>
                          <TableCell>
                            <div className="text-sm">
                              {formatDate(item.lastCountDate)}
                            </div>
                          </TableCell>
                          <TableCell>
                            <div className="flex space-x-2">
                              <Button
                                size="sm"
                                variant="ghost"
                                onClick={() => setSelectedInventoryItem(item)}
                              >
                                <Eye className="h-4 w-4" />
                              </Button>
                            </div>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>

                  {filteredInventoryItems.length === 0 && (
                    <div className="text-center py-12 text-muted-foreground">
                      No inventory items found
                    </div>
                  )}
                </>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="movements" className="space-y-4">
          <Card className="pos-card">
            <CardContent className="p-0">
              {!selectedBranchId && (
                <div className="text-center py-12 text-muted-foreground">
                  <Building className="h-12 w-12 mx-auto mb-4 text-muted-foreground" />
                  <p>Please select a branch to view movements</p>
                </div>
              )}

              {selectedBranchId && movementsLoading && (
                <div className="flex justify-center items-center py-12">
                  <Loader2 className="h-8 w-8 animate-spin" />
                </div>
              )}

              {selectedBranchId && movementsError && (
                <div className="text-center py-12 text-red-600">
                  <p>Error loading movements: {movementsError}</p>
                  <Button onClick={refetchMovements} className="mt-4">
                    Try Again
                  </Button>
                </div>
              )}

              {selectedBranchId && movementsData && movementsData.items && (
                <>
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Product</TableHead>
                        <TableHead>Movement Type</TableHead>
                        <TableHead>Quantity Change</TableHead>
                        <TableHead>Weight Change</TableHead>
                        <TableHead>Balance After</TableHead>
                        <TableHead>Reference</TableHead>
                        <TableHead>Date</TableHead>
                        <TableHead>User</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {movementsData.items.map((movement) => (
                        <TableRow key={movement.id}>
                          <TableCell>
                            <div>
                              <div className="font-medium">{movement.productName}</div>
                              <div className="text-sm text-muted-foreground">
                                {movement.productCode}
                              </div>
                            </div>
                          </TableCell>
                          <TableCell>
                            <Badge 
                              variant="secondary" 
                              className={getMovementTypeColor(movement.movementType)}
                            >
                              {movement.movementType}
                            </Badge>
                          </TableCell>
                          <TableCell>
                            <div className={movement.quantityChange >= 0 ? 'text-green-600' : 'text-red-600'}>
                              {movement.quantityChange >= 0 ? '+' : ''}{movement.quantityChange}
                            </div>
                          </TableCell>
                          <TableCell>
                            <div className={movement.weightChange >= 0 ? 'text-green-600' : 'text-red-600'}>
                              {movement.weightChange >= 0 ? '+' : ''}{movement.weightChange.toFixed(2)}g
                            </div>
                          </TableCell>
                          <TableCell>
                            <div>
                              <div>Qty: {movement.quantityBalance}</div>
                              <div className="text-sm text-muted-foreground">
                                Weight: {movement.weightBalance.toFixed(2)}g
                              </div>
                            </div>
                          </TableCell>
                          <TableCell>
                            <div className="text-sm">
                              {movement.referenceNumber || '-'}
                            </div>
                          </TableCell>
                          <TableCell>
                            <div className="text-sm">
                              {formatDate(movement.createdAt)}
                            </div>
                          </TableCell>
                          <TableCell>
                            <div className="text-sm">
                              {movement.createdBy}
                            </div>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>

                  {/* Pagination */}
                  <div className="flex items-center justify-between px-6 py-4 border-t">
                    <div className="text-sm text-muted-foreground">
                      Showing {movementsData.items.length} of {movementsData.totalCount} movements
                    </div>
                    <div className="flex space-x-2">
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={prevPage}
                        disabled={!hasPrevPage}
                      >
                        Previous
                      </Button>
                      <span className="flex items-center px-3 text-sm">
                        Page {movementsData.pageNumber} of {Math.ceil(movementsData.totalCount / movementsData.pageSize)}
                      </span>
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={nextPage}
                        disabled={!hasNextPage}
                      >
                        Next
                      </Button>
                    </div>
                  </div>
                </>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="alerts" className="space-y-4">
          <Card className="pos-card">
            <CardHeader>
              <CardTitle className="flex items-center">
                <AlertTriangle className="mr-2 h-5 w-5 text-orange-500" />
                Low Stock Alerts
              </CardTitle>
              <CardDescription>
                Items that require immediate attention
              </CardDescription>
            </CardHeader>
            <CardContent>
              {!selectedBranchId && (
                <div className="text-center py-8 text-muted-foreground">
                  <Building className="h-12 w-12 mx-auto mb-4 text-muted-foreground" />
                  <p>Please select a branch to view alerts</p>
                </div>
              )}

              {selectedBranchId && lowStockLoading && (
                <div className="flex justify-center py-8">
                  <Loader2 className="h-6 w-6 animate-spin" />
                </div>
              )}

              {selectedBranchId && !lowStockLoading && lowStockItems.length > 0 ? (
                <div className="space-y-4">
                  {lowStockItems.map((item) => (
                    <div key={item.id} className="flex items-center justify-between p-4 border rounded-lg">
                      <div className="flex items-center space-x-4">
                        <AlertTriangle className="h-8 w-8 text-orange-500" />
                        <div>
                          <h3 className="font-medium">{item.productName}</h3>
                          <p className="text-sm text-muted-foreground">{item.productCode}</p>
                        </div>
                      </div>
                      <div className="text-right">
                        <div className="text-lg font-semibold text-orange-600">
                          {item.quantityOnHand} / {item.reorderPoint}
                        </div>
                        <div className="text-sm text-muted-foreground">
                          Current / Reorder Point
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              ) : selectedBranchId && !lowStockLoading && lowStockItems.length === 0 ? (
                <div className="text-center py-8 text-muted-foreground">
                  <CheckCircle className="h-12 w-12 mx-auto mb-4 text-green-500" />
                  <p>All items are adequately stocked!</p>
                </div>
              ) : null}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Add Inventory Dialog */}
      <Dialog open={isAddInventoryOpen} onOpenChange={setIsAddInventoryOpen}>
        <DialogContent className="max-w-2xl bg-white border-gray-200 shadow-lg">
          <DialogHeader>
            <DialogTitle>Add Inventory</DialogTitle>
            <DialogDescription>
              Add stock for existing products through purchases or adjustments.
            </DialogDescription>
          </DialogHeader>
          
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="productId">Product *</Label>
              <Select 
                value={inventoryForm.productId.toString()} 
                onValueChange={(value: string) => setInventoryForm({...inventoryForm, productId: parseInt(value) || 0})}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select a product" />
                </SelectTrigger>
                <SelectContent className="bg-white border-gray-200 shadow-lg max-h-60">
                  {productsLoading ? (
                    <SelectItem value="loading" disabled>Loading products...</SelectItem>
                  ) : (
                    products.map((product) => (
                      <SelectItem 
                        key={product.id} 
                        value={product.id.toString()} 
                        className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]"
                      >
                        {product.productCode} - {product.name}
                      </SelectItem>
                    ))
                  )}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="movementType">Movement Type</Label>
              <Select value={inventoryForm.movementType} onValueChange={(value: string) => setInventoryForm({...inventoryForm, movementType: value})}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent className="bg-white border-gray-200 shadow-lg">
                  <SelectItem value="Purchase" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Purchase</SelectItem>
                  <SelectItem value="Adjustment" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Adjustment</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="quantity">Quantity *</Label>
              <Input
                id="quantity"
                type="number"
                value={inventoryForm.quantity}
                onChange={(e) => setInventoryForm({...inventoryForm, quantity: parseFloat(e.target.value) || 0})}
                placeholder="Enter quantity"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="weight">Weight (grams) *</Label>
              <Input
                id="weight"
                type="number"
                step="0.01"
                value={inventoryForm.weight}
                onChange={(e) => setInventoryForm({...inventoryForm, weight: parseFloat(e.target.value) || 0})}
                placeholder="Enter weight"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="referenceNumber">Reference Number</Label>
              <Input
                id="referenceNumber"
                value={inventoryForm.referenceNumber}
                onChange={(e) => setInventoryForm({...inventoryForm, referenceNumber: e.target.value})}
                placeholder="PO number, invoice, etc."
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="unitCost">Unit Cost</Label>
              <Input
                id="unitCost"
                type="number"
                step="0.01"
                value={inventoryForm.unitCost}
                onChange={(e) => setInventoryForm({...inventoryForm, unitCost: parseFloat(e.target.value) || 0})}
                placeholder="Cost per unit"
              />
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="notes">Notes</Label>
            <Input
              id="notes"
              value={inventoryForm.notes}
              onChange={(e) => setInventoryForm({...inventoryForm, notes: e.target.value})}
              placeholder="Additional notes"
            />
          </div>
          
          <div className="flex justify-end gap-3">
            <Button variant="outline" onClick={() => {
              setIsAddInventoryOpen(false);
              resetInventoryForm();
            }}>
              Cancel
            </Button>
            <Button 
              onClick={handleAddInventory} 
              disabled={addLoading || ownershipLoading}
              variant="golden"
            >
              {(addLoading || ownershipLoading) && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {ownershipLoading ? 'Validating Ownership...' : 'Add Inventory'}
            </Button>
          </div>
        </DialogContent>
      </Dialog>

      {/* Adjust Inventory Dialog */}
      <Dialog open={isAdjustInventoryOpen} onOpenChange={setIsAdjustInventoryOpen}>
        <DialogContent className="max-w-2xl bg-white border-gray-200 shadow-lg">
          <DialogHeader>
            <DialogTitle>Adjust Inventory</DialogTitle>
            <DialogDescription>
              Manually adjust inventory levels for stock corrections.
            </DialogDescription>
          </DialogHeader>
          
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="adjustProductId">Product *</Label>
              <Select 
                value={adjustmentForm.productId.toString()} 
                onValueChange={(value: string) => setAdjustmentForm({...adjustmentForm, productId: parseInt(value) || 0})}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select a product" />
                </SelectTrigger>
                <SelectContent className="bg-white border-gray-200 shadow-lg max-h-60">
                  {productsLoading ? (
                    <SelectItem value="loading" disabled>Loading products...</SelectItem>
                  ) : (
                    products.map((product) => (
                      <SelectItem 
                        key={product.id} 
                        value={product.id.toString()} 
                        className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]"
                      >
                        {product.productCode} - {product.name}
                      </SelectItem>
                    ))
                  )}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="reason">Reason *</Label>
              <Input
                id="reason"
                value={adjustmentForm.reason}
                onChange={(e) => setAdjustmentForm({...adjustmentForm, reason: e.target.value})}
                placeholder="Reason for adjustment"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="newQuantity">New Quantity *</Label>
              <Input
                id="newQuantity"
                type="number"
                value={adjustmentForm.newQuantity}
                onChange={(e) => setAdjustmentForm({...adjustmentForm, newQuantity: parseFloat(e.target.value) || 0})}
                placeholder="Set new quantity"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="newWeight">New Weight (grams) *</Label>
              <Input
                id="newWeight"
                type="number"
                step="0.01"
                value={adjustmentForm.newWeight}
                onChange={(e) => setAdjustmentForm({...adjustmentForm, newWeight: parseFloat(e.target.value) || 0})}
                placeholder="Set new weight"
              />
            </div>
          </div>
          
          <div className="flex justify-end gap-3 mt-6">
            <Button variant="outline" onClick={() => {
              setIsAdjustInventoryOpen(false);
              resetAdjustmentForm();
            }}>
              Cancel
            </Button>
            <Button 
              onClick={handleAdjustInventory} 
              disabled={adjustLoading || ownershipLoading}
              variant="golden"
            >
              {(adjustLoading || ownershipLoading) && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {ownershipLoading ? 'Validating Ownership...' : 'Adjust Inventory'}
            </Button>
          </div>
        </DialogContent>
      </Dialog>

      {/* Transfer Inventory Dialog */}
      <Dialog open={isTransferInventoryOpen} onOpenChange={setIsTransferInventoryOpen}>
        <DialogContent className="max-w-2xl bg-white border-gray-200 shadow-lg">
          <DialogHeader>
            <DialogTitle>Transfer Inventory</DialogTitle>
            <DialogDescription>
              Transfer stock between branches.
            </DialogDescription>
          </DialogHeader>
          
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="transferProductId">Product *</Label>
              <Select 
                value={transferForm.productId.toString()} 
                onValueChange={(value: string) => setTransferForm({...transferForm, productId: parseInt(value) || 0})}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select a product" />
                </SelectTrigger>
                <SelectContent className="bg-white border-gray-200 shadow-lg max-h-60">
                  {productsLoading ? (
                    <SelectItem value="loading" disabled>Loading products...</SelectItem>
                  ) : (
                    products.map((product) => (
                      <SelectItem 
                        key={product.id} 
                        value={product.id.toString()} 
                        className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]"
                      >
                        {product.productCode} - {product.name}
                      </SelectItem>
                    ))
                  )}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="transferNumber">Transfer Number *</Label>
              <Input
                id="transferNumber"
                value={transferForm.transferNumber}
                onChange={(e) => setTransferForm({...transferForm, transferNumber: e.target.value})}
                placeholder="Transfer reference number"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="fromBranch">From Branch</Label>
              <Select value={transferForm.fromBranchId.toString()} onValueChange={(value: string) => setTransferForm({...transferForm, fromBranchId: parseInt(value)})}>
                <SelectTrigger>
                  <SelectValue />
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
            <div className="space-y-2">
              <Label htmlFor="toBranch">To Branch</Label>
              <Select value={transferForm.toBranchId.toString()} onValueChange={(value: string) => setTransferForm({...transferForm, toBranchId: parseInt(value)})}>
                <SelectTrigger>
                  <SelectValue />
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
            <div className="space-y-2">
              <Label htmlFor="transferQuantity">Quantity *</Label>
              <Input
                id="transferQuantity"
                type="number"
                value={transferForm.quantity}
                onChange={(e) => setTransferForm({...transferForm, quantity: parseFloat(e.target.value) || 0})}
                placeholder="Quantity to transfer"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="transferWeight">Weight (grams) *</Label>
              <Input
                id="transferWeight"
                type="number"
                step="0.01"
                value={transferForm.weight}
                onChange={(e) => setTransferForm({...transferForm, weight: parseFloat(e.target.value) || 0})}
                placeholder="Weight to transfer"
              />
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="transferNotes">Notes</Label>
            <Input
              id="transferNotes"
              value={transferForm.notes}
              onChange={(e) => setTransferForm({...transferForm, notes: e.target.value})}
              placeholder="Transfer notes"
            />
          </div>
          
          <div className="flex justify-end gap-3 mt-6">
            <Button variant="outline" onClick={() => {
              setIsTransferInventoryOpen(false);
              resetTransferForm();
            }}>
              Cancel
            </Button>
            <Button 
              onClick={handleTransferInventory} 
              disabled={transferLoading || ownershipLoading}
              variant="golden"
            >
              {(transferLoading || ownershipLoading) && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {ownershipLoading ? 'Validating Ownership...' : 'Transfer Inventory'}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
