import React, { useState, useEffect } from 'react';
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
  Loader2,
} from 'lucide-react';
import { formatCurrency } from './utils/currency';
import { useProducts, useCustomers, useProcessSale, useGoldRates, useKaratTypes, usePaymentMethods } from '../hooks/useApi';
import { useAuth } from './AuthContext';
import api, { Product, Customer } from '../services/api';
import { EnumMapper, EnumLookupDto } from '../types/enums';

interface CartItem {
  id: number;
  productId: number;
  name: string;
  category: string;
  karat: string;
  weight: number;
  rate: number;
  makingCharges: number;
  quantity: number;
  unitPrice: number;
  discountPercentage: number;
  total: number;
}

export default function Sales() {
  const { user } = useAuth();
  const [cart, setCart] = useState<CartItem[]>([]);
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [customerSearch, setCustomerSearch] = useState('');
  const [paymentMethod, setPaymentMethod] = useState<'Cash' | 'Card' | 'BankTransfer' | 'Cheque'>('Cash');
  const [amountPaid, setAmountPaid] = useState<number>(0);
  const [isProcessingTransaction, setIsProcessingTransaction] = useState(false);
  const [isCustomerDialogOpen, setIsCustomerDialogOpen] = useState(false);
  
  // API hooks
  const { data: productsData, loading: productsLoading, execute: fetchProducts } = useProducts();
  const { data: customersData, loading: customersLoading, execute: fetchCustomers } = useCustomers();
  const { execute: processSale } = useProcessSale();
  const { data: goldRatesData, loading: goldRatesLoading } = useGoldRates();
  const { data: karatTypesData, execute: fetchKaratTypes } = useKaratTypes();
  const { data: paymentMethodsData, execute: fetchPaymentMethods } = usePaymentMethods();

  // Fetch data on component mount
  useEffect(() => {
    fetchProducts({ 
      searchTerm: searchQuery,
      isActive: true,
      pageNumber: 1,
      pageSize: 50
    });
    fetchKaratTypes();
    fetchPaymentMethods();
  }, [searchQuery, fetchProducts, fetchKaratTypes, fetchPaymentMethods]);

  useEffect(() => {
    if (customerSearch.trim()) {
      fetchCustomers({ 
        searchTerm: customerSearch,
        pageNumber: 1,
        pageSize: 20
      });
    }
  }, [customerSearch, fetchCustomers]);

  const products = productsData?.items || [];
  const customers = customersData?.items || [];

  const filteredProducts = products.filter(product =>
    product.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    product.categoryType.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const addToCart = (product: Product) => {
    const existingItem = cart.find(item => item.productId === product.id);
    
    // Get current gold rate from API data
    // Find gold rate data - handle different karat formats
    let goldRateData = goldRatesData?.find(rate => rate.karat === product.karatType);
    
    // If not found, try mapping from karat type enum
    if (!goldRateData && karatTypesData) {
      const karatTypeLookup = karatTypesData.find(kt => kt.name.toLowerCase() === product.karatType.toLowerCase());
      if (karatTypeLookup) {
        goldRateData = goldRatesData?.find(rate => rate.karat === EnumMapper.karatEnumToString(karatTypeLookup.value as any));
      }
    }
    
    const goldRate = goldRateData?.sellRate || 3000; // Fallback rate if API data not available
    
    const makingCharges = product.makingChargesApplicable ? (product.weight * goldRate * 0.15) : 0;
    const unitPrice = (product.weight * goldRate) + makingCharges;
    
    if (existingItem) {
      setCart(cart.map(item =>
        item.productId === product.id
          ? { 
              ...item, 
              quantity: item.quantity + 1, 
              total: (item.quantity + 1) * unitPrice * (1 - item.discountPercentage / 100)
            }
          : item
      ));
    } else {
      const newItem: CartItem = {
        id: Date.now(), // Temporary ID for cart management
        productId: product.id,
        name: product.name,
        category: product.categoryType,
        karat: product.karatType,
        weight: product.weight,
        rate: goldRate,
        makingCharges: makingCharges,
        quantity: 1,
        unitPrice: unitPrice,
        discountPercentage: 0,
        total: unitPrice,
      };
      setCart([...cart, newItem]);
    }
  };

  const updateQuantity = (cartItemId: number, quantity: number) => {
    if (quantity <= 0) {
      setCart(cart.filter(item => item.id !== cartItemId));
    } else {
      setCart(cart.map(item =>
        item.id === cartItemId
          ? { ...item, quantity, total: quantity * item.unitPrice * (1 - item.discountPercentage / 100) }
          : item
      ));
    }
  };

  const updateDiscount = (cartItemId: number, discountPercentage: number) => {
    setCart(cart.map(item => {
      if (item.id === cartItemId) {
        return {
          ...item,
          discountPercentage: Math.max(0, Math.min(100, discountPercentage)),
          total: item.quantity * item.unitPrice * (1 - discountPercentage / 100)
        };
      }
      return item;
    }));
  };

  const removeFromCart = (cartItemId: number) => {
    setCart(cart.filter(item => item.id !== cartItemId));
  };

  const subtotal = cart.reduce((sum, item) => sum + item.total, 0);
  const taxRate = 0.03; // 3% GST
  const tax = subtotal * taxRate;
  const total = subtotal + tax;

  const handleCheckout = async () => {
    if (cart.length === 0) {
      alert('Please add items to cart before checkout');
      return;
    }

    if (!user?.branch?.id) {
      alert('Branch information not available');
      return;
    }

    if (amountPaid < total) {
      alert('Insufficient amount paid');
      return;
    }

    try {
      setIsProcessingTransaction(true);
      
      const saleRequest = {
        branchId: user.branch.id,
        customerId: selectedCustomer?.id,
        items: cart.map(item => ({
          productId: item.productId,
          quantity: item.quantity,
          customDiscountPercentage: item.discountPercentage || undefined,
        })),
        amountPaid: amountPaid,
        paymentMethod: paymentMethod,
      };

      const transaction = await processSale(saleRequest);
      
      alert(`Sale completed successfully! Transaction Number: ${transaction.transactionNumber}`);
      
      // Clear cart and reset form
      setCart([]);
      setSelectedCustomer(null);
      setAmountPaid(0);
      setCustomerSearch('');
      setSearchQuery('');
      
    } catch (error) {
      console.error('Transaction failed:', error);
      alert(error instanceof Error ? error.message : 'Transaction failed. Please try again.');
    } finally {
      setIsProcessingTransaction(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl text-[#0D1B2A]">New Sale</h1>
        <div className="flex gap-3">
          <Dialog open={isCustomerDialogOpen} onOpenChange={setIsCustomerDialogOpen}>
            <DialogTrigger asChild>
              <Button variant="outline" className="touch-target hover:bg-[#F4E9B1] transition-colors">
                <User className="mr-2 h-4 w-4" />
                {selectedCustomer ? selectedCustomer.fullName : 'Select Customer'}
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-md bg-white border-gray-200 shadow-lg">
              <DialogHeader className="text-center pb-4">
                <DialogTitle className="text-xl font-bold text-gray-800">Select Customer</DialogTitle>
                <DialogDescription className="text-gray-500 text-sm">
                  Choose an existing customer or proceed without one
                </DialogDescription>
              </DialogHeader>
              <div className="space-y-4">
                <div className="relative">
                  <Search className="absolute left-3 top-3 h-4 w-4 text-gray-400" />
                  <Input
                    placeholder="Search by name or phone..."
                    value={customerSearch}
                    onChange={(e) => setCustomerSearch(e.target.value)}
                    className="pl-10 bg-gray-50 border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  />
                </div>
                <div className="max-h-60 overflow-auto space-y-2">
                  {customersLoading && customerSearch.trim() ? (
                    <div className="flex items-center justify-center p-4">
                      <Loader2 className="h-4 w-4 animate-spin" />
                      <span className="ml-2 text-sm">Loading customers...</span>
                    </div>
                  ) : customers.length === 0 && customerSearch.trim() ? (
                    <div className="text-center p-4 text-muted-foreground">
                      No customers found
                    </div>
                  ) : (
                    customers.map((customer) => (
                      <div
                        key={customer.id}
                        className="p-3 bg-gray-50 border border-gray-200 rounded-lg cursor-pointer hover:bg-gray-100 transition-colors"
                        onClick={() => {
                          setSelectedCustomer(customer);
                          setCustomerSearch('');
                          setIsCustomerDialogOpen(false);
                        }}
                      >
                        <div className="flex items-center justify-between">
                          <div>
                            <p className="font-medium text-gray-800">{customer.fullName}</p>
                            <p className="text-sm text-gray-500">{customer.mobileNumber || customer.email || 'No contact info'}</p>
                          </div>
                          <span className="text-sm font-medium text-gray-600">
                            Tier {customer.loyaltyTier}
                          </span>
                        </div>
                      </div>
                    ))
                  )}
                </div>
                <Button
                  variant="outline"
                  className="w-full touch-target hover:bg-[#F4E9B1] transition-colors"
                  onClick={() => {
                    setSelectedCustomer(null);
                    setIsCustomerDialogOpen(false);
                  }}
                >
                  Proceed without customer
                </Button>
              </div>
            </DialogContent>
          </Dialog>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Product Search & List */}
        <div className="lg:col-span-2 space-y-4">
          <Card className="pos-card">
            <CardHeader>
              <CardTitle>Products</CardTitle>
              <div className="relative">
                <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Search products..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="pl-10"
                />
              </div>
            </CardHeader>
            <CardContent>
              {productsLoading ? (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {[1, 2, 3, 4].map((i) => (
                    <div key={i} className="p-4 border rounded-lg">
                      <div className="animate-pulse">
                        <div className="h-4 bg-gray-300 rounded mb-2"></div>
                        <div className="h-3 bg-gray-300 rounded w-3/4 mb-2"></div>
                        <div className="h-3 bg-gray-300 rounded w-1/2 mb-2"></div>
                        <div className="h-8 bg-gray-300 rounded"></div>
                      </div>
                    </div>
                  ))}
                </div>
              ) : filteredProducts.length === 0 ? (
                <div className="text-center p-8 text-muted-foreground">
                  {searchQuery ? 'No products found matching your search' : 'No products available'}
                </div>
              ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {filteredProducts.map((product) => {
                    // Calculate price for display
                    const goldRate = product.karatType === '24K' ? 3270.5 : 
                                     product.karatType === '22K' ? 2997.13 : 
                                     product.karatType === '21K' ? 2727.94 : 2452.88;
                    const makingCharges = product.makingChargesApplicable ? (product.weight * goldRate * 0.15) : 0;
                    const totalPrice = (product.weight * goldRate) + makingCharges;
                    
                    return (
                      <div
                        key={product.id}
                        className="p-4 border rounded-lg hover:shadow-md transition-shadow"
                      >
                        <div className="flex items-center justify-between mb-2">
                          <h3 className="font-medium">{product.name}</h3>
                          <Badge variant="outline">{product.karatType}</Badge>
                        </div>
                        <div className="space-y-1 text-sm text-muted-foreground mb-3">
                          <p>Weight: {product.weight}g</p>
                          <p>Rate: {formatCurrency(goldRate)}/g</p>
                          <p>Making: {formatCurrency(makingCharges)}</p>
                          <p>Code: {product.productCode}</p>
                        </div>
                        <div className="flex items-center justify-between">
                          <p className="font-semibold">
                            {formatCurrency(totalPrice)}
                          </p>
                          <Button
                            size="sm"
                            variant="golden"
                            onClick={() => addToCart(product)}
                            disabled={!product.isActive}
                            className="touch-target"
                          >
                            <Plus className="h-4 w-4" />
                          </Button>
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Cart & Checkout */}
        <div className="space-y-4">
          <Card className="pos-card">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <ShoppingCart className="h-5 w-5" />
                Cart ({cart.length})
              </CardTitle>
            </CardHeader>
            <CardContent>
              {cart.length === 0 ? (
                <p className="text-center text-muted-foreground py-8">
                  No items in cart
                </p>
              ) : (
                <div className="space-y-4">
                  {cart.map((item) => (
                    <div key={item.id} className="p-3 border rounded-lg">
                      <div className="flex items-center justify-between mb-2">
                        <h4 className="font-medium text-sm">{item.name}</h4>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => removeFromCart(item.id)}
                        >
                          <X className="h-4 w-4" />
                        </Button>
                      </div>
                      <div className="text-xs text-muted-foreground mb-2">
                        {item.weight}g × {formatCurrency(item.rate)} + {formatCurrency(item.makingCharges)}
                      </div>
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-2">
                          <Button
                            variant="outline"
                            size="sm"
                            className="touch-target hover:bg-[#F4E9B1] transition-colors"
                            onClick={() => updateQuantity(item.id, item.quantity - 1)}
                          >
                            <Minus className="h-3 w-3" />
                          </Button>
                          <span className="w-8 text-center">{item.quantity}</span>
                          <Button
                            variant="outline"
                            size="sm"
                            className="touch-target hover:bg-[#F4E9B1] transition-colors"
                            onClick={() => updateQuantity(item.id, item.quantity + 1)}
                          >
                            <Plus className="h-3 w-3" />
                          </Button>
                        </div>
                        <p className="font-medium">{formatCurrency(item.total)}</p>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>

          {cart.length > 0 && (
            <Card className="pos-card">
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Calculator className="h-5 w-5" />
                  Bill Summary
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-2">
                  <div className="flex justify-between">
                    <span>Subtotal:</span>
                    <span>{formatCurrency(subtotal)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>GST (3%):</span>
                    <span>{formatCurrency(tax)}</span>
                  </div>
                  <Separator />
                  <div className="flex justify-between font-semibold text-lg">
                    <span>Total:</span>
                    <span>{formatCurrency(total)}</span>
                  </div>
                </div>
                
                <div className="space-y-3">
                  <div className="space-y-2">
                    <Label htmlFor="paymentMethod">Payment Method</Label>
                    <Select
                      value={paymentMethod}
                      onValueChange={(value) => setPaymentMethod(value as any)}
                    >
                      <SelectTrigger className="hover:bg-[#F4E9B1] transition-colors focus:ring-2 focus:ring-[#D4AF37] focus:border-[#D4AF37]">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent className="bg-white border-gray-200 shadow-lg">
                        {paymentMethodsData ? (
                          paymentMethodsData.map((method: EnumLookupDto) => (
                            <SelectItem 
                              key={method.value} 
                              value={method.name}
                              className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]"
                            >
                              {method.displayName}
                            </SelectItem>
                          ))
                        ) : (
                          // Fallback options if API data not loaded
                          <>
                            <SelectItem value="Cash" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Cash</SelectItem>
                            <SelectItem value="Card" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Card</SelectItem>
                            <SelectItem value="BankTransfer" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Bank Transfer</SelectItem>
                            <SelectItem value="Cheque" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Cheque</SelectItem>
                          </>
                        )}
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="amountPaid">Amount Paid</Label>
                    <Input
                      id="amountPaid"
                      type="number"
                      value={amountPaid || ''}
                      onChange={(e) => setAmountPaid(parseFloat(e.target.value) || 0)}
                      placeholder={formatCurrency(total)}
                      min="0"
                      step="0.01"
                      className="focus:ring-2 focus:ring-[#D4AF37] focus:border-[#D4AF37]"
                    />
                    {amountPaid > 0 && amountPaid !== total && (
                      <p className="text-sm text-muted-foreground">
                        Change: {formatCurrency(Math.max(0, amountPaid - total))}
                      </p>
                    )}
                  </div>
                  
                  <Button
                    className="w-full touch-target"
                    variant="golden"
                    onClick={handleCheckout}
                    disabled={isProcessingTransaction || cart.length === 0 || amountPaid < total}
                  >
                    {isProcessingTransaction ? (
                      <>
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                        Processing...
                      </>
                    ) : (
                      <>
                        <CreditCard className="mr-2 h-4 w-4" />
                        Complete Sale
                      </>
                    )}
                  </Button>
                </div>
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}