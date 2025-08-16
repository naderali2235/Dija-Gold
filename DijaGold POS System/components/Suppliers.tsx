import React, { useState } from 'react';
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
  ShoppingCart,
  FileText,
} from 'lucide-react';
import { useAuth } from './AuthContext';
import { formatCurrency } from './utils/currency';

interface Supplier {
  id: string;
  supplierNumber: string;
  name: string;
  contactPerson: string;
  phone: string;
  email: string;
  address: string;
  city: string;
  taxNumber: string;
  paymentTerms: string;
  creditLimit: number;
  currentBalance: number;
  totalPurchases: number;
  lastOrderDate: string;
  supplierSince: string;
  status: 'active' | 'inactive' | 'blocked';
  notes: string;
  categories: string[];
}

interface PurchaseOrder {
  id: string;
  poNumber: string;
  supplierId: string;
  supplierName: string;
  orderDate: string;
  expectedDelivery: string;
  totalAmount: number;
  status: 'draft' | 'sent' | 'confirmed' | 'received' | 'cancelled';
  items: number;
  notes: string;
}

interface SupplierTransaction {
  id: string;
  date: string;
  type: 'purchase' | 'payment' | 'credit_note' | 'debit_note';
  reference: string;
  amount: number;
  balance: number;
  description: string;
}

export default function Suppliers() {
  const { isManager } = useAuth();
  const [activeTab, setActiveTab] = useState('suppliers');
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [isNewSupplierOpen, setIsNewSupplierOpen] = useState(false);
  const [isNewPOOpen, setIsNewPOOpen] = useState(false);
  const [selectedSupplier, setSelectedSupplier] = useState<Supplier | null>(null);
  const [isEditMode, setIsEditMode] = useState(false);

  // Form states
  const [supplierForm, setSupplierForm] = useState({
    name: '',
    contactPerson: '',
    phone: '',
    email: '',
    address: '',
    city: '',
    taxNumber: '',
    paymentTerms: '',
    creditLimit: '',
    notes: '',
  });

  const [poForm, setPOForm] = useState({
    supplierId: '',
    expectedDelivery: '',
    notes: '',
    items: [{ description: '', quantity: '', unitPrice: '', total: '' }],
  });

  // Mock data
  const suppliers: Supplier[] = [
    {
      id: '1',
      supplierNumber: 'SUP-001',
      name: 'Cairo Gold Trading',
      contactPerson: 'Ahmed Mahmoud',
      phone: '+20 100 111 2222',
      email: 'ahmed@cairogold.com',
      address: '123 Khan El-Khalili',
      city: 'Cairo',
      taxNumber: '12345678901',
      paymentTerms: 'Net 30',
      creditLimit: 500000,
      currentBalance: 125000,
      totalPurchases: 2500000,
      lastOrderDate: '2024-01-15T10:00:00Z',
      supplierSince: '2020-01-15T09:00:00Z',
      status: 'active',
      notes: 'Primary gold supplier, excellent quality',
      categories: ['Gold Jewelry', 'Gold Bars'],
    },
    {
      id: '2',
      supplierNumber: 'SUP-002',
      name: 'Alexandria Precious Metals',
      contactPerson: 'Fatima Hassan',
      phone: '+20 101 333 4444',
      email: 'fatima@alexmetals.com',
      address: '456 Corniche Road',
      city: 'Alexandria',
      taxNumber: '23456789012',
      paymentTerms: 'Net 15',
      creditLimit: 300000,
      currentBalance: 75000,
      totalPurchases: 1200000,
      lastOrderDate: '2024-01-12T14:30:00Z',
      supplierSince: '2021-06-10T11:20:00Z',
      status: 'active',
      notes: 'Competitive pricing, good delivery times',
      categories: ['Silver Jewelry', 'Gemstones'],
    },
    {
      id: '3',
      supplierNumber: 'SUP-003',
      name: 'Luxor Heritage Crafts',
      contactPerson: 'Mohamed Ali',
      phone: '+20 102 555 6666',
      email: 'mohamed@luxorheritage.com',
      address: '789 Temple Street',
      city: 'Luxor',
      taxNumber: '34567890123',
      paymentTerms: 'Net 45',
      creditLimit: 200000,
      currentBalance: 0,
      totalPurchases: 450000,
      lastOrderDate: '2023-12-20T16:45:00Z',
      supplierSince: '2022-03-25T13:15:00Z',
      status: 'inactive',
      notes: 'Traditional designs, slow delivery',
      categories: ['Traditional Jewelry'],
    },
  ];

  const purchaseOrders: PurchaseOrder[] = [
    {
      id: '1',
      poNumber: 'PO-2024-001',
      supplierId: '1',
      supplierName: 'Cairo Gold Trading',
      orderDate: '2024-01-15T09:00:00Z',
      expectedDelivery: '2024-01-20T09:00:00Z',
      totalAmount: 150000,
      status: 'confirmed',
      items: 5,
      notes: 'Urgent order for wedding season',
    },
    {
      id: '2',
      poNumber: 'PO-2024-002',
      supplierId: '2',
      supplierName: 'Alexandria Precious Metals',
      orderDate: '2024-01-12T11:30:00Z',
      expectedDelivery: '2024-01-18T11:30:00Z',
      totalAmount: 85000,
      status: 'sent',
      items: 3,
      notes: 'Regular monthly order',
    },
    {
      id: '3',
      poNumber: 'PO-2024-003',
      supplierId: '1',
      supplierName: 'Cairo Gold Trading',
      orderDate: '2024-01-10T14:15:00Z',
      expectedDelivery: '2024-01-16T14:15:00Z',
      totalAmount: 95000,
      status: 'received',
      items: 4,
      notes: 'All items received in good condition',
    },
  ];

  const supplierTransactions: SupplierTransaction[] = [
    {
      id: '1',
      date: '2024-01-15T10:00:00Z',
      type: 'purchase',
      reference: 'PO-2024-001',
      amount: 150000,
      balance: 275000,
      description: 'Gold jewelry purchase',
    },
    {
      id: '2',
      date: '2024-01-14T15:30:00Z',
      type: 'payment',
      reference: 'PAY-2024-005',
      amount: -125000,
      balance: 125000,
      description: 'Payment for previous orders',
    },
    {
      id: '3',
      date: '2024-01-12T11:30:00Z',
      type: 'purchase',
      reference: 'PO-2024-002',
      amount: 85000,
      balance: 250000,
      description: 'Silver jewelry and gemstones',
    },
  ];

  const filteredSuppliers = suppliers.filter(supplier => {
    const matchesSearch = 
      supplier.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      supplier.contactPerson.toLowerCase().includes(searchQuery.toLowerCase()) ||
      supplier.supplierNumber.toLowerCase().includes(searchQuery.toLowerCase());
    
    const matchesStatus = statusFilter === 'all' || supplier.status === statusFilter;
    
    return matchesSearch && matchesStatus;
  });

  const getStatusBadge = (status: string) => {
    const variants = {
      active: { variant: 'default' as const, className: 'bg-green-100 text-green-800' },
      inactive: { variant: 'secondary' as const, className: 'bg-gray-100 text-gray-800' },
      blocked: { variant: 'destructive' as const, className: 'bg-red-100 text-red-800' },
    };

    const config = variants[status as keyof typeof variants];

    return (
      <Badge variant={config.variant} className={config.className}>
        {status.toUpperCase()}
      </Badge>
    );
  };

  const getPOStatusBadge = (status: string) => {
    const variants = {
      draft: { variant: 'outline' as const, className: 'bg-gray-100 text-gray-800' },
      sent: { variant: 'default' as const, className: 'bg-blue-100 text-blue-800' },
      confirmed: { variant: 'default' as const, className: 'bg-yellow-100 text-yellow-800' },
      received: { variant: 'default' as const, className: 'bg-green-100 text-green-800' },
      cancelled: { variant: 'destructive' as const, className: 'bg-red-100 text-red-800' },
    };

    const config = variants[status as keyof typeof variants];

    return (
      <Badge variant={config.variant} className={config.className}>
        {status.toUpperCase()}
      </Badge>
    );
  };

  const handleCreateSupplier = () => {
    if (!isManager) {
      alert('Only managers can create suppliers');
      return;
    }
    console.log('Creating supplier:', supplierForm);
    setIsNewSupplierOpen(false);
    resetSupplierForm();
  };

  const handleCreatePO = () => {
    if (!isManager) {
      alert('Only managers can create purchase orders');
      return;
    }
    console.log('Creating PO:', poForm);
    setIsNewPOOpen(false);
    resetPOForm();
  };

  const resetSupplierForm = () => {
    setSupplierForm({
      name: '',
      contactPerson: '',
      phone: '',
      email: '',
      address: '',
      city: '',
      taxNumber: '',
      paymentTerms: '',
      creditLimit: '',
      notes: '',
    });
  };

  const resetPOForm = () => {
    setPOForm({
      supplierId: '',
      expectedDelivery: '',
      notes: '',
      items: [{ description: '', quantity: '', unitPrice: '', total: '' }],
    });
  };

  const populateSupplierForm = (supplier: Supplier) => {
    setSupplierForm({
      name: supplier.name,
      contactPerson: supplier.contactPerson,
      phone: supplier.phone,
      email: supplier.email,
      address: supplier.address,
      city: supplier.city,
      taxNumber: supplier.taxNumber,
      paymentTerms: supplier.paymentTerms,
      creditLimit: supplier.creditLimit.toString(),
      notes: supplier.notes,
    });
  };

  const supplierStats = {
    total: suppliers.length,
    active: suppliers.filter(s => s.status === 'active').length,
    totalBalance: suppliers.reduce((sum, s) => sum + s.currentBalance, 0),
    totalPurchases: suppliers.reduce((sum, s) => sum + s.totalPurchases, 0),
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl text-[#0D1B2A]">Supplier Management</h1>
          <p className="text-muted-foreground">Manage suppliers and purchase orders</p>
        </div>
        {isManager && (
          <div className="flex gap-3">
            <Dialog open={isNewPOOpen} onOpenChange={setIsNewPOOpen}>
              <DialogTrigger asChild>
                <Button variant="outline" className="touch-target">
                  <FileText className="mr-2 h-4 w-4" />
                  New Purchase Order
                </Button>
              </DialogTrigger>
              <DialogContent className="max-w-3xl">
                <DialogHeader>
                  <DialogTitle>Create Purchase Order</DialogTitle>
                  <DialogDescription>
                    Create a new purchase order for supplier
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
                          {suppliers.filter(s => s.status === 'active').map(supplier => (
                            <SelectItem key={supplier.id} value={supplier.id}>
                              {supplier.name}
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
                  <Button onClick={handleCreatePO} className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
                    Create PO
                  </Button>
                </div>
              </DialogContent>
            </Dialog>
            
            <Dialog open={isNewSupplierOpen} onOpenChange={setIsNewSupplierOpen}>
              <DialogTrigger asChild>
                <Button className="touch-target pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
                  <Plus className="mr-2 h-4 w-4" />
                  Add Supplier
                </Button>
              </DialogTrigger>
              <DialogContent className="max-w-2xl">
                <DialogHeader>
                  <DialogTitle>Add New Supplier</DialogTitle>
                  <DialogDescription>
                    Enter supplier details to add to database
                  </DialogDescription>
                </DialogHeader>
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="name">Company Name *</Label>
                    <Input
                      id="name"
                      value={supplierForm.name}
                      onChange={(e) => setSupplierForm({...supplierForm, name: e.target.value})}
                      placeholder="Enter company name"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="contactPerson">Contact Person *</Label>
                    <Input
                      id="contactPerson"
                      value={supplierForm.contactPerson}
                      onChange={(e) => setSupplierForm({...supplierForm, contactPerson: e.target.value})}
                      placeholder="Primary contact"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="phone">Phone Number *</Label>
                    <Input
                      id="phone"
                      value={supplierForm.phone}
                      onChange={(e) => setSupplierForm({...supplierForm, phone: e.target.value})}
                      placeholder="+20 100 123 4567"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="email">Email Address</Label>
                    <Input
                      id="email"
                      type="email"
                      value={supplierForm.email}
                      onChange={(e) => setSupplierForm({...supplierForm, email: e.target.value})}
                      placeholder="supplier@email.com"
                    />
                  </div>
                  <div className="col-span-2 space-y-2">
                    <Label htmlFor="address">Address</Label>
                    <Textarea
                      id="address"
                      value={supplierForm.address}
                      onChange={(e) => setSupplierForm({...supplierForm, address: e.target.value})}
                      placeholder="Business address"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="city">City</Label>
                    <Input
                      id="city"
                      value={supplierForm.city}
                      onChange={(e) => setSupplierForm({...supplierForm, city: e.target.value})}
                      placeholder="Cairo, Alexandria, etc."
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="taxNumber">Tax Number</Label>
                    <Input
                      id="taxNumber"
                      value={supplierForm.taxNumber}
                      onChange={(e) => setSupplierForm({...supplierForm, taxNumber: e.target.value})}
                      placeholder="Tax registration number"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="paymentTerms">Payment Terms</Label>
                    <Select value={supplierForm.paymentTerms} onValueChange={(value) => setSupplierForm({...supplierForm, paymentTerms: value})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select terms" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Cash">Cash</SelectItem>
                        <SelectItem value="Net 15">Net 15 Days</SelectItem>
                        <SelectItem value="Net 30">Net 30 Days</SelectItem>
                        <SelectItem value="Net 45">Net 45 Days</SelectItem>
                        <SelectItem value="Net 60">Net 60 Days</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="creditLimit">Credit Limit (EGP)</Label>
                    <Input
                      id="creditLimit"
                      type="number"
                      value={supplierForm.creditLimit}
                      onChange={(e) => setSupplierForm({...supplierForm, creditLimit: e.target.value})}
                      placeholder="0"
                    />
                  </div>
                  <div className="col-span-2 space-y-2">
                    <Label htmlFor="notes">Notes</Label>
                    <Textarea
                      id="notes"
                      value={supplierForm.notes}
                      onChange={(e) => setSupplierForm({...supplierForm, notes: e.target.value})}
                      placeholder="Additional notes about supplier"
                    />
                  </div>
                </div>
                <div className="flex justify-end gap-3 mt-6">
                  <Button variant="outline" onClick={() => setIsNewSupplierOpen(false)}>
                    Cancel
                  </Button>
                  <Button onClick={handleCreateSupplier} className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
                    Add Supplier
                  </Button>
                </div>
              </DialogContent>
            </Dialog>
          </div>
        )}
      </div>

      {/* Supplier Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total Suppliers</p>
                <p className="text-2xl text-[#0D1B2A]">{supplierStats.total}</p>
              </div>
              <Truck className="h-8 w-8 text-[#D4AF37]" />
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Active Suppliers</p>
                <p className="text-2xl text-[#0D1B2A]">{supplierStats.active}</p>
              </div>
              <Truck className="h-8 w-8 text-green-600" />
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Outstanding Balance</p>
                <p className="text-2xl text-[#0D1B2A]">{formatCurrency(supplierStats.totalBalance)}</p>
              </div>
              <DollarSign className="h-8 w-8 text-red-600" />
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total Purchases</p>
                <p className="text-2xl text-[#0D1B2A]">{formatCurrency(supplierStats.totalPurchases)}</p>
              </div>
              <ShoppingCart className="h-8 w-8 text-blue-600" />
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="suppliers">Suppliers</TabsTrigger>
          <TabsTrigger value="purchase-orders">Purchase Orders</TabsTrigger>
          <TabsTrigger value="transactions">Transactions</TabsTrigger>
        </TabsList>
        
        <TabsContent value="suppliers" className="space-y-4">
          {/* Filters */}
          <Card className="pos-card">
            <CardContent className="pt-6">
              <div className="flex flex-col md:flex-row gap-4">
                <div className="relative flex-1">
                  <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                  <Input
                    placeholder="Search by name, contact person, or supplier number..."
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
                    <SelectItem value="active">Active</SelectItem>
                    <SelectItem value="inactive">Inactive</SelectItem>
                    <SelectItem value="blocked">Blocked</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </CardContent>
          </Card>

          {/* Suppliers Table */}
          <Card className="pos-card">
            <CardHeader>
              <CardTitle>Suppliers</CardTitle>
              <CardDescription>
                {filteredSuppliers.length} supplier(s) found
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Supplier #</TableHead>
                    <TableHead>Company</TableHead>
                    <TableHead>Contact</TableHead>
                    <TableHead>Payment Terms</TableHead>
                    <TableHead>Current Balance</TableHead>
                    <TableHead>Total Purchases</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Last Order</TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredSuppliers.map((supplier) => (
                    <TableRow key={supplier.id}>
                      <TableCell className="font-medium">{supplier.supplierNumber}</TableCell>
                      <TableCell>
                        <div>
                          <p className="font-medium">{supplier.name}</p>
                          <p className="text-sm text-muted-foreground">{supplier.city}</p>
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="space-y-1">
                          <p className="text-sm font-medium">{supplier.contactPerson}</p>
                          <div className="flex items-center gap-1 text-sm">
                            <Phone className="h-3 w-3" />
                            {supplier.phone}
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>{supplier.paymentTerms}</TableCell>
                      <TableCell className={supplier.currentBalance > 0 ? 'text-red-600' : 'text-green-600'}>
                        {formatCurrency(supplier.currentBalance)}
                      </TableCell>
                      <TableCell>{formatCurrency(supplier.totalPurchases)}</TableCell>
                      <TableCell>{getStatusBadge(supplier.status)}</TableCell>
                      <TableCell>{new Date(supplier.lastOrderDate).toLocaleDateString()}</TableCell>
                      <TableCell>
                        <div className="flex gap-2">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => {
                              setSelectedSupplier(supplier);
                              populateSupplierForm(supplier);
                            }}
                            className="touch-target"
                          >
                            <Eye className="h-4 w-4" />
                          </Button>
                          {isManager && (
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => {
                                setSelectedSupplier(supplier);
                                populateSupplierForm(supplier);
                                setIsEditMode(true);
                              }}
                              className="touch-target"
                            >
                              <Edit className="h-4 w-4" />
                            </Button>
                          )}
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>
        
        <TabsContent value="purchase-orders" className="space-y-4">
          <Card className="pos-card">
            <CardHeader>
              <CardTitle>Purchase Orders</CardTitle>
              <CardDescription>
                {purchaseOrders.length} purchase order(s) found
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>PO Number</TableHead>
                    <TableHead>Supplier</TableHead>
                    <TableHead>Order Date</TableHead>
                    <TableHead>Expected Delivery</TableHead>
                    <TableHead>Items</TableHead>
                    <TableHead>Total Amount</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {purchaseOrders.map((po) => (
                    <TableRow key={po.id}>
                      <TableCell className="font-medium">{po.poNumber}</TableCell>
                      <TableCell>{po.supplierName}</TableCell>
                      <TableCell>{new Date(po.orderDate).toLocaleDateString()}</TableCell>
                      <TableCell>{new Date(po.expectedDelivery).toLocaleDateString()}</TableCell>
                      <TableCell>{po.items}</TableCell>
                      <TableCell>{formatCurrency(po.totalAmount)}</TableCell>
                      <TableCell>{getPOStatusBadge(po.status)}</TableCell>
                      <TableCell>
                        <Button variant="outline" size="sm" className="touch-target">
                          <Eye className="h-4 w-4" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>
        
        <TabsContent value="transactions" className="space-y-4">
          <Card className="pos-card">
            <CardHeader>
              <CardTitle>Supplier Transactions</CardTitle>
              <CardDescription>
                Payment history and account movements
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Date</TableHead>
                    <TableHead>Type</TableHead>
                    <TableHead>Reference</TableHead>
                    <TableHead>Amount</TableHead>
                    <TableHead>Running Balance</TableHead>
                    <TableHead>Description</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {supplierTransactions.map((transaction) => (
                    <TableRow key={transaction.id}>
                      <TableCell>{new Date(transaction.date).toLocaleDateString()}</TableCell>
                      <TableCell className="capitalize">{transaction.type.replace('_', ' ')}</TableCell>
                      <TableCell className="font-medium">{transaction.reference}</TableCell>
                      <TableCell className={transaction.amount < 0 ? 'text-green-600' : 'text-red-600'}>
                        {transaction.amount < 0 ? '-' : '+'}{formatCurrency(Math.abs(transaction.amount))}
                      </TableCell>
                      <TableCell>{formatCurrency(transaction.balance)}</TableCell>
                      <TableCell>{transaction.description}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Supplier Details Dialog */}
      {selectedSupplier && (
        <Dialog open={!!selectedSupplier} onOpenChange={() => {
          setSelectedSupplier(null);
          setIsEditMode(false);
        }}>
          <DialogContent className="max-w-3xl">
            <DialogHeader>
              <DialogTitle>
                {isEditMode ? 'Edit Supplier' : 'Supplier Details'} - {selectedSupplier.supplierNumber}
              </DialogTitle>
            </DialogHeader>
            
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Company Name</Label>
                <Input
                  value={supplierForm.name}
                  onChange={(e) => setSupplierForm({...supplierForm, name: e.target.value})}
                  readOnly={!isEditMode}
                />
              </div>
              <div className="space-y-2">
                <Label>Contact Person</Label>
                <Input
                  value={supplierForm.contactPerson}
                  onChange={(e) => setSupplierForm({...supplierForm, contactPerson: e.target.value})}
                  readOnly={!isEditMode}
                />
              </div>
              <div className="space-y-2">
                <Label>Current Balance</Label>
                <div className="p-2 bg-muted rounded">
                  {formatCurrency(selectedSupplier.currentBalance)}
                </div>
              </div>
              <div className="space-y-2">
                <Label>Total Purchases</Label>
                <div className="p-2 bg-muted rounded">
                  {formatCurrency(selectedSupplier.totalPurchases)}
                </div>
              </div>
            </div>
            
            <div className="flex justify-end gap-3 mt-6">
              <Button variant="outline" onClick={() => {
                setSelectedSupplier(null);
                setIsEditMode(false);
              }}>
                {isEditMode ? 'Cancel' : 'Close'}
              </Button>
              {isEditMode && isManager && (
                <Button onClick={() => {
                  console.log('Updating supplier:', selectedSupplier.id, supplierForm);
                  setSelectedSupplier(null);
                  setIsEditMode(false);
                }} className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
                  Update Supplier
                </Button>
              )}
              {!isEditMode && isManager && (
                <Button onClick={() => setIsEditMode(true)} className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
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