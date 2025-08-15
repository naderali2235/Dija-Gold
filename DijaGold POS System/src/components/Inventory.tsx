import React, { useState } from 'react';
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
} from 'lucide-react';
import { useAuth } from './AuthContext';

interface InventoryItem {
  id: string;
  productId: string;
  sku: string;
  name: string;
  category: string;
  karat: string;
  weight: number;
  currentStock: number;
  minStock: number;
  maxStock: number;
  lastUpdated: string;
  location: string;
  value: number;
  status: 'in_stock' | 'low_stock' | 'out_of_stock' | 'excess_stock';
}

interface StockMovement {
  id: string;
  productId: string;
  sku: string;
  name: string;
  type: 'purchase' | 'sale' | 'return' | 'adjustment' | 'transfer';
  quantity: number;
  previousStock: number;
  newStock: number;
  date: string;
  reference: string;
  notes: string;
  user: string;
}

export default function Inventory() {
  const { isManager } = useAuth();
  const [activeTab, setActiveTab] = useState('overview');
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [isAdjustmentOpen, setIsAdjustmentOpen] = useState(false);
  const [selectedItem, setSelectedItem] = useState<InventoryItem | null>(null);
  const [adjustmentType, setAdjustmentType] = useState<'add' | 'remove' | 'set'>('add');
  const [adjustmentQuantity, setAdjustmentQuantity] = useState('');
  const [adjustmentReason, setAdjustmentReason] = useState('');

  // Mock data
  const inventoryItems: InventoryItem[] = [
    {
      id: '1',
      productId: 'prod1',
      sku: 'GLD-RNG-001',
      name: 'Classic Gold Ring',
      category: 'Ring',
      karat: '22K',
      weight: 5.5,
      currentStock: 15,
      minStock: 5,
      maxStock: 50,
      lastUpdated: '2024-01-15T10:30:00Z',
      location: 'Main Display',
      value: 500000,
      status: 'in_stock',
    },
    {
      id: '2',
      productId: 'prod2',
      sku: 'GLD-CHN-002',
      name: 'Designer Gold Chain',
      category: 'Chain',
      karat: '22K',
      weight: 12.3,
      currentStock: 3,
      minStock: 5,
      maxStock: 25,
      lastUpdated: '2024-01-14T15:45:00Z',
      location: 'Vault A',
      value: 225000,
      status: 'low_stock',
    },
    {
      id: '3',
      productId: 'prod3',
      sku: 'GLD-EAR-003',
      name: 'Pearl Drop Earrings',
      category: 'Earrings',
      karat: '18K',
      weight: 3.2,
      currentStock: 0,
      minStock: 5,
      maxStock: 20,
      lastUpdated: '2024-01-13T11:20:00Z',
      location: 'Display Case 2',
      value: 0,
      status: 'out_of_stock',
    },
    {
      id: '4',
      productId: 'prod4',
      sku: 'GLD-BNG-004',
      name: 'Traditional Bangles Set',
      category: 'Bangles',
      karat: '22K',
      weight: 25.6,
      currentStock: 45,
      minStock: 2,
      maxStock: 15,
      lastUpdated: '2024-01-12T16:30:00Z',
      location: 'Vault B',
      value: 7000000,
      status: 'excess_stock',
    },
  ];

  const stockMovements: StockMovement[] = [
    {
      id: '1',
      productId: 'prod1',
      sku: 'GLD-RNG-001',
      name: 'Classic Gold Ring',
      type: 'sale',
      quantity: -2,
      previousStock: 17,
      newStock: 15,
      date: '2024-01-15T10:30:00Z',
      reference: 'INV-2024-001',
      notes: 'Sale to Rajesh Kumar',
      user: 'John Cashier',
    },
    {
      id: '2',
      productId: 'prod2',
      sku: 'GLD-CHN-002',
      name: 'Designer Gold Chain',
      type: 'purchase',
      quantity: +5,
      previousStock: 3,
      newStock: 8,
      date: '2024-01-14T09:15:00Z',
      reference: 'PO-2024-003',
      notes: 'Purchase from Mumbai Gold House',
      user: 'Store Manager',
    },
    {
      id: '3',
      productId: 'prod3',
      sku: 'GLD-EAR-003',
      name: 'Pearl Drop Earrings',
      type: 'return',
      quantity: +1,
      previousStock: 0,
      newStock: 1,
      date: '2024-01-13T14:45:00Z',
      reference: 'RET-2024-001',
      notes: 'Customer return - size issue',
      user: 'Store Manager',
    },
    {
      id: '4',
      productId: 'prod4',
      sku: 'GLD-BNG-004',
      name: 'Traditional Bangles Set',
      type: 'adjustment',
      quantity: -3,
      previousStock: 48,
      newStock: 45,
      date: '2024-01-12T16:30:00Z',
      reference: 'ADJ-2024-001',
      notes: 'Inventory adjustment - damaged items removed',
      user: 'Store Manager',
    },
  ];

  const filteredItems = inventoryItems.filter(item => {
    const matchesSearch = 
      item.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      item.sku.toLowerCase().includes(searchQuery.toLowerCase());
    
    const matchesStatus = statusFilter === 'all' || item.status === statusFilter;
    
    return matchesSearch && matchesStatus;
  });

  const getStatusBadge = (status: string) => {
    const variants = {
      in_stock: { variant: 'default' as const, className: 'bg-green-100 text-green-800', icon: Package },
      low_stock: { variant: 'destructive' as const, className: 'bg-yellow-100 text-yellow-800', icon: AlertTriangle },
      out_of_stock: { variant: 'destructive' as const, className: 'bg-red-100 text-red-800', icon: AlertTriangle },
      excess_stock: { variant: 'default' as const, className: 'bg-blue-100 text-blue-800', icon: TrendingUp },
    };

    const config = variants[status as keyof typeof variants];
    const Icon = config.icon;

    return (
      <Badge variant={config.variant} className={config.className}>
        <Icon className="mr-1 h-3 w-3" />
        {status.replace('_', ' ').toUpperCase()}
      </Badge>
    );
  };

  const getMovementIcon = (type: string) => {
    const icons = {
      purchase: { icon: TrendingUp, className: 'text-green-600' },
      sale: { icon: TrendingDown, className: 'text-red-600' },
      return: { icon: TrendingUp, className: 'text-blue-600' },
      adjustment: { icon: ArrowUpDown, className: 'text-yellow-600' },
      transfer: { icon: ArrowUpDown, className: 'text-purple-600' },
    };

    const config = icons[type as keyof typeof icons];
    const Icon = config.icon;

    return <Icon className={`h-4 w-4 ${config.className}`} />;
  };

  const handleStockAdjustment = () => {
    if (!isManager || !selectedItem) {
      alert('Only managers can adjust stock');
      return;
    }
    
    // Mock stock adjustment
    console.log('Stock adjustment:', {
      item: selectedItem.id,
      type: adjustmentType,
      quantity: adjustmentQuantity,
      reason: adjustmentReason,
    });
    
    setIsAdjustmentOpen(false);
    setSelectedItem(null);
    setAdjustmentQuantity('');
    setAdjustmentReason('');
  };

  const inventoryStats = {
    totalItems: inventoryItems.length,
    totalValue: inventoryItems.reduce((sum, item) => sum + item.value, 0),
    lowStockItems: inventoryItems.filter(item => item.status === 'low_stock').length,
    outOfStockItems: inventoryItems.filter(item => item.status === 'out_of_stock').length,
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl text-[#0D1B2A]">Inventory Management</h1>
          <p className="text-muted-foreground">Track stock levels and movement history</p>
        </div>
        {isManager && (
          <Dialog open={isAdjustmentOpen} onOpenChange={setIsAdjustmentOpen}>
            <DialogTrigger asChild>
              <Button className="touch-target pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
                <ArrowUpDown className="mr-2 h-4 w-4" />
                Stock Adjustment
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Stock Adjustment</DialogTitle>
                <DialogDescription>
                  Adjust inventory quantities for selected item
                </DialogDescription>
              </DialogHeader>
              <div className="space-y-4">
                <div className="space-y-2">
                  <Label>Select Item</Label>
                  <Select 
                    value={selectedItem?.id || ''} 
                    onValueChange={(value) => setSelectedItem(inventoryItems.find(item => item.id === value) || null)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Choose an item" />
                    </SelectTrigger>
                    <SelectContent>
                      {inventoryItems.map(item => (
                        <SelectItem key={item.id} value={item.id}>
                          {item.sku} - {item.name} (Current: {item.currentStock})
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                
                {selectedItem && (
                  <>
                    <div className="p-3 bg-muted rounded-lg">
                      <div className="flex justify-between">
                        <span>Current Stock:</span>
                        <span className="font-medium">{selectedItem.currentStock} pcs</span>
                      </div>
                    </div>
                    
                    <div className="space-y-2">
                      <Label>Adjustment Type</Label>
                      <Select value={adjustmentType} onValueChange={(value: 'add' | 'remove' | 'set') => setAdjustmentType(value)}>
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="add">Add Stock</SelectItem>
                          <SelectItem value="remove">Remove Stock</SelectItem>
                          <SelectItem value="set">Set Stock Level</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                    
                    <div className="space-y-2">
                      <Label>Quantity</Label>
                      <Input
                        type="number"
                        value={adjustmentQuantity}
                        onChange={(e) => setAdjustmentQuantity(e.target.value)}
                        placeholder={adjustmentType === 'set' ? 'New stock level' : 'Quantity to adjust'}
                      />
                    </div>
                    
                    <div className="space-y-2">
                      <Label>Reason</Label>
                      <Select value={adjustmentReason} onValueChange={setAdjustmentReason}>
                        <SelectTrigger>
                          <SelectValue placeholder="Select reason" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="damaged">Damaged Items</SelectItem>
                          <SelectItem value="theft">Theft/Loss</SelectItem>
                          <SelectItem value="recount">Physical Recount</SelectItem>
                          <SelectItem value="transfer">Branch Transfer</SelectItem>
                          <SelectItem value="other">Other</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                  </>
                )}
              </div>
              <div className="flex justify-end gap-3 mt-6">
                <Button variant="outline" onClick={() => setIsAdjustmentOpen(false)}>
                  Cancel
                </Button>
                <Button 
                  onClick={handleStockAdjustment}
                  disabled={!selectedItem || !adjustmentQuantity || !adjustmentReason}
                  className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]"
                >
                  Apply Adjustment
                </Button>
              </div>
            </DialogContent>
          </Dialog>
        )}
      </div>

      {/* Inventory Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total Items</p>
                <p className="text-2xl text-[#0D1B2A]">{inventoryStats.totalItems}</p>
              </div>
              <Package className="h-8 w-8 text-[#D4AF37]" />
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total Value</p>
                <p className="text-2xl text-[#0D1B2A]">{formatCurrency(inventoryStats.totalValue)}</p>
              </div>
              <TrendingUp className="h-8 w-8 text-green-600" />
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Low Stock</p>
                <p className="text-2xl text-[#0D1B2A]">{inventoryStats.lowStockItems}</p>
              </div>
              <AlertTriangle className="h-8 w-8 text-yellow-600" />
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Out of Stock</p>
                <p className="text-2xl text-[#0D1B2A]">{inventoryStats.outOfStockItems}</p>
              </div>
              <AlertTriangle className="h-8 w-8 text-red-600" />
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="movements">Movement History</TabsTrigger>
          <TabsTrigger value="alerts">Stock Alerts</TabsTrigger>
        </TabsList>
        
        <TabsContent value="overview" className="space-y-4">
          {/* Filters */}
          <Card className="pos-card">
            <CardContent className="pt-6">
              <div className="flex flex-col md:flex-row gap-4">
                <div className="relative flex-1">
                  <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                  <Input
                    placeholder="Search by product name or SKU..."
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
                    <SelectItem value="in_stock">In Stock</SelectItem>
                    <SelectItem value="low_stock">Low Stock</SelectItem>
                    <SelectItem value="out_of_stock">Out of Stock</SelectItem>
                    <SelectItem value="excess_stock">Excess Stock</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </CardContent>
          </Card>

          {/* Inventory Table */}
          <Card className="pos-card">
            <CardHeader>
              <CardTitle>Inventory Items</CardTitle>
              <CardDescription>
                {filteredItems.length} item(s) found
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>SKU</TableHead>
                    <TableHead>Product</TableHead>
                    <TableHead>Category</TableHead>
                    <TableHead>Current Stock</TableHead>
                    <TableHead>Min/Max</TableHead>
                    <TableHead>Value</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Location</TableHead>
                    <TableHead>Last Updated</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredItems.map((item) => (
                    <TableRow key={item.id}>
                      <TableCell className="font-medium">{item.sku}</TableCell>
                      <TableCell>
                        <div>
                          <p className="font-medium">{item.name}</p>
                          <p className="text-sm text-muted-foreground">{item.karat} - {item.weight}g</p>
                        </div>
                      </TableCell>
                      <TableCell>{item.category}</TableCell>
                      <TableCell className="text-center">
                        <span className="text-lg font-medium">{item.currentStock}</span>
                      </TableCell>
                      <TableCell>
                        <div className="text-sm">
                          <p>Min: {item.minStock}</p>
                          <p>Max: {item.maxStock}</p>
                        </div>
                      </TableCell>
                      <TableCell>{formatCurrency(item.value)}</TableCell>
                      <TableCell>{getStatusBadge(item.status)}</TableCell>
                      <TableCell>{item.location}</TableCell>
                      <TableCell>{new Date(item.lastUpdated).toLocaleDateString()}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>
        
        <TabsContent value="movements" className="space-y-4">
          <Card className="pos-card">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <History className="h-5 w-5 text-[#D4AF37]" />
                Stock Movements
              </CardTitle>
              <CardDescription>Recent inventory changes and transactions</CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Date</TableHead>
                    <TableHead>Product</TableHead>
                    <TableHead>Type</TableHead>
                    <TableHead>Quantity</TableHead>
                    <TableHead>Stock Change</TableHead>
                    <TableHead>Reference</TableHead>
                    <TableHead>User</TableHead>
                    <TableHead>Notes</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {stockMovements.map((movement) => (
                    <TableRow key={movement.id}>
                      <TableCell>{new Date(movement.date).toLocaleDateString()}</TableCell>
                      <TableCell>
                        <div>
                          <p className="font-medium">{movement.name}</p>
                          <p className="text-sm text-muted-foreground">{movement.sku}</p>
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          {getMovementIcon(movement.type)}
                          <span className="capitalize">{movement.type}</span>
                        </div>
                      </TableCell>
                      <TableCell>
                        <span className={movement.quantity > 0 ? 'text-green-600' : 'text-red-600'}>
                          {movement.quantity > 0 ? '+' : ''}{movement.quantity}
                        </span>
                      </TableCell>
                      <TableCell>
                        <div className="text-sm">
                          <p>{movement.previousStock} → {movement.newStock}</p>
                        </div>
                      </TableCell>
                      <TableCell>{movement.reference}</TableCell>
                      <TableCell>{movement.user}</TableCell>
                      <TableCell>
                        <p className="max-w-40 truncate text-sm">{movement.notes}</p>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>
        
        <TabsContent value="alerts" className="space-y-4">
          <Card className="pos-card">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <AlertTriangle className="h-5 w-5 text-red-600" />
                Stock Alerts
              </CardTitle>
              <CardDescription>Items requiring immediate attention</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {inventoryItems
                  .filter(item => item.status === 'low_stock' || item.status === 'out_of_stock')
                  .map((item) => (
                    <div key={item.id} className="flex items-center justify-between p-4 border rounded-lg">
                      <div className="flex items-center gap-3">
                        <AlertTriangle className={`h-5 w-5 ${item.status === 'out_of_stock' ? 'text-red-600' : 'text-yellow-600'}`} />
                        <div>
                          <p className="font-medium">{item.name}</p>
                          <p className="text-sm text-muted-foreground">{item.sku} - {item.category}</p>
                        </div>
                      </div>
                      <div className="text-right">
                        <p className="font-medium">
                          {item.currentStock} / {item.minStock} minimum
                        </p>
                        {getStatusBadge(item.status)}
                      </div>
                    </div>
                  ))}
                
                {inventoryItems.filter(item => item.status === 'low_stock' || item.status === 'out_of_stock').length === 0 && (
                  <div className="text-center py-8">
                    <Package className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                    <p className="text-muted-foreground">All items are well stocked!</p>
                  </div>
                )}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}