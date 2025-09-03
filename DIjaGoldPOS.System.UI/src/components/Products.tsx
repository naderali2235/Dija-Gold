import React, { useState, useEffect, useCallback } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Badge } from './ui/badge';
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
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from './ui/dropdown-menu';
import {
  Package,
  Plus,
  Upload,
  Download,
  Search,
  Edit,
  Trash2,
  MoreHorizontal,
  Filter,
  Eye,
  DollarSign,
  Loader2,
  AlertCircle,
} from 'lucide-react';
import { useAuth } from './AuthContext';
import { formatCurrency } from './utils/currency';
import { usePaginatedProducts, useCreateProduct, useUpdateProduct, useDeleteProduct, useKaratTypes, useProductCategoryTypes, useSuppliers, useGoldRates, useMakingCharges } from '../hooks/useApi';
import { lookupsApi } from '../services/api';
import { Product, SupplierDto } from '../services/api';
import { LookupHelper } from '../types/lookups';
import { calculateProductPricing, getProductPricingFromAPI } from '../utils/pricing';
import { parseExcelFile, generateProductsTemplateXlsx, generateProductsTemplateCsv } from '../utils/excel';

// Simple in-memory cache to dedupe pricing calls per product (helps with StrictMode double effects)
const pricingCache = new Map<number, { price: number; ts: number }>();
// Coalesce concurrent requests per product id
const pricingInFlight = new Map<number, Promise<number>>();
const PRICING_TTL_MS = 30_000; // 30 seconds

// Component to handle async price display
function ProductPriceDisplay({ product, goldRatesData, makingChargesData, goldRatesVersion, makingChargesVersion }: { 
  product: Product, 
  goldRatesData: any[], 
  makingChargesData: any[],
  goldRatesVersion: string,
  makingChargesVersion: string,
}) {
  const [price, setPrice] = useState<number | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let mounted = true;
    const fetchPrice = async () => {
      try {
        console.debug('[ProductPriceDisplay] fetch start', { productId: product.id, goldRatesVersion, makingChargesVersion, ts: Date.now() });
        if (!mounted) return;
        setLoading(true);

        // Use cache if fresh
        const cached = pricingCache.get(product.id);
        if (cached && Date.now() - cached.ts < PRICING_TTL_MS) {
          setPrice(cached.price);
          console.debug('[ProductPriceDisplay] cache hit', { productId: product.id, price: cached.price });
          return;
        }

        // Coalesce concurrent requests
        let promise = pricingInFlight.get(product.id);
        if (!promise) {
          promise = getProductPricingFromAPI(product.id, 1).then((p) => p.estimatedTotalPrice);
          pricingInFlight.set(product.id, promise);
        } else {
          console.debug('[ProductPriceDisplay] in-flight reuse', { productId: product.id });
        }
        const apiPrice = await promise.finally(() => pricingInFlight.delete(product.id));
        if (!mounted) return;
        setPrice(apiPrice);
        pricingCache.set(product.id, { price: apiPrice, ts: Date.now() });
        console.debug('[ProductPriceDisplay] fetch success', { productId: product.id, price: apiPrice, ts: Date.now() });
      } catch (error) {
        console.error('Error fetching price from API:', error);
        // Fallback to local calculation
        try {
          if (goldRatesData && makingChargesData) {
            const pricing = calculateProductPricing(product, goldRatesData, makingChargesData, 1, null, []);
            if (!mounted) return;
            setPrice(pricing.finalTotal);
            pricingCache.set(product.id, { price: pricing.finalTotal, ts: Date.now() });
            console.debug('[ProductPriceDisplay] fallback success', { productId: product.id, price: pricing.finalTotal, ts: Date.now() });
          } else {
            if (!mounted) return;
            setPrice(0);
            console.debug('[ProductPriceDisplay] fallback no data -> 0', { productId: product.id, ts: Date.now() });
          }
        } catch (fallbackError) {
          console.error('Fallback calculation failed:', fallbackError);
          if (!mounted) return;
          setPrice(0);
        }
      } finally {
        if (!mounted) return;
        setLoading(false);
        console.debug('[ProductPriceDisplay] fetch end', { productId: product.id, ts: Date.now() });
      }
    };

    fetchPrice();
    return () => {
      mounted = false;
    };
  }, [product.id, goldRatesVersion, makingChargesVersion]);

  if (loading) {
    return <Loader2 className="h-4 w-4 animate-spin" />;
  }

  return <span>{formatCurrency(price || 0)}</span>;
}

// Memoized wrapper to prevent unnecessary rerenders
const MemoProductPriceDisplay = React.memo(
  ProductPriceDisplay,
  (prev, next) =>
    prev.product.id === next.product.id &&
    prev.goldRatesVersion === next.goldRatesVersion &&
    prev.makingChargesVersion === next.makingChargesVersion
);

export default function Products() {
  const { isManager } = useAuth();
  const [searchQuery, setSearchQuery] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('all');
  const [karatFilter, setKaratFilter] = useState('all');
  const [isNewProductOpen, setIsNewProductOpen] = useState(false);
  const [isBulkUploadOpen, setIsBulkUploadOpen] = useState(false);
  const [bulkFile, setBulkFile] = useState<File | null>(null);
  const [bulkRows, setBulkRows] = useState<any[]>([]);
  const [bulkParsing, setBulkParsing] = useState(false);
  const [bulkUploading, setBulkUploading] = useState(false);
  const [bulkProgress, setBulkProgress] = useState({ total: 0, success: 0, failed: 0 });
  const [bulkErrors, setBulkErrors] = useState<string[]>([]);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [isEditMode, setIsEditMode] = useState(false);
  const [isViewDialogOpen, setIsViewDialogOpen] = useState(false);

  // API hooks (paginated)
  const { 
    data: productsData, 
    loading: productsLoading, 
    error: productsError,
    params: productParams,
    updateParams: updateProductParams,
    fetchData: fetchProducts,
    nextPage: nextProductsPage,
    prevPage: prevProductsPage,
    hasNextPage: productsHasNext,
    hasPrevPage: productsHasPrev,
  } = usePaginatedProducts({ pageNumber: 1, pageSize: 20, isActive: true });
  const { execute: createProduct, loading: creatingProduct } = useCreateProduct();
  const { execute: updateProduct, loading: updatingProduct } = useUpdateProduct();
  const { execute: deleteProduct, loading: deletingProduct } = useDeleteProduct();
  
  // Lookup data hooks
  const { data: karatTypesData, loading: karatTypesLoading, fetchKaratTypes } = useKaratTypes();
  const { data: categoryTypesData, loading: categoryTypesLoading, fetchCategories: fetchCategoryTypes } = useProductCategoryTypes();
  const { data: suppliersData, loading: suppliersLoading, execute: fetchSuppliers } = useSuppliers();
  
  // Pricing data hooks
  const { data: goldRatesData, loading: goldRatesLoading, fetchRates } = useGoldRates();
  const { data: makingChargesData, loading: makingChargesLoading, fetchCharges } = useMakingCharges();
  // Stable version tokens to avoid effect loops from changing array references
  const goldRatesVersion = React.useMemo(() => String(goldRatesData?.length ?? 0), [goldRatesData]);
  const makingChargesVersion = React.useMemo(() => String(makingChargesData?.length ?? 0), [makingChargesData]);
  
  // Subcategory state
  const [subCategoriesData, setSubCategoriesData] = useState<any[]>([]);
  const [subCategoriesLoading, setSubCategoriesLoading] = useState(false);
  
  // Function to fetch subcategories based on category
  const fetchSubCategories = useCallback(async (categoryId: number) => {
    try {
      setSubCategoriesLoading(true);
      const subcategories = await lookupsApi.getSubCategories(categoryId);
      setSubCategoriesData(subcategories);
    } catch (error) {
      console.error('Error fetching subcategories:', error);
      setSubCategoriesData([]);
    } finally {
      setSubCategoriesLoading(false);
    }
  }, []);

  // Form state for new/edit product
  const [productForm, setProductForm] = useState<{
    productCode: string;
    name: string;
    categoryType: string;
    karatType: string;
    weight: string;
    brand: string;
    designStyle: string;
    subCategoryId: string;
    shape: string;
    purityCertificateNumber: string;
    countryOfOrigin: string;
    yearOfMinting: string;
    faceValue: string;
    hasNumismaticValue: boolean;
    makingChargesApplicable: boolean;
    useProductMakingCharges: boolean;
    supplierId: string;
  }>({
    productCode: '',
    name: '',
    categoryType: 'GoldJewelry',
    karatType: '22K',
    weight: '',
    brand: '',
    designStyle: '',
    subCategoryId: '',
    shape: '',
    purityCertificateNumber: '',
    countryOfOrigin: '',
    yearOfMinting: '',
    faceValue: '',
    hasNumismaticValue: false,
    makingChargesApplicable: true,
    useProductMakingCharges: false,
    supplierId: 'none',
  });

  // Fetch lookup data on mount
  useEffect(() => {
    fetchKaratTypes();
    fetchCategoryTypes();
    fetchSuppliers();
    fetchRates();
    fetchCharges();
  }, [fetchKaratTypes, fetchCategoryTypes, fetchSuppliers, fetchRates, fetchCharges]);
  
  // Fetch subcategories when category changes
  useEffect(() => {
    if (productForm.categoryType && categoryTypesData) {
      const categoryId = LookupHelper.getValue(categoryTypesData, productForm.categoryType);
      if (categoryId) {
        fetchSubCategories(categoryId);
      }
    }
  }, [productForm.categoryType, categoryTypesData, fetchSubCategories]);

  // Fetch products when filters change (reset to page 1)
  useEffect(() => {
    updateProductParams({
      searchTerm: searchQuery || undefined,
      categoryTypeId: categoryFilter !== 'all' ? LookupHelper.getValue(categoryTypesData || [], categoryFilter) : undefined,
      karatTypeId: karatFilter !== 'all' ? LookupHelper.getValue(karatTypesData || [], karatFilter) : undefined,
      isActive: true,
      pageNumber: 1,
    });
  }, [searchQuery, categoryFilter, karatFilter, categoryTypesData, karatTypesData, updateProductParams]);

  // Get products from API
  const products = productsData?.items || [];
  const totalCount = productsData?.totalCount || 0;
  const pageNumber = productsData?.pageNumber || 1;
  const pageSize = productsData?.pageSize || productParams?.pageSize || 20;
  const totalPages = productsData?.totalPages || Math.ceil((totalCount || 0) / (pageSize || 1));
  
  // Generate categories and karats from API data, fallback to backend category types
  const categories = categoryTypesData ? ['GoldJewelry', 'Bullion', 'Coins'] : ['GoldJewelry', 'Bullion', 'Coins'];
  const categoryDisplayNames = {
    'GoldJewelry': 'Gold Jewelry',
    'Bullion': 'Bullion',
    'Coins': 'Gold Coins'
  };
  const karats = karatTypesData ? LookupHelper.toSelectOptionsByName(karatTypesData).map((option: {value: string, label: string}) => option.value) : ['18K', '21K', '22K', '24K'];
  const suppliers = suppliersData?.items || [];
  const supplierOptions = suppliers.map(supplier => ({
    value: supplier.id.toString(),
    label: `${supplier.companyName}${supplier.contactPersonName ? ` (${supplier.contactPersonName})` : ''}`
  }));

  // Form handling
  const resetForm = () => {
    setProductForm({
      productCode: '',
      name: '',
      categoryType: 'GoldJewelry',
      karatType: '22K',
      weight: '',
      brand: '',
      designStyle: '',
      subCategoryId: 'none',
      shape: '',
      purityCertificateNumber: '',
      countryOfOrigin: '',
      yearOfMinting: '',
      faceValue: '',
      hasNumismaticValue: false,
      makingChargesApplicable: true,
      useProductMakingCharges: false,
      supplierId: 'none',
    });
  };

  const openEditDialog = (product: Product) => {
    console.log('Opening edit dialog for product:', product);
    setSelectedProduct(product);
    
    const formData = {
      productCode: product.productCode,
      name: product.name,
      categoryType: LookupHelper.getDisplayName(categoryTypesData || [], product.categoryTypeId),
      karatType: LookupHelper.getDisplayName(karatTypesData || [], product.karatTypeId),
      weight: product.weight.toString(),
      brand: product.brand || '',
      designStyle: product.designStyle || '',
      subCategoryId: product.subCategoryId?.toString() || 'none',
      shape: product.shape || '',
      purityCertificateNumber: product.purityCertificateNumber || '',
      countryOfOrigin: product.countryOfOrigin || '',
      yearOfMinting: product.yearOfMinting?.toString() || '',
      faceValue: product.faceValue?.toString() || '',
      hasNumismaticValue: product.hasNumismaticValue || false,
      makingChargesApplicable: product.makingChargesApplicable,
      useProductMakingCharges: product.useProductMakingCharges,
      supplierId: product.supplierId?.toString() || 'none',
    };
    
    console.log('Setting form data:', formData);
    setProductForm(formData);
    setIsEditMode(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!isManager) {
      alert('Only managers can manage products');
      return;
    }
    
    // Validate required fields
    if (!productForm.productCode.trim()) {
      alert('Product code is required');
      return;
    }
    
    if (!productForm.name.trim()) {
      alert('Product name is required');
      return;
    }
    
    if (!productForm.weight || isNaN(parseFloat(productForm.weight)) || parseFloat(productForm.weight) <= 0) {
      alert('Valid weight is required (must be greater than 0)');
      return;
    }
    
    if (!productForm.categoryType) {
      alert('Category is required');
      return;
    }
    
    if (!productForm.karatType) {
      alert('Karat is required');
      return;
    }
    
    try {
      const weight = parseFloat(productForm.weight);
      
      const productData = {
        productCode: productForm.productCode.trim(),
        name: productForm.name.trim(),
        categoryTypeId: LookupHelper.getValue(categoryTypesData || [], productForm.categoryType) || 1,
        karatTypeId: LookupHelper.getValue(karatTypesData || [], productForm.karatType) || 3,
        weight: weight,
        brand: productForm.brand?.trim() || undefined,
        designStyle: productForm.designStyle?.trim() || undefined,
        subCategoryId: productForm.subCategoryId && productForm.subCategoryId !== '' && productForm.subCategoryId !== 'none' ? parseInt(productForm.subCategoryId) : undefined,
        shape: productForm.shape?.trim() || undefined,
        purityCertificateNumber: productForm.purityCertificateNumber?.trim() || undefined,
        countryOfOrigin: productForm.countryOfOrigin?.trim() || undefined,
        yearOfMinting: productForm.yearOfMinting ? parseInt(productForm.yearOfMinting) : undefined,
        faceValue: productForm.faceValue ? parseFloat(productForm.faceValue) : undefined,
        hasNumismaticValue: productForm.hasNumismaticValue,
        makingChargesApplicable: productForm.makingChargesApplicable,
        useProductMakingCharges: productForm.useProductMakingCharges,
        supplierId: productForm.supplierId && productForm.supplierId !== '' && productForm.supplierId !== 'none' ? parseInt(productForm.supplierId) : undefined,
        isActive: true,
      };

      console.log('Submitting product data:', productData);
      
      if (isEditMode && selectedProduct) {
        await updateProduct(selectedProduct.id, productData);
        alert('Product updated successfully!');
      } else {
        await createProduct(productData);
        alert('Product created successfully!');
      }

      // Refresh list and reset to first page with current filters
      updateProductParams({
        searchTerm: searchQuery || undefined,
        categoryTypeId: categoryFilter !== 'all' ? LookupHelper.getValue(categoryTypesData || [], categoryFilter) : undefined,
        karatTypeId: karatFilter !== 'all' ? LookupHelper.getValue(karatTypesData || [], karatFilter) : undefined,
        isActive: true,
        pageNumber: 1,
      });

      // Reset form and close dialog
      resetForm();
      setSelectedProduct(null);
      setIsEditMode(false);
      setIsNewProductOpen(false);
    } catch (error) {
      console.error('Error saving product:', error);
      
      // Show more detailed error information
      if (error instanceof Error) {
        try {
          // Try to parse error message as JSON to show validation errors
          const errorData = JSON.parse(error.message);
          if (errorData.errors) {
            const errorMessages = Object.entries(errorData.errors)
              .map(([field, messages]) => `${field}: ${Array.isArray(messages) ? messages.join(', ') : messages}`)
              .join('\n');
            alert(`Validation errors:\n${errorMessages}`);
          } else {
            alert(error.message);
          }
        } catch {
          alert(error.message);
        }
      } else {
        alert('Failed to save product');
      }
    }
  };

  const handleDelete = async (product: Product) => {
    if (!isManager) {
      alert('Only managers can delete products');
      return;
    }

    if (!window.confirm(`Are you sure you want to delete "${product.name}"?`)) {
      return;
    }

    try {
      await deleteProduct(product.id);
      alert('Product deleted successfully!');
      // Refresh current page
      fetchProducts();
    } catch (error) {
      console.error('Error deleting product:', error);
      alert(error instanceof Error ? error.message : 'Failed to delete product');
    }
  };

  // Bulk upload helpers (scoped within Products component)
  const resetBulkState = () => {
    setBulkFile(null);
    setBulkRows([]);
    setBulkParsing(false);
    setBulkUploading(false);
    setBulkProgress({ total: 0, success: 0, failed: 0 });
    setBulkErrors([]);
  };

  const handleBulkFileChange = async (file?: File) => {
    if (!file) return;
    try {
      setBulkParsing(true);
      const rows = await parseExcelFile(file);
      setBulkRows(rows);
      setBulkProgress({ total: rows.length, success: 0, failed: 0 });
    } catch (err) {
      console.error('Failed to parse file:', err);
      alert('Failed to parse file. Ensure it is a valid .xlsx or .csv.');
      setBulkRows([]);
    } finally {
      setBulkParsing(false);
    }
  };

  const boolVal = (v: any, def = false) => {
    if (typeof v === 'boolean') return v;
    if (v == null || v === '') return def;
    const s = String(v).trim().toLowerCase();
    return ['true', 'yes', 'y', '1'].includes(s);
  };

  const numVal = (v: any): number | undefined => {
    if (v === '' || v == null) return undefined;
    const n = Number(v);
    return isNaN(n) ? undefined : n;
  };

  const mapRowToCreateProduct = (row: any) => {
    const categoryTypeName = (row.CategoryType ?? row.categoryType ?? '').toString();
    const karatTypeName = (row.KaratType ?? row.karatType ?? '').toString();
    const categoryTypeId = LookupHelper.getValue(categoryTypesData || [], categoryTypeName);
    const karatTypeId = LookupHelper.getValue(karatTypesData || [], karatTypeName);
    if (!categoryTypeId) throw new Error(`Invalid CategoryType '${categoryTypeName}'`);
    if (!karatTypeId) throw new Error(`Invalid KaratType '${karatTypeName}'`);
      
    const weightRaw = row.Weight ?? row.weight;
    const weight = Number(weightRaw);
    if (!weight || isNaN(weight) || weight <= 0) throw new Error(`Invalid Weight '${weightRaw}'`);

    const productData = {
      productCode: String(row.ProductCode ?? row.productCode ?? '').trim(),
      name: String(row.Name ?? row.name ?? '').trim(),
      categoryTypeId,
      karatTypeId,
      weight,
      brand: (row.Brand ?? row.brand)?.toString().trim() || undefined,
      designStyle: (row.DesignStyle ?? row.designStyle)?.toString().trim() || undefined,
      subCategoryId: numVal(row.SubCategoryId ?? row.subCategoryId),
      shape: (row.Shape ?? row.shape)?.toString().trim() || undefined,
      purityCertificateNumber: (row.PurityCertificateNumber ?? row.purityCertificateNumber)?.toString().trim() || undefined,
      countryOfOrigin: (row.CountryOfOrigin ?? row.countryOfOrigin)?.toString().trim() || undefined,
      yearOfMinting: numVal(row.YearOfMinting ?? row.yearOfMinting),
      faceValue: numVal(row.FaceValue ?? row.faceValue),
      hasNumismaticValue: boolVal(row.HasNumismaticValue ?? row.hasNumismaticValue, false),
      makingChargesApplicable: boolVal(row.MakingChargesApplicable ?? row.makingChargesApplicable, true),
      useProductMakingCharges: boolVal(row.UseProductMakingCharges ?? row.useProductMakingCharges, false),
      supplierId: numVal(row.SupplierId ?? row.supplierId),
      isActive: true,
    } as any;

    if (!productData.productCode) throw new Error('ProductCode is required');
    if (!productData.name) throw new Error('Name is required');
    return productData;
  };

  const handleStartBulkUpload = async () => {
    if (!isManager) {
      alert('Only managers can upload products');
      return;
    }
    if (bulkRows.length === 0) {
      alert('No rows parsed. Please select a valid file.');
      return;
    }
    setBulkUploading(true);
    setBulkErrors([]);
    let success = 0;
    let failed = 0;
    for (let i = 0; i < bulkRows.length; i++) {
      const row = bulkRows[i];
      try {
        const productData = mapRowToCreateProduct(row);
        await createProduct(productData as any);
        success++;
      } catch (err: any) {
        failed++;
        const msg = err?.message || String(err);
        setBulkErrors((prev: string[]) => [...prev, `Row ${i + 2}: ${msg}`]);
      } finally {
        setBulkProgress((prev: { total: number; success: number; failed: number }) => ({ ...prev, success, failed }));
      }
    }

    updateProductParams({
      searchTerm: searchQuery || undefined,
      categoryTypeId: categoryFilter !== 'all' ? LookupHelper.getValue(categoryTypesData || [], categoryFilter) : undefined,
      karatTypeId: karatFilter !== 'all' ? LookupHelper.getValue(karatTypesData || [], karatFilter) : undefined,
      isActive: true,
      pageNumber: 1,
    });

    setBulkUploading(false);
    alert(`Bulk upload completed. Success: ${success}, Failed: ${failed}`);
  };

  const handleDownloadTemplateXlsx = async () => {
    try {
      const blob = await generateProductsTemplateXlsx();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'products_template.xlsx';
      a.click();
      URL.revokeObjectURL(url);
    } catch (e) {
      console.error(e);
      alert('Failed to generate XLSX template');
    }
  };

  const handleDownloadTemplateCsv = () => {
    try {
      const blob = generateProductsTemplateCsv();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'products_template.csv';
      a.click();
      URL.revokeObjectURL(url);
    } catch (e) {
      console.error(e);
      alert('Failed to generate CSV template');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl text-[#0D1B2A]">Product Management</h1>
          <p className="text-muted-foreground">Manage jewelry inventory and product catalog</p>
        </div>
        {isManager && (
          <Dialog open={isNewProductOpen} onOpenChange={setIsNewProductOpen}>
            <DialogTrigger asChild>
              <Button className="touch-target" variant="golden">
                <Plus className="mr-2 h-4 w-4" />
                Add Product
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto bg-white border-gray-200 shadow-lg">
              <DialogHeader>
                <DialogTitle>{isEditMode ? 'Edit Product' : 'Add New Product'}</DialogTitle>
                <DialogDescription>
                  {isEditMode ? 'Update product information' : 'Enter product details to add to inventory'}
                </DialogDescription>
              </DialogHeader>
              <form onSubmit={handleSubmit}>
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="productCode">Product Code</Label>
                    <Input
                      id="productCode"
                      value={productForm.productCode}
                      onChange={(e) => setProductForm({...productForm, productCode: e.target.value})}
                      placeholder="GLD-XXX-XXX"
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="name">Product Name</Label>
                    <Input
                      id="name"
                      value={productForm.name}
                      onChange={(e) => setProductForm({...productForm, name: e.target.value})}
                      placeholder="Enter product name"
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="categoryType">Category</Label>
                    <Select value={productForm.categoryType} onValueChange={(value) => setProductForm({...productForm, categoryType: value as any})} required>
                      <SelectTrigger>
                        <SelectValue placeholder="Select category" />
                      </SelectTrigger>
                      <SelectContent className="bg-white border-gray-200 shadow-lg">
                        {categories.map((cat: string) => (
                          <SelectItem key={cat} value={cat} className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">{categoryDisplayNames[cat as keyof typeof categoryDisplayNames]}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="subCategoryId">Subcategory (Optional)</Label>
                    <Select 
                      value={productForm.subCategoryId} 
                      onValueChange={(value) => setProductForm({...productForm, subCategoryId: value})}
                      disabled={subCategoriesLoading}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder={subCategoriesLoading ? "Loading subcategories..." : "Select subcategory"} />
                      </SelectTrigger>
                      <SelectContent className="bg-white border-gray-200 shadow-lg">
                        <SelectItem value="none" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">None</SelectItem>
                        {subCategoriesData.map((subcat: any) => (
                          <SelectItem key={subcat.id} value={subcat.id.toString()} className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">
                            {subcat.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="karatType">Karat</Label>
                    <Select value={productForm.karatType} onValueChange={(value) => setProductForm({...productForm, karatType: value as any})} required>
                      <SelectTrigger>
                        <SelectValue placeholder="Select karat" />
                      </SelectTrigger>
                      <SelectContent className="bg-white border-gray-200 shadow-lg">
                        {karats.map((karat: string) => (
                          <SelectItem key={karat} value={karat} className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">{karat}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="weight">Weight (grams)</Label>
                    <Input
                      id="weight"
                      type="number"
                      step="0.001"
                      min="0.001"
                      value={productForm.weight}
                      onChange={(e) => setProductForm({...productForm, weight: e.target.value})}
                      placeholder="0.0"
                      required
                    />
                  </div>
                <div className="space-y-2">
                  <Label htmlFor="shape">Shape/Dimensions</Label>
                  <Input
                    id="shape"
                    value={productForm.shape}
                    onChange={(e) => setProductForm({...productForm, shape: e.target.value})}
                    placeholder="Size, length, diameter, shape"
                  />
                </div>
                                  <div className="space-y-2">
                    <Label htmlFor="supplierId">Supplier</Label>
                    <Select value={productForm.supplierId === 'none' ? '' : productForm.supplierId} onValueChange={(value) => setProductForm({...productForm, supplierId: value})} disabled={suppliersLoading}>
                      <SelectTrigger>
                        <SelectValue placeholder={suppliersLoading ? "Loading suppliers..." : "Select supplier"} />
                      </SelectTrigger>
                      <SelectContent className="bg-white border-gray-200 shadow-lg">
                        {suppliersLoading ? (
                          <SelectItem value="loading" disabled>Loading suppliers...</SelectItem>
                        ) : (
                          <>
                            <SelectItem value="none" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">
                              None (No supplier)
                            </SelectItem>
                            {supplierOptions.map((supplier) => (
                              <SelectItem key={supplier.value} value={supplier.value} className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">
                                {supplier.label}
                              </SelectItem>
                            ))}
                          </>
                        )}
                      </SelectContent>
                    </Select>
                  </div>
                <div className="space-y-2">
                  <Label htmlFor="brand">Brand</Label>
                  <Input
                    id="brand"
                    value={productForm.brand}
                    onChange={(e) => setProductForm({...productForm, brand: e.target.value})}
                    placeholder="DijaGold Premium, Classic, etc."
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="yearOfMinting">Year of Minting</Label>
                  <Input
                    id="yearOfMinting"
                    type="number"
                    value={productForm.yearOfMinting}
                    onChange={(e) => setProductForm({...productForm, yearOfMinting: e.target.value})}
                    placeholder="2024"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="faceValue">Face Value</Label>
                  <Input
                    id="faceValue"
                    type="number"
                    value={productForm.faceValue}
                    onChange={(e) => setProductForm({...productForm, faceValue: e.target.value})}
                    placeholder="0"
                  />
                </div>
                <div className="col-span-2 space-y-2">
                  <Label htmlFor="countryOfOrigin">Country of Origin</Label>
                  <Input
                    id="countryOfOrigin"
                    value={productForm.countryOfOrigin}
                    onChange={(e) => setProductForm({...productForm, countryOfOrigin: e.target.value})}
                    placeholder="Country of origin"
                  />
                </div>
              </div>
              <div className="flex justify-end gap-3 mt-6">
                <Button type="button" variant="outline" onClick={() => setIsNewProductOpen(false)}>
                  Cancel
                </Button>
                <Button 
                  type="submit" 
                  variant="golden"
                  disabled={creatingProduct || updatingProduct}
                >
                  {creatingProduct || updatingProduct ? (
                    <>
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      {isEditMode ? 'Updating...' : 'Creating...'}
                    </>
                  ) : (
                    isEditMode ? 'Update Product' : 'Add Product'
                  )}
                </Button>
              </div>
              </form>
            </DialogContent>
          </Dialog>
        )}
        {isManager && (
          <Dialog open={isBulkUploadOpen} onOpenChange={(open) => { setIsBulkUploadOpen(open); if (!open) resetBulkState(); }}>
            <DialogTrigger asChild>
              <Button className="touch-target" variant="outline">
                <Upload className="mr-2 h-4 w-4" />
                Bulk Upload
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto bg-white border-gray-200 shadow-lg">
              <DialogHeader>
                <DialogTitle>Bulk Upload Products</DialogTitle>
                <DialogDescription>Upload an Excel (.xlsx) or CSV file using the provided template columns.</DialogDescription>
              </DialogHeader>
              <div className="space-y-4">
                <div className="flex gap-2">
                  <Button type="button" variant="outline" onClick={handleDownloadTemplateXlsx}>
                    <Download className="mr-2 h-4 w-4" /> Template (XLSX)
                  </Button>
                  <Button type="button" variant="outline" onClick={handleDownloadTemplateCsv}>
                    <Download className="mr-2 h-4 w-4" /> Template (CSV)
                  </Button>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="bulkFile">Select file</Label>
                  <Input id="bulkFile" type="file" accept=".xlsx,.csv" onChange={(e) => { const f = e.target.files?.[0]; setBulkFile(f || null); if (f) handleBulkFileChange(f); }} />
                  {bulkParsing && (
                    <div className="flex items-center text-sm text-muted-foreground"><Loader2 className="mr-2 h-4 w-4 animate-spin" /> Parsing file...</div>
                  )}
                  {bulkRows.length > 0 && (
                    <div className="text-sm">
                      Parsed rows: <b>{bulkRows.length}</b>
                    </div>
                  )}
                </div>
                {bulkRows.length > 0 && (
                  <div className="space-y-2">
                    <div className="text-sm">Progress: {bulkProgress.success} succeeded, {bulkProgress.failed} failed of {bulkProgress.total}</div>
                    <div className="flex justify-end gap-2">
                      <Button type="button" variant="outline" onClick={() => resetBulkState()} disabled={bulkUploading}>Reset</Button>
                      <Button type="button" variant="golden" onClick={handleStartBulkUpload} disabled={bulkUploading}>
                        {bulkUploading ? (<><Loader2 className="mr-2 h-4 w-4 animate-spin" /> Uploading...</>) : 'Start Upload'}
                      </Button>
                    </div>
                    {bulkErrors.length > 0 && (
                      <div className="max-h-40 overflow-auto border rounded p-2 text-sm text-red-600 bg-red-50">
                        {bulkErrors.map((e, i) => (<div key={i}>{e}</div>))}
                      </div>
                    )}
                  </div>
                )}
              </div>
            </DialogContent>
          </Dialog>
        )}
      </div>

      {/* Filters */}
      <Card className="pos-card">
        <CardContent className="pt-6">
          <div className="flex flex-col md:flex-row gap-4">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search products by name, SKU, or description..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-10 touch-target"
                data-testid="product-search"
              />
            </div>
            <Button
              variant="outline"
              onClick={() =>
                updateProductParams({
                  searchTerm: searchQuery || undefined,
                  categoryTypeId:
                    categoryFilter !== 'all'
                      ? LookupHelper.getValue(categoryTypesData || [], categoryFilter)
                      : undefined,
                  karatTypeId:
                    karatFilter !== 'all'
                      ? LookupHelper.getValue(karatTypesData || [], karatFilter)
                      : undefined,
                  isActive: true,
                  pageNumber: 1,
                })
              }
              data-testid="product-search-btn"
            >
              Search
            </Button>
            <Select value={categoryFilter} onValueChange={setCategoryFilter}>
              <SelectTrigger className="w-full md:w-40">
                <SelectValue placeholder="Category" />
              </SelectTrigger>
              <SelectContent className="bg-white border-gray-200 shadow-lg">
                <SelectItem value="all" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">All Categories</SelectItem>
                {categories.map((cat: string) => (
                  <SelectItem key={cat} value={cat} className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">{categoryDisplayNames[cat as keyof typeof categoryDisplayNames]}</SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select value={karatFilter} onValueChange={setKaratFilter}>
              <SelectTrigger className="w-full md:w-32">
                <SelectValue placeholder="Karat" />
              </SelectTrigger>
              <SelectContent className="bg-white border-gray-200 shadow-lg">
                <SelectItem value="all" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">All Karats</SelectItem>
                {karats.map((karat: string) => (
                  <SelectItem key={karat} value={karat} className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">{karat}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Products Table */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle>Products</CardTitle>
          <CardDescription>
            {productsLoading ? 'Loading...' : `${totalCount} product(s) found`}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {productsLoading ? (
            <div className="space-y-4">
              {[1, 2, 3, 4, 5].map((i) => (
                <div key={i} className="flex items-center space-x-4">
                  <div className="animate-pulse flex-1 space-y-2">
                    <div className="h-4 bg-gray-300 rounded w-1/4"></div>
                    <div className="h-3 bg-gray-300 rounded w-3/4"></div>
                  </div>
                </div>
              ))}
            </div>
          ) : productsError ? (
            <div className="text-center py-8">
              <AlertCircle className="h-12 w-12 text-red-500 mx-auto mb-4" />
              <p className="text-red-600">Error loading products: {productsError}</p>
              <Button 
                onClick={() => fetchProducts()} 
                className="mt-4"
              >
                Retry
              </Button>
            </div>
          ) : products.length === 0 ? (
            <div className="text-center py-8">
              <Package className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <p className="text-muted-foreground">
                {searchQuery || categoryFilter !== 'all' || karatFilter !== 'all' 
                  ? 'No products found matching your filters'
                  : 'No products in catalog'}
              </p>
            </div>
          ) : (
            <>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Product Code</TableHead>
                  <TableHead>Product</TableHead>
                  <TableHead>Category</TableHead>
                  <TableHead>Karat</TableHead>
                  <TableHead>Weight</TableHead>
                  <TableHead>Est. Price</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {products.map((product) => {
                  return (
                    <TableRow key={product.id} data-testid="product-row" data-product-id={product.id}>
                      <TableCell className="font-medium">{product.productCode}</TableCell>
                      <TableCell>
                        <div>
                          <p className="font-medium">{product.name}</p>
                          {product.brand && <p className="text-sm text-muted-foreground">{product.brand}</p>}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div>
                          <p>{LookupHelper.getDisplayName(categoryTypesData || [], product.categoryTypeId)}</p>
                          {product.subCategory && <p className="text-sm text-muted-foreground">{typeof product.subCategory === 'string' ? product.subCategory : product.subCategory.name}</p>}
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline">{LookupHelper.getDisplayName(karatTypesData || [], product.karatTypeId)}</Badge>
                      </TableCell>
                      <TableCell>{product.weight}g</TableCell>
                      <TableCell>
                        <div>
                          <p className="font-medium" data-testid="pricing-total">
                            <MemoProductPriceDisplay 
                              product={product} 
                              goldRatesData={goldRatesData || []} 
                              makingChargesData={makingChargesData || []}
                              goldRatesVersion={goldRatesVersion}
                              makingChargesVersion={makingChargesVersion}
                            />
                          </p>
                          {product.makingChargesApplicable && (
                            <p className="text-sm text-muted-foreground">+ Making Charges</p>
                          )}
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge variant={product.isActive ? "default" : "secondary"}>
                          {product.isActive ? "Active" : "Inactive"}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" className="h-8 w-8 p-0">
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end" className="bg-white border-gray-200 shadow-lg">
                            <DropdownMenuItem 
                              onClick={() => {
                                setSelectedProduct(product);
                                setIsViewDialogOpen(true);
                              }}
                              className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]"
                            >
                              <Eye className="mr-2 h-4 w-4" />
                              View Details
                            </DropdownMenuItem>
                            {isManager && (
                              <>
                                <DropdownMenuItem 
                                  onClick={() => {
                                    openEditDialog(product);
                                    setIsNewProductOpen(true);
                                  }}
                                  className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]"
                                >
                                  <Edit className="mr-2 h-4 w-4" />
                                  Edit Product
                                </DropdownMenuItem>
                                <DropdownMenuItem 
                                  onClick={() => handleDelete(product)}
                                  className="text-red-600 hover:bg-red-50 focus:bg-red-50 focus:text-red-700"
                                  disabled={deletingProduct}
                                >
                                  <Trash2 className="mr-2 h-4 w-4" />
                                  {deletingProduct ? 'Deleting...' : 'Delete Product'}
                                </DropdownMenuItem>
                              </>
                            )}
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
            {/* Pagination Controls */}
            <div className="flex flex-col md:flex-row items-center justify-between gap-3 mt-4">
              <div className="text-sm text-muted-foreground">
                Page {pageNumber} of {Math.max(totalPages, 1)} • Showing {(products.length > 0 ? (pageNumber - 1) * pageSize + 1 : 0)}–{(pageNumber - 1) * pageSize + products.length} of {totalCount}
              </div>
              <div className="flex items-center gap-2">
                <span className="text-sm">Rows per page</span>
                <Select
                  value={String(pageSize)}
                  onValueChange={(v) => updateProductParams({ pageSize: Number(v), pageNumber: 1 })}
                >
                  <SelectTrigger className="w-24">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent className="bg-white border-gray-200 shadow-lg">
                    {[10, 20, 50, 100].map(s => (
                      <SelectItem key={s} value={String(s)} className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">{s}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <div className="flex items-center gap-2">
                  <Button variant="outline" disabled={!productsHasPrev} onClick={prevProductsPage}>
                    Prev
                  </Button>
                  <Button variant="outline" disabled={!productsHasNext} onClick={nextProductsPage}>
                    Next
                  </Button>
                </div>
              </div>
            </div>
            </>
          )}
        </CardContent>
      </Card>
      {/* View Details Dialog */}
      {selectedProduct && (
        <Dialog
          open={isViewDialogOpen}
          onOpenChange={(open) => {
            setIsViewDialogOpen(open);
            if (!open) {
              setSelectedProduct(null);
            }
          }}
        >
          <DialogContent className="max-w-2xl bg-white border-gray-200 shadow-lg">
            <DialogHeader>
              <DialogTitle>Product Details</DialogTitle>
              <DialogDescription>Quick view of the selected product</DialogDescription>
            </DialogHeader>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-muted-foreground">Product Code</p>
                <p className="font-medium">{selectedProduct.productCode}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Name</p>
                <p className="font-medium">{selectedProduct.name}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Category</p>
                <p className="font-medium">{LookupHelper.getDisplayName(categoryTypesData || [], selectedProduct.categoryTypeId)}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Karat</p>
                <p className="font-medium">{LookupHelper.getDisplayName(karatTypesData || [], selectedProduct.karatTypeId)}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Weight</p>
                <p className="font-medium">{selectedProduct.weight} g</p>
              </div>
              {selectedProduct.brand && (
                <div>
                  <p className="text-sm text-muted-foreground">Brand</p>
                  <p className="font-medium">{selectedProduct.brand}</p>
                </div>
              )}
              {selectedProduct.shape && (
                <div>
                  <p className="text-sm text-muted-foreground">Shape/Dimensions</p>
                  <p className="font-medium">{selectedProduct.shape}</p>
                </div>
              )}
              {selectedProduct.subCategory && (
                <div className="col-span-2">
                  <p className="text-sm text-muted-foreground">Subcategory</p>
                  <p className="font-medium">{typeof selectedProduct.subCategory === 'string' ? selectedProduct.subCategory : selectedProduct.subCategory.name}</p>
                </div>
              )}
              {selectedProduct.supplierId && (
                <div className="col-span-2">
                  <p className="text-sm text-muted-foreground">Supplier</p>
                  <p className="font-medium">
                    {(() => {
                      const s = (suppliersData?.items || []).find((x: SupplierDto) => x.id === selectedProduct.supplierId);
                      return s ? `${s.companyName}${s.contactPersonName ? ` (${s.contactPersonName})` : ''}` : '—';
                    })()}
                  </p>
                </div>
              )}
            </div>
            <div className="flex justify-end gap-2 mt-4">
              <Button variant="outline" onClick={() => setIsViewDialogOpen(false)}>Close</Button>
              {isManager && (
                <Button
                  variant="golden"
                  onClick={() => {
                    if (selectedProduct) {
                      openEditDialog(selectedProduct);
                      setIsNewProductOpen(true);
                      setIsViewDialogOpen(false);
                    }
                  }}
                >
                  <Edit className="mr-2 h-4 w-4" /> Edit
                </Button>
              )}
            </div>
          </DialogContent>
        </Dialog>
      )}
    </div>
  );
}