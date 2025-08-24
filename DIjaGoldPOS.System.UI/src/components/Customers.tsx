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
  Users,
  Plus,
  Search,
  Phone,
  Mail,
  MapPin,
  Star,
  Gift,
  Calendar,
  ShoppingBag,
  Edit,
  Eye,
  Loader2,
  TrendingDown,
  DollarSign,
  Trash2,
  RefreshCw,
} from 'lucide-react';
import { formatCurrency } from './utils/currency';
import { 
  usePaginatedCustomers, 
  useCreateCustomer, 
  useCustomer, 
  useUpdateCustomer, 
  useDeleteCustomer,
  useGetCustomerOrders,
  useCustomerLoyalty,
  useUpdateCustomerLoyalty,
  useSearchCustomers,
  useTransactionTypes, 
  useTransactionStatuses 
} from '../hooks/useApi';
import { 
  Customer, 
  CreateCustomerRequest, 
  UpdateCustomerRequest, 
  CustomerSearchRequest,
  CustomerOrdersHistoryDto,
  CustomerLoyaltyDto,
  UpdateCustomerLoyaltyRequest
} from '../services/api';
import { LookupHelper } from '../types/lookups';

interface CustomerTransaction {
  id: string;
  transactionNumber: string;
  date: string;
  type: 'sale' | 'return' | 'repair';
  amount: number;
  items: number;
  status: string;
}

export default function Customers() {
  const [activeTab, setActiveTab] = useState('all');
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [isNewCustomerOpen, setIsNewCustomerOpen] = useState(false);
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(null);
  const [isEditMode, setIsEditMode] = useState(false);
  const [showBulkActions, setShowBulkActions] = useState(false);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [customerToDelete, setCustomerToDelete] = useState<number | null>(null);

  // API hooks
  const { 
    data: customersData, 
    loading: customersLoading, 
    error: customersError,
    params,
    updateParams,
    nextPage,
    prevPage,
    setPage,
    hasNextPage,
    hasPrevPage
  } = usePaginatedCustomers({
    pageNumber: 1,
    pageSize: 20,
    isActive: true
  });
  const { execute: createCustomer, loading: createLoading, error: createError } = useCreateCustomer();
  const { execute: fetchCustomer, data: singleCustomer, loading: singleCustomerLoading } = useCustomer();
  const { execute: updateCustomer, loading: updateLoading, error: updateError } = useUpdateCustomer();
  const { execute: deleteCustomer, loading: deleteLoading } = useDeleteCustomer();
  const { execute: getCustomerOrders, data: customerOrdersData, loading: ordersLoading } = useGetCustomerOrders();
  const { execute: getCustomerLoyalty, data: customerLoyaltyData, loading: loyaltyLoading } = useCustomerLoyalty();
  const { execute: updateCustomerLoyalty, loading: updateLoyaltyLoading } = useUpdateCustomerLoyalty();
  const { execute: searchCustomers, data: searchResults, loading: searchLoading } = useSearchCustomers();

  // Form state for new/edit customer
  const [customerForm, setCustomerForm] = useState({
    fullName: '',
    nationalId: '',
    mobileNumber: '',
    email: '',
    address: '',
    loyaltyTier: 1,
    defaultDiscountPercentage: 0,
    makingChargesWaived: false,
    notes: '',
  });

  // State for customer transactions
  const [customerTransactions, setCustomerTransactions] = useState<CustomerTransaction[]>([]);
  const [transactionsLoading, setTransactionsLoading] = useState(false);
  
  // API hooks for lookups
  const { data: transactionTypesData, execute: fetchTransactionTypes } = useTransactionTypes();
  const { data: transactionStatusesData, execute: fetchTransactionStatuses } = useTransactionStatuses();

  // Fetch customers on component mount
  useEffect(() => {
    updateParams({
      searchTerm: searchQuery,
      pageNumber: 1,
      pageSize: 20
    });
    // Fetch lookup data
    fetchTransactionTypes();
    fetchTransactionStatuses();
  }, [fetchTransactionTypes, fetchTransactionStatuses]);

  // Function to get transaction type display name
  const getTransactionTypeName = (type: string | number): 'sale' | 'return' | 'repair' => {
    // If it's already a string, it might be the display name
    if (typeof type === 'string') {
      // Check if it's a numeric string (enum value)
      const numericValue = parseInt(type);
      if (!isNaN(numericValue)) {
        const transactionType = transactionTypesData?.find(t => t.id === numericValue);
        const displayName = transactionType ? transactionType.name.toLowerCase() : type.toLowerCase();
        return (displayName === 'sale' || displayName === 'return' || displayName === 'repair') 
          ? displayName as 'sale' | 'return' | 'repair' 
          : 'sale';
      }
      // If it's already a display name, return as is if valid
      const lowerType = type.toLowerCase();
      return (lowerType === 'sale' || lowerType === 'return' || lowerType === 'repair') 
        ? lowerType as 'sale' | 'return' | 'repair' 
        : 'sale';
    }
    
    // If it's a number, find the display name
    if (typeof type === 'number') {
      const transactionType = transactionTypesData?.find(t => t.id === type);
      const displayName = transactionType ? transactionType.name.toLowerCase() : 'sale';
      return (displayName === 'sale' || displayName === 'return' || displayName === 'repair') 
        ? displayName as 'sale' | 'return' | 'repair' 
        : 'sale';
    }
    
    return 'sale';
  };

  // Function to fetch customer transactions
  const fetchCustomerTransactions = async (customerId: number) => {
    try {
      setTransactionsLoading(true);
      const result = await getCustomerOrders(customerId, {});
      
      // Transform API transactions to component format
      const transformedTransactions: CustomerTransaction[] = result.orders.map((order: any) => ({
        id: order.orderId.toString(),
        transactionNumber: order.orderNumber,
        date: order.orderDate,
        type: 'sale' as const,
        amount: order.totalAmount,
        items: 1, // Default to 1 since we don't have item count in orders
        status: 'completed',
      }));
      
      setCustomerTransactions(transformedTransactions);
    } catch (error) {
      console.error('Failed to fetch customer transactions:', error);
      setCustomerTransactions([]);
    } finally {
      setTransactionsLoading(false);
    }
  };

  // Filter customers based on search and status
  const filteredCustomers = (customersData?.items || []).filter((customer: Customer) => {
    const matchesSearch = 
      (customer.fullName && customer.fullName.toLowerCase().includes(searchQuery.toLowerCase())) ||
      (customer.mobileNumber && customer.mobileNumber?.includes(searchQuery)) ||
      (customer.email && customer.email?.toLowerCase().includes(searchQuery.toLowerCase())) ||
      (customer.nationalId && customer.nationalId?.toLowerCase().includes(searchQuery.toLowerCase()));
    
    const matchesStatus = 
      statusFilter === 'all' || 
      (statusFilter === 'active' ? customer.isActive : false) ||
      (statusFilter === 'inactive' ? !customer.isActive : false) ||
      (statusFilter === 'vip' ? customer.loyaltyTier >= 3 : false);
    
    return matchesSearch && matchesStatus;
  });

  const getStatusBadge = (status: string) => {
    const variants = {
      active: { variant: 'default' as const, className: 'bg-green-100 text-green-800' },
      inactive: { variant: 'secondary' as const, className: 'bg-gray-100 text-gray-800' },
      vip: { variant: 'default' as const, className: 'bg-purple-100 text-purple-800' },
    };

    const config = variants[status as keyof typeof variants];

    return (
      <Badge variant={config.variant} className={config.className}>
        {status === 'vip' && <Star className="mr-1 h-3 w-3" />}
        {status.toUpperCase()}
      </Badge>
    );
  };

  const handleCreateCustomer = async () => {
    try {
      await createCustomer({
        fullName: customerForm.fullName,
        nationalId: customerForm.nationalId || undefined,
        mobileNumber: customerForm.mobileNumber || undefined,
        email: customerForm.email || undefined,
        address: customerForm.address || undefined,
        notes: customerForm.notes || undefined,
        loyaltyTier: customerForm.loyaltyTier,
        defaultDiscountPercentage: customerForm.defaultDiscountPercentage,
        makingChargesWaived: customerForm.makingChargesWaived
      });
      
      setIsNewCustomerOpen(false);
      resetForm();
      
      // Refresh customer list
      updateParams({
        searchTerm: searchQuery,
        pageNumber: 1,
        pageSize: 20
      });
    } catch (error) {
      console.error('Failed to create customer:', error);
    }
  };

  const handleUpdateCustomer = async () => {
    if (!selectedCustomer) return;
    
    try {
      await updateCustomer(selectedCustomer.id, {
        id: selectedCustomer.id,
        fullName: customerForm.fullName,
        nationalId: customerForm.nationalId || undefined,
        mobileNumber: customerForm.mobileNumber || undefined,
        email: customerForm.email || undefined,
        address: customerForm.address || undefined,
        notes: customerForm.notes || undefined,
        loyaltyTier: customerForm.loyaltyTier,
        defaultDiscountPercentage: customerForm.defaultDiscountPercentage,
        makingChargesWaived: customerForm.makingChargesWaived,
      });
      
      setSelectedCustomer(null);
      setIsEditMode(false);
      resetForm();
      
      // Refresh customer list
      updateParams({
        searchTerm: searchQuery,
        pageNumber: 1,
        pageSize: 20
      });
    } catch (error) {
      console.error('Failed to update customer:', error);
    }
  };

  const resetForm = () => {
    setCustomerForm({
      fullName: '',
      nationalId: '',
      mobileNumber: '',
      email: '',
      address: '',
      loyaltyTier: 1,
      defaultDiscountPercentage: 0,
      makingChargesWaived: false,
      notes: '',
    });
  };

  const populateForm = (customer: Customer) => {
    setCustomerForm({
      fullName: customer.fullName,
      nationalId: customer.nationalId || '',
      mobileNumber: customer.mobileNumber || '',
      email: customer.email || '',
      address: customer.address || '',
      loyaltyTier: customer.loyaltyTier,
      defaultDiscountPercentage: customer.defaultDiscountPercentage || 0,
      makingChargesWaived: customer.makingChargesWaived,
      notes: customer.notes || '',
    });

    // Load customer loyalty data
    getCustomerLoyalty(customer.id);
    // Load customer orders
    getCustomerOrders(customer.id, {});
  };

  const handleSearch = () => {
    updateParams({
      searchTerm: searchQuery,
      pageNumber: 1,
      pageSize: 20
    });
  };

  const handleDeleteCustomer = (customerId: number) => {
    setCustomerToDelete(customerId);
    setDeleteConfirmOpen(true);
  };

  const confirmDeleteCustomer = async () => {
    if (!customerToDelete) return;

    try {
      await deleteCustomer(customerToDelete);
      
      // Refresh customer list
      updateParams({
        searchTerm: searchQuery,
        pageNumber: 1,
        pageSize: 20
      });
      
      setDeleteConfirmOpen(false);
      setCustomerToDelete(null);
    } catch (error) {
      console.error('Failed to delete customer:', error);
    }
  };

  const handleUpdateLoyalty = async () => {
    if (!selectedCustomer) return;

    try {
      await updateCustomerLoyalty(selectedCustomer.id, {
        customerId: selectedCustomer.id,
        loyaltyTier: customerForm.loyaltyTier,
        loyaltyPoints: customerLoyaltyData?.currentPoints || 0,
        defaultDiscountPercentage: customerForm.defaultDiscountPercentage,
        makingChargesWaived: customerForm.makingChargesWaived,
      });

      // Refresh customer data
      await fetchCustomer(selectedCustomer.id);
      await getCustomerLoyalty(selectedCustomer.id);
      
      // Refresh customer list
      updateParams({
        searchTerm: searchQuery,
        pageNumber: 1,
        pageSize: 20
      });
    } catch (error) {
      console.error('Failed to update customer loyalty:', error);
    }
  };

  const customerStats = {
    total: customersData?.items?.length || 0,
    active: (customersData?.items || []).filter((c: Customer) => c.isActive).length,
    vip: (customersData?.items || []).filter((c: Customer) => c.loyaltyTier >= 3).length, // Assuming VIP is tier 3+
    totalValue: (customersData?.items || []).reduce((sum: number, c: Customer) => sum + (c.totalPurchaseAmount || 0), 0),
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl text-[#0D1B2A]">Customer Management</h1>
          <p className="text-muted-foreground">Manage customer database and relationships</p>
        </div>
        <Dialog open={isNewCustomerOpen} onOpenChange={setIsNewCustomerOpen}>
          <DialogTrigger asChild>
            <Button className="touch-target" variant="golden">
              <Plus className="mr-2 h-4 w-4" />
              Add Customer
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-2xl bg-white border-gray-200 shadow-lg">
            <DialogHeader>
              <DialogTitle>Add New Customer</DialogTitle>
              <DialogDescription>
                Enter customer details to add to database
              </DialogDescription>
            </DialogHeader>
            {createError && (
              <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
                {createError}
              </div>
            )}
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="fullName">Full Name *</Label>
                <Input
                  id="fullName"
                  value={customerForm.fullName}
                  onChange={(e) => setCustomerForm({...customerForm, fullName: e.target.value})}
                  placeholder="Enter full name"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="nationalId">National ID</Label>
                <Input
                  id="nationalId"
                  value={customerForm.nationalId}
                  onChange={(e) => setCustomerForm({...customerForm, nationalId: e.target.value})}
                  placeholder="National ID number"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="mobileNumber">Mobile Number</Label>
                <Input
                  id="mobileNumber"
                  value={customerForm.mobileNumber}
                  onChange={(e) => setCustomerForm({...customerForm, mobileNumber: e.target.value})}
                  placeholder="+20 100 123 4567"
                />
              </div>
              <div className="col-span-2 space-y-2">
                <Label htmlFor="email">Email Address</Label>
                <Input
                  id="email"
                  type="email"
                  value={customerForm.email}
                  onChange={(e) => setCustomerForm({...customerForm, email: e.target.value})}
                  placeholder="customer@email.com"
                />
              </div>
              <div className="col-span-2 space-y-2">
                <Label htmlFor="address">Address</Label>
                <Textarea
                  id="address"
                  value={customerForm.address}
                  onChange={(e) => setCustomerForm({...customerForm, address: e.target.value})}
                  placeholder="Street address"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="loyaltyTier">Loyalty Tier</Label>
                <Select value={customerForm.loyaltyTier.toString()} onValueChange={(value) => setCustomerForm({...customerForm, loyaltyTier: parseInt(value)})}>
                  <SelectTrigger className="bg-white border border-gray-300 focus:border-blue-500 focus:ring-2 focus:ring-blue-200">
                    <SelectValue placeholder="Select loyalty tier" />
                  </SelectTrigger>
                  <SelectContent className="bg-white border border-gray-300 shadow-lg">
                    <SelectItem value="1">Tier 1 - Standard</SelectItem>
                    <SelectItem value="2">Tier 2 - Silver</SelectItem>
                    <SelectItem value="3">Tier 3 - Gold</SelectItem>
                    <SelectItem value="4">Tier 4 - Platinum</SelectItem>
                    <SelectItem value="5">Tier 5 - Diamond</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="defaultDiscountPercentage">Default Discount (%)</Label>
                <Input
                  id="defaultDiscountPercentage"
                  type="number"
                  min="0"
                  max="100"
                  step="0.1"
                  value={customerForm.defaultDiscountPercentage}
                  onChange={(e) => setCustomerForm({...customerForm, defaultDiscountPercentage: parseFloat(e.target.value) || 0})}
                  placeholder="0"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="makingChargesWaived">Making Charges Waived</Label>
                <Select value={customerForm.makingChargesWaived.toString()} onValueChange={(value) => setCustomerForm({...customerForm, makingChargesWaived: value === 'true'})}>
                  <SelectTrigger className="bg-white border border-gray-300 focus:border-blue-500 focus:ring-2 focus:ring-blue-200">
                    <SelectValue placeholder="Select option" />
                  </SelectTrigger>
                  <SelectContent className="bg-white border border-gray-300 shadow-lg">
                    <SelectItem value="false">No - Apply making charges</SelectItem>
                    <SelectItem value="true">Yes - Waive making charges</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="col-span-2 space-y-2">
                <Label htmlFor="notes">Notes</Label>
                <Textarea
                  id="notes"
                  value={customerForm.notes}
                  onChange={(e) => setCustomerForm({...customerForm, notes: e.target.value})}
                  placeholder="Additional notes about customer preferences"
                />
              </div>
            </div>
            <div className="flex justify-end gap-3 mt-6">
              <Button variant="outline" onClick={() => setIsNewCustomerOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleCreateCustomer} variant="golden" disabled={createLoading}>
                {createLoading ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Adding...
                  </>
                ) : (
                  'Add Customer'
                )}
              </Button>
            </div>
          </DialogContent>
        </Dialog>
      </div>

      {/* Customer Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total Customers</p>
                <p className="text-2xl text-[#0D1B2A]">{customerStats.total}</p>
              </div>
              <Users className="h-8 w-8 text-[#D4AF37]" />
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Active Customers</p>
                <p className="text-2xl text-[#0D1B2A]">{customerStats.active}</p>
              </div>
              <Users className="h-8 w-8 text-green-600" />
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">VIP Customers</p>
                <p className="text-2xl text-[#0D1B2A]">{customerStats.vip}</p>
              </div>
              <Star className="h-8 w-8 text-purple-600" />
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total Value</p>
                <p className="text-2xl text-[#0D1B2A]">{formatCurrency(customerStats.totalValue)}</p>
              </div>
              <ShoppingBag className="h-8 w-8 text-blue-600" />
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
                placeholder="Search by name, phone, email, or customer number..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
                className="pl-10 touch-target"
              />
            </div>
            <Select value={statusFilter} onValueChange={setStatusFilter}>
              <SelectTrigger className="w-full md:w-40">
                <SelectValue placeholder="Status" />
              </SelectTrigger>
              <SelectContent className="bg-white border-gray-200 shadow-lg">
                <SelectItem value="all" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">All Status</SelectItem>
                <SelectItem value="active" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Active</SelectItem>
                <SelectItem value="inactive" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Inactive</SelectItem>
                <SelectItem value="vip" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">VIP</SelectItem>
              </SelectContent>
            </Select>
            <Button onClick={handleSearch} variant="outline" className="touch-target">
              <Search className="mr-2 h-4 w-4" />
              Search
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Customers Table */}
      <Card className="pos-card">
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Customers</CardTitle>
              <CardDescription>
                {customersLoading ? 'Loading customers...' : `${filteredCustomers.length} customer(s) found`}
              </CardDescription>
            </div>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setShowBulkActions(!showBulkActions)}
                className="touch-target"
              >
                Bulk Actions
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {customersError && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-4">
              Error loading customers: {customersError}
            </div>
          )}
          
          {customersLoading ? (
            <div className="text-center py-8">
              <Loader2 className="h-8 w-8 animate-spin text-muted-foreground mx-auto mb-4" />
              <p className="text-muted-foreground">Loading customers...</p>
            </div>
          ) : filteredCustomers.length === 0 ? (
            <div className="text-center py-8">
              <Users className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <p className="text-muted-foreground">No customers found</p>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>National ID</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead>Contact</TableHead>
                  <TableHead>Total Purchases</TableHead>
                  <TableHead>Loyalty Tier</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Last Purchase</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredCustomers.map((customer: Customer) => (
                  <TableRow key={customer.id}>
                    <TableCell className="font-medium">{customer.nationalId || 'N/A'}</TableCell>
                    <TableCell>
                      <div>
                        <p className="font-medium">{customer.fullName}</p>
                        <p className="text-sm text-muted-foreground">Since {new Date(customer.createdAt).getFullYear()}</p>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="space-y-1">
                        <div className="flex items-center gap-1 text-sm">
                          <Phone className="h-3 w-3" />
                          {customer.mobileNumber || 'N/A'}
                        </div>
                        {customer.email && (
                          <div className="flex items-center gap-1 text-sm">
                            <Mail className="h-3 w-3" />
                            {customer.email}
                          </div>
                        )}
                      </div>
                    </TableCell>
                    <TableCell>{formatCurrency(customer.totalPurchaseAmount)}</TableCell>
                    <TableCell>
                      <div className="flex items-center gap-1">
                        <Gift className="h-4 w-4 text-[#D4AF37]" />
                        Tier {customer.loyaltyTier}
                      </div>
                    </TableCell>
                    <TableCell>{getStatusBadge(customer.isActive ? 'active' : 'inactive')}</TableCell>
                    <TableCell>{customer.lastPurchaseDate ? new Date(customer.lastPurchaseDate).toLocaleDateString() : 'N/A'}</TableCell>
                    <TableCell>
                      <div className="flex gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => {
                            setSelectedCustomer(customer);
                            populateForm(customer);
                          }}
                          className="touch-target hover:bg-[#F4E9B1] transition-colors"
                        >
                          <Eye className="mr-2 h-4 w-4" />
                          View
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => {
                            setSelectedCustomer(customer);
                            populateForm(customer);
                            setIsEditMode(true);
                          }}
                          className="touch-target hover:bg-[#F4E9B1] transition-colors"
                        >
                          <Edit className="mr-2 h-4 w-4" />
                          Edit
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => handleDeleteCustomer(customer.id)}
                          className="touch-target hover:bg-red-50 hover:text-red-600 transition-colors"
                          disabled={deleteLoading}
                        >
                          <Trash2 className="mr-2 h-4 w-4" />
                          Delete
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

      {/* Customer Details Dialog */}
      {selectedCustomer && (
        <Dialog open={!!selectedCustomer} onOpenChange={() => {
          setSelectedCustomer(null);
          setIsEditMode(false);
        }}>
          <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto bg-white border-gray-200 shadow-lg">
            <DialogHeader>
              <DialogTitle>
                {isEditMode ? 'Edit Customer' : 'Customer Details'} - {selectedCustomer.nationalId || selectedCustomer.fullName}
              </DialogTitle>
            </DialogHeader>
            {updateError && (
              <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
                {updateError}
              </div>
            )}
            
            <Tabs defaultValue="details" className="w-full">
              <TabsList>
                <TabsTrigger value="details">Details</TabsTrigger>
                <TabsTrigger value="transactions">Transactions</TabsTrigger>
                <TabsTrigger value="loyalty">Loyalty</TabsTrigger>
              </TabsList>
              
              <TabsContent value="details" className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="fullName">Full Name</Label>
                    <Input
                      id="fullName"
                      value={customerForm.fullName}
                      onChange={(e) => setCustomerForm({...customerForm, fullName: e.target.value})}
                      readOnly={!isEditMode}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="nationalId">National ID</Label>
                    <Input
                      id="nationalId"
                      value={customerForm.nationalId}
                      onChange={(e) => setCustomerForm({...customerForm, nationalId: e.target.value})}
                      readOnly={!isEditMode}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="mobileNumber">Mobile Number</Label>
                    <Input
                      id="mobileNumber"
                      value={customerForm.mobileNumber}
                      onChange={(e) => setCustomerForm({...customerForm, mobileNumber: e.target.value})}
                      readOnly={!isEditMode}
                    />
                  </div>
                  <div className="col-span-2 space-y-2">
                    <Label htmlFor="email">Email Address</Label>
                    <Input
                      id="email"
                      value={customerForm.email}
                      onChange={(e) => setCustomerForm({...customerForm, email: e.target.value})}
                      readOnly={!isEditMode}
                    />
                  </div>
                  <div className="col-span-2 space-y-2">
                    <Label htmlFor="address">Address</Label>
                    <Textarea
                      id="address"
                      value={customerForm.address}
                      onChange={(e) => setCustomerForm({...customerForm, address: e.target.value})}
                      readOnly={!isEditMode}
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="loyaltyTier">Loyalty Tier</Label>
                    {isEditMode ? (
                      <Select value={customerForm.loyaltyTier.toString()} onValueChange={(value) => setCustomerForm({...customerForm, loyaltyTier: parseInt(value)})}>
                        <SelectTrigger className="bg-white border border-gray-300 focus:border-blue-500 focus:ring-2 focus:ring-blue-200">
                          <SelectValue placeholder="Select loyalty tier" />
                        </SelectTrigger>
                        <SelectContent className="bg-white border border-gray-300 shadow-lg">
                          <SelectItem value="1">Tier 1 - Standard</SelectItem>
                          <SelectItem value="2">Tier 2 - Silver</SelectItem>
                          <SelectItem value="3">Tier 3 - Gold</SelectItem>
                          <SelectItem value="4">Tier 4 - Platinum</SelectItem>
                          <SelectItem value="5">Tier 5 - Diamond</SelectItem>
                        </SelectContent>
                      </Select>
                    ) : (
                      <Input
                        value={`Tier ${customerForm.loyaltyTier}`}
                        readOnly
                      />
                    )}
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="defaultDiscountPercentage">Default Discount (%)</Label>
                    <Input
                      id="defaultDiscountPercentage"
                      type="number"
                      min="0"
                      max="100"
                      step="0.1"
                      value={customerForm.defaultDiscountPercentage}
                      onChange={(e) => setCustomerForm({...customerForm, defaultDiscountPercentage: parseFloat(e.target.value) || 0})}
                      readOnly={!isEditMode}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="makingChargesWaived">Making Charges Waived</Label>
                    {isEditMode ? (
                      <Select value={customerForm.makingChargesWaived.toString()} onValueChange={(value) => setCustomerForm({...customerForm, makingChargesWaived: value === 'true'})}>
                        <SelectTrigger className="bg-white border border-gray-300 focus:border-blue-500 focus:ring-2 focus:ring-blue-200">
                          <SelectValue placeholder="Select option" />
                        </SelectTrigger>
                        <SelectContent className="bg-white border border-gray-300 shadow-lg">
                          <SelectItem value="false">No - Apply making charges</SelectItem>
                          <SelectItem value="true">Yes - Waive making charges</SelectItem>
                        </SelectContent>
                      </Select>
                    ) : (
                      <Input
                        value={customerForm.makingChargesWaived ? "Yes - Waived" : "No - Applied"}
                        readOnly
                      />
                    )}
                  </div>
                </div>
              </TabsContent>
              
              <TabsContent value="transactions" className="space-y-4">
                <div className="flex justify-between items-center">
                  <h3 className="text-lg font-semibold">Customer Orders</h3>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => fetchCustomerTransactions(selectedCustomer.id)}
                    disabled={ordersLoading}
                  >
                                         {ordersLoading ? (
                       <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                     ) : (
                       <Calendar className="mr-2 h-4 w-4" />
                     )}
                    Refresh Orders
                  </Button>
                </div>
                
                {customerOrdersData?.orders && customerOrdersData.orders.length > 0 ? (
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Date</TableHead>
                        <TableHead>Order #</TableHead>
                        <TableHead>Items</TableHead>
                        <TableHead>Total Amount</TableHead>
                        <TableHead>Status</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {customerOrdersData.orders.map((order: any) => (
                        <TableRow key={order.orderId}>
                          <TableCell>{new Date(order.orderDate).toLocaleDateString()}</TableCell>
                          <TableCell className="font-medium">{order.orderNumber}</TableCell>
                          <TableCell>{order.items?.length || 0} items</TableCell>
                          <TableCell className="font-semibold">
                            {formatCurrency(order.totalAmount)}
                          </TableCell>
                          <TableCell>
                            <Badge variant="default" className="bg-green-100 text-green-800">
                              {order.status || 'Completed'}
                            </Badge>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                ) : (
                  <div className="text-center py-8 text-muted-foreground">
                    <ShoppingBag className="h-12 w-12 mx-auto mb-4 text-gray-400" />
                    <p>No orders found for this customer</p>
                  </div>
                )}
              </TabsContent>
              
              <TabsContent value="loyalty" className="space-y-4">
                <div className="flex justify-between items-center">
                  <h3 className="text-lg font-semibold">Loyalty Information</h3>
                  {isEditMode && (
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleUpdateLoyalty()}
                      disabled={updateLoyaltyLoading}
                    >
                      {updateLoyaltyLoading ? (
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      ) : (
                        <Star className="mr-2 h-4 w-4" />
                      )}
                      Update Loyalty
                    </Button>
                  )}
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <Card className="pos-card">
                    <CardContent className="pt-6">
                      <div className="text-center">
                        <Gift className="h-12 w-12 text-[#D4AF37] mx-auto mb-2" />
                        <p className="text-2xl font-bold text-[#0D1B2A]">{selectedCustomer.loyaltyTier}</p>
                        <p className="text-sm text-muted-foreground">Loyalty Tier</p>
                        {isEditMode && (
                          <Select 
                            value={customerForm.loyaltyTier.toString()} 
                            onValueChange={(value) => setCustomerForm({...customerForm, loyaltyTier: parseInt(value)})}
                          >
                            <SelectTrigger className="mt-2">
                              <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                              <SelectItem value="1">Tier 1 - Standard</SelectItem>
                              <SelectItem value="2">Tier 2 - Silver</SelectItem>
                              <SelectItem value="3">Tier 3 - Gold</SelectItem>
                              <SelectItem value="4">Tier 4 - Platinum</SelectItem>
                              <SelectItem value="5">Tier 5 - Diamond</SelectItem>
                            </SelectContent>
                          </Select>
                        )}
                      </div>
                    </CardContent>
                  </Card>
                  
                  <Card className="pos-card">
                    <CardContent className="pt-6">
                      <div className="text-center">
                        <Star className="h-12 w-12 text-purple-600 mx-auto mb-2" />
                        <p className="text-lg font-medium text-[#0D1B2A]">{selectedCustomer.isActive ? 'ACTIVE' : 'INACTIVE'}</p>
                        <p className="text-sm text-muted-foreground">Customer Status</p>
                      </div>
                    </CardContent>
                  </Card>
                </div>
                
                <div className="grid grid-cols-2 gap-4">
                  <Card className="pos-card">
                    <CardContent className="pt-6">
                      <div className="text-center">
                        <TrendingDown className="h-12 w-12 text-green-600 mx-auto mb-2" />
                        <p className="text-2xl font-bold text-[#0D1B2A]">{selectedCustomer.defaultDiscountPercentage || 0}%</p>
                        <p className="text-sm text-muted-foreground">Default Discount</p>
                        {isEditMode && (
                          <Input
                            type="number"
                            min="0"
                            max="100"
                            step="0.1"
                            value={customerForm.defaultDiscountPercentage}
                            onChange={(e) => setCustomerForm({...customerForm, defaultDiscountPercentage: parseFloat(e.target.value) || 0})}
                            className="mt-2"
                            placeholder="Discount %"
                          />
                        )}
                      </div>
                    </CardContent>
                  </Card>
                  
                  <Card className="pos-card">
                    <CardContent className="pt-6">
                      <div className="text-center">
                        <DollarSign className="h-12 w-12 text-blue-600 mx-auto mb-2" />
                        <p className="text-lg font-medium text-[#0D1B2A]">{selectedCustomer.makingChargesWaived ? 'WAIVED' : 'APPLIED'}</p>
                        <p className="text-sm text-muted-foreground">Making Charges</p>
                        {isEditMode && (
                          <Select 
                            value={customerForm.makingChargesWaived.toString()} 
                            onValueChange={(value) => setCustomerForm({...customerForm, makingChargesWaived: value === 'true'})}
                          >
                            <SelectTrigger className="mt-2">
                              <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                              <SelectItem value="false">Apply Making Charges</SelectItem>
                              <SelectItem value="true">Waive Making Charges</SelectItem>
                            </SelectContent>
                          </Select>
                        )}
                      </div>
                    </CardContent>
                  </Card>
                </div>

                {customerLoyaltyData && (
                  <Card className="pos-card">
                    <CardContent className="pt-6">
                      <h4 className="font-semibold mb-4">Detailed Loyalty Information</h4>
                      <div className="grid grid-cols-2 gap-4 text-sm">
                        <div>
                          <p><strong>Current Points:</strong> {customerLoyaltyData.currentPoints || 0}</p>
                          <p><strong>Total Orders:</strong> {customerLoyaltyData.totalOrders || 0}</p>
                          <p><strong>Total Purchase Amount:</strong> {formatCurrency(customerLoyaltyData.totalPurchaseAmount || 0)}</p>
                        </div>
                        <div>
                          <p><strong>Last Purchase Date:</strong> {customerLoyaltyData.lastPurchaseDate ? new Date(customerLoyaltyData.lastPurchaseDate).toLocaleDateString() : 'N/A'}</p>
                          <p><strong>Points to Next Tier:</strong> {customerLoyaltyData.pointsToNextTier || 0}</p>
                          <p><strong>Current Tier:</strong> {customerLoyaltyData.currentTier || 1}</p>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                )}
                
                <div className="space-y-2">
                  <Label>Customer Information</Label>
                  <div className="text-sm text-muted-foreground">
                    <p>Customer since: {new Date(selectedCustomer.createdAt).toLocaleDateString()}</p>
                    <p>Total purchases: {formatCurrency(selectedCustomer.totalPurchaseAmount)}</p>
                    {selectedCustomer.lastPurchaseDate && (
                      <p>Last purchase: {new Date(selectedCustomer.lastPurchaseDate).toLocaleDateString()}</p>
                    )}
                  </div>
                </div>
              </TabsContent>
            </Tabs>
            
            <div className="flex justify-end gap-3 mt-6">
              <Button variant="outline" onClick={() => {
                setSelectedCustomer(null);
                setIsEditMode(false);
              }}>
                {isEditMode ? 'Cancel' : 'Close'}
              </Button>
              {isEditMode && (
                <Button onClick={handleUpdateCustomer} variant="golden" disabled={updateLoading}>
                  {updateLoading ? (
                    <>
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      Updating...
                    </>
                  ) : (
                    'Update Customer'
                  )}
                </Button>
              )}
              {!isEditMode && (
                <Button onClick={() => setIsEditMode(true)} variant="golden">
                  <Edit className="mr-2 h-4 w-4" />
                  Edit Customer
                </Button>
              )}
            </div>
          </DialogContent>
        </Dialog>
      )}

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteConfirmOpen} onOpenChange={setDeleteConfirmOpen}>
        <DialogContent className="max-w-md bg-white border-gray-200 shadow-lg">
          <DialogHeader>
            <DialogTitle>Confirm Delete</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete this customer? This action cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <div className="flex justify-end gap-3 mt-6">
            <Button 
              variant="outline" 
              onClick={() => {
                setDeleteConfirmOpen(false);
                setCustomerToDelete(null);
              }}
            >
              Cancel
            </Button>
            <Button 
              variant="destructive" 
              onClick={confirmDeleteCustomer}
              disabled={deleteLoading}
            >
              {deleteLoading ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Deleting...
                </>
              ) : (
                'Delete Customer'
              )}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}