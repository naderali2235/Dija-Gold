import React, { useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Badge } from './ui/badge';
import { Separator } from './ui/separator';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from './ui/select';
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
  Search,
  ShoppingCart,
  Plus,
  Minus,
  X,
  User,
  Calculator,
  CreditCard,
  FileText,
  HelpCircle,
} from 'lucide-react';
import { formatCurrency } from './utils/currency';

interface CartItem {
  id: string;
  name: string;
  category: string;
  karat: string;
  weight: number;
  rate: number;
  makingCharges: number;
  quantity: number;
  total: number;
}

interface Customer {
  id: string;
  name: string;
  phone: string;
  loyaltyPoints: number;
}

export default function Sales() {
  const [cart, setCart] = useState<CartItem[]>([]);
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [customerSearch, setCustomerSearch] = useState('');

  // Mock data
  const products = [
    { id: '1', name: 'Gold Ring', category: 'Ring', karat: '22K', weight: 5.5, rate: 6000, makingCharges: 500, stock: 10 },
    { id: '2', name: 'Gold Chain', category: 'Chain', karat: '22K', weight: 12.3, rate: 6000, makingCharges: 800, stock: 5 },
    { id: '3', name: 'Gold Earrings', category: 'Earrings', karat: '18K', weight: 3.2, rate: 4900, makingCharges: 400, stock: 8 },
    { id: '4', name: 'Gold Bangles', category: 'Bangles', karat: '22K', weight: 25.6, rate: 6000, makingCharges: 1200, stock: 3 },
  ];

  const customers = [
    { id: '1', name: 'Rajesh Kumar', phone: '+91 98765 43210', loyaltyPoints: 450 },
    { id: '2', name: 'Priya Sharma', phone: '+91 87654 32109', loyaltyPoints: 1200 },
    { id: '3', name: 'Amit Patel', phone: '+91 76543 21098', loyaltyPoints: 890 },
  ];

  const filteredProducts = products.filter(product =>
    product.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    product.category.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const filteredCustomers = customers.filter(customer =>
    customer.name.toLowerCase().includes(customerSearch.toLowerCase()) ||
    customer.phone.includes(customerSearch)
  );

  const addToCart = (product: typeof products[0]) => {
    const existingItem = cart.find(item => item.id === product.id);
    
    if (existingItem) {
      setCart(cart.map(item =>
        item.id === product.id
          ? { ...item, quantity: item.quantity + 1, total: (item.quantity + 1) * (item.weight * item.rate + item.makingCharges) }
          : item
      ));
    } else {
      const total = product.weight * product.rate + product.makingCharges;
      setCart([...cart, {
        id: product.id,
        name: product.name,
        category: product.category,
        karat: product.karat,
        weight: product.weight,
        rate: product.rate,
        makingCharges: product.makingCharges,
        quantity: 1,
        total: total,
      }]);
    }
  };

  const updateQuantity = (id: string, quantity: number) => {
    if (quantity <= 0) {
      setCart(cart.filter(item => item.id !== id));
    } else {
      setCart(cart.map(item =>
        item.id === id
          ? { ...item, quantity, total: quantity * (item.weight * item.rate + item.makingCharges) }
          : item
      ));
    }
  };

  const removeFromCart = (id: string) => {
    setCart(cart.filter(item => item.id !== id));
  };

  const subtotal = cart.reduce((sum, item) => sum + item.total, 0);
  const taxRate = 0.03; // 3% GST
  const tax = subtotal * taxRate;
  const total = subtotal + tax;

  const handleCheckout = () => {
    // Mock checkout process
    alert('Sale completed successfully!');
    setCart([]);
    setSelectedCustomer(null);
  };

  return (
    <div className="h-screen flex flex-col bg-gray-50">
      {/* Header */}
      <div className="flex items-center justify-between p-6 bg-white border-b">
        <h1 className="text-3xl font-semibold text-gray-800">New Sale</h1>
        <Dialog>
          <DialogTrigger asChild>
            <Button variant="outline" className="flex items-center gap-2 bg-white hover:bg-gray-50">
              <User className="h-4 w-4" />
              {selectedCustomer ? selectedCustomer.name : 'Select Customer'}
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-md">
            <DialogHeader>
              <DialogTitle>Select Customer</DialogTitle>
              <DialogDescription>
                Choose an existing customer or proceed without one
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-4">
              <div className="relative">
                <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Search by name or phone..."
                  value={customerSearch}
                  onChange={(e) => setCustomerSearch(e.target.value)}
                  className="pl-10"
                />
              </div>
              <div className="max-h-60 overflow-auto space-y-2">
                {filteredCustomers.map((customer) => (
                  <div
                    key={customer.id}
                    className="p-3 border rounded-lg cursor-pointer hover:bg-muted/50"
                    onClick={() => {
                      setSelectedCustomer(customer);
                      setCustomerSearch('');
                    }}
                  >
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="font-medium">{customer.name}</p>
                        <p className="text-sm text-muted-foreground">{customer.phone}</p>
                      </div>
                      <Badge variant="secondary">{customer.loyaltyPoints} pts</Badge>
                    </div>
                  </div>
                ))}
              </div>
              <Button
                variant="outline"
                className="w-full"
                onClick={() => setSelectedCustomer(null)}
              >
                Proceed without customer
              </Button>
            </div>
          </DialogContent>
        </Dialog>
      </div>

      {/* Main Content */}
      <div className="flex-1 flex gap-6 p-6">
        {/* Products Section - Left Panel */}
        <div className="flex-1 bg-white rounded-lg shadow-sm">
          <div className="p-6 border-b">
            <h2 className="text-xl font-semibold text-gray-800 mb-4">Products</h2>
            <div className="relative">
              <Search className="absolute left-3 top-3 h-4 w-4 text-gray-400" />
              <Input
                placeholder="Search products..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-10 border-gray-200 focus:border-yellow-500 focus:ring-yellow-500"
              />
            </div>
          </div>
          <div className="p-6">
            <div className="grid grid-cols-2 gap-4">
              {filteredProducts.map((product) => (
                <div
                  key={product.id}
                  className="bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow"
                >
                  <div className="flex items-start justify-between mb-3">
                    <h3 className="font-medium text-gray-800">{product.name}</h3>
                    <Badge className="bg-yellow-100 text-yellow-800 border-yellow-200 text-xs">
                      {product.karat}
                    </Badge>
                  </div>
                  <div className="space-y-1 text-sm text-gray-600 mb-4">
                    <p>Weight: {product.weight}g</p>
                    <p>Rate: {formatCurrency(product.rate)}/g</p>
                    <p>Making: {formatCurrency(product.makingCharges)}</p>
                    <p>Stock: {product.stock} pcs</p>
                  </div>
                  <div className="flex items-center justify-between">
                    <p className="font-semibold text-gray-800 text-lg">
                      {formatCurrency(product.weight * product.rate + product.makingCharges)}
                    </p>
                    <Button
                      size="sm"
                      onClick={() => addToCart(product)}
                      disabled={product.stock === 0}
                      className="bg-yellow-500 hover:bg-yellow-600 text-white w-10 h-10 p-0 rounded"
                    >
                      <Plus className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Right Sidebar */}
        <div className="w-96 space-y-6">
          {/* Cart Section - Top Right */}
          <div className="bg-white rounded-lg shadow-sm">
            <div className="p-6 border-b">
              <h2 className="text-xl font-semibold text-gray-800 flex items-center gap-2">
                <ShoppingCart className="h-5 w-5" />
                Cart ({cart.length})
              </h2>
            </div>
            <div className="p-6">
              {cart.length === 0 ? (
                <p className="text-center text-gray-500 py-8">
                  No items in cart
                </p>
              ) : (
                <div className="space-y-4">
                  {cart.map((item) => (
                    <div key={item.id} className="border border-gray-200 rounded-lg p-4">
                      <div className="flex items-start justify-between mb-2">
                        <h4 className="font-medium text-gray-800">{item.name}</h4>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => removeFromCart(item.id)}
                          className="text-gray-400 hover:text-gray-600 p-1"
                        >
                          <X className="h-4 w-4" />
                        </Button>
                      </div>
                      <div className="text-sm text-gray-600 mb-3">
                        {item.weight}g × {formatCurrency(item.rate)} + {formatCurrency(item.makingCharges)}
                      </div>
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-2">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => updateQuantity(item.id, item.quantity - 1)}
                            className="w-8 h-8 p-0 border-gray-300"
                          >
                            <Minus className="h-3 w-3" />
                          </Button>
                          <span className="w-8 text-center font-medium">{item.quantity}</span>
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => updateQuantity(item.id, item.quantity + 1)}
                            className="w-8 h-8 p-0 border-gray-300"
                          >
                            <Plus className="h-3 w-3" />
                          </Button>
                        </div>
                        <p className="font-semibold text-gray-800">
                          {formatCurrency(item.total)}
                        </p>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>

          {/* Bill Summary Section - Bottom Right */}
          {cart.length > 0 && (
            <div className="bg-white rounded-lg shadow-sm">
              <div className="p-6 border-b">
                <h2 className="text-xl font-semibold text-gray-800 flex items-center gap-2">
                  <FileText className="h-5 w-5" />
                  Bill Summary
                </h2>
              </div>
              <div className="p-6 space-y-4">
                <div className="space-y-3">
                  <div className="flex justify-between text-gray-600">
                    <span>Subtotal:</span>
                    <span>{formatCurrency(subtotal)}</span>
                  </div>
                  <div className="flex justify-between text-gray-600">
                    <span>GST (3%):</span>
                    <span>{formatCurrency(tax)}</span>
                  </div>
                  <Separator />
                  <div className="flex justify-between font-semibold text-lg text-gray-800">
                    <span>Total:</span>
                    <span>{formatCurrency(total)}</span>
                  </div>
                </div>
                
                <div className="space-y-4">
                  <Select defaultValue="cash">
                    <SelectTrigger className="border-gray-200">
                      <SelectValue placeholder="Payment method" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="cash">Cash</SelectItem>
                      <SelectItem value="card">Card</SelectItem>
                      <SelectItem value="upi">UPI</SelectItem>
                    </SelectContent>
                  </Select>
                  
                  <Button
                    className="w-full bg-yellow-500 hover:bg-yellow-600 text-white font-semibold py-3 text-lg flex items-center justify-center gap-2"
                    onClick={handleCheckout}
                  >
                    <FileText className="h-5 w-5" />
                    Complete Sale
                  </Button>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Help Button */}
      <div className="fixed bottom-6 right-6">
        <Button
          variant="outline"
          size="sm"
          className="rounded-full w-12 h-12 p-0 bg-white shadow-lg hover:bg-gray-50"
        >
          <HelpCircle className="h-5 w-5" />
        </Button>
      </div>
    </div>
  );
}