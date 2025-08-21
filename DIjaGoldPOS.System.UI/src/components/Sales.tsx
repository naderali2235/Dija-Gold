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
  Calendar,
  Receipt,
  ChevronLeft,
  ChevronRight,
} from 'lucide-react';
import { formatCurrency } from './utils/currency';
import { useProducts, useCustomers, useProcessSale, useGoldRates, useMakingCharges, useKaratTypes, usePaymentMethods, useTaxConfigurations, useSearchTransactions } from '../hooks/useApi';
import { useAuth } from './AuthContext';
import api, { Product, Customer, Transaction } from '../services/api';
import { EnumMapper, EnumLookupDto, ProductCategoryType, KaratType } from '../types/enums';
import { calculateProductPricing, getProductPricingFromAPI } from '../utils/pricing';

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
  
  // Today's transactions state
  const [todayTransactionsPage, setTodayTransactionsPage] = useState(1);
  const [todayTransactionsPageSize] = useState(10);
  
  // API hooks
  const { data: productsData, loading: productsLoading, execute: fetchProducts } = useProducts();
  const { data: customersData, loading: customersLoading, execute: fetchCustomers } = useCustomers();
  const { execute: processSale } = useProcessSale();
  const { data: goldRatesData, loading: goldRatesLoading, fetchRates } = useGoldRates();
  const { data: makingChargesData, loading: makingChargesLoading, fetchCharges } = useMakingCharges();
  const { data: karatTypesData, fetchKaratTypes } = useKaratTypes();
  const { data: paymentMethodsData, execute: fetchPaymentMethods } = usePaymentMethods();
  const { data: taxConfigurationsData, fetchTaxConfigurations } = useTaxConfigurations();
  const { data: transactionsData, loading: transactionsLoading, execute: fetchTransactions } = useSearchTransactions();

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
    fetchTaxConfigurations();
    fetchRates();
    fetchCharges();
  }, [searchQuery, fetchProducts, fetchKaratTypes, fetchPaymentMethods, fetchTaxConfigurations, fetchRates, fetchCharges]);

  useEffect(() => {
    if (customerSearch.trim()) {
      fetchCustomers({ 
        searchTerm: customerSearch,
        pageNumber: 1,
        pageSize: 20
      });
    }
  }, [customerSearch, fetchCustomers]);

  // Fetch today's transactions
  useEffect(() => {
    if (user?.branch?.id) {
      // Get today's date in local timezone - match the Dashboard logic exactly
      const today = new Date().getFullYear() + '-' + 
        String(new Date().getMonth() + 1).padStart(2, '0') + '-' + 
        String(new Date().getDate()).padStart(2, '0');
      
      console.log('Fetching transactions for today:', today);
      
      fetchTransactions({
        branchId: user.branch.id,
        transactionType: 'Sale',
        fromDate: today, // Use today only, not yesterday
        toDate: today,   // Use today only, not yesterday
        pageNumber: todayTransactionsPage,
        pageSize: todayTransactionsPageSize
      });
    }
  }, [user?.branch?.id, todayTransactionsPage, todayTransactionsPageSize, fetchTransactions]);

  // Recalculate cart pricing when customer changes
  useEffect(() => {
    if (cart.length > 0) {
      recalculateCartPricing(selectedCustomer);
    }
  }, [selectedCustomer]);

  const products = productsData?.items || [];
  const customers = customersData?.items || [];

  const filteredProducts = products.filter(product =>
    product.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    EnumMapper.productCategoryEnumToString(product.categoryType as ProductCategoryType).toLowerCase().includes(searchQuery.toLowerCase())
  );

  const addToCart = async (product: Product) => {
    // Ensure all required data is loaded before proceeding
    if (!goldRatesData || !makingChargesData || goldRatesLoading || makingChargesLoading) {
      console.warn('Pricing data not fully loaded, cannot add to cart');
      return;
    }

    const existingItem = cart.find(item => item.productId === product.id);
    
    try {
      // Use backend API for accurate pricing calculation
      const pricingData = await getProductPricingFromAPI(product.id, 1, selectedCustomer?.id);
      const unitPrice = pricingData.estimatedTotalPrice;
      const makingCharges = pricingData.estimatedMakingCharges;
      const goldRate = pricingData.currentGoldRate;
    
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
          category: EnumMapper.productCategoryEnumToString(product.categoryType as ProductCategoryType),
          karat: EnumMapper.karatEnumToString(product.karatType as KaratType),
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
    } catch (error) {
      console.error('Error calculating price for product:', product.id, error);
      // Fallback to local calculation if API fails
      try {
        if (!goldRatesData || !makingChargesData) {
          alert('Pricing data not available. Please try again.');
          return;
        }
        
        const pricing = calculateProductPricing(product, goldRatesData, makingChargesData, 1, selectedCustomer, taxConfigurationsData || []);
        const unitPrice = pricing.finalTotal;
        
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
            id: Date.now(),
            productId: product.id,
            name: product.name,
            category: EnumMapper.productCategoryEnumToString(product.categoryType as ProductCategoryType),
            karat: EnumMapper.karatEnumToString(product.karatType as KaratType),
            weight: product.weight,
            rate: pricing.goldRate,
            makingCharges: pricing.makingChargesAmount,
            quantity: 1,
            unitPrice: unitPrice,
            discountPercentage: 0,
            total: unitPrice,
          };
          setCart([...cart, newItem]);
        }
      } catch (fallbackError) {
        console.error('Fallback calculation failed:', fallbackError);
        alert('Error calculating product price. Please try again.');
      }
    }
  };

  const updateQuantity = async (cartItemId: number, quantity: number) => {
    if (quantity <= 0) {
      setCart(cart.filter(item => item.id !== cartItemId));
      return;
    }

    const cartItem = cart.find(item => item.id === cartItemId);
    if (!cartItem) return;

    try {
      // Recalculate pricing with new quantity using backend API
      const pricingData = await getProductPricingFromAPI(cartItem.productId, quantity, selectedCustomer?.id);
      const newUnitPrice = pricingData.estimatedTotalPrice / quantity; // Calculate unit price from total
      const newTotal = pricingData.estimatedTotalPrice * (1 - cartItem.discountPercentage / 100);

      setCart(cart.map(item =>
        item.id === cartItemId
          ? { 
              ...item, 
              quantity, 
              unitPrice: newUnitPrice,
              makingCharges: pricingData.estimatedMakingCharges,
              rate: pricingData.currentGoldRate,
              total: newTotal
            }
          : item
      ));
    } catch (error) {
      console.error('Error recalculating price for quantity change:', error);
      // Fallback to simple multiplication
      setCart(cart.map(item =>
        item.id === cartItemId
          ? { ...item, quantity, total: quantity * item.unitPrice * (1 - item.discountPercentage / 100) }
          : item
      ));
    }
  };

  const updateDiscount = async (cartItemId: number, discountPercentage: number) => {
    const cartItem = cart.find(item => item.id === cartItemId);
    if (!cartItem) return;

    const validDiscountPercentage = Math.max(0, Math.min(100, discountPercentage));

    try {
      // Recalculate pricing with backend API to get base price without discount
      const pricingData = await getProductPricingFromAPI(cartItem.productId, cartItem.quantity, selectedCustomer?.id);
      const baseTotal = pricingData.estimatedTotalPrice;
      const newTotal = baseTotal * (1 - validDiscountPercentage / 100);

      setCart(cart.map(item => {
        if (item.id === cartItemId) {
          return {
            ...item,
            discountPercentage: validDiscountPercentage,
            total: newTotal
          };
        }
        return item;
      }));
    } catch (error) {
      console.error('Error recalculating price for discount change:', error);
      // Fallback to simple calculation
      setCart(cart.map(item => {
        if (item.id === cartItemId) {
          return {
            ...item,
            discountPercentage: validDiscountPercentage,
            total: item.quantity * item.unitPrice * (1 - validDiscountPercentage / 100)
          };
        }
        return item;
      }));
    }
  };

  const removeFromCart = (cartItemId: number) => {
    setCart(cart.filter(item => item.id !== cartItemId));
  };

  // Recalculate all cart items when customer changes
  const recalculateCartPricing = async (newCustomer: Customer | null) => {
    if (cart.length === 0) return;

    try {
      const updatedCart = await Promise.all(
        cart.map(async (item) => {
          try {
            const pricingData = await getProductPricingFromAPI(item.productId, item.quantity, newCustomer?.id);
            const newUnitPrice = pricingData.estimatedTotalPrice / item.quantity;
            const newTotal = pricingData.estimatedTotalPrice * (1 - item.discountPercentage / 100);

            return {
              ...item,
              unitPrice: newUnitPrice,
              makingCharges: pricingData.estimatedMakingCharges,
              rate: pricingData.currentGoldRate,
              total: newTotal
            };
          } catch (error) {
            console.error(`Error recalculating price for item ${item.productId}:`, error);
            return item; // Keep original item if pricing fails
          }
        })
      );

      setCart(updatedCart);
    } catch (error) {
      console.error('Error recalculating cart pricing:', error);
    }
  };

  // Calculate bill summary using backend API for each item to ensure accuracy
  const [billSummary, setBillSummary] = useState({
    subtotal: 0,
    totalMakingCharges: 0,
    totalDiscountAmount: 0,
    totalTax: 0,
    total: 0
  });

  // Recalculate bill summary whenever cart changes
  useEffect(() => {
    const calculateBillSummary = async () => {
      if (cart.length === 0) {
        setBillSummary({
          subtotal: 0,
          totalMakingCharges: 0,
          totalDiscountAmount: 0,
          totalTax: 0,
          total: 0
        });
        return;
      }

      try {
        let subtotal = 0;
        let totalMakingCharges = 0;
        let totalDiscountAmount = 0;
        let totalTax = 0;

        console.log('=== FRONTEND CALCULATION DEBUG (API-based) ===');
        console.log('Processing', cart.length, 'items');

        // Calculate each item using backend API
        for (const item of cart) {
          try {
            console.log(`Product ${item.productId} (${item.name}) - Quantity: ${item.quantity}`);
            
            // Get backend pricing for this item (without custom discount)
            const backendPricing = await getProductPricingFromAPI(item.productId, item.quantity, selectedCustomer?.id);
            
            // Extract components from backend response
            const itemGoldValue = backendPricing.estimatedBasePrice;
            const itemMakingCharges = backendPricing.estimatedMakingCharges;
            const itemTotalBeforeCustomDiscount = backendPricing.estimatedTotalPrice;
            
            // Calculate custom discount on the backend total
            let itemCustomDiscount = 0;
            let itemFinalTotal = itemTotalBeforeCustomDiscount;
            if (item.discountPercentage && item.discountPercentage > 0) {
              itemCustomDiscount = itemTotalBeforeCustomDiscount * (item.discountPercentage / 100);
              itemFinalTotal = itemTotalBeforeCustomDiscount - itemCustomDiscount;
            }
            
            // Calculate tax on final amount (this should match what's in the backend total)
            const itemTax = itemTotalBeforeCustomDiscount - itemGoldValue - itemMakingCharges;
            
            console.log(`  Backend Gold Value: ${itemGoldValue.toFixed(2)}, Making Charges: ${itemMakingCharges.toFixed(2)}`);
            console.log(`  Backend Total: ${itemTotalBeforeCustomDiscount.toFixed(2)}, Custom Discount: ${itemCustomDiscount.toFixed(2)}`);
            console.log(`  Calculated Tax: ${itemTax.toFixed(2)}, Final Total: ${itemFinalTotal.toFixed(2)}`);
            
            // Accumulate totals
            subtotal += itemGoldValue;
            totalMakingCharges += itemMakingCharges;
            totalDiscountAmount += itemCustomDiscount;
            totalTax += itemTax;
            
          } catch (error) {
            console.error(`Error calculating pricing for item ${item.productId}:`, error);
            // Fallback to cart item values if API fails
            subtotal += item.total;
          }
        }

        const total = subtotal + totalMakingCharges + totalTax - totalDiscountAmount;
        
        console.log('=== FRONTEND TOTALS (API-based) ===');
        console.log('Subtotal (Gold Value):', subtotal.toFixed(2));
        console.log('Total Making Charges:', totalMakingCharges.toFixed(2));
        console.log('Total Discount:', totalDiscountAmount.toFixed(2));
        console.log('Total Tax:', totalTax.toFixed(2));
        console.log('FINAL TOTAL:', total.toFixed(2));
        console.log('=== END FRONTEND DEBUG ===');

        setBillSummary({
          subtotal,
          totalMakingCharges,
          totalDiscountAmount,
          totalTax,
          total
        });
        
      } catch (error) {
        console.error('Error calculating bill summary:', error);
      }
    };

    calculateBillSummary();
  }, [cart, selectedCustomer]);

  // Destructure for easier access
  const { subtotal, totalMakingCharges, totalDiscountAmount, totalTax, total } = billSummary;

  const debugCalculation = async () => {
    if (!user?.branch?.id) {
      alert('Branch information not available');
      return;
    }

    const saleRequest = {
      branchId: user.branch.id,
      customerId: selectedCustomer?.id,
      items: cart.map(item => ({
        productId: item.productId,
        quantity: item.quantity,
        customDiscountPercentage: item.discountPercentage || undefined,
      })),
      amountPaid: amountPaid,
      paymentMethod: EnumMapper.paymentMethodStringToEnum(paymentMethod),
    };

    try {
      const response = await fetch('/api/transactions/debug-calculation', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(saleRequest)
      });

      const result = await response.json();
      console.log('=== BACKEND DEBUG RESULT ===');
      console.log(result);
      console.log('=== END BACKEND DEBUG ===');
    } catch (error) {
      console.error('Error calling debug endpoint:', error);
    }
  };

  const handleCheckout = async () => {
    console.log('User data:', user);
    console.log('User branch:', user?.branch);
    
    if (cart.length === 0) {
      alert('Please add items to cart before checkout');
      return;
    }

    if (!user?.branch?.id) {
      console.error('Branch information missing. User:', user);
      alert('Branch information not available. Please contact your administrator to assign you to a branch.');
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
        paymentMethod: EnumMapper.paymentMethodStringToEnum(paymentMethod),
      };

      const transaction = await processSale(saleRequest);
      
      alert(`Sale completed successfully! Transaction Number: ${transaction.transactionNumber}`);
      
      // Clear cart and reset form
      setCart([]);
      setSelectedCustomer(null);
      setAmountPaid(0);
      setCustomerSearch('');
      setSearchQuery('');
      
      // Refresh today's transactions list to show the new transaction
      if (user?.branch?.id) {
        // Get today's date in local timezone - match the refresh button logic exactly
        const today = new Date().getFullYear() + '-' + 
          String(new Date().getMonth() + 1).padStart(2, '0') + '-' + 
          String(new Date().getDate()).padStart(2, '0');
        
        fetchTransactions({
          branchId: user.branch.id,
          transactionType: 'Sale',
          fromDate: today, // Use today only, not yesterday
          toDate: today,   // Use today only, not yesterday
          pageNumber: todayTransactionsPage,
          pageSize: todayTransactionsPageSize
        });
      }
      
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
                <div className="text-center p-8">
                  <Loader2 className="h-8 w-8 animate-spin mx-auto mb-4" />
                  <p className="text-muted-foreground">Loading products...</p>
                </div>
              ) : filteredProducts.length === 0 ? (
                <div className="text-center p-8 text-muted-foreground">
                  {searchQuery ? 'No products found matching your search' : 'No products available'}
                </div>
              ) : (goldRatesLoading || makingChargesLoading) ? (
                <div className="text-center p-8">
                  <Loader2 className="h-8 w-8 animate-spin mx-auto mb-4" />
                  <p className="text-muted-foreground">Loading pricing data...</p>
                </div>
              ) : (!goldRatesData || !makingChargesData) ? (
                <div className="text-center p-8">
                  <Loader2 className="h-8 w-8 animate-spin mx-auto mb-4" />
                  <p className="text-muted-foreground">Loading pricing configuration...</p>
                </div>
              ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {filteredProducts.map((product) => {
                    // All data is now guaranteed to be loaded, safe to calculate pricing
                    let goldRate = 0;
                    let makingCharges = 0;
                    let totalPrice = 0;
                    
                    try {
                      const pricing = calculateProductPricing(product, goldRatesData, makingChargesData, 1, selectedCustomer);
                      goldRate = pricing.goldRate;
                      makingCharges = pricing.makingChargesAmount;
                      totalPrice = pricing.totalPrice;
                    } catch (error) {
                      console.error('Error calculating price for product:', product.id, error);
                      // Set default values if calculation fails
                      goldRate = 0;
                      makingCharges = 0;
                      totalPrice = 0;
                    }
                    
                    return (
                      <div
                        key={product.id}
                        className="p-4 border rounded-lg hover:shadow-md transition-shadow"
                      >
                        <div className="flex items-center justify-between mb-2">
                          <h3 className="font-medium">{product.name}</h3>
                          <Badge variant="outline">{EnumMapper.karatEnumToString(product.karatType as KaratType)}</Badge>
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
                    <span>Subtotal (Gold Value):</span>
                    <span>{formatCurrency(subtotal)}</span>
                  </div>
                  {totalMakingCharges > 0 && (
                    <div className="flex justify-between">
                      <span>Making Charges:</span>
                      <span>{formatCurrency(totalMakingCharges)}</span>
                    </div>
                  )}
                  {totalDiscountAmount > 0 && (
                    <div className="flex justify-between text-green-600">
                      <span>Discount:</span>
                      <span>-{formatCurrency(totalDiscountAmount)}</span>
                    </div>
                  )}
                  <div className="flex justify-between">
                    <span>
                      {taxConfigurationsData && taxConfigurationsData.length > 0 
                        ? taxConfigurationsData
                            .filter(tax => tax.isMandatory && tax.isCurrent)
                            .map(tax => `${tax.taxName} (${tax.taxRate}%)`)
                            .join(', ')
                        : 'Tax'
                      }:
                    </span>
                    <span>{formatCurrency(totalTax)}</span>
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
                  
                  <div className="space-y-2">
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
                </div>
              </CardContent>
            </Card>
          )}
        </div>
      </div>

      {/* Today's Transactions Section */}
      <Card className="pos-card">
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="flex items-center gap-2">
                <Calendar className="h-5 w-5" />
                Today's Sales Transactions
              </CardTitle>
              <CardDescription>
                Showing sales transactions for {user?.branch?.name || 'your branch'} (last 24 hours)
              </CardDescription>
            </div>
            <Button
              variant="outline"
              size="sm"
              onClick={() => {
                if (user?.branch?.id) {
                  // Get today's date in local timezone - match the useEffect logic exactly
                  const today = new Date().getFullYear() + '-' + 
                    String(new Date().getMonth() + 1).padStart(2, '0') + '-' + 
                    String(new Date().getDate()).padStart(2, '0');
                  
                  fetchTransactions({
                    branchId: user.branch.id,
                    transactionType: 'Sale',
                    fromDate: today, // Use today only, not yesterday
                    toDate: today,   // Use today only, not yesterday
                    pageNumber: todayTransactionsPage,
                    pageSize: todayTransactionsPageSize
                  });
                }
              }}
              disabled={transactionsLoading}
              className="touch-target hover:bg-[#F4E9B1] transition-colors"
            >
              {transactionsLoading ? (
                <Loader2 className="h-4 w-4 animate-spin" />
              ) : (
                <Calendar className="h-4 w-4" />
              )}
              Refresh
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {transactionsLoading ? (
            <div className="flex items-center justify-center p-8">
              <Loader2 className="h-6 w-6 animate-spin" />
              <span className="ml-2">Loading today's transactions...</span>
            </div>
          ) : transactionsData?.items && transactionsData.items.length > 0 ? (
            <div className="space-y-4">
              <div className="overflow-x-auto">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Transaction #</TableHead>
                      <TableHead>Time</TableHead>
                      <TableHead>Customer</TableHead>
                      <TableHead>Cashier</TableHead>
                      <TableHead>Items</TableHead>
                      <TableHead>Subtotal</TableHead>
                      <TableHead>Tax</TableHead>
                      <TableHead>Total</TableHead>
                      <TableHead>Paid</TableHead>
                      <TableHead>Change</TableHead>
                      <TableHead>Status</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {transactionsData.items.map((transaction) => (
                      <TableRow key={transaction.id}>
                        <TableCell className="font-mono text-sm">
                          {transaction.transactionNumber}
                        </TableCell>
                        <TableCell>
                          {new Date(transaction.transactionDate).toLocaleTimeString('en-US', {
                            hour: '2-digit',
                            minute: '2-digit',
                            hour12: true
                          })}
                        </TableCell>
                        <TableCell>
                          {transaction.customerName || 'Walk-in Customer'}
                        </TableCell>
                        <TableCell>
                          {transaction.cashierName}
                        </TableCell>
                        <TableCell>
                          {transaction.items?.length || 0} items
                        </TableCell>
                        <TableCell>
                          {formatCurrency(transaction.subtotal)}
                        </TableCell>
                        <TableCell>
                          {formatCurrency(transaction.totalTaxAmount)}
                        </TableCell>
                        <TableCell className="font-semibold">
                          {formatCurrency(transaction.totalAmount)}
                        </TableCell>
                        <TableCell>
                          {formatCurrency(transaction.amountPaid)}
                        </TableCell>
                        <TableCell>
                          {transaction.changeGiven > 0 ? (
                            <span className="text-green-600">
                              +{formatCurrency(transaction.changeGiven)}
                            </span>
                          ) : transaction.changeGiven < 0 ? (
                            <span className="text-red-600">
                              {formatCurrency(Math.abs(transaction.changeGiven))}
                            </span>
                          ) : (
                            <span className="text-gray-500">-</span>
                          )}
                        </TableCell>
                        <TableCell>
                          <Badge 
                            variant={transaction.status === 'Completed' ? 'default' : 'secondary'}
                            className={transaction.status === 'Completed' ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'}
                          >
                            {transaction.statusDisplayName || transaction.status}
                          </Badge>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>

              {/* Pagination */}
              {(() => {
                const totalPages = Math.ceil(transactionsData.totalCount / todayTransactionsPageSize);
                return totalPages > 1 ? (
                  <div className="flex items-center justify-between">
                    <div className="text-sm text-muted-foreground">
                      Showing {((todayTransactionsPage - 1) * todayTransactionsPageSize) + 1} to{' '}
                      {Math.min(todayTransactionsPage * todayTransactionsPageSize, transactionsData.totalCount)} of{' '}
                      {transactionsData.totalCount} transactions
                    </div>
                    <div className="flex items-center gap-2">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => setTodayTransactionsPage(prev => Math.max(1, prev - 1))}
                        disabled={todayTransactionsPage === 1}
                        className="touch-target hover:bg-[#F4E9B1] transition-colors"
                      >
                        <ChevronLeft className="h-4 w-4" />
                        Previous
                      </Button>
                      <span className="text-sm font-medium">
                        Page {todayTransactionsPage} of {totalPages}
                      </span>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => setTodayTransactionsPage(prev => Math.min(totalPages, prev + 1))}
                        disabled={todayTransactionsPage === totalPages}
                        className="touch-target hover:bg-[#F4E9B1] transition-colors"
                      >
                        Next
                        <ChevronRight className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>
                ) : null;
              })()}

              {/* Summary */}
              <div className="bg-gray-50 p-4 rounded-lg">
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-center">
                  <div>
                    <p className="text-sm text-muted-foreground">Total Transactions</p>
                    <p className="text-2xl font-bold text-[#0D1B2A]">{transactionsData.totalCount}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Total Sales</p>
                    <p className="text-2xl font-bold text-green-600">
                      {formatCurrency(transactionsData.items.reduce((sum, t) => sum + t.totalAmount, 0))}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Total Tax</p>
                    <p className="text-2xl font-bold text-blue-600">
                      {formatCurrency(transactionsData.items.reduce((sum, t) => sum + t.totalTaxAmount, 0))}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Average Sale</p>
                    <p className="text-2xl font-bold text-purple-600">
                      {transactionsData.totalCount > 0 
                        ? formatCurrency(transactionsData.items.reduce((sum, t) => sum + t.totalAmount, 0) / transactionsData.totalCount)
                        : formatCurrency(0)
                      }
                    </p>
                  </div>
                </div>
              </div>
            </div>
          ) : (
            <div className="text-center p-8 text-muted-foreground">
              <Receipt className="h-12 w-12 mx-auto mb-4 text-gray-400" />
              <p className="text-lg font-medium">No sales transactions today</p>
              <p className="text-sm">Start making sales to see them appear here</p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}