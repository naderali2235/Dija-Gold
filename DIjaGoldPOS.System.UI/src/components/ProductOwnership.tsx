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
  Package,
  Plus,
  Search,
  AlertTriangle,
  DollarSign,
  TrendingUp,
  TrendingDown,
  Eye,
  Edit,
  Trash2,
  Bell,
  FileText,
  Shield,
  Send,
  Filter,
  BarChart3,
  Users,
  MapPin,
  Calendar,
  Clock,
  CheckCircle,
  XCircle,
} from 'lucide-react';
import { formatCurrency } from './utils/currency';
import { useAuth } from './AuthContext';
import { 
  ProductOwnershipDto,
  ProductOwnershipRequest,
  OwnershipAlertDto,
  OwnershipMovementDto,
  Product,
  SupplierDto
} from '../services/api';
import { 
  usePaginatedProductOwnership,
  useGetOwnershipAlerts,
  useGetLowOwnershipProducts,
  useGetProductsWithOutstandingPayments,
  useCreateOrUpdateOwnership,
  useGetOwnershipMovements,
  useUpdateOwnershipAfterPayment,
  useProducts,
  useSuppliers
} from '../hooks/useApi';
import { toast } from 'sonner';

export default function ProductOwnership() {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState('overview');
  const [isNewOwnershipOpen, setIsNewOwnershipOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedOwnership, setSelectedOwnership] = useState<ProductOwnershipDto | null>(null);
  const [selectedOwnershipForDetails, setSelectedOwnershipForDetails] = useState<ProductOwnershipDto | null>(null);
  const [ownershipMovements, setOwnershipMovements] = useState<OwnershipMovementDto[]>([]);

  // Form state for new ownership
  const [newOwnership, setNewOwnership] = useState({
    productId: '',
    supplierId: '',
    totalQuantity: '',
    totalWeight: '',
    ownedQuantity: '',
    ownedWeight: '',
    totalCost: '',
    amountPaid: '',
  });


  // State for ownership details dialog
  const [ownershipDetailsOpen, setOwnershipDetailsOpen] = useState(false);

  // State for payment dialog
  const [paymentDialogOpen, setPaymentDialogOpen] = useState(false);
  const [paymentForm, setPaymentForm] = useState({
    paymentAmount: '',
    referenceNumber: '',
  });

  // API hooks
  const productOwnershipList = usePaginatedProductOwnership(user?.branch?.id || 0, {
    searchTerm: searchQuery,
    pageSize: 50
  });
  
  const ownershipAlerts = useGetOwnershipAlerts();
  const lowOwnershipProducts = useGetLowOwnershipProducts();
  const outstandingPayments = useGetProductsWithOutstandingPayments();
  const createOwnership = useCreateOrUpdateOwnership();
  const getOwnershipMovements = useGetOwnershipMovements();
  const updateOwnershipPayment = useUpdateOwnershipAfterPayment();
  
  const products = useProducts();
  const suppliers = useSuppliers();

  // Fetch data on component mount
  useEffect(() => {
    if (user?.branch?.id) {
      ownershipAlerts.execute(user.branch.id);
      lowOwnershipProducts.execute(0.5);
      outstandingPayments.execute();
      products.execute({ pageSize: 1000 });
      suppliers.execute({ pageSize: 1000 });
    }
  }, [user?.branch?.id]);

  // Update search when searchQuery changes
  useEffect(() => {
    if (user?.branch?.id) {
      productOwnershipList.updateParams({ searchTerm: searchQuery });
    }
  }, [searchQuery, user?.branch?.id]);

  const handleCreateOwnership = async () => {
    if (!user?.branch?.id) {
      toast.error('Branch information not available');
      return;
    }

    if (!newOwnership.productId || !newOwnership.totalQuantity || !newOwnership.totalWeight) {
      toast.error('Please fill in all required fields');
      return;
    }

    try {
      const request: ProductOwnershipRequest = {
        productId: parseInt(newOwnership.productId),
        branchId: user.branch.id,
        supplierId: newOwnership.supplierId ? parseInt(newOwnership.supplierId) : undefined,
        totalQuantity: parseFloat(newOwnership.totalQuantity),
        totalWeight: parseFloat(newOwnership.totalWeight),
        ownedQuantity: parseFloat(newOwnership.ownedQuantity) || 0,
        ownedWeight: parseFloat(newOwnership.ownedWeight) || 0,
        totalCost: parseFloat(newOwnership.totalCost) || 0,
        amountPaid: parseFloat(newOwnership.amountPaid) || 0,
      };

      await createOwnership.execute(request);
      
      toast.success('Product ownership created successfully');
      setIsNewOwnershipOpen(false);
      setNewOwnership({
        productId: '',
        supplierId: '',
        totalQuantity: '',
        totalWeight: '',
        ownedQuantity: '',
        ownedWeight: '',
        totalCost: '',
        amountPaid: '',
      });
      
      // Refresh data
      productOwnershipList.fetchData();
      ownershipAlerts.execute(user.branch.id);
      lowOwnershipProducts.execute(0.5);
      outstandingPayments.execute();
    } catch (error) {
      console.error('Failed to create ownership:', error);
      toast.error('Failed to create product ownership');
    }
  };


  const handleViewOwnershipDetails = async (ownership: ProductOwnershipDto) => {
    try {
      const movements = await getOwnershipMovements.execute(ownership.id);
      setOwnershipMovements(movements);
      setSelectedOwnershipForDetails(ownership);
      setOwnershipDetailsOpen(true);
    } catch (error) {
      console.error('Failed to fetch ownership movements:', error);
      toast.error('Failed to load ownership details');
    }
  };

  const handleMakePayment = async () => {
    if (!selectedOwnership) {
      toast.error('No ownership selected');
      return;
    }

    if (!paymentForm.paymentAmount || !paymentForm.referenceNumber) {
      toast.error('Please fill in all payment fields');
      return;
    }

    try {
      await updateOwnershipPayment.execute({
        productOwnershipId: selectedOwnership.id,
        paymentAmount: parseFloat(paymentForm.paymentAmount),
        referenceNumber: paymentForm.referenceNumber,
      });

      toast.success('Payment processed successfully');
      setPaymentDialogOpen(false);
      setPaymentForm({ paymentAmount: '', referenceNumber: '' });
      setSelectedOwnership(null);
      
      // Refresh data
      productOwnershipList.fetchData();
      ownershipAlerts.execute(user?.branch?.id || 0);
      lowOwnershipProducts.execute(0.5);
      outstandingPayments.execute();
    } catch (error) {
      console.error('Failed to process payment:', error);
      toast.error('Failed to process payment');
    }
  };

  const getOwnershipStatusColor = (percentage: number) => {
    if (percentage >= 80) return 'bg-green-100 text-green-800';
    if (percentage >= 50) return 'bg-yellow-100 text-yellow-800';
    return 'bg-red-100 text-red-800';
  };

  const getAlertSeverityColor = (severity: string) => {
    switch (severity.toLowerCase()) {
      case 'high': return 'bg-red-100 text-red-800';
      case 'medium': return 'bg-yellow-100 text-yellow-800';
      case 'low': return 'bg-blue-100 text-blue-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  if (productOwnershipList.loading || ownershipAlerts.loading || lowOwnershipProducts.loading || outstandingPayments.loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Product Ownership</h1>
          <p className="text-muted-foreground">
            Manage product ownership and track payments
          </p>
        </div>
        <div className="flex gap-2">
          <Button onClick={() => setIsNewOwnershipOpen(true)}>
            <Plus className="h-4 w-4 mr-2" />
            New Ownership
          </Button>
        </div>
      </div>

      {/* Alerts Summary */}
      {ownershipAlerts.data && ownershipAlerts.data.length > 0 && (
        <Card className="border-orange-200 bg-orange-50">
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-orange-800">
              <Bell className="h-5 w-5" />
              Ownership Alerts ({ownershipAlerts.data.length})
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              {ownershipAlerts.data.slice(0, 3).map((alert: OwnershipAlertDto, index: number) => (
                <div key={index} className="flex items-center justify-between p-2 bg-white rounded">
                  <div>
                    <p className="font-medium">{alert.productName}</p>
                    <p className="text-sm text-gray-600">{alert.message}</p>
                  </div>
                  <Badge className={getAlertSeverityColor(alert.severity)}>
                    {alert.severity}
                  </Badge>
                </div>
              ))}
              {ownershipAlerts.data.length > 3 && (
                <p className="text-sm text-orange-700">
                  +{ownershipAlerts.data.length - 3} more alerts...
                </p>
              )}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Main Content */}
      <Tabs value={activeTab} onValueChange={setActiveTab} className="space-y-4">
        <TabsList>
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="low-ownership">Low Ownership</TabsTrigger>
          <TabsTrigger value="outstanding-payments">Outstanding Payments</TabsTrigger>
          <TabsTrigger value="movements">Movement History</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Product Ownership Summary</CardTitle>
              <CardDescription>
                Overview of all product ownership records
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="flex gap-4 mb-4">
                <Input
                  placeholder="Search products, suppliers..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="max-w-sm"
                />
              </div>
              
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Product</TableHead>
                    <TableHead>Supplier</TableHead>
                    <TableHead>Ownership %</TableHead>
                    <TableHead>Total Cost</TableHead>
                    <TableHead>Amount Paid</TableHead>
                    <TableHead>Outstanding</TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {productOwnershipList.data?.items.map((ownership: ProductOwnershipDto) => (
                    <TableRow key={ownership.id}>
                      <TableCell>
                        <div>
                          <p className="font-medium">{ownership.productName}</p>
                          <p className="text-sm text-gray-500">{ownership.productCode}</p>
                        </div>
                      </TableCell>
                      <TableCell>{ownership.supplierId || 'N/A'}</TableCell>
                      <TableCell>
                        <Badge className={getOwnershipStatusColor(ownership.ownershipPercentage)}>
                          {ownership.ownershipPercentage.toFixed(1)}%
                        </Badge>
                      </TableCell>
                      <TableCell>{formatCurrency(ownership.totalCost)}</TableCell>
                      <TableCell>{formatCurrency(ownership.amountPaid)}</TableCell>
                      <TableCell>
                        <span className={ownership.outstandingAmount > 0 ? 'text-red-600 font-medium' : 'text-green-600'}>
                          {formatCurrency(ownership.outstandingAmount)}
                        </span>
                      </TableCell>
                      <TableCell>
                        <div className="flex gap-2">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleViewOwnershipDetails(ownership)}
                          >
                            <Eye className="h-4 w-4" />
                          </Button>
                          {ownership.outstandingAmount > 0 && (
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => {
                                setSelectedOwnership(ownership);
                                setPaymentDialogOpen(true);
                              }}
                            >
                              <DollarSign className="h-4 w-4" />
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

        <TabsContent value="low-ownership" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Low Ownership Products</CardTitle>
              <CardDescription>
                Products with ownership percentage below 50%
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Product</TableHead>
                    <TableHead>Ownership %</TableHead>
                    <TableHead>Outstanding Amount</TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {lowOwnershipProducts.data?.map((ownership: ProductOwnershipDto) => (
                    <TableRow key={ownership.id}>
                      <TableCell>
                        <div>
                          <p className="font-medium">{ownership.productName}</p>
                          <p className="text-sm text-gray-500">{ownership.productCode}</p>
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge className="bg-red-100 text-red-800">
                          {ownership.ownershipPercentage.toFixed(1)}%
                        </Badge>
                      </TableCell>
                      <TableCell className="text-red-600 font-medium">
                        {formatCurrency(ownership.outstandingAmount)}
                      </TableCell>
                      <TableCell>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => {
                            setSelectedOwnership(ownership);
                            setPaymentDialogOpen(true);
                          }}
                        >
                          <DollarSign className="h-4 w-4 mr-2" />
                          Make Payment
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="outstanding-payments" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Outstanding Payments</CardTitle>
              <CardDescription>
                Products with pending payments
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Product</TableHead>
                    <TableHead>Supplier</TableHead>
                    <TableHead>Outstanding Amount</TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {outstandingPayments.data?.map((ownership: ProductOwnershipDto) => (
                    <TableRow key={ownership.id}>
                      <TableCell>
                        <div>
                          <p className="font-medium">{ownership.productName}</p>
                          <p className="text-sm text-gray-500">{ownership.productCode}</p>
                        </div>
                      </TableCell>
                      <TableCell>{ownership.supplierId || 'N/A'}</TableCell>
                      <TableCell className="text-red-600 font-medium">
                        {formatCurrency(ownership.outstandingAmount)}
                      </TableCell>
                      <TableCell>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => {
                            setSelectedOwnership(ownership);
                            setPaymentDialogOpen(true);
                          }}
                        >
                          <DollarSign className="h-4 w-4 mr-2" />
                          Pay Now
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="movements" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Ownership Movement History</CardTitle>
              <CardDescription>
                Track all ownership changes and movements
              </CardDescription>
            </CardHeader>
            <CardContent>
              {selectedOwnershipForDetails ? (
                <div>
                  <div className="mb-4 p-4 bg-gray-50 rounded">
                    <h3 className="font-medium">{selectedOwnershipForDetails.productName}</h3>
                    <p className="text-sm text-gray-600">{selectedOwnershipForDetails.productCode}</p>
                  </div>
                  
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Date</TableHead>
                        <TableHead>Type</TableHead>
                        <TableHead>Quantity Change</TableHead>
                        <TableHead>Weight Change</TableHead>
                        <TableHead>Amount Change</TableHead>
                        <TableHead>Reference</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {ownershipMovements.map((movement) => (
                        <TableRow key={movement.id}>
                          <TableCell>{new Date(movement.movementDate).toLocaleDateString()}</TableCell>
                          <TableCell>
                            <Badge variant="outline">{movement.movementType}</Badge>
                          </TableCell>
                          <TableCell>{movement.quantityChange}</TableCell>
                          <TableCell>{movement.weightChange}g</TableCell>
                          <TableCell>{formatCurrency(movement.amountChange)}</TableCell>
                          <TableCell>{movement.referenceNumber || 'N/A'}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              ) : (
                <p className="text-center text-gray-500 py-8">
                  Select an ownership record to view movement history
                </p>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* New Ownership Dialog */}
      <Dialog open={isNewOwnershipOpen} onOpenChange={setIsNewOwnershipOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Create New Product Ownership</DialogTitle>
            <DialogDescription>
              Add a new product ownership record
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <Label htmlFor="product">Product</Label>
              <Select value={newOwnership.productId} onValueChange={(value) => setNewOwnership({...newOwnership, productId: value})}>
                <SelectTrigger>
                  <SelectValue placeholder="Select product" />
                </SelectTrigger>
                <SelectContent>
                  {products.data?.items.map((product: Product) => (
                    <SelectItem key={product.id} value={product.id.toString()}>
                      {product.name} ({product.productCode})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            
            <div>
              <Label htmlFor="supplier">Supplier (Optional)</Label>
              <Select value={newOwnership.supplierId} onValueChange={(value) => setNewOwnership({...newOwnership, supplierId: value})}>
                <SelectTrigger>
                  <SelectValue placeholder="Select supplier" />
                </SelectTrigger>
                <SelectContent>
                  {suppliers.data?.items.map((supplier: SupplierDto) => (
                    <SelectItem key={supplier.id} value={supplier.id.toString()}>
                      {supplier.companyName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label htmlFor="totalQuantity">Total Quantity</Label>
                <Input
                  id="totalQuantity"
                  type="number"
                  value={newOwnership.totalQuantity}
                  onChange={(e) => setNewOwnership({...newOwnership, totalQuantity: e.target.value})}
                />
              </div>
              <div>
                <Label htmlFor="totalWeight">Total Weight (g)</Label>
                <Input
                  id="totalWeight"
                  type="number"
                  step="0.01"
                  value={newOwnership.totalWeight}
                  onChange={(e) => setNewOwnership({...newOwnership, totalWeight: e.target.value})}
                />
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label htmlFor="ownedQuantity">Owned Quantity</Label>
                <Input
                  id="ownedQuantity"
                  type="number"
                  value={newOwnership.ownedQuantity}
                  onChange={(e) => setNewOwnership({...newOwnership, ownedQuantity: e.target.value})}
                />
              </div>
              <div>
                <Label htmlFor="ownedWeight">Owned Weight (g)</Label>
                <Input
                  id="ownedWeight"
                  type="number"
                  step="0.01"
                  value={newOwnership.ownedWeight}
                  onChange={(e) => setNewOwnership({...newOwnership, ownedWeight: e.target.value})}
                />
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label htmlFor="totalCost">Total Cost</Label>
                <Input
                  id="totalCost"
                  type="number"
                  step="0.01"
                  value={newOwnership.totalCost}
                  onChange={(e) => setNewOwnership({...newOwnership, totalCost: e.target.value})}
                />
              </div>
              <div>
                <Label htmlFor="amountPaid">Amount Paid</Label>
                <Input
                  id="amountPaid"
                  type="number"
                  step="0.01"
                  value={newOwnership.amountPaid}
                  onChange={(e) => setNewOwnership({...newOwnership, amountPaid: e.target.value})}
                />
              </div>
            </div>

            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setIsNewOwnershipOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleCreateOwnership}>
                Create Ownership
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Payment Dialog */}
      <Dialog open={paymentDialogOpen} onOpenChange={setPaymentDialogOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Make Payment</DialogTitle>
            <DialogDescription>
              Process payment for outstanding amount
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            {selectedOwnership && (
              <div className="p-4 bg-gray-50 rounded">
                <p className="font-medium">{selectedOwnership.productName}</p>
                <p className="text-sm text-gray-600">Outstanding: {formatCurrency(selectedOwnership.outstandingAmount)}</p>
              </div>
            )}
            
            <div>
              <Label htmlFor="paymentAmount">Payment Amount</Label>
              <Input
                id="paymentAmount"
                type="number"
                step="0.01"
                value={paymentForm.paymentAmount}
                onChange={(e) => setPaymentForm({...paymentForm, paymentAmount: e.target.value})}
              />
            </div>

            <div>
              <Label htmlFor="referenceNumber">Reference Number</Label>
              <Input
                id="referenceNumber"
                value={paymentForm.referenceNumber}
                onChange={(e) => setPaymentForm({...paymentForm, referenceNumber: e.target.value})}
                placeholder="Payment reference or receipt number"
              />
            </div>

            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setPaymentDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleMakePayment}>
                Process Payment
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Ownership Details Dialog */}
      <Dialog open={ownershipDetailsOpen} onOpenChange={setOwnershipDetailsOpen}>
        <DialogContent className="max-w-4xl">
          <DialogHeader>
            <DialogTitle>Ownership Details</DialogTitle>
            <DialogDescription>
              Detailed view of ownership information and movement history
            </DialogDescription>
          </DialogHeader>
          {selectedOwnershipForDetails && (
            <div className="space-y-6">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label>Product</Label>
                  <p className="font-medium">{selectedOwnershipForDetails.productName}</p>
                  <p className="text-sm text-gray-600">{selectedOwnershipForDetails.productCode}</p>
                </div>
                <div>
                  <Label>Supplier</Label>
                  <p className="font-medium">
                    {selectedOwnershipForDetails.supplierId ? 
                      suppliers.data?.items.find((s: SupplierDto) => s.id === selectedOwnershipForDetails.supplierId)?.companyName || `Supplier ${selectedOwnershipForDetails.supplierId}` 
                      : 'N/A'
                    }
                  </p>
                </div>
                <div>
                  <Label>Ownership Percentage</Label>
                  <Badge className={getOwnershipStatusColor(selectedOwnershipForDetails.ownershipPercentage)}>
                    {selectedOwnershipForDetails.ownershipPercentage.toFixed(1)}%
                  </Badge>
                </div>
                <div>
                  <Label>Outstanding Amount</Label>
                  <p className={`font-medium ${selectedOwnershipForDetails.outstandingAmount > 0 ? 'text-red-600' : 'text-green-600'}`}>
                    {formatCurrency(selectedOwnershipForDetails.outstandingAmount)}
                  </p>
                </div>
              </div>

              <div>
                <h3 className="font-medium mb-2">Movement History</h3>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Date</TableHead>
                      <TableHead>Type</TableHead>
                      <TableHead>Quantity</TableHead>
                      <TableHead>Weight</TableHead>
                      <TableHead>Amount</TableHead>
                      <TableHead>Reference</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {ownershipMovements.map((movement) => (
                      <TableRow key={movement.id}>
                        <TableCell>{new Date(movement.movementDate).toLocaleDateString()}</TableCell>
                        <TableCell>
                          <Badge variant="outline">{movement.movementType}</Badge>
                        </TableCell>
                        <TableCell>{movement.quantityChange}</TableCell>
                        <TableCell>{movement.weightChange}g</TableCell>
                        <TableCell>{formatCurrency(movement.amountChange)}</TableCell>
                        <TableCell>{movement.referenceNumber || 'N/A'}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>

    </div>
  );
}
