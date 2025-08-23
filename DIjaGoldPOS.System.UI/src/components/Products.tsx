import React, { useState, useEffect } from 'react';
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
import { useProducts, useCreateProduct, useUpdateProduct, useDeleteProduct, useKaratTypes, useProductCategoryTypes, useSuppliers, useGoldRates, useMakingCharges } from '../hooks/useApi';
import { Product, SupplierDto } from '../services/api';
import { EnumMapper, ProductCategoryType, KaratType } from '../types/enums';
import { calculateProductPricing, getProductPricingFromAPI } from '../utils/pricing';

// Component to handle async price display
function ProductPriceDisplay({ product, goldRatesData, makingChargesData }: { 
  product: Product, 
  goldRatesData: any[], 
  makingChargesData: any[] 
}) {
  const [price, setPrice] = useState<number | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchPrice = async () => {
      try {
        setLoading(true);
        const pricing = await getProductPricingFromAPI(product.id, 1);
        setPrice(pricing.estimatedTotalPrice);
      } catch (error) {
        console.error('Error fetching price from API:', error);
        // Fallback to local calculation
        try {
          if (goldRatesData && makingChargesData) {
            const pricing = calculateProductPricing(product, goldRatesData, makingChargesData, 1, null, []);
            setPrice(pricing.finalTotal);
          } else {
            setPrice(0);
          }
        } catch (fallbackError) {
          console.error('Fallback calculation failed:', fallbackError);
          setPrice(0);
        }
      } finally {
        setLoading(false);
      }
    };

    fetchPrice();
  }, [product.id, goldRatesData, makingChargesData]);

  if (loading) {
    return <Loader2 className="h-4 w-4 animate-spin" />;
  }

  return <span>{formatCurrency(price || 0)}</span>;
}

export default function Products() {
  const { isManager } = useAuth();
  const [searchQuery, setSearchQuery] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('all');
  const [karatFilter, setKaratFilter] = useState('all');
  const [isNewProductOpen, setIsNewProductOpen] = useState(false);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [isEditMode, setIsEditMode] = useState(false);

  // API hooks
  const { data: productsData, loading: productsLoading, error: productsError, execute: fetchProducts } = useProducts();
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

  // Form state for new/edit product
  const [productForm, setProductForm] = useState({
    productCode: '',
    name: '',
    categoryType: 'GoldJewelry' as 'GoldJewelry' | 'Bullion' | 'Coins',
    karatType: '22K' as '18K' | '21K' | '22K' | '24K',
    weight: '',
    brand: '',
    designStyle: '',
    subCategory: '',
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

  // Fetch products on mount and when filters change
  useEffect(() => {
    fetchProducts({
      searchTerm: searchQuery || undefined,
      categoryType: categoryFilter !== 'all' ? categoryFilter : undefined,
      karatType: karatFilter !== 'all' ? karatFilter : undefined,
      isActive: true,
      pageNumber: 1,
      pageSize: 100,
    });
  }, [searchQuery, categoryFilter, karatFilter, fetchProducts]);

  // Get products from API
  const products = productsData?.items || [];
  
  // Generate categories and karats from API data, fallback to backend category types
  const categories = categoryTypesData ? ['GoldJewelry', 'Bullion', 'Coins'] : ['GoldJewelry', 'Bullion', 'Coins'];
  const categoryDisplayNames = {
    'GoldJewelry': 'Gold Jewelry',
    'Bullion': 'Bullion',
    'Coins': 'Gold Coins'
  };
  const karats = karatTypesData ? EnumMapper.lookupToSelectOptions(karatTypesData).map((option: {value: string, label: string}) => option.value) : ['18K', '21K', '22K', '24K'];
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
      subCategory: '',
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
      categoryType: EnumMapper.productCategoryEnumToString(product.categoryType),
      karatType: EnumMapper.karatEnumToString(product.karatType),
      weight: product.weight.toString(),
      brand: product.brand || '',
      designStyle: product.designStyle || '',
      subCategory: product.subCategory || '',
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
        categoryType: EnumMapper.productCategoryStringToEnum(productForm.categoryType),
        karatType: EnumMapper.karatStringToEnum(productForm.karatType),
        weight: weight,
        brand: productForm.brand?.trim() || undefined,
        designStyle: productForm.designStyle?.trim() || undefined,
        subCategory: productForm.subCategory?.trim() || undefined,
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

      await fetchProducts({
        searchTerm: searchQuery || undefined,
        categoryType: categoryFilter !== 'all' ? categoryFilter : undefined,
        karatType: karatFilter !== 'all' ? karatFilter : undefined,
        isActive: true,
        pageNumber: 1,
        pageSize: 100,
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
      
      await fetchProducts({
        searchTerm: searchQuery || undefined,
        categoryType: categoryFilter !== 'all' ? categoryFilter : undefined,
        karatType: karatFilter !== 'all' ? karatFilter : undefined,
        isActive: true,
        pageNumber: 1,
        pageSize: 100,
      });
    } catch (error) {
      console.error('Error deleting product:', error);
      alert(error instanceof Error ? error.message : 'Failed to delete product');
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
                    <Label htmlFor="subCategory">Subcategory</Label>
                    <Input
                      id="subCategory"
                      value={productForm.subCategory}
                      onChange={(e) => setProductForm({...productForm, subCategory: e.target.value})}
                      placeholder="Wedding, Designer, Traditional"
                    />
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
              />
            </div>
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
            {productsLoading ? 'Loading...' : `${products.length} product(s) found`}
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
                onClick={() => fetchProducts({
                  searchTerm: searchQuery || undefined,
                  categoryType: categoryFilter !== 'all' ? categoryFilter : undefined,
                  karatType: karatFilter !== 'all' ? karatFilter : undefined,
                  isActive: true,
                  pageNumber: 1,
                  pageSize: 100,
                })} 
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
                    <TableRow key={product.id}>
                      <TableCell className="font-medium">{product.productCode}</TableCell>
                      <TableCell>
                        <div>
                          <p className="font-medium">{product.name}</p>
                          {product.brand && <p className="text-sm text-muted-foreground">{product.brand}</p>}
                          {product.supplierName && <p className="text-sm text-muted-foreground">by {product.supplierName}</p>}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div>
                          <p>{categoryDisplayNames[EnumMapper.productCategoryEnumToString(product.categoryType) as keyof typeof categoryDisplayNames]}</p>
                          {product.subCategory && <p className="text-sm text-muted-foreground">{product.subCategory}</p>}
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline">{EnumMapper.karatEnumToString(product.karatType)}</Badge>
                      </TableCell>
                      <TableCell>{product.weight}g</TableCell>
                      <TableCell>
                        <div>
                          <p className="font-medium">
                            <ProductPriceDisplay 
                              product={product} 
                              goldRatesData={goldRatesData || []} 
                              makingChargesData={makingChargesData || []} 
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
                                // Could add a view details dialog here
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
          )}
        </CardContent>
      </Card>
    </div>
  );
}