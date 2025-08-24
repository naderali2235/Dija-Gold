import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Badge } from './ui/badge';
import { Textarea } from './ui/textarea';
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
  DialogTrigger,
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
  FileText,
  Plus,
  Search,
  Eye,
  Edit,
  Truck,
  Calendar,
  DollarSign,
  Package,
  CheckCircle,
  Clock,
  AlertTriangle,
  X,
  Loader2,
} from 'lucide-react';
import { useAuth } from './AuthContext';
import { formatCurrency } from './utils/currency';
import { 
  PurchaseOrderDto, 
  PurchaseOrderItemDto,
  CreatePurchaseOrderRequest,
  CreatePurchaseOrderItemRequest,
  ReceivePurchaseOrderRequest,
  ReceivePurchaseOrderItemDto,
  SupplierDto,
  Product,
  BranchDto
} from '../services/api';
import { 
  useSearchPurchaseOrders,
  useCreatePurchaseOrder,
  useReceivePurchaseOrder,
  usePaginatedSuppliers,
  usePaginatedProducts,
  usePaginatedBranches
} from '../hooks/useApi';
import { toast } from 'sonner';

export default function PurchaseOrders() {
  const { isManager } = useAuth();
  const [activeTab, setActiveTab] = useState('all');
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [isNewPOOpen, setIsNewPOOpen] = useState(false);
  const [selectedPO, setSelectedPO] = useState<PurchaseOrderDto | null>(null);
  const [isEditMode, setIsEditMode] = useState(false);

  // API hooks
  const { 
    data: purchaseOrdersData, 
    loading: purchaseOrdersLoading, 
    error: purchaseOrdersError,
    execute: searchPurchaseOrders 
  } = useSearchPurchaseOrders();

  const { 
    loading: createLoading, 
    error: createError,
    execute: createPurchaseOrder 
  } = useCreatePurchaseOrder();

  const { 
    loading: receiveLoading, 
    error: receiveError,
    execute: receivePurchaseOrder 
  } = useReceivePurchaseOrder();

  // Load suppliers, products, and branches
  const { 
    data: suppliersData, 
    loading: suppliersLoading,
    fetchData: fetchSuppliers 
  } = usePaginatedSuppliers({
    pageNumber: 1,
    pageSize: 100,
    isActive: true
  });

  const { 
    data: productsData, 
    loading: productsLoading,
    fetchData: fetchProducts 
  } = usePaginatedProducts({
    pageNumber: 1,
    pageSize: 100,
    isActive: true
  });

  const { 
    data: branchesData, 
    loading: branchesLoading,
    fetchData: fetchBranches 
  } = usePaginatedBranches({
    pageNumber: 1,
    pageSize: 100,
    isActive: true
  });

  // Form state for new PO
  const [poForm, setPOForm] = useState({
    supplierId: '',
    branchId: '',
    expectedDelivery: '',
    terms: '',
    notes: '',
    items: [{ productId: '', quantity: '', weight: '', unitCost: '', notes: '' }],
  });

  // Load data on component mount
  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      // Load purchase orders
      await searchPurchaseOrders({
        pageNumber: 1,
        pageSize: 100
      });

      // Load suppliers, products, and branches
      await Promise.all([
        fetchSuppliers(),
        fetchProducts(),
        fetchBranches()
      ]);
    } catch (error) {
      console.error('Error loading data:', error);
      toast.error('Failed to load data');
    }
  };

  // Extract data from API responses
  const purchaseOrders = purchaseOrdersData?.items || [];
  const suppliers = suppliersData?.items || [];
  const products = productsData?.items || [];
  const branches = branchesData?.items || [];

  const filteredPOs = purchaseOrders.filter(po => {
    const matchesSearch = 
      po.purchaseOrderNumber.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (po.supplierId && po.supplierId.toString().includes(searchQuery.toLowerCase()));
    
    const matchesStatus = statusFilter === 'all' || po.status === statusFilter;
    const matchesTab = activeTab === 'all' || po.status === activeTab;
    
    return matchesSearch && matchesStatus && matchesTab;
  });

  const getStatusBadge = (status: string) => {
    const variants = {
      'Pending': { variant: 'outline' as const, className: 'bg-gray-100 text-gray-800', icon: Edit },
      'Sent': { variant: 'default' as const, className: 'bg-blue-100 text-blue-800', icon: Truck },
      'Confirmed': { variant: 'default' as const, className: 'bg-yellow-100 text-yellow-800', icon: CheckCircle },
      'Received': { variant: 'default' as const, className: 'bg-green-100 text-green-800', icon: Package },
      'Cancelled': { variant: 'destructive' as const, className: 'bg-red-100 text-red-800', icon: X },
    };

    const config = variants[status as keyof typeof variants] || variants['Pending'];
    const Icon = config.icon;

    return (
      <Badge variant={config.variant} className={config.className}>
        <Icon className="mr-1 h-3 w-3" />
        {status.toUpperCase()}
      </Badge>
    );
  };

  const addPOItem = () => {
    setPOForm({
      ...poForm,
      items: [...poForm.items, { productId: '', quantity: '', weight: '', unitCost: '', notes: '' }],
    });
  };

  const removePOItem = (index: number) => {
    const newItems = poForm.items.filter((_, i) => i !== index);
    setPOForm({ ...poForm, items: newItems });
  };

  const updatePOItem = (index: number, field: string, value: string) => {
    const newItems = [...poForm.items];
    newItems[index] = { ...newItems[index], [field]: value };
    setPOForm({ ...poForm, items: newItems });
  };

  const calculateTotal = (quantity: string, unitCost: string) => {
    const qty = parseFloat(quantity) || 0;
    const cost = parseFloat(unitCost) || 0;
    return qty * cost;
  };

  const getTotalAmount = () => {
    return poForm.items.reduce((sum, item) => {
      return sum + calculateTotal(item.quantity, item.unitCost);
    }, 0);
  };

  const handleCreatePO = async () => {
    if (!isManager) {
      toast.error('Only managers can create purchase orders');
      return;
    }

    if (!poForm.supplierId || !poForm.branchId) {
      toast.error('Please select supplier and branch');
      return;
    }

    if (poForm.items.length === 0 || poForm.items.some(item => !item.productId || !item.quantity || !item.unitCost)) {
      toast.error('Please add at least one item with all required fields');
      return;
    }

    try {
      const request: CreatePurchaseOrderRequest = {
        supplierId: parseInt(poForm.supplierId),
        branchId: parseInt(poForm.branchId),
        expectedDeliveryDate: poForm.expectedDelivery || undefined,
        terms: poForm.terms || undefined,
        notes: poForm.notes || undefined,
        items: poForm.items.map(item => ({
          productId: parseInt(item.productId),
          quantityOrdered: parseFloat(item.quantity),
          weightOrdered: parseFloat(item.weight),
          unitCost: parseFloat(item.unitCost),
          notes: item.notes || undefined
        }))
      };

      await createPurchaseOrder(request);
      toast.success('Purchase order created successfully');
      setIsNewPOOpen(false);
      resetForm();
      loadData(); // Reload the data
    } catch (error) {
      console.error('Error creating purchase order:', error);
      toast.error('Failed to create purchase order');
    }
  };

  const handleStatusUpdate = async (poId: number, newStatus: string) => {
    if (!isManager) {
      toast.error('Only managers can update purchase order status');
      return;
    }

    try {
      // Note: The current API only has endpoints for create, get, search, and receive
      // Status updates for 'sent', 'confirmed', etc. would need additional API endpoints
      // For now, we'll show a message indicating this functionality needs backend support
      toast.info(`Status update to ${newStatus} requires backend API enhancement`);
      
      // Uncomment below when status update API endpoint is available:
      // await purchaseOrdersApi.updateStatus(poId, newStatus);
      // toast.success(`Purchase order status updated to ${newStatus}`);
      // loadData(); // Reload the data
    } catch (error) {
      console.error('Error updating purchase order status:', error);
      toast.error('Failed to update purchase order status');
    }
  };

  const handleReceivePO = async (poId: number, items: ReceivePurchaseOrderItemDto[]) => {
    if (!isManager) {
      toast.error('Only managers can receive purchase orders');
      return;
    }

    try {
      const request: ReceivePurchaseOrderRequest = {
        purchaseOrderId: poId,
        items: items
      };

      await receivePurchaseOrder(request);
      toast.success('Purchase order received successfully');
      loadData(); // Reload the data
    } catch (error) {
      console.error('Error receiving purchase order:', error);
      toast.error('Failed to receive purchase order');
    }
  };

  const resetForm = () => {
    setPOForm({
      supplierId: '',
      branchId: '',
      expectedDelivery: '',
      terms: '',
      notes: '',
      items: [{ productId: '', quantity: '', weight: '', unitCost: '', notes: '' }],
    });
  };

  const poStats = {
    total: purchaseOrders.length,
    pending: purchaseOrders.filter(po => ['Pending', 'Sent', 'Confirmed'].includes(po.status)).length,
    received: purchaseOrders.filter(po => po.status === 'Received').length,
    totalValue: purchaseOrders.reduce((sum, po) => sum + po.totalAmount, 0),
  };

  // Helper function to get supplier name by ID
  const getSupplierName = (supplierId: number) => {
    const supplier = suppliers.find(s => s.id === supplierId);
    return supplier ? supplier.companyName : `Supplier ID: ${supplierId}`;
  };

  // Helper function to get branch name by ID
  const getBranchName = (branchId: number) => {
    const branch = branches.find(b => b.id === branchId);
    return branch ? branch.name : `Branch ID: ${branchId}`;
  };

  // Helper function to get product name by ID
  const getProductName = (productId: number) => {
    const product = products.find(p => p.id === productId);
    return product ? product.name : `Product ID: ${productId}`;
  };

  // Helper function to get product code by ID
  const getProductCode = (productId: number) => {
    const product = products.find(p => p.id === productId);
    return product ? product.productCode : '';
  };

  const loading = purchaseOrdersLoading || suppliersLoading || productsLoading || branchesLoading || createLoading || receiveLoading;

  if (loading && purchaseOrders.length === 0) {
    return (
      <div className="flex items-center justify-center h-64">
        <Loader2 className="h-8 w-8 animate-spin" />
        <span className="ml-2">Loading purchase orders...</span>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl text-[#0D1B2A]">Purchase Orders</h1>
          <p className="text-muted-foreground">Manage supplier purchase orders and deliveries</p>
        </div>
        {isManager && (
          <Dialog open={isNewPOOpen} onOpenChange={setIsNewPOOpen}>
            <DialogTrigger asChild>
              <Button className="touch-target pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
                <Plus className="mr-2 h-4 w-4" />
                New Purchase Order
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
              <DialogHeader>
                <DialogTitle>Create Purchase Order</DialogTitle>
                <DialogDescription>
                  Create a new purchase order for supplier inventory
                </DialogDescription>
              </DialogHeader>
              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label>Supplier</Label>
                    <Select value={poForm.supplierId} onValueChange={(value) => setPOForm({...poForm, supplierId: value})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select supplier" />
                      </SelectTrigger>
                      <SelectContent>
                        {suppliers.map(supplier => (
                          <SelectItem key={supplier.id} value={supplier.id.toString()}>
                            {supplier.companyName} - {supplier.contactPersonName || 'No contact'}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label>Branch</Label>
                    <Select value={poForm.branchId} onValueChange={(value) => setPOForm({...poForm, branchId: value})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select branch" />
                      </SelectTrigger>
                      <SelectContent>
                        {branches.map(branch => (
                          <SelectItem key={branch.id} value={branch.id.toString()}>
                            {branch.name} ({branch.code})
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label>Expected Delivery</Label>
                    <Input
                      type="date"
                      value={poForm.expectedDelivery}
                      onChange={(e) => setPOForm({...poForm, expectedDelivery: e.target.value})}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label>Terms</Label>
                    <Input
                      value={poForm.terms}
                      onChange={(e) => setPOForm({...poForm, terms: e.target.value})}
                      placeholder="Payment terms"
                    />
                  </div>
                </div>

                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <Label>Items</Label>
                    <Button type="button" variant="outline" onClick={addPOItem} size="sm">
                      <Plus className="mr-1 h-3 w-3" />
                      Add Item
                    </Button>
                  </div>
                  
                  {poForm.items.map((item, index) => (
                    <div key={index} className="grid grid-cols-12 gap-2 items-end">
                      <div className="col-span-3 space-y-1">
                        <Label className="text-xs">Product</Label>
                        <Select value={item.productId} onValueChange={(value) => updatePOItem(index, 'productId', value)}>
                          <SelectTrigger>
                            <SelectValue placeholder="Select product" />
                          </SelectTrigger>
                          <SelectContent>
                            {products.map(product => (
                              <SelectItem key={product.id} value={product.id.toString()}>
                                {product.name} ({product.productCode})
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </div>
                      <div className="col-span-2 space-y-1">
                        <Label className="text-xs">Quantity</Label>
                        <Input
                          type="number"
                          value={item.quantity}
                          onChange={(e) => updatePOItem(index, 'quantity', e.target.value)}
                          placeholder="0"
                        />
                      </div>
                      <div className="col-span-2 space-y-1">
                        <Label className="text-xs">Weight (g)</Label>
                        <Input
                          type="number"
                          value={item.weight}
                          onChange={(e) => updatePOItem(index, 'weight', e.target.value)}
                          placeholder="0.00"
                        />
                      </div>
                      <div className="col-span-2 space-y-1">
                        <Label className="text-xs">Unit Cost</Label>
                        <Input
                          type="number"
                          value={item.unitCost}
                          onChange={(e) => updatePOItem(index, 'unitCost', e.target.value)}
                          placeholder="0.00"
                        />
                      </div>
                      <div className="col-span-2 space-y-1">
                        <Label className="text-xs">Total</Label>
                        <Input
                          value={formatCurrency(calculateTotal(item.quantity, item.unitCost))}
                          readOnly
                          className="bg-muted"
                        />
                      </div>
                      <div className="col-span-1">
                        {poForm.items.length > 1 && (
                          <Button
                            type="button"
                            variant="ghost"
                            size="sm"
                            onClick={() => removePOItem(index)}
                          >
                            <X className="h-4 w-4" />
                          </Button>
                        )}
                      </div>
                    </div>
                  ))}
                  
                  <div className="text-right pt-4 border-t">
                    <p className="text-lg font-semibold">
                      Total: {formatCurrency(getTotalAmount())}
                    </p>
                  </div>
                </div>

                <div className="space-y-2">
                  <Label>Notes</Label>
                  <Textarea
                    value={poForm.notes}
                    onChange={(e) => setPOForm({...poForm, notes: e.target.value})}
                    placeholder="Additional notes for this purchase order"
                  />
                </div>
              </div>
              <div className="flex justify-end gap-3 mt-6">
                <Button variant="outline" onClick={() => setIsNewPOOpen(false)}>
                  Cancel
                </Button>
                <Button 
                  onClick={handleCreatePO} 
                  disabled={createLoading}
                  className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]"
                >
                  {createLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                  Create Purchase Order
                </Button>
              </div>
            </DialogContent>
          </Dialog>
        )}
      </div>

      {/* PO Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total Orders</p>
                <p className="text-2xl text-[#0D1B2A]">{poStats.total}</p>
              </div>
              <FileText className="h-8 w-8 text-[#D4AF37]" />
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Pending</p>
                <p className="text-2xl text-[#0D1B2A]">{poStats.pending}</p>
              </div>
              <Clock className="h-8 w-8 text-yellow-600" />
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Received</p>
                <p className="text-2xl text-[#0D1B2A]">{poStats.received}</p>
              </div>
              <CheckCircle className="h-8 w-8 text-green-600" />
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total Value</p>
                <p className="text-2xl text-[#0D1B2A]">{formatCurrency(poStats.totalValue)}</p>
              </div>
              <DollarSign className="h-8 w-8 text-blue-600" />
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card className="pos-card">
        <CardContent className="pt-6">
          <div className="flex flex-col md:flex-row gap-4">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search by PO number or supplier name..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-10 touch-target"
              />
            </div>
            <Select value={statusFilter} onValueChange={setStatusFilter}>
              <SelectTrigger className="w-full md:w-40">
                <SelectValue placeholder="Status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="Pending">Pending</SelectItem>
                <SelectItem value="Sent">Sent</SelectItem>
                <SelectItem value="Confirmed">Confirmed</SelectItem>
                <SelectItem value="Received">Received</SelectItem>
                <SelectItem value="Cancelled">Cancelled</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Status Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="grid w-full grid-cols-6">
          <TabsTrigger value="all">All Orders</TabsTrigger>
          <TabsTrigger value="Pending">Pending</TabsTrigger>
          <TabsTrigger value="Sent">Sent</TabsTrigger>
          <TabsTrigger value="Confirmed">Confirmed</TabsTrigger>
          <TabsTrigger value="Received">Received</TabsTrigger>
          <TabsTrigger value="Cancelled">Cancelled</TabsTrigger>
        </TabsList>

        <TabsContent value={activeTab} className="mt-6">
          <Card className="pos-card">
            <CardHeader>
              <CardTitle>Purchase Orders</CardTitle>
              <CardDescription>
                {filteredPOs.length} purchase order(s) found
              </CardDescription>
            </CardHeader>
            <CardContent>
              {filteredPOs.length === 0 ? (
                <div className="text-center py-8">
                  <FileText className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                  <p className="text-muted-foreground">No purchase orders found</p>
                </div>
              ) : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>PO Number</TableHead>
                      <TableHead>Supplier</TableHead>
                      <TableHead>Branch</TableHead>
                      <TableHead>Order Date</TableHead>
                      <TableHead>Expected Delivery</TableHead>
                      <TableHead>Total Amount</TableHead>
                      <TableHead>Status</TableHead>
                      <TableHead>Actions</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {filteredPOs.map((po) => (
                      <TableRow key={po.id}>
                        <TableCell className="font-medium">{po.purchaseOrderNumber}</TableCell>
                        <TableCell>
                          <div>
                            <p className="font-medium">{getSupplierName(po.supplierId)}</p>
                            <p className="text-sm text-muted-foreground">ID: {po.supplierId}</p>
                          </div>
                        </TableCell>
                        <TableCell>
                          <div>
                            <p className="font-medium">{getBranchName(po.branchId)}</p>
                            <p className="text-sm text-muted-foreground">ID: {po.branchId}</p>
                          </div>
                        </TableCell>
                        <TableCell>{new Date(po.orderDate).toLocaleDateString()}</TableCell>
                        <TableCell>
                          <div>
                            <p>{po.expectedDeliveryDate ? new Date(po.expectedDeliveryDate).toLocaleDateString() : 'Not set'}</p>
                            {po.actualDeliveryDate && (
                              <p className="text-sm text-green-600">
                                Delivered: {new Date(po.actualDeliveryDate).toLocaleDateString()}
                              </p>
                            )}
                          </div>
                        </TableCell>
                        <TableCell>{formatCurrency(po.totalAmount)}</TableCell>
                        <TableCell>{getStatusBadge(po.status)}</TableCell>
                        <TableCell>
                          <div className="flex gap-2">
                            <Dialog>
                              <DialogTrigger asChild>
                                <Button variant="outline" size="sm" className="touch-target">
                                  <Eye className="h-4 w-4" />
                                </Button>
                              </DialogTrigger>
                              <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
                                <DialogHeader>
                                  <DialogTitle>{po.purchaseOrderNumber} - Details</DialogTitle>
                                </DialogHeader>
                                <div className="space-y-6">
                                  <div className="grid grid-cols-2 gap-4">
                                    <div>
                                      <Label>Supplier</Label>
                                      <p className="font-medium">{getSupplierName(po.supplierId)}</p>
                                      <p className="text-sm text-muted-foreground">ID: {po.supplierId}</p>
                                    </div>
                                    <div>
                                      <Label>Branch</Label>
                                      <p className="font-medium">{getBranchName(po.branchId)}</p>
                                      <p className="text-sm text-muted-foreground">ID: {po.branchId}</p>
                                    </div>
                                    <div>
                                      <Label>Status</Label>
                                      <div className="mt-1">{getStatusBadge(po.status)}</div>
                                    </div>
                                    <div>
                                      <Label>Total Amount</Label>
                                      <p className="text-lg font-semibold">{formatCurrency(po.totalAmount)}</p>
                                    </div>
                                  </div>

                                  <div>
                                    <Label>Items</Label>
                                    <Table>
                                      <TableHeader>
                                        <TableRow>
                                          <TableHead>Product</TableHead>
                                          <TableHead>Quantity</TableHead>
                                          <TableHead>Weight</TableHead>
                                          <TableHead>Unit Cost</TableHead>
                                          <TableHead>Total</TableHead>
                                          <TableHead>Status</TableHead>
                                        </TableRow>
                                      </TableHeader>
                                      <TableBody>
                                        {po.items.map((item) => (
                                          <TableRow key={item.id}>
                                            <TableCell>
                                              <div>
                                                <p className="font-medium">{getProductName(item.productId)}</p>
                                                <p className="text-sm text-muted-foreground">{getProductCode(item.productId)}</p>
                                              </div>
                                            </TableCell>
                                            <TableCell>
                                              {item.quantityOrdered}
                                              {item.quantityReceived > 0 && (
                                                <span className="text-sm text-green-600 ml-2">
                                                  ({item.quantityReceived} received)
                                                </span>
                                              )}
                                            </TableCell>
                                            <TableCell>
                                              {item.weightOrdered}g
                                              {item.weightReceived > 0 && (
                                                <span className="text-sm text-green-600 ml-2">
                                                  ({item.weightReceived}g received)
                                                </span>
                                              )}
                                            </TableCell>
                                            <TableCell>{formatCurrency(item.unitCost)}</TableCell>
                                            <TableCell>{formatCurrency(item.lineTotal)}</TableCell>
                                            <TableCell>
                                              <Badge variant={item.status === 'Received' ? 'default' : 'outline'}>
                                                {item.status}
                                              </Badge>
                                            </TableCell>
                                          </TableRow>
                                        ))}
                                      </TableBody>
                                    </Table>
                                  </div>

                                  {po.notes && (
                                    <div>
                                      <Label>Notes</Label>
                                      <p className="text-sm">{po.notes}</p>
                                    </div>
                                  )}

                                  {isManager && po.status !== 'Received' && po.status !== 'Cancelled' && (
                                    <div className="flex gap-2 pt-4 border-t">
                                      {po.status === 'Pending' && (
                                        <Button
                                          onClick={() => handleStatusUpdate(po.id, 'Sent')}
                                          size="sm"
                                        >
                                          Send to Supplier
                                        </Button>
                                      )}
                                      {po.status === 'Sent' && (
                                        <Button
                                          onClick={() => handleStatusUpdate(po.id, 'Confirmed')}
                                          size="sm"
                                        >
                                          Mark Confirmed
                                        </Button>
                                      )}
                                      {po.status === 'Confirmed' && (
                                        <Button
                                          onClick={() => handleReceivePO(po.id, po.items.map(item => ({
                                            purchaseOrderItemId: item.id,
                                            quantityReceived: item.quantityOrdered - item.quantityReceived,
                                            weightReceived: item.weightOrdered - item.weightReceived
                                          })))}
                                          size="sm"
                                        >
                                          Mark Received
                                        </Button>
                                      )}
                                      <Button
                                        onClick={() => handleStatusUpdate(po.id, 'Cancelled')}
                                        variant="destructive"
                                        size="sm"
                                      >
                                        Cancel Order
                                      </Button>
                                    </div>
                                  )}
                                </div>
                              </DialogContent>
                            </Dialog>
                            {isManager && (
                              <Button variant="outline" size="sm" className="touch-target">
                                <Edit className="h-4 w-4" />
                              </Button>
                            )}
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
      </Tabs>
    </div>
  );
}
