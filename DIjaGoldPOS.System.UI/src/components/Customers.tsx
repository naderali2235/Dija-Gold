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
} from 'lucide-react';
import { formatCurrency } from './utils/currency';

interface Customer {
  id: string;
  customerNumber: string;
  name: string;
  phone: string;
  email: string;
  address: string;
  city: string;
  dateOfBirth?: string;
  anniversaryDate?: string;
  loyaltyPoints: number;
  totalPurchases: number;
  lastPurchaseDate: string;
  customerSince: string;
  status: 'active' | 'inactive' | 'vip';
  notes: string;
  preferredCategories: string[];
}

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

  // Form state for new/edit customer
  const [customerForm, setCustomerForm] = useState({
    name: '',
    phone: '',
    email: '',
    address: '',
    city: '',
    dateOfBirth: '',
    anniversaryDate: '',
    notes: '',
  });

  // Mock data
  const customers: Customer[] = [
    {
      id: '1',
      customerNumber: 'CUST-001',
      name: 'Ahmed Hassan',
      phone: '+20 100 123 4567',
      email: 'ahmed.hassan@email.com',
      address: '123 Tahrir Square',
      city: 'Cairo',
      dateOfBirth: '1985-06-15',
      anniversaryDate: '2020-03-20',
      loyaltyPoints: 2450,
      totalPurchases: 125000,
      lastPurchaseDate: '2024-01-15T10:30:00Z',
      customerSince: '2020-03-20T09:00:00Z',
      status: 'vip',
      notes: 'Prefers traditional designs, frequent buyer',
      preferredCategories: ['Rings', 'Necklaces'],
    },
    {
      id: '2',
      customerNumber: 'CUST-002',
      name: 'Fatima El-Sayed',
      phone: '+20 101 987 6543',
      email: 'fatima.elsayed@email.com',
      address: '456 Nile Corniche',
      city: 'Alexandria',
      dateOfBirth: '1990-12-10',
      loyaltyPoints: 890,
      totalPurchases: 45000,
      lastPurchaseDate: '2024-01-12T14:20:00Z',
      customerSince: '2022-08-15T11:30:00Z',
      status: 'active',
      notes: 'Interested in modern designs',
      preferredCategories: ['Earrings', 'Bracelets'],
    },
    {
      id: '3',
      customerNumber: 'CUST-003',
      name: 'Mohamed Ali',
      phone: '+20 102 555 7890',
      email: 'mohamed.ali@email.com',
      address: '789 Garden City',
      city: 'Cairo',
      loyaltyPoints: 150,
      totalPurchases: 8500,
      lastPurchaseDate: '2023-11-20T16:45:00Z',
      customerSince: '2023-06-10T13:15:00Z',
      status: 'inactive',
      notes: 'Occasional buyer, price sensitive',
      preferredCategories: ['Chains'],
    },
  ];

  const customerTransactions: CustomerTransaction[] = [
    {
      id: '1',
      transactionNumber: 'INV-2024-001',
      date: '2024-01-15T10:30:00Z',
      type: 'sale',
      amount: 25000,
      items: 2,
      status: 'completed',
    },
    {
      id: '2',
      transactionNumber: 'INV-2024-002',
      date: '2024-01-12T14:20:00Z',
      type: 'sale',
      amount: 15000,
      items: 1,
      status: 'completed',
    },
    {
      id: '3',
      transactionNumber: 'RET-2024-001',
      date: '2024-01-10T11:15:00Z',
      type: 'return',
      amount: -5000,
      items: 1,
      status: 'completed',
    },
  ];

  const filteredCustomers = customers.filter(customer => {
    const matchesSearch = 
      customer.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      customer.phone.includes(searchQuery) ||
      customer.email.toLowerCase().includes(searchQuery.toLowerCase()) ||
      customer.customerNumber.toLowerCase().includes(searchQuery.toLowerCase());
    
    const matchesStatus = statusFilter === 'all' || customer.status === statusFilter;
    
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

  const handleCreateCustomer = () => {
    console.log('Creating customer:', customerForm);
    setIsNewCustomerOpen(false);
    resetForm();
  };

  const handleUpdateCustomer = () => {
    console.log('Updating customer:', selectedCustomer?.id, customerForm);
    setSelectedCustomer(null);
    setIsEditMode(false);
    resetForm();
  };

  const resetForm = () => {
    setCustomerForm({
      name: '',
      phone: '',
      email: '',
      address: '',
      city: '',
      dateOfBirth: '',
      anniversaryDate: '',
      notes: '',
    });
  };

  const populateForm = (customer: Customer) => {
    setCustomerForm({
      name: customer.name,
      phone: customer.phone,
      email: customer.email,
      address: customer.address,
      city: customer.city,
      dateOfBirth: customer.dateOfBirth || '',
      anniversaryDate: customer.anniversaryDate || '',
      notes: customer.notes,
    });
  };

  const customerStats = {
    total: customers.length,
    active: customers.filter(c => c.status === 'active').length,
    vip: customers.filter(c => c.status === 'vip').length,
    totalValue: customers.reduce((sum, c) => sum + c.totalPurchases, 0),
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
            <Button className="touch-target pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
              <Plus className="mr-2 h-4 w-4" />
              Add Customer
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-2xl">
            <DialogHeader>
              <DialogTitle>Add New Customer</DialogTitle>
              <DialogDescription>
                Enter customer details to add to database
              </DialogDescription>
            </DialogHeader>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="name">Full Name *</Label>
                <Input
                  id="name"
                  value={customerForm.name}
                  onChange={(e) => setCustomerForm({...customerForm, name: e.target.value})}
                  placeholder="Enter full name"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="phone">Phone Number *</Label>
                <Input
                  id="phone"
                  value={customerForm.phone}
                  onChange={(e) => setCustomerForm({...customerForm, phone: e.target.value})}
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
                <Label htmlFor="city">City</Label>
                <Input
                  id="city"
                  value={customerForm.city}
                  onChange={(e) => setCustomerForm({...customerForm, city: e.target.value})}
                  placeholder="Cairo, Alexandria, etc."
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="dateOfBirth">Date of Birth</Label>
                <Input
                  id="dateOfBirth"
                  type="date"
                  value={customerForm.dateOfBirth}
                  onChange={(e) => setCustomerForm({...customerForm, dateOfBirth: e.target.value})}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="anniversaryDate">Anniversary Date</Label>
                <Input
                  id="anniversaryDate"
                  type="date"
                  value={customerForm.anniversaryDate}
                  onChange={(e) => setCustomerForm({...customerForm, anniversaryDate: e.target.value})}
                />
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
              <Button onClick={handleCreateCustomer} className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
                Add Customer
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
                <SelectItem value="vip">VIP</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Customers Table */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle>Customers</CardTitle>
          <CardDescription>
            {filteredCustomers.length} customer(s) found
          </CardDescription>
        </CardHeader>
        <CardContent>
          {filteredCustomers.length === 0 ? (
            <div className="text-center py-8">
              <Users className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <p className="text-muted-foreground">No customers found</p>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Customer #</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead>Contact</TableHead>
                  <TableHead>City</TableHead>
                  <TableHead>Total Purchases</TableHead>
                  <TableHead>Loyalty Points</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Last Purchase</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredCustomers.map((customer) => (
                  <TableRow key={customer.id}>
                    <TableCell className="font-medium">{customer.customerNumber}</TableCell>
                    <TableCell>
                      <div>
                        <p className="font-medium">{customer.name}</p>
                        <p className="text-sm text-muted-foreground">Since {new Date(customer.customerSince).getFullYear()}</p>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="space-y-1">
                        <div className="flex items-center gap-1 text-sm">
                          <Phone className="h-3 w-3" />
                          {customer.phone}
                        </div>
                        {customer.email && (
                          <div className="flex items-center gap-1 text-sm">
                            <Mail className="h-3 w-3" />
                            {customer.email}
                          </div>
                        )}
                      </div>
                    </TableCell>
                    <TableCell>{customer.city}</TableCell>
                    <TableCell>{formatCurrency(customer.totalPurchases)}</TableCell>
                    <TableCell>
                      <div className="flex items-center gap-1">
                        <Gift className="h-4 w-4 text-[#D4AF37]" />
                        {customer.loyaltyPoints} pts
                      </div>
                    </TableCell>
                    <TableCell>{getStatusBadge(customer.status)}</TableCell>
                    <TableCell>{new Date(customer.lastPurchaseDate).toLocaleDateString()}</TableCell>
                    <TableCell>
                      <div className="flex gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => {
                            setSelectedCustomer(customer);
                            populateForm(customer);
                          }}
                          className="touch-target"
                        >
                          <Eye className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => {
                            setSelectedCustomer(customer);
                            populateForm(customer);
                            setIsEditMode(true);
                          }}
                          className="touch-target"
                        >
                          <Edit className="h-4 w-4" />
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
          <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
            <DialogHeader>
              <DialogTitle>
                {isEditMode ? 'Edit Customer' : 'Customer Details'} - {selectedCustomer.customerNumber}
              </DialogTitle>
            </DialogHeader>
            
            <Tabs defaultValue="details" className="w-full">
              <TabsList>
                <TabsTrigger value="details">Details</TabsTrigger>
                <TabsTrigger value="transactions">Transactions</TabsTrigger>
                <TabsTrigger value="loyalty">Loyalty</TabsTrigger>
              </TabsList>
              
              <TabsContent value="details" className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="name">Full Name</Label>
                    <Input
                      id="name"
                      value={customerForm.name}
                      onChange={(e) => setCustomerForm({...customerForm, name: e.target.value})}
                      readOnly={!isEditMode}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="phone">Phone Number</Label>
                    <Input
                      id="phone"
                      value={customerForm.phone}
                      onChange={(e) => setCustomerForm({...customerForm, phone: e.target.value})}
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
                    <Label htmlFor="city">City</Label>
                    <Input
                      id="city"
                      value={customerForm.city}
                      onChange={(e) => setCustomerForm({...customerForm, city: e.target.value})}
                      readOnly={!isEditMode}
                    />
                  </div>
                </div>
              </TabsContent>
              
              <TabsContent value="transactions" className="space-y-4">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Date</TableHead>
                      <TableHead>Transaction #</TableHead>
                      <TableHead>Type</TableHead>
                      <TableHead>Items</TableHead>
                      <TableHead>Amount</TableHead>
                      <TableHead>Status</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {customerTransactions.map((transaction) => (
                      <TableRow key={transaction.id}>
                        <TableCell>{new Date(transaction.date).toLocaleDateString()}</TableCell>
                        <TableCell className="font-medium">{transaction.transactionNumber}</TableCell>
                        <TableCell className="capitalize">{transaction.type}</TableCell>
                        <TableCell>{transaction.items}</TableCell>
                        <TableCell className={transaction.amount < 0 ? 'text-red-600' : ''}>
                          {formatCurrency(Math.abs(transaction.amount))}
                        </TableCell>
                        <TableCell>
                          <Badge variant="default" className="bg-green-100 text-green-800">
                            {transaction.status}
                          </Badge>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TabsContent>
              
              <TabsContent value="loyalty" className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <Card className="pos-card">
                    <CardContent className="pt-6">
                      <div className="text-center">
                        <Gift className="h-12 w-12 text-[#D4AF37] mx-auto mb-2" />
                        <p className="text-2xl font-bold text-[#0D1B2A]">{selectedCustomer.loyaltyPoints}</p>
                        <p className="text-sm text-muted-foreground">Loyalty Points</p>
                      </div>
                    </CardContent>
                  </Card>
                  
                  <Card className="pos-card">
                    <CardContent className="pt-6">
                      <div className="text-center">
                        <Star className="h-12 w-12 text-purple-600 mx-auto mb-2" />
                        <p className="text-lg font-medium text-[#0D1B2A]">{selectedCustomer.status.toUpperCase()}</p>
                        <p className="text-sm text-muted-foreground">Customer Status</p>
                      </div>
                    </CardContent>
                  </Card>
                </div>
                
                <div className="space-y-2">
                  <Label>Preferred Categories</Label>
                  <div className="flex gap-2 flex-wrap">
                    {selectedCustomer.preferredCategories.map((category, index) => (
                      <Badge key={index} variant="outline">{category}</Badge>
                    ))}
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
                <Button onClick={handleUpdateCustomer} className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
                  Update Customer
                </Button>
              )}
              {!isEditMode && (
                <Button onClick={() => setIsEditMode(true)} className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
                  <Edit className="mr-2 h-4 w-4" />
                  Edit Customer
                </Button>
              )}
            </div>
          </DialogContent>
        </Dialog>
      )}
    </div>
  );
}