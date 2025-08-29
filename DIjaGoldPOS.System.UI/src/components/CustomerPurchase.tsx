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
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from './ui/alert-dialog';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from './ui/dropdown-menu';
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from './ui/tabs';
import {
  Coins,
  Plus,
  Search,
  Eye,
  Edit,
  Trash2,
  MoreHorizontal,
  Loader2,
  DollarSign,
  Scale,
  Calendar,
  User,
  Building,
  CreditCard,
  Receipt,
  AlertCircle,
  CheckCircle,
  XCircle,
} from 'lucide-react';
import { useAuth } from './AuthContext';
import {
  usePaginatedCustomerPurchases,
  useCreateCustomerPurchase,
  useCustomerPurchase,
  useUpdateCustomerPurchasePayment,
  useCancelCustomerPurchase,
  useCustomerPurchaseSummary,
  useKaratTypes,
  usePaymentMethods,
  useCustomers,
} from '../hooks/useApi';
import {
  CustomerPurchaseDto,
  CreateCustomerPurchaseRequest,
  CustomerPurchaseSearchRequest,
} from '../services/api';

const CustomerPurchase: React.FC = () => {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState('list');
  const [searchParams, setSearchParams] = useState<CustomerPurchaseSearchRequest>({
    pageNumber: 1,
    pageSize: 20,
  });

  // Dialog states
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const [viewDialogOpen, setViewDialogOpen] = useState(false);
  const [editPaymentDialogOpen, setEditPaymentDialogOpen] = useState(false);
  const [cancelDialogOpen, setCancelDialogOpen] = useState(false);

  // Selected purchase for actions
  const [selectedPurchase, setSelectedPurchase] = useState<CustomerPurchaseDto | null>(null);

  // Form states
  const [createForm, setCreateForm] = useState<CreateCustomerPurchaseRequest>({
    customerId: 0,
    branchId: user?.branch?.id || 1,
    totalAmount: 0,
    amountPaid: 0,
    paymentMethodId: 1,
    notes: '',
    items: []
  });

  const [editPaymentForm, setEditPaymentForm] = useState({
    amountPaid: 0
  });

  // API hooks
  const { data: purchasesData, loading: purchasesLoading, updateParams } = usePaginatedCustomerPurchases(searchParams);
  const { execute: createPurchase, loading: createLoading } = useCreateCustomerPurchase();
  const { execute: getPurchase, loading: getPurchaseLoading } = useCustomerPurchase();
  const { execute: updatePayment, loading: updatePaymentLoading } = useUpdateCustomerPurchasePayment();
  const { execute: cancelPurchase, loading: cancelLoading } = useCancelCustomerPurchase();
  const { execute: getSummary, data: summaryData } = useCustomerPurchaseSummary();

  // Lookup data
  const { data: karatTypes } = useKaratTypes();
  const { data: paymentMethods } = usePaymentMethods();
  const { data: customersData } = useCustomers();

  // Load summary on component mount
  useEffect(() => {
    if (!user?.branch?.id) return; // Don't fetch if no branch ID

    const today = new Date();
    const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
    const lastDayOfMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0);

    getSummary(
      firstDayOfMonth.toISOString().split('T')[0],
      lastDayOfMonth.toISOString().split('T')[0],
      user.branch.id
    ).then(result => {
      // Handle the result if needed
    }).catch(error => {
      console.error('Error fetching summary:', error);
    });
  }, [user?.branch?.id]); // Remove getSummary from dependencies

  // Handle search
  const handleSearch = (params: Partial<CustomerPurchaseSearchRequest>) => {
    setSearchParams(prev => ({ ...prev, ...params, pageNumber: 1 }));
    updateParams({ ...searchParams, ...params, pageNumber: 1 });
  };

  // Handle create purchase
  const handleCreatePurchase = async () => {
    try {
      if (!createForm.customerId || !createForm.items.length) {
        alert('Please select a customer and add at least one item');
        return;
      }

      const result = await createPurchase(createForm);
      if (result) {
        setCreateDialogOpen(false);
        resetCreateForm();
        // Refresh the list
        updateParams(searchParams);
        // Refresh summary
        if (user?.branch?.id) {
          const today = new Date();
          const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
          const lastDayOfMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0);
          await getSummary(
            firstDayOfMonth.toISOString().split('T')[0],
            lastDayOfMonth.toISOString().split('T')[0],
            user.branch.id
          );
        }
      }
    } catch (error) {
      console.error('Error creating purchase:', error);
      alert('Failed to create purchase. Please try again.');
    }
  };

  // Reset create form
  const resetCreateForm = () => {
    setCreateForm({
      customerId: 0,
      branchId: user?.branch?.id || 1,
      totalAmount: 0,
      amountPaid: 0,
      paymentMethodId: 1,
      notes: '',
      items: []
    });
  };

  // Add item to create form
  const addItemToForm = () => {
    setCreateForm(prev => ({
      ...prev,
      items: [...prev.items, {
        karatTypeId: 1,
        weight: 0,
        unitPrice: 0,
        totalAmount: 0,
        notes: ''
      }]
    }));
  };

  // Remove item from create form
  const removeItemFromForm = (index: number) => {
    setCreateForm(prev => ({
      ...prev,
      items: prev.items.filter((_, i) => i !== index)
    }));
  };

  // Update item in create form
  const updateItemInForm = (index: number, field: string, value: any) => {
    setCreateForm(prev => ({
      ...prev,
      items: prev.items.map((item, i) =>
        i === index ? { ...item, [field]: value } : item
      )
    }));
  };

  // Calculate total amount when items change
  useEffect(() => {
    const total = createForm.items.reduce((sum, item) => sum + (item.totalAmount || 0), 0);
    setCreateForm(prev => ({ ...prev, totalAmount: total, amountPaid: total }));
  }, [createForm.items]);

  // Handle view purchase
  const handleViewPurchase = async (purchaseId: number) => {
    try {
      const purchase = await getPurchase(purchaseId);
      setSelectedPurchase(purchase);
      setViewDialogOpen(true);
    } catch (error) {
      console.error('Error fetching purchase:', error);
      alert('Failed to load purchase details');
    }
  };

  // Handle edit payment
  const handleEditPayment = (purchase: CustomerPurchaseDto) => {
    setSelectedPurchase(purchase);
    setEditPaymentForm({ amountPaid: purchase.amountPaid });
    setEditPaymentDialogOpen(true);
  };

  // Handle update payment
  const handleUpdatePayment = async () => {
    if (!selectedPurchase) return;

    try {
      await updatePayment(selectedPurchase.id, editPaymentForm.amountPaid);
      setEditPaymentDialogOpen(false);
      setSelectedPurchase(null);
      // Refresh the list
      updateParams(searchParams);
    } catch (error) {
      console.error('Error updating payment:', error);
      alert('Failed to update payment. Please try again.');
    }
  };

  // Handle cancel purchase
  const handleCancelPurchase = async () => {
    if (!selectedPurchase) return;

    try {
      await cancelPurchase(selectedPurchase.id);
      setCancelDialogOpen(false);
      setSelectedPurchase(null);
      // Refresh the list
      updateParams(searchParams);
    } catch (error) {
      console.error('Error cancelling purchase:', error);
      alert('Failed to cancel purchase. Please try again.');
    }
  };

  // Format currency
  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  // Format weight
  const formatWeight = (weight: number) => {
    return `${weight.toFixed(3)}g`;
  };

  // Get status badge
  const getStatusBadge = (purchase: CustomerPurchaseDto) => {
    const isFullyPaid = purchase.amountPaid >= purchase.totalAmount;
    if (isFullyPaid) {
      return <Badge className="bg-green-100 text-green-800"><CheckCircle className="w-3 h-3 mr-1" />Paid</Badge>;
    } else if (purchase.amountPaid > 0) {
      return <Badge className="bg-yellow-100 text-yellow-800"><AlertCircle className="w-3 h-3 mr-1" />Partial</Badge>;
    } else {
      return <Badge className="bg-red-100 text-red-800"><XCircle className="w-3 h-3 mr-1" />Unpaid</Badge>;
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Customer Purchases</h1>
          <p className="text-muted-foreground">
            Manage customer gold purchases and payments
          </p>
        </div>
        <Button onClick={() => setCreateDialogOpen(true)}>
          <Plus className="w-4 h-4 mr-2" />
          New Purchase
        </Button>
      </div>

      {/* Summary Cards */}
      {summaryData && (
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Total Purchases</CardTitle>
              <Coins className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{summaryData.totalPurchases}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Total Weight</CardTitle>
              <Scale className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{summaryData.totalWeight.toFixed(3)}g</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Total Amount</CardTitle>
              <DollarSign className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{formatCurrency(summaryData.totalAmount)}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Outstanding</CardTitle>
              <CreditCard className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{formatCurrency(summaryData.totalOutstanding)}</div>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Main Content */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="list">Purchase List</TabsTrigger>
          <TabsTrigger value="search">Search</TabsTrigger>
        </TabsList>

        <TabsContent value="list" className="space-y-4">
          {/* Purchase List */}
          <Card>
            <CardHeader>
              <CardTitle>Recent Purchases</CardTitle>
              <CardDescription>
                List of customer gold purchases
              </CardDescription>
            </CardHeader>
            <CardContent>
              {purchasesLoading ? (
                <div className="flex justify-center items-center py-8">
                  <Loader2 className="w-6 h-6 animate-spin" />
                </div>
              ) : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Purchase #</TableHead>
                      <TableHead>Customer</TableHead>
                      <TableHead>Date</TableHead>
                      <TableHead>Weight</TableHead>
                      <TableHead>Total Amount</TableHead>
                      <TableHead>Paid</TableHead>
                      <TableHead>Status</TableHead>
                      <TableHead className="text-right">Actions</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {purchasesData?.items.map((purchase) => (
                      <TableRow key={purchase.id}>
                        <TableCell className="font-medium">{purchase.purchaseNumber}</TableCell>
                        <TableCell>{purchase.customerName}</TableCell>
                        <TableCell>{new Date(purchase.purchaseDate).toLocaleDateString()}</TableCell>
                        <TableCell>{purchase.items.reduce((sum, item) => sum + item.weight, 0).toFixed(3)}g</TableCell>
                        <TableCell>{formatCurrency(purchase.totalAmount)}</TableCell>
                        <TableCell>{formatCurrency(purchase.amountPaid)}</TableCell>
                        <TableCell>{getStatusBadge(purchase)}</TableCell>
                        <TableCell className="text-right">
                          <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                              <Button variant="ghost" size="sm">
                                <MoreHorizontal className="w-4 h-4" />
                              </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="end">
                              <DropdownMenuItem onClick={() => handleViewPurchase(purchase.id)}>
                                <Eye className="w-4 h-4 mr-2" />
                                View Details
                              </DropdownMenuItem>
                              <DropdownMenuItem onClick={() => handleEditPayment(purchase)}>
                                <Edit className="w-4 h-4 mr-2" />
                                Update Payment
                              </DropdownMenuItem>
                              <DropdownMenuItem
                                onClick={() => {
                                  setSelectedPurchase(purchase);
                                  setCancelDialogOpen(true);
                                }}
                                className="text-red-600"
                              >
                                <Trash2 className="w-4 h-4 mr-2" />
                                Cancel Purchase
                              </DropdownMenuItem>
                            </DropdownMenuContent>
                          </DropdownMenu>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}

              {/* Pagination */}
              {purchasesData && purchasesData.totalPages > 1 && (
                <div className="flex justify-between items-center mt-4">
                  <div className="text-sm text-muted-foreground">
                    Showing {purchasesData.items.length} of {purchasesData.totalCount} purchases
                  </div>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => updateParams({ ...searchParams, pageNumber: searchParams.pageNumber! - 1 })}
                      disabled={!purchasesData.hasPrevPage}
                    >
                      Previous
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => updateParams({ ...searchParams, pageNumber: searchParams.pageNumber! + 1 })}
                      disabled={!purchasesData.hasNextPage}
                    >
                      Next
                    </Button>
                  </div>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="search" className="space-y-4">
          {/* Search Form */}
          <Card>
            <CardHeader>
              <CardTitle>Search Purchases</CardTitle>
              <CardDescription>
                Search customer purchases by various criteria
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="purchaseNumber">Purchase Number</Label>
                  <Input
                    id="purchaseNumber"
                    placeholder="Enter purchase number"
                    value={searchParams.purchaseNumber || ''}
                    onChange={(e) => setSearchParams(prev => ({ ...prev, purchaseNumber: e.target.value }))}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="customer">Customer</Label>
                  <Select
                    value={searchParams.customerId?.toString() || ''}
                    onValueChange={(value) => setSearchParams(prev => ({
                      ...prev,
                      customerId: value ? parseInt(value) : undefined
                    }))}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select customer" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="">All Customers</SelectItem>
                      {customersData?.items.map((customer) => (
                        <SelectItem key={customer.id} value={customer.id.toString()}>
                          {customer.fullName}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="fromDate">From Date</Label>
                  <Input
                    id="fromDate"
                    type="date"
                    value={searchParams.fromDate || ''}
                    onChange={(e) => setSearchParams(prev => ({ ...prev, fromDate: e.target.value }))}
                  />
                </div>
              </div>
              <div className="flex gap-2">
                <Button onClick={() => handleSearch({})}>
                  <Search className="w-4 h-4 mr-2" />
                  Search
                </Button>
                <Button
                  variant="outline"
                  onClick={() => {
                    setSearchParams({ pageNumber: 1, pageSize: 20 });
                    handleSearch({});
                  }}
                >
                  Clear
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Create Purchase Dialog */}
      <Dialog open={createDialogOpen} onOpenChange={setCreateDialogOpen}>
        <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Create Customer Purchase</DialogTitle>
            <DialogDescription>
              Record a new customer gold purchase
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-6">
            {/* Customer Selection */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="customer">Customer *</Label>
                <Select
                  value={createForm.customerId.toString()}
                  onValueChange={(value) => setCreateForm(prev => ({ ...prev, customerId: parseInt(value) }))}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select customer" />
                  </SelectTrigger>
                  <SelectContent>
                    {customersData?.items.map((customer) => (
                      <SelectItem key={customer.id} value={customer.id.toString()}>
                        {customer.fullName}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="paymentMethod">Payment Method *</Label>
                <Select
                  value={createForm.paymentMethodId.toString()}
                  onValueChange={(value) => setCreateForm(prev => ({ ...prev, paymentMethodId: parseInt(value) }))}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select payment method" />
                  </SelectTrigger>
                  <SelectContent>
                    {paymentMethods?.map((method) => (
                      <SelectItem key={method.id} value={method.id.toString()}>
                        {method.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>

            {/* Gold Items */}
            <div className="space-y-4">
              <div className="flex justify-between items-center">
                <Label className="text-base font-medium">Gold Items *</Label>
                <Button type="button" variant="outline" size="sm" onClick={addItemToForm}>
                  <Plus className="w-4 h-4 mr-2" />
                  Add Item
                </Button>
              </div>

              {createForm.items.map((item, index) => (
                <Card key={index}>
                  <CardContent className="pt-4">
                    <div className="grid grid-cols-1 md:grid-cols-5 gap-4 items-end">
                      <div className="space-y-2">
                        <Label>Karat Type *</Label>
                        <Select
                          value={item.karatTypeId.toString()}
                          onValueChange={(value) => updateItemInForm(index, 'karatTypeId', parseInt(value))}
                        >
                          <SelectTrigger>
                            <SelectValue placeholder="Select karat" />
                          </SelectTrigger>
                          <SelectContent>
                            {karatTypes?.map((karat) => (
                              <SelectItem key={karat.id} value={karat.id.toString()}>
                                {karat.name}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </div>
                      <div className="space-y-2">
                        <Label>Weight (g) *</Label>
                        <Input
                          type="number"
                          step="0.001"
                          placeholder="0.000"
                          value={item.weight || ''}
                          onChange={(e) => updateItemInForm(index, 'weight', parseFloat(e.target.value) || 0)}
                        />
                      </div>
                      <div className="space-y-2">
                        <Label>Unit Price *</Label>
                        <Input
                          type="number"
                          step="0.01"
                          placeholder="0.00"
                          value={item.unitPrice || ''}
                          onChange={(e) => updateItemInForm(index, 'unitPrice', parseFloat(e.target.value) || 0)}
                        />
                      </div>
                      <div className="space-y-2">
                        <Label>Total Amount</Label>
                        <Input
                          type="number"
                          step="0.01"
                          value={item.totalAmount.toFixed(2)}
                          readOnly
                        />
                      </div>
                      <div className="flex gap-2">
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={() => removeItemFromForm(index)}
                          disabled={createForm.items.length === 1}
                        >
                          <Trash2 className="w-4 h-4" />
                        </Button>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>

            {/* Totals */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Total Amount</Label>
                <Input
                  type="number"
                  value={createForm.totalAmount.toFixed(2)}
                  readOnly
                />
              </div>
              <div className="space-y-2">
                <Label>Amount Paid *</Label>
                <Input
                  type="number"
                  step="0.01"
                  value={createForm.amountPaid}
                  onChange={(e) => setCreateForm(prev => ({ ...prev, amountPaid: parseFloat(e.target.value) || 0 }))}
                />
              </div>
            </div>

            {/* Notes */}
            <div className="space-y-2">
              <Label htmlFor="notes">Notes</Label>
              <Textarea
                id="notes"
                placeholder="Additional notes about the purchase"
                value={createForm.notes}
                onChange={(e) => setCreateForm(prev => ({ ...prev, notes: e.target.value }))}
              />
            </div>

            {/* Actions */}
            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setCreateDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleCreatePurchase} disabled={createLoading}>
                {createLoading && <Loader2 className="w-4 h-4 mr-2 animate-spin" />}
                Create Purchase
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* View Purchase Dialog */}
      <Dialog open={viewDialogOpen} onOpenChange={setViewDialogOpen}>
        <DialogContent className="max-w-4xl">
          <DialogHeader>
            <DialogTitle>Purchase Details</DialogTitle>
            <DialogDescription>
              {selectedPurchase?.purchaseNumber} - {selectedPurchase?.customerName}
            </DialogDescription>
          </DialogHeader>
          {selectedPurchase && (
            <div className="space-y-6">
              {/* Purchase Info */}
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Purchase Number</Label>
                  <p className="text-sm">{selectedPurchase.purchaseNumber}</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Date</Label>
                  <p className="text-sm">{new Date(selectedPurchase.purchaseDate).toLocaleDateString()}</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Customer</Label>
                  <p className="text-sm">{selectedPurchase.customerName}</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Branch</Label>
                  <p className="text-sm">{selectedPurchase.branchName}</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Payment Method</Label>
                  <p className="text-sm">{selectedPurchase.paymentMethodName}</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Status</Label>
                  <div className="mt-1">{getStatusBadge(selectedPurchase)}</div>
                </div>
              </div>

              {/* Items */}
              <div>
                <Label className="text-base font-medium">Gold Items</Label>
                <Table className="mt-2">
                  <TableHeader>
                    <TableRow>
                      <TableHead>Karat Type</TableHead>
                      <TableHead>Weight</TableHead>
                      <TableHead>Unit Price</TableHead>
                      <TableHead>Total Amount</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {selectedPurchase.items.map((item, index) => (
                      <TableRow key={index}>
                        <TableCell>{item.karatTypeName}</TableCell>
                        <TableCell>{formatWeight(item.weight)}</TableCell>
                        <TableCell>{formatCurrency(item.unitPrice)}</TableCell>
                        <TableCell>{formatCurrency(item.totalAmount)}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>

              {/* Totals */}
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Total Amount</Label>
                  <p className="text-lg font-medium">{formatCurrency(selectedPurchase.totalAmount)}</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Amount Paid</Label>
                  <p className="text-lg font-medium">{formatCurrency(selectedPurchase.amountPaid)}</p>
                </div>
              </div>

              {/* Notes */}
              {selectedPurchase.notes && (
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Notes</Label>
                  <p className="text-sm mt-1">{selectedPurchase.notes}</p>
                </div>
              )}
            </div>
          )}
        </DialogContent>
      </Dialog>

      {/* Edit Payment Dialog */}
      <Dialog open={editPaymentDialogOpen} onOpenChange={setEditPaymentDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Update Payment</DialogTitle>
            <DialogDescription>
              Update the payment amount for purchase {selectedPurchase?.purchaseNumber}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="amountPaid">Amount Paid</Label>
              <Input
                id="amountPaid"
                type="number"
                step="0.01"
                value={editPaymentForm.amountPaid}
                onChange={(e) => setEditPaymentForm({ amountPaid: parseFloat(e.target.value) || 0 })}
              />
            </div>
            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setEditPaymentDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleUpdatePayment} disabled={updatePaymentLoading}>
                {updatePaymentLoading && <Loader2 className="w-4 h-4 mr-2 animate-spin" />}
                Update Payment
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Cancel Purchase Dialog */}
      <AlertDialog open={cancelDialogOpen} onOpenChange={setCancelDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Cancel Purchase</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to cancel purchase {selectedPurchase?.purchaseNumber}?
              This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleCancelPurchase} className="bg-red-600 hover:bg-red-700">
              {cancelLoading && <Loader2 className="w-4 h-4 mr-2 animate-spin" />}
              Cancel Purchase
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
};

export default CustomerPurchase;
