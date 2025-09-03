import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Badge } from './ui/badge';
import { Textarea } from './ui/textarea';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from './ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from './ui/select';
import { Tabs, TabsContent, TabsList, TabsTrigger } from './ui/tabs';
import { toast } from 'sonner';
import { Plus, ShoppingCart, Eye, Zap, Trash2, Edit, Truck, CheckCircle, Package, X, Loader2, Clock, DollarSign, Search, FileText } from 'lucide-react';
import { 
  usePaginatedSuppliers, 
  usePaginatedProducts, 
  usePaginatedBranches, 
  useKaratTypes, 
  useSearchPurchaseOrders, 
  useCreatePurchaseOrder, 
  useReceivePurchaseOrder,
  useUpdatePurchaseOrderStatus,
  useCreateRawGoldPurchaseOrder,
  useUpdateRawGoldPurchaseOrderStatus,
  useReceiveRawGoldPurchaseOrder,
  useProcessPurchaseOrderPayment,
  useProcessRawGoldPurchaseOrderPayment,
  useCurrentUser,
  usePaymentMethods
} from '../hooks/useApi';
import { 
  PurchaseOrderDto, 
  PurchaseOrderItemDto,
  CreatePurchaseOrderRequest,
  CreatePurchaseOrderItemRequest,
  UpdatePurchaseOrderStatusRequest,
  ReceivePurchaseOrderRequest,
  ReceivePurchaseOrderItemDto,
  SupplierDto,
  Product,
  BranchDto
} from '../services/api';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from './ui/table';

// Utility function for currency formatting
const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD'
  }).format(amount);
};

// Helper: compute amount paid and outstanding with fallbacks
const getAmountPaid = (po: any) => {
  return typeof po?.amountPaid === 'number' ? po.amountPaid : 0;
};

const getOutstanding = (po: any) => {
  if (typeof po?.outstandingBalance === 'number') return Math.max(0, po.outstandingBalance);
  const total = typeof po?.totalAmount === 'number' ? po.totalAmount : 0;
  const paid = getAmountPaid(po);
  const outstanding = total - paid;
  return Math.max(0, Number(outstanding.toFixed(2)));
};

export default function PurchaseOrders() {
  const [currentUser, setCurrentUser] = useState<any>(null);
  const { fetchUser } = useCurrentUser();
  const isManager = currentUser?.role === 'Manager' || currentUser?.roles?.includes('Manager');
  const [activeTab, setActiveTab] = useState('all');
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [orderType, setOrderType] = useState('all'); // 'all', 'regular', 'raw-gold'
  const [isNewPOOpen, setIsNewPOOpen] = useState(false);
  const [isNewRawGoldPOOpen, setIsNewRawGoldPOOpen] = useState(false);
  const [selectedPO, setSelectedPO] = useState<(PurchaseOrderDto & { type?: 'regular' | 'raw-gold' }) | null>(null);
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

  const { 
    execute: updatePurchaseOrderStatus 
  } = useUpdatePurchaseOrderStatus();

  const { 
    execute: createRawGoldPurchaseOrder 
  } = useCreateRawGoldPurchaseOrder();

  const { 
    execute: updateRawGoldPurchaseOrderStatus 
  } = useUpdateRawGoldPurchaseOrderStatus();

  const { 
    execute: receiveRawGoldPurchaseOrder 
  } = useReceiveRawGoldPurchaseOrder();

  // Payment hooks
  const { execute: processPurchaseOrderPayment } = useProcessPurchaseOrderPayment();
  const { execute: processRawGoldPurchaseOrderPayment } = useProcessRawGoldPurchaseOrderPayment();
  const { data: paymentMethods, execute: fetchPaymentMethods } = usePaymentMethods();

  // Payment dialog state
  const [isPayDialogOpen, setIsPayDialogOpen] = useState(false);
  const [paymentForm, setPaymentForm] = useState({ amount: '', paymentMethodId: '', notes: '', referenceNumber: '' });

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
    fetchData: fetchProducts,
    updateParams: updateProductParams,
    params: productParams
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

  // Load karat types using the hook
  const { 
    data: karatTypes, 
    loading: karatTypesLoading,
    fetchKaratTypes 
  } = useKaratTypes();

  // Form state for regular PO
  const [poForm, setPOForm] = useState({
    supplierId: '',
    branchId: '',
    expectedDelivery: '',
    terms: '',
    notes: '',
    items: [{ productId: '', quantity: '', weight: '', unitCost: '', notes: '' }],
  });

  // Product filters for regular PO items
  const [productSearchTerm, setProductSearchTerm] = useState('');
  const [productKaratFilter, setProductKaratFilter] = useState<string>('');

  // Update products list when filters change
  useEffect(() => {
    const karatId = productKaratFilter ? parseInt(productKaratFilter, 10) : undefined;
    updateProductParams({
      pageNumber: 1,
      pageSize: productParams?.pageSize || 100,
      isActive: true,
      searchTerm: productSearchTerm || undefined,
      karatTypeId: karatId,
    });
  }, [productSearchTerm, productKaratFilter, updateProductParams, productParams?.pageSize]);

  // Form state for raw gold PO
  const [rawGoldPOForm, setRawGoldPOForm] = useState({
    supplierId: '',
    branchId: '',
    expectedDelivery: '',
    terms: '',
    notes: '',
    items: [{ karatTypeId: '', weightOrdered: '', costPerGram: '', description: '', notes: '' }],
  });

  // Load data on component mount
  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      // Load current user first
      const user = await fetchUser();
      if (user) {
        setCurrentUser(user);
        console.log('Current user loaded:', user);
        console.log('User role:', user.roles);
      }

      // Load both regular and raw gold purchase orders
      await Promise.all([
        searchPurchaseOrders({
          pageNumber: 1,
          pageSize: 100
        }),
        loadRawGoldPurchaseOrders()
      ]);

      // Load suppliers, products, branches, karat types, and payment methods
      await Promise.all([
        fetchSuppliers(),
        fetchProducts(),
        fetchBranches(),
        fetchKaratTypes(),
        fetchPaymentMethods()
      ]);
    } catch (error) {
      console.error('Error loading data:', error);
      toast.error('Failed to load data');
    }
  };

  const handlePayPO = async (po: any) => {
    if (!isManager) {
      toast.error('Only managers can process purchase order payments');
      return;
    }
    try {
      setSelectedPO(po);
      // Ensure payment methods are loaded
      if (!paymentMethods || (Array.isArray(paymentMethods) && paymentMethods.length === 0)) {
        await fetchPaymentMethods();
      }
      setPaymentForm({
        amount: getOutstanding(po).toFixed(2),
        paymentMethodId: '',
        notes: '',
        referenceNumber: po.purchaseOrderNumber || ''
      });
      setIsPayDialogOpen(true);
    } catch (error) {
      console.error('Error preparing payment dialog:', error);
      toast.error('Failed to open payment dialog');
    }
  };

  const handleSubmitPayment = async () => {
    if (!isManager) {
      toast.error('Only managers can process purchase order payments');
      return;
    }
    if (!selectedPO) {
      toast.error('No purchase order selected');
      return;
    }
    try {
      const amount = parseFloat(paymentForm.amount);
      const paymentMethodId = parseInt(paymentForm.paymentMethodId, 10);
      if (isNaN(amount) || amount <= 0) {
        toast.error('Enter a valid amount');
        return;
      }
      if (isNaN(paymentMethodId) || paymentMethodId <= 0) {
        toast.error('Select a payment method');
        return;
      }
      let request: any;
      if (selectedPO.type === 'raw-gold') {
        request = {
          RawGoldPurchaseOrderId: selectedPO.id,
          PaymentAmount: amount,
          PaymentMethodId: paymentMethodId,
          Notes: paymentForm.notes || undefined,
          ReferenceNumber: paymentForm.referenceNumber || undefined,
        };
        await processRawGoldPurchaseOrderPayment(selectedPO.id, request);
      } else {
        request = {
          PurchaseOrderId: selectedPO.id,
          PaymentAmount: amount,
          PaymentMethodId: paymentMethodId,
          Notes: paymentForm.notes || undefined,
          ReferenceNumber: paymentForm.referenceNumber || undefined,
        };
        await processPurchaseOrderPayment(selectedPO.id, request);
      }

      toast.success('Payment processed successfully');
      setIsPayDialogOpen(false);
      setPaymentForm({ amount: '', paymentMethodId: '', notes: '', referenceNumber: '' });
      setSelectedPO(null);
      loadData();
    } catch (error) {
      console.error('Error processing payment:', error);
      toast.error('Failed to process payment');
    }
  };

  // State for raw gold purchase orders
  const [rawGoldPurchaseOrders, setRawGoldPurchaseOrders] = useState<any[]>([]);

  const loadRawGoldPurchaseOrders = async () => {
    try {
      const { rawGoldPurchaseOrdersApi } = await import('../services/api');
      const rawGoldPOs = await rawGoldPurchaseOrdersApi.getRawGoldPurchaseOrders();
      
      // Convert raw gold POs to match the regular PO format for display
      const formattedRawGoldPOs = rawGoldPOs.map((po: any) => ({
        ...po,
        type: 'raw-gold',
        items: po.rawGoldPurchaseOrderItems || po.items || [],
        // Ensure consistent property names
        purchaseOrderNumber: po.purchaseOrderNumber,
        orderDate: po.orderDate,
        expectedDeliveryDate: po.expectedDeliveryDate,
        actualDeliveryDate: po.actualDeliveryDate,
        totalAmount: po.totalAmount,
        // Map payment fields if present and compute fallback
        amountPaid: typeof po.amountPaid === 'number' ? po.amountPaid : 0,
        outstandingBalance: typeof po.outstandingBalance === 'number' ? po.outstandingBalance : Math.max(0, (po.totalAmount || 0) - (po.amountPaid || 0)),
        status: po.status,
        supplierId: po.supplierId,
        branchId: po.branchId
      }));
      
      setRawGoldPurchaseOrders(formattedRawGoldPOs);
    } catch (error) {
      console.error('Error loading raw gold purchase orders:', error);
    }
  };

  // Extract data from API responses
  const regularPurchaseOrders = purchaseOrdersData?.items || [];
  const allPurchaseOrders = [...regularPurchaseOrders, ...rawGoldPurchaseOrders];
  const purchaseOrders = allPurchaseOrders;
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

  const addRawGoldPOItem = () => {
    setRawGoldPOForm({
      ...rawGoldPOForm,
      items: [...rawGoldPOForm.items, { karatTypeId: '', weightOrdered: '', costPerGram: '', description: '', notes: '' }],
    });
  };

  const removeRawGoldPOItem = (index: number) => {
    const newItems = rawGoldPOForm.items.filter((_, i) => i !== index);
    setRawGoldPOForm({ ...rawGoldPOForm, items: newItems });
  };

  const updateRawGoldPOItem = (index: number, field: string, value: string) => {
    const newItems = [...rawGoldPOForm.items];
    newItems[index] = { ...newItems[index], [field]: value };
    setRawGoldPOForm({ ...rawGoldPOForm, items: newItems });
  };

  const calculateRawGoldTotal = (weightOrdered: string, costPerGram: string) => {
    const w = parseFloat(weightOrdered) || 0;
    const cost = parseFloat(costPerGram) || 0;
    return w * cost;
  };

  const getRawGoldTotalAmount = () => {
    return rawGoldPOForm.items.reduce((total, item) => {
      return total + calculateRawGoldTotal(item.weightOrdered, item.costPerGram);
    }, 0);
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

  const handleCreateRawGoldPO = async () => {
    if (!isManager) {
      toast.error('Only managers can create raw gold purchase orders');
      return;
    }

    if (!rawGoldPOForm.supplierId || !rawGoldPOForm.branchId) {
      toast.error('Please select supplier and branch');
      return;
    }

    if (rawGoldPOForm.items.length === 0 || rawGoldPOForm.items.some(item => !item.karatTypeId || !item.weightOrdered || !item.costPerGram)) {
      toast.error('Please add at least one item with all required fields');
      return;
    }

    try {
      const request = {
        supplierId: parseInt(rawGoldPOForm.supplierId),
        branchId: parseInt(rawGoldPOForm.branchId),
        notes: rawGoldPOForm.notes,
        items: rawGoldPOForm.items.map(item => ({
          karatTypeId: parseInt(item.karatTypeId),
          weightOrdered: parseFloat(item.weightOrdered),
          unitCostPerGram: parseFloat(item.costPerGram),
          description: item.description || '',
          notes: item.notes || undefined
        }))
      };

      await createRawGoldPurchaseOrder(request);
      toast.success('Raw gold purchase order created successfully');
      setIsNewRawGoldPOOpen(false);
      setIsEditMode(false);
      setSelectedPO(null);
      resetRawGoldForm();
      loadData(); // Reload the data
    } catch (error) {
      console.error('Error creating raw gold purchase order:', error);
      toast.error('Failed to create raw gold purchase order');
    }
  };

  const handleUpdateRawGoldPO = async () => {
    if (!isManager) {
      toast.error('Only managers can update raw gold purchase orders');
      return;
    }

    if (!selectedPO) {
      toast.error('No purchase order selected for editing');
      return;
    }

    if (!rawGoldPOForm.supplierId || !rawGoldPOForm.branchId) {
      toast.error('Please select supplier and branch');
      return;
    }

    if (rawGoldPOForm.items.length === 0 || rawGoldPOForm.items.some(item => !item.karatTypeId || !item.weightOrdered || !item.costPerGram)) {
      toast.error('Please add at least one item with all required fields');
      return;
    }

    try {
      const { rawGoldPurchaseOrdersApi } = await import('../services/api');
      const request = {
        supplierId: parseInt(rawGoldPOForm.supplierId),
        branchId: parseInt(rawGoldPOForm.branchId),
        notes: rawGoldPOForm.notes,
        items: rawGoldPOForm.items.map(item => ({
          karatTypeId: parseInt(item.karatTypeId),
          weightOrdered: parseFloat(item.weightOrdered),
          unitCostPerGram: parseFloat(item.costPerGram),
          description: item.description || '',
          notes: item.notes || undefined
        }))
      };

      await rawGoldPurchaseOrdersApi.updateRawGoldPurchaseOrder(selectedPO.id, request);
      toast.success('Raw gold purchase order updated successfully');
      setIsNewRawGoldPOOpen(false);
      setIsEditMode(false);
      setSelectedPO(null);
      resetRawGoldForm();
      loadData(); // Reload the data
    } catch (error) {
      console.error('Error updating raw gold purchase order:', error);
      toast.error('Failed to update raw gold purchase order');
    }
  };

  const handleStatusUpdate = async (poId: number, newStatus: string, poType?: string) => {
    if (!isManager) {
      toast.error('Only managers can update purchase order status');
      return;
    }

    try {
      if (poType === 'raw-gold') {
        // Use raw gold purchase orders hook for raw gold POs
        const request = {
          newStatus: newStatus,
          statusNotes: undefined
        };
        
        await updateRawGoldPurchaseOrderStatus(poId, request);
      } else {
        // Use regular purchase orders hook for regular POs
        const request: UpdatePurchaseOrderStatusRequest = {
          newStatus: newStatus,
          statusNotes: undefined
        };
        
        await updatePurchaseOrderStatus(poId, request);
      }
      
      toast.success(`Purchase order status updated to ${newStatus}`);
      loadData(); // Reload the data
    } catch (error) {
      console.error('Error updating purchase order status:', error);
      toast.error('Failed to update purchase order status');
    }
  };

  const handleReceivePO = async (poId: number, items: ReceivePurchaseOrderItemDto[], poType?: string) => {
    if (!isManager) {
      toast.error('Only managers can receive purchase orders');
      return;
    }

    try {
      if (poType === 'raw-gold') {
        // Use raw gold purchase orders hook for raw gold POs
        const request = {
          items: items.map(item => ({
            rawGoldPurchaseOrderItemId: item.purchaseOrderItemId,
            weightReceived: item.weightReceived
          }))
        };
        
        await receiveRawGoldPurchaseOrder(poId, request);
      } else {
        // Use regular purchase orders hook for regular POs
        const request = {
          purchaseOrderId: poId,
          items: items
        };
        
        await receivePurchaseOrder(request);
      }
      
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

  const resetRawGoldForm = () => {
    setRawGoldPOForm({
      supplierId: '',
      branchId: '',
      expectedDelivery: '',
      terms: '',
      notes: '',
      items: [{ karatTypeId: '', weightOrdered: '', costPerGram: '', description: '', notes: '' }],
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

  // Helper function to get karat type name by ID
  const getKaratTypeName = (karatTypeId: number) => {
    const karatType = karatTypes?.find(k => k.id === karatTypeId);
    return karatType ? karatType.name : `Karat ID: ${karatTypeId}`;
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
          <div className="flex gap-2">
            <Dialog open={isNewPOOpen} onOpenChange={setIsNewPOOpen}>
              <DialogTrigger asChild>
                <Button className="touch-target pos-button-primary bg-blue-600 hover:bg-blue-700 text-white" data-testid="new-regular-po-btn">
                  <ShoppingCart className="mr-2 h-4 w-4" />
                  New Regular PO
                </Button>
              </DialogTrigger>
            <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto" data-testid="regular-po-dialog">
              <DialogHeader>
                <DialogTitle>Create Regular Purchase Order</DialogTitle>
                <DialogDescription>
                  Create a new regular purchase order for supplier inventory
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
                  <div className="grid grid-cols-12 gap-3">
                    <div className="col-span-8 space-y-1">
                      <Label className="text-xs">Search Products</Label>
                      <div className="relative">
                        <Input
                          placeholder="Search by name or code"
                          value={productSearchTerm}
                          onChange={(e) => setProductSearchTerm(e.target.value)}
                          className="pr-8"
                        />
                        <Search className="absolute right-2 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                      </div>
                    </div>
                    <div className="col-span-4 space-y-1">
                      <Label className="text-xs">Filter by Karat</Label>
                      <Select
                        value={productKaratFilter || ''}
                        onValueChange={(value) => setProductKaratFilter(value === 'all' ? '' : value)}
                      >
                        <SelectTrigger>
                          <SelectValue placeholder="All karats" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="all">All</SelectItem>
                          {karatTypes?.map((k) => (
                            <SelectItem key={k.id} value={k.id.toString()}>{k.name}</SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                  </div>
                  <div className="flex items-center justify-between">
                    <Label>Items</Label>
                    <Button type="button" variant="outline" onClick={addPOItem} size="sm" data-testid="po-add-item-btn">
                      <Plus className="mr-1 h-3 w-3" />
                      Add Item
                    </Button>
                  </div>
                  
                  {poForm.items.map((item, index) => (
                    <div key={index} className="space-y-3 p-4 border rounded-lg">
                      <div className="grid grid-cols-12 gap-2 items-end">
                        <div className="col-span-4 space-y-1">
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
                          {item.productId && (
                            (() => {
                              const p = products.find(p => p.id === parseInt(item.productId));
                              return (
                                <div className="mt-1 rounded border bg-muted/30 p-2">
                                  {p ? (
                                    <div className="flex flex-wrap gap-4 text-xs">
                                      <span><span className="font-medium">Name:</span> {p.name}</span>
                                      <span><span className="font-medium">Code:</span> {p.productCode}</span>
                                      <span><span className="font-medium">Weight:</span> {p.weight} g</span>
                                    </div>
                                  ) : (
                                    <p className="text-xs text-muted-foreground">Product details unavailable.</p>
                                  )}
                                </div>
                              );
                            })()
                          )}
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
                        <div className="col-span-1 space-y-1">
                          <Label className="text-xs">Total</Label>
                          <Input
                            value={formatCurrency(calculateTotal(item.quantity, item.unitCost))}
                            readOnly
                            className="bg-muted text-xs"
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
                      
                      <div className="space-y-1">
                        <Label className="text-xs">Notes</Label>
                        <Input
                          value={item.notes}
                          onChange={(e) => updatePOItem(index, 'notes', e.target.value)}
                          placeholder="Item notes"
                          className="text-xs"
                        />
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
                  className="pos-button-primary bg-blue-600 hover:bg-blue-700 text-white"
                  data-testid="po-submit-btn"
                >
                  {createLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                  Create Regular Purchase Order
                </Button>
              </div>
            </DialogContent>
          </Dialog>
          
          <Dialog open={isNewRawGoldPOOpen} onOpenChange={setIsNewRawGoldPOOpen}>
            <DialogTrigger asChild>
              <Button className="touch-target pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]" data-testid="new-raw-gold-po-btn">
                <Zap className="mr-2 h-4 w-4" />
                New Raw Gold PO
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-7xl max-h-screen overflow-y-auto" data-testid="raw-gold-po-dialog">
              <DialogHeader>
                <DialogTitle>{isEditMode && selectedPO ? 'Edit Raw Gold Purchase Order' : 'Create Raw Gold Purchase Order'}</DialogTitle>
                <DialogDescription>
                  {isEditMode && selectedPO ? 'Edit the raw gold purchase order details' : 'Create a new raw gold purchase order for supplier inventory'}
                </DialogDescription>
              </DialogHeader>
              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label>Supplier</Label>
                    <Select value={rawGoldPOForm.supplierId} onValueChange={(value) => setRawGoldPOForm({...rawGoldPOForm, supplierId: value})}>
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
                    <Select value={rawGoldPOForm.branchId} onValueChange={(value) => setRawGoldPOForm({...rawGoldPOForm, branchId: value})}>
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
                      value={rawGoldPOForm.expectedDelivery}
                      onChange={(e) => setRawGoldPOForm({...rawGoldPOForm, expectedDelivery: e.target.value})}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label>Terms</Label>
                    <Input
                      value={rawGoldPOForm.terms}
                      onChange={(e) => setRawGoldPOForm({...rawGoldPOForm, terms: e.target.value})}
                      placeholder="Payment terms"
                    />
                  </div>
                </div>

                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <Label>Raw Gold Items</Label>
                    <Button type="button" variant="outline" onClick={addRawGoldPOItem} size="sm" data-testid="raw-po-add-item-btn">
                      <Plus className="mr-1 h-3 w-3" />
                      Add Raw Gold Item
                    </Button>
                  </div>
                  
                  {rawGoldPOForm.items.map((item, index) => (
                    <div key={index} className="space-y-3 p-4 border rounded-lg bg-yellow-50">
                      <div className="grid grid-cols-12 gap-2 items-end">
                        <div className="col-span-3 space-y-1">
                          <Label className="text-xs">Karat Type</Label>
                          <Select value={item.karatTypeId} onValueChange={(value) => updateRawGoldPOItem(index, 'karatTypeId', value)}>
                            <SelectTrigger>
                              <SelectValue placeholder="Select karat type" />
                            </SelectTrigger>
                            <SelectContent>
                              {karatTypes?.map(karatType => (
                                <SelectItem key={karatType.id} value={karatType.id.toString()}>
                                  {karatType.name}
                                </SelectItem>
                              )) || []}
                            </SelectContent>
                          </Select>
                        </div>
                        <div className="col-span-2 space-y-1">
                          <Label className="text-xs">Weight (g)</Label>
                          <Input
                            type="number"
                            value={item.weightOrdered}
                            onChange={(e) => updateRawGoldPOItem(index, 'weightOrdered', e.target.value)}
                            placeholder="0.00"
                          />
                        </div>
                        <div className="col-span-2 space-y-1">
                          <Label className="text-xs">Cost per Gram</Label>
                          <Input
                            type="number"
                            value={item.costPerGram}
                            onChange={(e) => updateRawGoldPOItem(index, 'costPerGram', e.target.value)}
                            placeholder="0.00"
                          />
                        </div>
                        <div className="col-span-4 space-y-1">
                          <Label className="text-xs">Total Cost</Label>
                          <Input
                            value={formatCurrency(calculateRawGoldTotal(item.weightOrdered, item.costPerGram))}
                            readOnly
                            className="bg-muted text-xs"
                          />
                        </div>
                      </div>
                      <div className="grid grid-cols-12 gap-2">
                        <div className="col-span-11 space-y-1">
                          <Label className="text-xs">Description</Label>
                          <Input
                            value={item.description}
                            onChange={(e) => updateRawGoldPOItem(index, 'description', e.target.value)}
                            placeholder="e.g., 24K Gold Bars, 22K Gold Scrap"
                          />
                        </div>
                        <div className="col-span-1">
                          {rawGoldPOForm.items.length > 1 && (
                            <Button
                              type="button"
                              variant="ghost"
                              size="sm"
                              onClick={() => removeRawGoldPOItem(index)}
                            >
                              <X className="h-4 w-4" />
                            </Button>
                          )}
                        </div>
                      </div>
                      
                      <div className="space-y-1">
                        <Label className="text-xs">Notes</Label>
                        <Input
                          value={item.notes}
                          onChange={(e) => updateRawGoldPOItem(index, 'notes', e.target.value)}
                          placeholder="Raw gold item notes"
                          className="text-xs"
                        />
                      </div>
                    </div>
                  ))}
                  
                  <div className="text-right pt-4 border-t">
                    <p className="text-lg font-semibold">
                      Total: {formatCurrency(getRawGoldTotalAmount())}
                    </p>
                  </div>
                </div>

                <div className="space-y-2">
                  <Label>Notes</Label>
                  <Textarea
                    value={rawGoldPOForm.notes}
                    onChange={(e) => setRawGoldPOForm({...rawGoldPOForm, notes: e.target.value})}
                    placeholder="Additional notes for this raw gold purchase order"
                  />
                </div>
              </div>
              <div className="flex justify-end gap-3 mt-6">
                <Button variant="outline" onClick={() => {
                  setIsNewRawGoldPOOpen(false);
                  setIsEditMode(false);
                  setSelectedPO(null);
                  resetRawGoldForm();
                }}>
                  Cancel
                </Button>
                <Button 
                  onClick={isEditMode ? handleUpdateRawGoldPO : handleCreateRawGoldPO} 
                  disabled={createLoading}
                  className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]"
                  data-testid="raw-po-submit-btn"
                >
                  {createLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                  {isEditMode ? 'Update Raw Gold Purchase Order' : 'Create Raw Gold Purchase Order'}
                </Button>
              </div>
            </DialogContent>
          </Dialog>
          
          {/* Payment Dialog */}
          <Dialog open={isPayDialogOpen} onOpenChange={setIsPayDialogOpen}>
            <DialogContent className="max-w-lg" data-testid="po-payment-dialog">
              <DialogHeader>
                <DialogTitle>Process Purchase Order Payment</DialogTitle>
                <DialogDescription>
                  {selectedPO ? (
                    <div className="mt-2 text-sm">
                      <div className="flex items-center justify-between">
                        <span className="text-muted-foreground">PO Number</span>
                        <span className="font-medium">{selectedPO.purchaseOrderNumber}</span>
                      </div>
                      <div className="flex items-center justify-between">
                        <span className="text-muted-foreground">Outstanding</span>
                        <span className="font-semibold">{formatCurrency(getOutstanding(selectedPO))}</span>
                      </div>
                    </div>
                  ) : null}
                </DialogDescription>
              </DialogHeader>
              <div className="space-y-4">
                <div className="space-y-2">
                  <Label>Amount</Label>
                  <Input
                    type="number"
                    value={paymentForm.amount}
                    onChange={(e) => setPaymentForm({ ...paymentForm, amount: e.target.value })}
                    placeholder="0.00"
                    data-testid="po-payment-amount"
                  />
                </div>
                <div className="space-y-2">
                  <Label>Payment Method</Label>
                  <Select
                    value={paymentForm.paymentMethodId}
                    onValueChange={(value) => setPaymentForm({ ...paymentForm, paymentMethodId: value })}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select payment method" />
                    </SelectTrigger>
                    <SelectContent>
                      {(paymentMethods || []).map((pm: any) => (
                        <SelectItem key={pm.id} value={pm.id.toString()}>
                          {pm.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label>Reference Number</Label>
                  <Input
                    value={paymentForm.referenceNumber}
                    onChange={(e) => setPaymentForm({ ...paymentForm, referenceNumber: e.target.value })}
                    placeholder="Optional reference"
                  />
                </div>
                <div className="space-y-2">
                  <Label>Notes</Label>
                  <Textarea
                    value={paymentForm.notes}
                    onChange={(e) => setPaymentForm({ ...paymentForm, notes: e.target.value })}
                    placeholder="Optional notes"
                  />
                </div>
              </div>
              <div className="flex justify-end gap-2 mt-4">
                <Button variant="outline" onClick={() => setIsPayDialogOpen(false)} data-testid="po-payment-cancel">
                  Cancel
                </Button>
                <Button onClick={handleSubmitPayment} className="pos-button-primary">
                  Submit Payment
                </Button>
              </div>
            </DialogContent>
          </Dialog>
          </div>
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
                      <TableHead>Type</TableHead>
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
                      <TableRow key={`${po.type || 'regular'}-${po.id}`}>
                        <TableCell className="font-medium">{po.purchaseOrderNumber}</TableCell>
                        <TableCell>
                          {po.type === 'raw-gold' ? (
                            <Badge className="bg-yellow-100 text-yellow-800">
                              <Zap className="mr-1 h-3 w-3" />
                              Raw Gold
                            </Badge>
                          ) : (
                            <Badge className="bg-blue-100 text-blue-800">
                              <ShoppingCart className="mr-1 h-3 w-3" />
                              Regular
                            </Badge>
                          )}
                        </TableCell>
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
                            {isManager && (
                              <Button
                                onClick={() => handlePayPO(po)}
                                size="sm"
                                variant="outline"
                                className="text-green-700 hover:bg-green-50"
                                disabled={getOutstanding(po) <= 0}
                              >
                                <DollarSign className="h-4 w-4 mr-1" />
                                Pay PO
                              </Button>
                            )}
                            {/* Status Change Buttons */}
                            {isManager && po.status !== 'Received' && po.status !== 'Cancelled' && (
                              <>
                                {po.status === 'Pending' && (
                                  <Button
                                    onClick={() => handleStatusUpdate(po.id, 'Sent', po.type)}
                                    size="sm"
                                    variant="outline"
                                    className="text-blue-600 hover:bg-blue-50"
                                  >
                                    <Truck className="h-4 w-4 mr-1" />
                                    Send
                                  </Button>
                                )}
                                {po.status === 'Sent' && (
                                  <Button
                                    onClick={() => handleStatusUpdate(po.id, 'Confirmed', po.type)}
                                    size="sm"
                                    variant="outline"
                                    className="text-yellow-600 hover:bg-yellow-50"
                                  >
                                    <CheckCircle className="h-4 w-4 mr-1" />
                                    Confirm
                                  </Button>
                                )}
                                {po.status === 'Confirmed' && (
                                  <Button
                                    onClick={() => handleReceivePO(po.id, po.items.map((item: any) => {
                                      if (po.type === 'raw-gold') {
                                        return {
                                          purchaseOrderItemId: item.id,
                                          weightReceived: item.weightOrdered - (item.weightReceived || 0)
                                        };
                                      } else {
                                        return {
                                          purchaseOrderItemId: item.id,
                                          quantityReceived: item.quantityOrdered - (item.quantityReceived || 0),
                                          weightReceived: item.weightOrdered - (item.weightReceived || 0)
                                        };
                                      }
                                    }), po.type)}
                                    size="sm"
                                    variant="outline"
                                    className="text-green-600 hover:bg-green-50"
                                  >
                                    <Package className="h-4 w-4 mr-1" />
                                    Receive
                                  </Button>
                                )}
                              </>
                            )}
                            
                            {/* Edit Buttons */}
                            {isManager && po.status === 'Pending' && (
                              <>
                                {po.type === 'raw-gold' ? (
                                  <Button
                                    onClick={() => {
                                      setSelectedPO(po);
                                      setIsEditMode(true);
                                      // Populate the raw gold form with existing data
                                      setRawGoldPOForm({
                                        supplierId: po.supplierId.toString(),
                                        branchId: po.branchId.toString(),
                                        expectedDelivery: po.expectedDeliveryDate ? new Date(po.expectedDeliveryDate).toISOString().split('T')[0] : '',
                                        terms: po.terms || '',
                                        notes: po.notes || '',
                                        items: po.items.map((item: any) => ({
                                          karatTypeId: item.karatTypeId.toString(),
                                          weightOrdered: item.weightOrdered.toString(),
                                          costPerGram: item.unitCostPerGram ? item.unitCostPerGram.toString() : item.costPerGram?.toString() || '',
                                          description: item.description || '',
                                          notes: item.notes || ''
                                        }))
                                      });
                                      setIsNewRawGoldPOOpen(true);
                                    }}
                                    variant="outline"
                                    size="sm"
                                    className="text-yellow-600 hover:bg-yellow-50"
                                  >
                                    <Edit className="h-4 w-4" />
                                  </Button>
                                ) : (
                                  <Button
                                    onClick={() => {
                                      setSelectedPO(po);
                                      setIsEditMode(true);
                                      // Populate the regular form with existing data
                                      setPOForm({
                                        supplierId: po.supplierId.toString(),
                                        branchId: po.branchId.toString(),
                                        expectedDelivery: po.expectedDeliveryDate ? new Date(po.expectedDeliveryDate).toISOString().split('T')[0] : '',
                                        terms: po.terms || '',
                                        notes: po.notes || '',
                                        items: po.items.map((item: any) => ({
                                          productId: item.productId.toString(),
                                          quantity: item.quantityOrdered.toString(),
                                          weight: item.weightOrdered.toString(),
                                          unitCost: item.unitCost.toString(),
                                          notes: item.notes || ''
                                        }))
                                      });
                                      setIsNewPOOpen(true);
                                    }}
                                    variant="outline"
                                    size="sm"
                                    className="text-blue-600 hover:bg-blue-50"
                                  >
                                    <Edit className="h-4 w-4" />
                                  </Button>
                                )}
                              </>
                            )}

                            {/* Delete Button for Pending Orders */}
                            {isManager && po.status === 'Pending' && (
                              <Button
                                onClick={() => handleStatusUpdate(po.id, 'Cancelled', po.type)}
                                variant="outline"
                                size="sm"
                                className="text-red-600 hover:bg-red-50"
                              >
                                <Trash2 className="h-4 w-4" />
                              </Button>
                            )}

                            {/* View Details Dialog */}
                            <Dialog>
                              <DialogTrigger asChild>
                                <Button variant="outline" size="sm" className="touch-target">
                                  <Eye className="h-4 w-4" />
                                </Button>
                              </DialogTrigger>
                              <DialogContent className="max-w-7xl max-h-screen overflow-y-auto">
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
                                    <div>
                                      <Label>Amount Paid</Label>
                                      <p className="text-lg font-semibold">{formatCurrency(getAmountPaid(po))}</p>
                                    </div>
                                    <div>
                                      <Label>Outstanding Amount</Label>
                                      <p className="text-lg font-semibold {getOutstanding(po) > 0 ? 'text-red-600' : 'text-green-600'}">
                                        {formatCurrency(getOutstanding(po))}
                                      </p>
                                    </div>
                                    <div>
                                      <Label>Order Type</Label>
                                      <div className="mt-1">
                                        {po.type === 'raw-gold' ? (
                                          <Badge className="bg-yellow-100 text-yellow-800">
                                            <Zap className="mr-1 h-3 w-3" />
                                            Raw Gold
                                          </Badge>
                                        ) : (
                                          <Badge className="bg-blue-100 text-blue-800">
                                            <ShoppingCart className="mr-1 h-3 w-3" />
                                            Regular
                                          </Badge>
                                        )}
                                      </div>
                                    </div>
                                  </div>

                                  <div>
                                    <Label>Items</Label>
                                    <Table>
                                      <TableHeader>
                                        <TableRow>
                                          {po.type === 'raw-gold' ? (
                                            <>
                                              <TableHead>Karat Type</TableHead>
                                              <TableHead>Weight Ordered</TableHead>
                                              <TableHead>Weight Received</TableHead>
                                              <TableHead>Cost per Gram</TableHead>
                                              <TableHead>Total</TableHead>
                                              <TableHead>Status</TableHead>
                                            </>
                                          ) : (
                                            <>
                                              <TableHead>Product</TableHead>
                                              <TableHead>Quantity</TableHead>
                                              <TableHead>Weight</TableHead>
                                              <TableHead>Unit Cost</TableHead>
                                              <TableHead>Total</TableHead>
                                              <TableHead>Status</TableHead>
                                            </>
                                          )}
                                        </TableRow>
                                      </TableHeader>
                                      <TableBody>
                                        {po.items.map((item: any) => (
                                          <TableRow key={item.id}>
                                            {po.type === 'raw-gold' ? (
                                              <>
                                                <TableCell>
                                                  <div>
                                                    <p className="font-medium">{getKaratTypeName(item.karatTypeId)}</p>
                                                    <p className="text-sm text-muted-foreground">{item.description}</p>
                                                  </div>
                                                </TableCell>
                                                <TableCell>{item.weightOrdered}g</TableCell>
                                                <TableCell>
                                                  {item.weightReceived || 0}g
                                                  {item.weightReceived > 0 && (
                                                    <span className="text-sm text-green-600 ml-2">
                                                      ✓
                                                    </span>
                                                  )}
                                                </TableCell>
                                                <TableCell>{formatCurrency(item.unitCostPerGram)}</TableCell>
                                                <TableCell>{formatCurrency(item.lineTotal)}</TableCell>
                                                <TableCell>
                                                  <Badge variant={item.status === 'Received' ? 'default' : 'outline'}>
                                                    {item.status}
                                                  </Badge>
                                                </TableCell>
                                              </>
                                            ) : (
                                              <>
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
                                              </>
                                            )}
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
                                          onClick={() => handleStatusUpdate(po.id, 'Sent', po.type)}
                                          size="sm"
                                        >
                                          Send to Supplier
                                        </Button>
                                      )}
                                      {po.status === 'Sent' && (
                                        <Button
                                          onClick={() => handleStatusUpdate(po.id, 'Confirmed', po.type)}
                                          size="sm"
                                        >
                                          Mark Confirmed
                                        </Button>
                                      )}
                                      {po.status === 'Confirmed' && (
                                        <Button
                                          onClick={() => handleReceivePO(po.id, po.items.map((item: any) => ({
                                            purchaseOrderItemId: item.id,
                                            quantityReceived: item.quantityOrdered - item.quantityReceived,
                                            weightReceived: item.weightOrdered - item.weightReceived
                                          })), po.type)}
                                          size="sm"
                                        >
                                          Mark Received
                                        </Button>
                                      )}
                                      <Button
                                        onClick={() => handleStatusUpdate(po.id, 'Cancelled', po.type)}
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
