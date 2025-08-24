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
  Eye,
  Printer,
} from 'lucide-react';
import { formatCurrency } from './utils/currency';
import { useProducts, useSearchCustomers, useCreateSaleOrder, useGoldRates, useMakingCharges, useKaratTypes, useProductCategoryTypes, usePaymentMethods, useTaxConfigurations, useSearchOrders } from '../hooks/useApi';
import { useAuth } from './AuthContext';
import api, { Product, Customer, OrderDto, productOwnershipApi } from '../services/api';
import { LookupHelper, EnumLookupDto } from '../types/lookups';
import { calculateProductPricing, getProductPricingFromAPI } from '../utils/pricing';
// import TransactionDetailsDialog from './TransactionDetailsDialog'; // Temporarily removed
import ReceiptPrinter from './ReceiptPrinter';

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
  const [isProcessingOrder, setIsProcessingOrder] = useState(false);
  const [isCustomerDialogOpen, setIsCustomerDialogOpen] = useState(false);
  
  // Order details dialog state
  const [selectedOrder, setSelectedOrder] = useState<OrderDto | null>(null);
  const [isOrderDetailsOpen, setIsOrderDetailsOpen] = useState(false);
  
  // Today's orders state
  const [todayOrdersPage, setTodayOrdersPage] = useState(1);
  const [todayOrdersPageSize] = useState(10);
  
  // Receipt printer state
  const [isReceiptPrinterOpen, setIsReceiptPrinterOpen] = useState(false);
  const [selectedOrderForReceipt, setSelectedOrderForReceipt] = useState<number | null>(null);
  
  // Ownership validation state
  const [ownershipValidationErrors, setOwnershipValidationErrors] = useState<{[key: number]: string}>({});
  const [isValidatingOwnership, setIsValidatingOwnership] = useState(false);
  
  // API hooks
  const { data: productsData, loading: productsLoading, execute: fetchProducts } = useProducts();
  const { data: searchResults, loading: customersLoading, execute: searchCustomers } = useSearchCustomers();
  const { execute: createSaleOrder } = useCreateSaleOrder();
  const { data: goldRatesData, loading: goldRatesLoading, fetchRates } = useGoldRates();
  const { data: makingChargesData, loading: makingChargesLoading, fetchCharges } = useMakingCharges();
  const { data: karatTypesData, fetchKaratTypes } = useKaratTypes();
  const { data: categoryTypesData, fetchCategories: fetchCategoryTypes } = useProductCategoryTypes();
  const { data: paymentMethodsData, execute: fetchPaymentMethods } = usePaymentMethods();
  const { data: taxConfigurationsData, fetchTaxConfigurations } = useTaxConfigurations();
  const { data: ordersData, loading: ordersLoading, execute: fetchOrders } = useSearchOrders();

  // Check if all critical pricing data is loaded
  const isPricingDataReady = !goldRatesLoading && !makingChargesLoading && 
    goldRatesData && goldRatesData.length > 0 && 
    makingChargesData && makingChargesData.length > 0;

  // Fetch data on component mount
  useEffect(() => {
    fetchProducts({ 
      searchTerm: searchQuery,
      isActive: true,
      pageNumber: 1,
      pageSize: 50
    });
    fetchKaratTypes();
    fetchCategoryTypes();
    fetchPaymentMethods();
    fetchTaxConfigurations();
    fetchRates();
    fetchCharges();
  }, [searchQuery, fetchProducts, fetchKaratTypes, fetchPaymentMethods, fetchTaxConfigurations, fetchRates, fetchCharges]);

  // Ensure all pricing data is loaded before allowing interactions
  useEffect(() => {
    if (goldRatesLoading || makingChargesLoading) {
      return; // Still loading
    }
    
    if (!goldRatesData || !makingChargesData || goldRatesData.length === 0 || makingChargesData.length === 0) {
      console.log('Pricing data not yet available, waiting for configuration...');
      return;
    }
    
    console.log('Pricing data loaded successfully:', {
      goldRatesCount: goldRatesData.length,
      makingChargesCount: makingChargesData.length
    });
  }, [goldRatesData, makingChargesData, goldRatesLoading, makingChargesLoading]);

  useEffect(() => {
    if (customerSearch.trim()) {
      searchCustomers({ 
        searchTerm: customerSearch,
        limit: 20
      });
    }
  }, [customerSearch, searchCustomers]);

  // Fetch today's orders
  useEffect(() => {
    if (user?.branch?.id) {
      // Get today's date in local timezone - match the Dashboard logic exactly
      const today = new Date().getFullYear() + '-' + 
        String(new Date().getMonth() + 1).padStart(2, '0') + '-' + 
        String(new Date().getDate()).padStart(2, '0');
      
      console.log('Fetching orders for today:', today);
      
      fetchOrders({
        branchId: user.branch.id,
        orderTypeId: 1, // Sale order type ID (assuming 1 is for sales)
        fromDate: today, // Use today only, not yesterday
        toDate: today,   // Use today only, not yesterday
        page: todayOrdersPage,
        pageSize: todayOrdersPageSize
      });
    }
  }, [user?.branch?.id, todayOrdersPage, todayOrdersPageSize, fetchOrders]);

  // Recalculate cart pricing when customer changes
  useEffect(() => {
    if (cart.length > 0 && isPricingDataReady) {
      recalculateCartPricing(selectedCustomer);
    }
  }, [selectedCustomer, isPricingDataReady]);

  const products = productsData?.items || [];
  const customers = searchResults || [];

  const filteredProducts = products.filter(product =>
    product.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    LookupHelper.getDisplayName(categoryTypesData || [], product.categoryTypeId).toLowerCase().includes(searchQuery.toLowerCase())
  );

  const validateOwnership = async (productId: number, quantity: number): Promise<boolean> => {
    if (!user?.branch?.id) return true; // Skip validation if no branch info
    
    try {
      setIsValidatingOwnership(true);
      const validation = await productOwnershipApi.validateOwnership({
        productId,
        branchId: user.branch.id,
        requestedQuantity: quantity
      });
      
      if (!validation.canSell) {
        setOwnershipValidationErrors(prev => ({
          ...prev,
          [productId]: validation.message
        }));
        return false;
      }
      
      // Clear any previous errors for this product
      setOwnershipValidationErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors[productId];
        return newErrors;
      });
      
      return true;
    } catch (error) {
      console.error('Ownership validation failed:', error);
      setOwnershipValidationErrors(prev => ({
        ...prev,
        [productId]: 'Failed to validate ownership'
      }));
      return false;
    } finally {
      setIsValidatingOwnership(false);
    }
  };

  const addToCart = async (product: Product) => {
    // Ensure all required data is loaded before proceeding
    if (!isPricingDataReady) {
      console.warn('Pricing data not fully loaded, cannot add to cart');
      return;
    }

    // Validate ownership before adding to cart
    const newQuantity = (cart.find(item => item.productId === product.id)?.quantity || 0) + 1;
    const isValid = await validateOwnership(product.id, newQuantity);
    if (!isValid) {
      return; // Don't add to cart if ownership validation fails
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
          category: LookupHelper.getDisplayName(categoryTypesData || [], product.categoryTypeId),
          karat: LookupHelper.getDisplayName(karatTypesData || [], product.karatTypeId),
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
        if (!isPricingDataReady) {
          alert('Pricing data not available. Please try again.');
          return;
        }
        
        const pricing = calculateProductPricing(product, goldRatesData!, makingChargesData!, 1, selectedCustomer, taxConfigurationsData || []);
        
        // Check if pricing calculation returned valid values
        if (pricing.goldRate === 0) {
          alert('Unable to calculate pricing for this product. Please check pricing configuration.');
          return;
        }
        
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
            category: LookupHelper.getDisplayName(categoryTypesData || [], product.categoryTypeId),
            karat: LookupHelper.getDisplayName(karatTypesData || [], product.karatTypeId),
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

    // Validate ownership for new quantity
    const isValid = await validateOwnership(cartItem.productId, quantity);
    if (!isValid) {
      return; // Don't update quantity if ownership validation fails
    }

    // Ensure pricing data is ready before recalculating
    if (!isPricingDataReady) {
      console.warn('Pricing data not ready, cannot update quantity');
      return;
    }

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

    // Ensure pricing data is ready before recalculating
    if (!isPricingDataReady) {
      console.warn('Pricing data not ready, cannot update discount');
      return;
    }

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

    // Ensure pricing data is ready before recalculating
    if (!isPricingDataReady) {
      console.warn('Pricing data not ready, cannot recalculate cart pricing');
      return;
    }

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

      // Ensure pricing data is ready before calculating bill summary
      if (!isPricingDataReady) {
        console.log('Pricing data not ready, skipping bill summary calculation');
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
  }, [cart, selectedCustomer, isPricingDataReady]);

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
      paymentMethod: LookupHelper.getValue(paymentMethodsData || [], paymentMethod) || 1,
    };

    try {
      // Use the API service instead of direct fetch
      const result = await api.financialTransactions.debugCalculation(saleRequest);
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

    // Validate ownership for all items in cart before checkout
    try {
      setIsValidatingOwnership(true);
      const validationPromises = cart.map(item => 
        validateOwnership(item.productId, item.quantity)
      );
      
      const validationResults = await Promise.all(validationPromises);
      const hasValidationErrors = validationResults.some(result => !result);
      
      if (hasValidationErrors) {
        alert('Some items cannot be sold due to ownership restrictions. Please check the items in your cart.');
        return;
      }
    } catch (error) {
      console.error('Ownership validation failed during checkout:', error);
      alert('Failed to validate ownership. Please try again.');
      return;
    } finally {
      setIsValidatingOwnership(false);
    }

    try {
      setIsProcessingOrder(true);
      
      const saleRequest = {
        branchId: user.branch.id,
        customerId: selectedCustomer?.id,
        items: cart.map(item => ({
          productId: item.productId,
          quantity: item.quantity,
          customDiscountPercentage: item.discountPercentage || undefined,
        })),
        amountPaid: amountPaid,
        paymentMethodId: LookupHelper.getValue(paymentMethodsData || [], paymentMethod) || 1,
      };

      const order = await createSaleOrder(saleRequest);
      
      // Update ownership after successful sale
      try {
        const branchId = user?.branch?.id;
        if (branchId) {
                  const ownershipUpdatePromises = cart.map(item => 
          productOwnershipApi.updateOwnershipAfterSale({
            productId: item.productId,
            branchId: branchId,
            soldQuantity: item.quantity,
            referenceNumber: order.orderNumber
          })
        );
          
          await Promise.all(ownershipUpdatePromises);
          console.log('Ownership updated successfully after sale');
        }
      } catch (error) {
        console.error('Failed to update ownership after sale:', error);
        // Don't fail the transaction, but log the error
      }
      
      alert(`Sale completed successfully! Order Number: ${order.orderNumber}`);
      
      // Clear cart and reset form
      setCart([]);
      setSelectedCustomer(null);
      setAmountPaid(0);
      setCustomerSearch('');
      setSearchQuery('');
      setOwnershipValidationErrors({}); // Clear any ownership errors
      
      // Refresh today's orders list to show the new order
      if (user?.branch?.id) {
        // Get today's date in local timezone - match the refresh button logic exactly
        const today = new Date().getFullYear() + '-' + 
          String(new Date().getMonth() + 1).padStart(2, '0') + '-' + 
          String(new Date().getDate()).padStart(2, '0');
        
        fetchOrders({
          branchId: user.branch.id,
          orderTypeId: 1, // Sale order type ID
          fromDate: today, // Use today only, not yesterday
          toDate: today,   // Use today only, not yesterday
          page: todayOrdersPage,
          pageSize: todayOrdersPageSize
        });
      }
      
    } catch (error) {
      console.error('Order creation failed:', error);
      alert(error instanceof Error ? error.message : 'Order creation failed. Please try again.');
    } finally {
      setIsProcessingOrder(false);
    }
  };

  const handleViewOrderDetails = async (order: any) => {
    try {
      // Get full order details from the orders API
      const fullOrder = await api.orders.getOrder(order.id);
      
      setSelectedOrder(fullOrder);
      setIsOrderDetailsOpen(true);
    } catch (error) {
      console.error('Failed to fetch order details:', error);
      alert('Failed to load order details. Please try again.');
    }
  };

  const handlePrintOrder = async (orderId: number) => {
    setSelectedOrderForReceipt(orderId);
    setIsReceiptPrinterOpen(true);
  };

  const handleGenerateReceipt = async (orderId: number) => {
    try {
      // For orders, we might need to get the associated financial transaction ID
      // or create a new receipt generation endpoint for orders
      const order = await api.orders.getOrder(orderId);
      if (order.financialTransactionId) {
        const result = await api.financialTransactions.generateBrowserReceipt(order.financialTransactionId);
        return result;
      } else {
        throw new Error('No financial transaction associated with this order');
      }
    } catch (error) {
      console.error('Error generating receipt:', error);
      throw error;
    }
  };

  return (
    <div className="space-y-6">
      
      {/* Show loading state when pricing data is not ready */}
      {!isPricingDataReady && (
        <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
          <div className="flex items-center">
            <Loader2 className="h-5 w-5 animate-spin text-yellow-600 mr-3" />
            <div>
              <p className="text-yellow-800 font-medium">Loading Pricing Configuration</p>
              <p className="text-yellow-700 text-sm">
                {goldRatesLoading || makingChargesLoading 
                  ? 'Loading gold rates and making charges...' 
                  : 'Waiting for pricing data to be configured...'
                }
              </p>
            </div>
          </div>
        </div>
      )}
      
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
                    customers.map((customer: Customer) => (
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
              ) : !isPricingDataReady ? (
                <div className="text-center p-8">
                  <Loader2 className="h-8 w-8 animate-spin mx-auto mb-4" />
                  <p className="text-muted-foreground">
                    {goldRatesLoading || makingChargesLoading 
                      ? 'Loading pricing data...' 
                      : 'Loading pricing configuration...'
                    }
                  </p>
                </div>
              ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {filteredProducts.map((product) => {
                    // All data is now guaranteed to be loaded, safe to calculate pricing
                    let goldRate = 0;
                    let makingCharges = 0;
                    let totalPrice = 0;
                    
                    // Only calculate pricing when all data is ready
                    if (isPricingDataReady && goldRatesData && makingChargesData) {
                      try {
                        const pricing = calculateProductPricing(product, goldRatesData, makingChargesData, 1, selectedCustomer, taxConfigurationsData || []);
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
                    }
                    
                    return (
                      <div
                        key={product.id}
                        className={`p-4 border rounded-lg hover:shadow-md transition-shadow ${
                          ownershipValidationErrors[product.id] ? 'border-red-200 bg-red-50' : ''
                        }`}
                      >
                        <div className="flex items-center justify-between mb-2">
                          <h3 className="font-medium">{product.name}</h3>
                          <Badge variant="outline">{LookupHelper.getDisplayName(karatTypesData || [], product.karatTypeId)}</Badge>
                        </div>
                        <div className="space-y-1 text-sm text-muted-foreground mb-3">
                          <p>Weight: {product.weight}g</p>
                          <p>Rate: {goldRate > 0 ? formatCurrency(goldRate) : 'Loading...'}/g</p>
                          <p>Making: {makingCharges > 0 ? formatCurrency(makingCharges) : 'Loading...'}</p>
                          <p>Code: {product.productCode}</p>
                        </div>
                        {ownershipValidationErrors[product.id] && (
                          <div className="mb-3 p-2 bg-red-100 border border-red-200 rounded text-xs text-red-700">
                            <p className="font-medium">Ownership Error:</p>
                            <p>{ownershipValidationErrors[product.id]}</p>
                          </div>
                        )}
                        <div className="flex items-center justify-between">
                          <p className="font-semibold">
                            {totalPrice > 0 ? formatCurrency(totalPrice) : 'Calculating...'}
                          </p>
                          <Button
                            size="sm"
                            variant="golden"
                            onClick={() => addToCart(product)}
                            disabled={!product.isActive || !isPricingDataReady || goldRate === 0 || isValidatingOwnership}
                            className="touch-target"
                          >
                            {isValidatingOwnership ? (
                              <Loader2 className="h-4 w-4 animate-spin" />
                            ) : (
                              <Plus className="h-4 w-4" />
                            )}
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
                    <div key={item.id} className={`p-3 border rounded-lg ${
                      ownershipValidationErrors[item.productId] ? 'border-red-200 bg-red-50' : ''
                    }`}>
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
                      {ownershipValidationErrors[item.productId] && (
                        <div className="mb-2 p-2 bg-red-100 border border-red-200 rounded text-xs text-red-700">
                          <p className="font-medium">Ownership Error:</p>
                          <p>{ownershipValidationErrors[item.productId]}</p>
                        </div>
                      )}
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
                          paymentMethodsData.map((method: any) => (
                            <SelectItem 
                              key={method.id} 
                              value={method.name}
                              className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]"
                            >
                              {method.name}
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
                      disabled={isProcessingOrder || isValidatingOwnership || cart.length === 0 || amountPaid < total || !isPricingDataReady}
                    >
                      {isProcessingOrder || isValidatingOwnership ? (
                        <>
                          <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                          {isValidatingOwnership ? 'Validating Ownership...' : 'Processing...'}
                        </>
                      ) : (
                        <>
                          <CreditCard className="mr-2 h-4 w-4" />
                          Complete Sale
                        </>
                      )}
                    </Button>
                    {!isPricingDataReady && (
                      <p className="text-xs text-yellow-600 text-center">
                        Checkout disabled until pricing configuration is loaded
                      </p>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          )}
        </div>
      </div>

      {/* Today's Orders Section */}
      <Card className="pos-card">
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="flex items-center gap-2">
                <Calendar className="h-5 w-5" />
                Today's Sales Orders
              </CardTitle>
              <CardDescription>
                Showing sales orders for {user?.branch?.name || 'your branch'} (today)
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
                  
                  fetchOrders({
                    branchId: user.branch.id,
                    orderTypeId: 1, // Sale order type ID
                    fromDate: today, // Use today only, not yesterday
                    toDate: today,   // Use today only, not yesterday
                    page: todayOrdersPage,
                    pageSize: todayOrdersPageSize
                  });
                }
              }}
              disabled={ordersLoading}
              className="touch-target hover:bg-[#F4E9B1] transition-colors"
            >
              {ordersLoading ? (
                <Loader2 className="h-4 w-4 animate-spin" />
              ) : (
                <Calendar className="h-4 w-4" />
              )}
              Refresh
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {ordersLoading ? (
            <div className="flex items-center justify-center p-8">
              <Loader2 className="h-6 w-6 animate-spin" />
              <span className="ml-2">Loading today's orders...</span>
            </div>
          ) : ordersData?.items && ordersData.items.length > 0 ? (
            <div className="space-y-4">
              <div className="overflow-x-auto">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Order #</TableHead>
                      <TableHead>Time</TableHead>
                      <TableHead>Customer</TableHead>
                      <TableHead>Cashier</TableHead>
                      <TableHead>Items</TableHead>
                      <TableHead>Total</TableHead>
                      <TableHead>Status</TableHead>
                      <TableHead>Actions</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {ordersData.items.map((order) => (
                      <TableRow key={order.id}>
                        <TableCell className="font-mono text-sm">
                          {order.orderNumber}
                        </TableCell>
                        <TableCell>
                          {new Date(order.orderDate).toLocaleTimeString('en-US', {
                            hour: '2-digit',
                            minute: '2-digit',
                            hour12: true
                          })}
                        </TableCell>
                        <TableCell>
                          {order.customerName || 'Walk-in Customer'}
                        </TableCell>
                        <TableCell>
                          {order.cashierName}
                        </TableCell>
                        <TableCell>
                          {order.items?.length || 0} items
                        </TableCell>
                        <TableCell className="font-semibold">
                          {formatCurrency(order.items?.reduce((sum, item) => sum + item.totalAmount, 0) || 0)}
                        </TableCell>
                        <TableCell>
                          <Badge 
                            variant={order.statusDescription === 'Completed' ? 'default' : 'secondary'}
                            className={order.statusDescription === 'Completed' ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'}
                          >
                            {order.statusDescription}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => handleViewOrderDetails(order as any)}
                              className="h-8 w-8 p-0 hover:bg-blue-50"
                              title="View Details"
                            >
                              <Eye className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => handlePrintOrder(order.id)}
                              className="h-8 w-8 p-0 hover:bg-green-50"
                              title="Print Receipt"
                            >
                              <Printer className="h-4 w-4" />
                            </Button>
                          </div>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>

              {/* Pagination */}
              {(() => {
                const totalPages = Math.ceil(ordersData.totalCount / todayOrdersPageSize);
                return totalPages > 1 ? (
                  <div className="flex items-center justify-between">
                    <div className="text-sm text-muted-foreground">
                      Showing {((todayOrdersPage - 1) * todayOrdersPageSize) + 1} to{' '}
                      {Math.min(todayOrdersPage * todayOrdersPageSize, ordersData.totalCount)} of{' '}
                      {ordersData.totalCount} orders
                    </div>
                    <div className="flex items-center gap-2">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => setTodayOrdersPage(prev => Math.max(1, prev - 1))}
                        disabled={todayOrdersPage === 1}
                        className="touch-target hover:bg-[#F4E9B1] transition-colors"
                      >
                        <ChevronLeft className="h-4 w-4" />
                        Previous
                      </Button>
                      <span className="text-sm font-medium">
                        Page {todayOrdersPage} of {totalPages}
                      </span>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => setTodayOrdersPage(prev => Math.min(totalPages, prev + 1))}
                        disabled={todayOrdersPage === totalPages}
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
                <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-center">
                  <div>
                    <p className="text-sm text-muted-foreground">Total Orders</p>
                    <p className="text-2xl font-bold text-[#0D1B2A]">{ordersData.totalCount}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Total Sales</p>
                    <p className="text-2xl font-bold text-green-600">
                      {formatCurrency(ordersData.items.reduce((sum, order) => {
                        const orderTotal = order.items?.reduce((itemSum, item) => itemSum + item.totalAmount, 0) || 0;
                        return sum + orderTotal;
                      }, 0))}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Average Sale</p>
                    <p className="text-2xl font-bold text-purple-600">
                      {ordersData.totalCount > 0 ? (
                        (() => {
                          const totalSales = ordersData.items.reduce((sum, order) => {
                            const orderTotal = order.items?.reduce((itemSum, item) => itemSum + item.totalAmount, 0) || 0;
                            return sum + orderTotal;
                          }, 0);
                          return formatCurrency(totalSales / ordersData.totalCount);
                        })()
                      ) : formatCurrency(0)}
                    </p>
                  </div>
                </div>
              </div>
            </div>
          ) : (
            <div className="text-center p-8 text-muted-foreground">
              <Receipt className="h-12 w-12 mx-auto mb-4 text-gray-400" />
              <p className="text-lg font-medium">No sales orders today</p>
              <p className="text-sm">Start making sales to see them appear here</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Order Details Dialog */}
      {/* TODO: Create OrderDetailsDialog component or adapt TransactionDetailsDialog for orders */}
      {selectedOrder && (
        <Dialog open={isOrderDetailsOpen} onOpenChange={setIsOrderDetailsOpen}>
          <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
            <DialogHeader>
              <DialogTitle>Order Details - {selectedOrder.orderNumber}</DialogTitle>
            </DialogHeader>
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p><strong>Customer:</strong> {selectedOrder.customerName || 'Walk-in Customer'}</p>
                  <p><strong>Date:</strong> {new Date(selectedOrder.orderDate).toLocaleString()}</p>
                  <p><strong>Status:</strong> {selectedOrder.statusDescription}</p>
                </div>
                <div>
                  <p><strong>Cashier:</strong> {selectedOrder.cashierName}</p>
                  <p><strong>Branch ID:</strong> {selectedOrder.branchId}</p>
                </div>
              </div>
              
              {selectedOrder.items && selectedOrder.items.length > 0 && (
                <div>
                  <h3 className="font-semibold mb-2">Items</h3>
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Product</TableHead>
                        <TableHead>Quantity</TableHead>
                        <TableHead>Unit Price</TableHead>
                        <TableHead>Total</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {selectedOrder.items.map((item) => (
                        <TableRow key={item.id}>
                          <TableCell>{item.productName}</TableCell>
                          <TableCell>{item.quantity}</TableCell>
                          <TableCell>{formatCurrency(item.unitPrice)}</TableCell>
                          <TableCell>{formatCurrency(item.totalAmount)}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              )}
              
              <div className="flex justify-end gap-2">
                <Button
                  variant="outline"
                  onClick={() => handlePrintOrder(selectedOrder.id)}
                >
                  <Printer className="h-4 w-4 mr-2" />
                  Print Receipt
                </Button>
              </div>
            </div>
          </DialogContent>
        </Dialog>
      )}

      {/* Receipt Printer Dialog */}
      <ReceiptPrinter
        isOpen={isReceiptPrinterOpen}
        onClose={() => {
          setIsReceiptPrinterOpen(false);
          setSelectedOrderForReceipt(null);
        }}
        transactionId={selectedOrderForReceipt || undefined}
        onGenerateReceipt={handleGenerateReceipt}
      />
    </div>
  );
}