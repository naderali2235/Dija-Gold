import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Badge } from './ui/badge';
import { Textarea } from './ui/textarea';
import { Switch } from './ui/switch';
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
  Truck,
  Plus,
  Search,
  Phone,
  Mail,
  MapPin,
  DollarSign,
  Package,
  Calendar,
  Edit,
  Eye,
  Loader2,
  CheckCircle,
  XCircle,
  Building,
  CreditCard,
} from 'lucide-react';
import { useAuth } from './AuthContext';
import { formatCurrency } from './utils/currency';
import { toast } from 'sonner';
import {
  usePaginatedSuppliers,
  useCreateSupplier,
  useUpdateSupplier,
  useDeleteSupplier,
  useSupplierProducts,
  useSupplierBalance,
  useUpdateSupplierBalance,
  useSupplierTransactions,
} from '../hooks/useApi';
import {
  SupplierDto,
  CreateSupplierRequest,
  UpdateSupplierRequest,
  SupplierProductsDto,
  SupplierBalanceDto,
  UpdateSupplierBalanceRequest,
  SupplierTransactionDto,
} from '../services/api';

interface SupplierFormData {
  companyName: string;
  contactPersonName: string;
  phone: string;
  email: string;
  address: string;
  taxRegistrationNumber: string;
  commercialRegistrationNumber: string;
  creditLimit: number;
  paymentTermsDays: number;
  creditLimitEnforced: boolean;
  paymentTerms: string;
  notes: string;
}

const defaultSupplierForm: SupplierFormData = {
  companyName: '',
  contactPersonName: '',
  phone: '',
  email: '',
  address: '',
  taxRegistrationNumber: '',
  commercialRegistrationNumber: '',
  creditLimit: 0,
  paymentTermsDays: 30,
  creditLimitEnforced: true,
  paymentTerms: '',
  notes: '',
};

export default function Suppliers() {
  const { user: currentUser, isManager } = useAuth();
  
  // State management
  const [activeTab, setActiveTab] = useState('suppliers');
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [isNewSupplierOpen, setIsNewSupplierOpen] = useState(false);
  const [selectedSupplier, setSelectedSupplier] = useState<SupplierDto | null>(null);
  const [isEditMode, setIsEditMode] = useState(false);
  const [supplierForm, setSupplierForm] = useState<SupplierFormData>(defaultSupplierForm);
  
  // Balance update form
  const [isBalanceUpdateOpen, setIsBalanceUpdateOpen] = useState(false);
  const [balanceForm, setBalanceForm] = useState({
    amount: 0,
    transactionType: 'payment',
    notes: '',
  });

  // API Hooks
  const {
    data: suppliersData,
    loading: suppliersLoading,
    error: suppliersError,
    fetchData: refetchSuppliers,
    updateParams: updateSuppliersParams,
    hasNextPage,
    hasPrevPage,
    nextPage,
    prevPage,
    setPage,
  } = usePaginatedSuppliers({
    searchTerm: searchQuery,
    isActive: statusFilter === 'active' ? true : statusFilter === 'inactive' ? false : undefined,
  }) as {
    data: { items: SupplierDto[]; totalCount: number; pageNumber: number; pageSize: number } | null;
    loading: boolean;
    error: string | null;
    fetchData: () => Promise<any>;
    updateParams: (params: any) => void;
    hasNextPage: boolean;
    hasPrevPage: boolean;
    nextPage: () => void;
    prevPage: () => void;
    setPage: (page: number) => void;
  };

  const { execute: createSupplier, loading: createLoading } = useCreateSupplier();
  const { execute: updateSupplier, loading: updateLoading } = useUpdateSupplier();
  const { execute: deleteSupplier, loading: deleteLoading } = useDeleteSupplier();
  const { execute: fetchSupplierProducts, loading: productsLoading } = useSupplierProducts();
  const { execute: fetchSupplierBalance, loading: balanceLoading } = useSupplierBalance();
  const { execute: updateSupplierBalance, loading: balanceUpdateLoading } = useUpdateSupplierBalance();
  const { execute: fetchSupplierTransactions, loading: transactionsLoading } = useSupplierTransactions();

  // Local state for supplier details
  const [supplierProducts, setSupplierProducts] = useState<SupplierProductsDto | null>(null);
  const [supplierBalance, setSupplierBalance] = useState<SupplierBalanceDto | null>(null);
  const [supplierTransactions, setSupplierTransactions] = useState<SupplierTransactionDto[] | null>(null);

  // Update search parameters with debounce
  useEffect(() => {
    const timeoutId = setTimeout(() => {
      updateSuppliersParams({
        searchTerm: searchQuery || undefined,
        isActive: statusFilter === 'active' ? true : statusFilter === 'inactive' ? false : undefined,
        pageNumber: 1,
      });
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [searchQuery, statusFilter, updateSuppliersParams]);

  // Handlers
  const handleCreateSupplier = async () => {
    if (!supplierForm.companyName) {
      toast.error('Company name is required');
      return;
    }

    try {
      const createSupplierData: CreateSupplierRequest = {
        companyName: supplierForm.companyName,
        contactPersonName: supplierForm.contactPersonName || undefined,
        phone: supplierForm.phone || undefined,
        email: supplierForm.email || undefined,
        address: supplierForm.address || undefined,
        taxRegistrationNumber: supplierForm.taxRegistrationNumber || undefined,
        commercialRegistrationNumber: supplierForm.commercialRegistrationNumber || undefined,
        creditLimit: supplierForm.creditLimit,
        paymentTermsDays: supplierForm.paymentTermsDays,
        creditLimitEnforced: supplierForm.creditLimitEnforced,
        paymentTerms: supplierForm.paymentTerms || undefined,
        notes: supplierForm.notes || undefined,
      };

      await createSupplier(createSupplierData);
      toast.success('Supplier created successfully');
      setIsNewSupplierOpen(false);
      resetForm();
      refetchSuppliers();
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to create supplier');
    }
  };

  const handleUpdateSupplier = async () => {
    if (!selectedSupplier) return;

    try {
      const updateSupplierData: UpdateSupplierRequest = {
        id: selectedSupplier.id,
        companyName: supplierForm.companyName,
        contactPersonName: supplierForm.contactPersonName || undefined,
        phone: supplierForm.phone || undefined,
        email: supplierForm.email || undefined,
        address: supplierForm.address || undefined,
        taxRegistrationNumber: supplierForm.taxRegistrationNumber || undefined,
        commercialRegistrationNumber: supplierForm.commercialRegistrationNumber || undefined,
        creditLimit: supplierForm.creditLimit,
        paymentTermsDays: supplierForm.paymentTermsDays,
        creditLimitEnforced: supplierForm.creditLimitEnforced,
        paymentTerms: supplierForm.paymentTerms || undefined,
        notes: supplierForm.notes || undefined,
      };

      await updateSupplier(selectedSupplier.id, updateSupplierData);
      toast.success('Supplier updated successfully');
      setSelectedSupplier(null);
      setIsEditMode(false);
      resetForm();
      refetchSuppliers();
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to update supplier');
    }
  };

  const handleDeleteSupplier = async (supplierId: number, supplierName: string) => {
    if (!window.confirm(`Are you sure you want to delete ${supplierName}?`)) return;

    try {
      await deleteSupplier(supplierId);
      toast.success('Supplier deleted successfully');
      refetchSuppliers();
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to delete supplier');
    }
  };

  const handleViewSupplierDetails = async (supplier: SupplierDto) => {
    setSelectedSupplier(supplier);
    populateForm(supplier);

    // Fetch supplier details
    try {
      const [products, balance, transactions] = await Promise.all([
        fetchSupplierProducts(supplier.id),
        fetchSupplierBalance(supplier.id),
        fetchSupplierTransactions(supplier.id, { pageSize: 10 }),
      ]);
      setSupplierProducts(products);
      setSupplierBalance(balance);
      setSupplierTransactions(transactions);
    } catch (error) {
      console.error('Failed to fetch supplier details:', error);
    }
  };

  const handleUpdateBalance = async () => {
    if (!selectedSupplier) return;

    if (!balanceForm.amount || balanceForm.amount === 0) {
      toast.error('Amount is required');
      return;
    }

    try {
      const updateBalanceData: UpdateSupplierBalanceRequest = {
        supplierId: selectedSupplier.id,
        amount: balanceForm.amount,
        transactionType: balanceForm.transactionType,
        notes: balanceForm.notes || undefined,
      };

      await updateSupplierBalance(selectedSupplier.id, updateBalanceData);
      toast.success('Supplier balance updated successfully');
      setIsBalanceUpdateOpen(false);
      setBalanceForm({ amount: 0, transactionType: 'payment', notes: '' });
      
      // Refresh supplier balance
      const updatedBalance = await fetchSupplierBalance(selectedSupplier.id);
      setSupplierBalance(updatedBalance);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to update supplier balance');
    }
  };

  const resetForm = () => {
    setSupplierForm(defaultSupplierForm);
  };

  const populateForm = (supplier: SupplierDto) => {
    setSupplierForm({
      companyName: supplier.companyName,
      contactPersonName: supplier.contactPersonName || '',
      phone: supplier.phone || '',
      email: supplier.email || '',
      address: supplier.address || '',
      taxRegistrationNumber: supplier.taxRegistrationNumber || '',
      commercialRegistrationNumber: supplier.commercialRegistrationNumber || '',
      creditLimit: supplier.creditLimit,
      paymentTermsDays: supplier.paymentTermsDays,
      creditLimitEnforced: supplier.creditLimitEnforced,
      paymentTerms: supplier.paymentTerms || '',
      notes: supplier.notes || '',
    });
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const getStatusBadgeColor = (isActive: boolean) => {
    return isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800';
  };

  const getBalanceColor = (balance: number) => {
    if (balance > 0) return 'text-red-600'; // Owe money
    if (balance < 0) return 'text-green-600'; // Credit balance
    return 'text-gray-600'; // Zero balance
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <h1 className="text-3xl text-[#0D1B2A]">Suppliers Management</h1>
        {isManager && (
          <Button 
            onClick={() => setIsNewSupplierOpen(true)}
            className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]"
          >
            <Plus className="mr-2 h-4 w-4" />
            Add Supplier
          </Button>
        )}
      </div>

      {/* Filters */}
      <Card className="pos-card">
        <CardContent className="pt-6">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div className="relative">
              <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search suppliers..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-10"
              />
            </div>
            <Select value={statusFilter} onValueChange={setStatusFilter}>
              <SelectTrigger>
                <SelectValue placeholder="Filter by status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="active">Active</SelectItem>
                <SelectItem value="inactive">Inactive</SelectItem>
              </SelectContent>
            </Select>
            <div />
            <Button 
              variant="outline" 
              onClick={() => {
                setSearchQuery('');
                setStatusFilter('all');
              }}
            >
              Clear Filters
            </Button>
          </div>
        </CardContent>
      </Card>

      <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
        <TabsList className="grid w-full grid-cols-2">
          <TabsTrigger value="suppliers">Suppliers</TabsTrigger>
          <TabsTrigger value="transactions">Transactions</TabsTrigger>
        </TabsList>

        <TabsContent value="suppliers" className="space-y-4">
          {/* Suppliers Table */}
          <Card className="pos-card">
            <CardContent className="p-0">
              {suppliersLoading && (
                <div className="flex justify-center items-center py-12">
                  <Loader2 className="h-8 w-8 animate-spin" />
                </div>
              )}

              {suppliersError && (
                <div className="text-center py-12 text-red-600">
                  <p>Error loading suppliers: {suppliersError}</p>
                  <Button onClick={refetchSuppliers} className="mt-4">
                    Try Again
                  </Button>
                </div>
              )}

              {suppliersData && suppliersData.items && (
                <>
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Supplier</TableHead>
                        <TableHead>Contact</TableHead>
                        <TableHead>Balance</TableHead>
                        <TableHead>Credit Limit</TableHead>
                        <TableHead>Status</TableHead>
                        <TableHead>Last Transaction</TableHead>
                        <TableHead>Actions</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {suppliersData.items.map((supplier) => (
                        <TableRow key={supplier.id}>
                          <TableCell>
                            <div className="flex items-center space-x-3">
                              <Building className="h-8 w-8 text-muted-foreground" />
                              <div>
                                <div className="font-medium">{supplier.companyName}</div>
                                <div className="text-sm text-muted-foreground">
                                  {supplier.contactPersonName}
                                </div>
                                {supplier.taxRegistrationNumber && (
                                  <div className="text-xs text-muted-foreground">
                                    Tax: {supplier.taxRegistrationNumber}
                                  </div>
                                )}
                              </div>
                            </div>
                          </TableCell>
                          <TableCell>
                            <div className="space-y-1">
                              {supplier.phone && (
                                <div className="flex items-center text-sm">
                                  <Phone className="h-3 w-3 mr-1" />
                                  {supplier.phone}
                                </div>
                              )}
                              {supplier.email && (
                                <div className="flex items-center text-sm">
                                  <Mail className="h-3 w-3 mr-1" />
                                  {supplier.email}
                                </div>
                              )}
                              {supplier.address && (
                                <div className="flex items-center text-sm">
                                  <MapPin className="h-3 w-3 mr-1" />
                                  {supplier.address.substring(0, 30)}...
                                </div>
                              )}
                            </div>
                          </TableCell>
                          <TableCell>
                            <div className={`font-medium ${getBalanceColor(supplier.currentBalance)}`}>
                              {formatCurrency(supplier.currentBalance)}
                            </div>
                          </TableCell>
                          <TableCell>
                            <div className="space-y-1">
                              <div className="font-medium">
                                {formatCurrency(supplier.creditLimit)}
                              </div>
                              <div className="text-xs text-muted-foreground">
                                {supplier.paymentTermsDays} days
                              </div>
                              {supplier.creditLimitEnforced && (
                                <Badge variant="outline" className="text-xs">
                                  Enforced
                                </Badge>
                              )}
                            </div>
                          </TableCell>
                          <TableCell>
                            <Badge 
                              variant="secondary" 
                              className={getStatusBadgeColor(supplier.isActive)}
                            >
                              {supplier.isActive ? (
                                <span className="flex items-center">
                                  <CheckCircle className="h-3 w-3 mr-1" />
                                  Active
                                </span>
                              ) : (
                                <span className="flex items-center">
                                  <XCircle className="h-3 w-3 mr-1" />
                                  Inactive
                                </span>
                              )}
                            </Badge>
                          </TableCell>
                          <TableCell>
                            <div className="text-sm">
                              {supplier.lastTransactionDate 
                                ? formatDate(supplier.lastTransactionDate) 
                                : 'Never'}
                            </div>
                          </TableCell>
                          <TableCell>
                            <div className="flex space-x-2">
                              <Button
                                size="sm"
                                variant="ghost"
                                onClick={() => handleViewSupplierDetails(supplier)}
                              >
                                <Eye className="h-4 w-4" />
                              </Button>
                              {isManager && (
                                <Button
                                  size="sm"
                                  variant="ghost"
                                  onClick={() => handleDeleteSupplier(supplier.id, supplier.companyName)}
                                  disabled={deleteLoading}
                                >
                                  <XCircle className="h-4 w-4" />
                                </Button>
                              )}
                            </div>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>

                  {/* Pagination */}
                  <div className="flex items-center justify-between px-6 py-4 border-t">
                    <div className="text-sm text-muted-foreground">
                      Showing {suppliersData.items.length} of {suppliersData.totalCount} suppliers
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
                        Page {suppliersData.pageNumber} of {Math.ceil(suppliersData.totalCount / suppliersData.pageSize)}
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

        <TabsContent value="transactions" className="space-y-4">
          <Card className="pos-card">
            <CardContent className="pt-6">
              <div className="text-center py-8 text-muted-foreground">
                Select a supplier to view transaction history
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Create Supplier Dialog */}
      <Dialog open={isNewSupplierOpen} onOpenChange={setIsNewSupplierOpen}>
        <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Add New Supplier</DialogTitle>
            <DialogDescription>
              Create a new supplier record with company information and payment terms.
            </DialogDescription>
          </DialogHeader>
          
          <div className="grid grid-cols-2 gap-6">
            <div className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="companyName">Company Name *</Label>
                <Input
                  id="companyName"
                  value={supplierForm.companyName}
                  onChange={(e) => setSupplierForm({...supplierForm, companyName: e.target.value})}
                  placeholder="Enter company name"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="contactPersonName">Contact Person</Label>
                <Input
                  id="contactPersonName"
                  value={supplierForm.contactPersonName}
                  onChange={(e) => setSupplierForm({...supplierForm, contactPersonName: e.target.value})}
                  placeholder="Enter contact person name"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="phone">Phone</Label>
                <Input
                  id="phone"
                  value={supplierForm.phone}
                  onChange={(e) => setSupplierForm({...supplierForm, phone: e.target.value})}
                  placeholder="Enter phone number"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="email">Email</Label>
                <Input
                  id="email"
                  type="email"
                  value={supplierForm.email}
                  onChange={(e) => setSupplierForm({...supplierForm, email: e.target.value})}
                  placeholder="Enter email address"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="address">Address</Label>
                <Textarea
                  id="address"
                  value={supplierForm.address}
                  onChange={(e) => setSupplierForm({...supplierForm, address: e.target.value})}
                  placeholder="Enter address"
                  rows={3}
                />
              </div>
            </div>

            <div className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="taxRegistrationNumber">Tax Registration Number</Label>
                <Input
                  id="taxRegistrationNumber"
                  value={supplierForm.taxRegistrationNumber}
                  onChange={(e) => setSupplierForm({...supplierForm, taxRegistrationNumber: e.target.value})}
                  placeholder="Enter tax registration number"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="commercialRegistrationNumber">Commercial Registration</Label>
                <Input
                  id="commercialRegistrationNumber"
                  value={supplierForm.commercialRegistrationNumber}
                  onChange={(e) => setSupplierForm({...supplierForm, commercialRegistrationNumber: e.target.value})}
                  placeholder="Enter commercial registration"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="creditLimit">Credit Limit</Label>
                <Input
                  id="creditLimit"
                  type="number"
                  value={supplierForm.creditLimit}
                  onChange={(e) => setSupplierForm({...supplierForm, creditLimit: parseFloat(e.target.value) || 0})}
                  placeholder="Enter credit limit"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="paymentTermsDays">Payment Terms (Days)</Label>
                <Input
                  id="paymentTermsDays"
                  type="number"
                  value={supplierForm.paymentTermsDays}
                  onChange={(e) => setSupplierForm({...supplierForm, paymentTermsDays: parseInt(e.target.value) || 30})}
                  placeholder="Enter payment terms in days"
                />
              </div>
              <div className="flex items-center space-x-2">
                <Switch
                  checked={supplierForm.creditLimitEnforced}
                  onCheckedChange={(checked: boolean) => setSupplierForm({...supplierForm, creditLimitEnforced: checked})}
                />
                <Label>Enforce Credit Limit</Label>
              </div>
              <div className="space-y-2">
                <Label htmlFor="paymentTerms">Payment Terms Description</Label>
                <Input
                  id="paymentTerms"
                  value={supplierForm.paymentTerms}
                  onChange={(e) => setSupplierForm({...supplierForm, paymentTerms: e.target.value})}
                  placeholder="e.g., Net 30, COD, etc."
                />
              </div>
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="notes">Notes</Label>
            <Textarea
              id="notes"
              value={supplierForm.notes}
              onChange={(e) => setSupplierForm({...supplierForm, notes: e.target.value})}
              placeholder="Additional notes about the supplier"
              rows={3}
            />
          </div>
          
          <div className="flex justify-end gap-3 mt-6">
            <Button variant="outline" onClick={() => {
              setIsNewSupplierOpen(false);
              resetForm();
            }}>
              Cancel
            </Button>
            <Button 
              onClick={handleCreateSupplier} 
              disabled={createLoading}
              className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]"
            >
              {createLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Create Supplier
            </Button>
          </div>
        </DialogContent>
      </Dialog>

      {/* Supplier Details Dialog - Basic structure, will add the rest in next part */}
      {selectedSupplier && (
        <Dialog open={!!selectedSupplier} onOpenChange={() => {
          setSelectedSupplier(null);
          setIsEditMode(false);
          setSupplierProducts(null);
          setSupplierBalance(null);
          setSupplierTransactions(null);
        }}>
          <DialogContent className="max-w-6xl max-h-[90vh] overflow-y-auto">
            <DialogHeader>
              <DialogTitle className="flex items-center gap-2">
                <Building className="h-5 w-5" />
                {selectedSupplier.companyName}
                <Badge variant="secondary" className={getStatusBadgeColor(selectedSupplier.isActive)}>
                  {selectedSupplier.isActive ? 'Active' : 'Inactive'}
                </Badge>
              </DialogTitle>
              <DialogDescription>
                Supplier information, products, balance, and transaction history
              </DialogDescription>
            </DialogHeader>
            
            <div className="flex justify-end gap-3 mt-6">
              <Button variant="outline" onClick={() => {
                setSelectedSupplier(null);
                setIsEditMode(false);
              }}>
                Close
              </Button>
              {isManager && (
                <Button 
                  onClick={() => setIsEditMode(true)} 
                  className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]"
                >
                  <Edit className="mr-2 h-4 w-4" />
                  Edit Supplier
                </Button>
              )}
            </div>
          </DialogContent>
        </Dialog>
      )}
    </div>
  );
}
