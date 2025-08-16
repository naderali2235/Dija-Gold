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
import { useProducts, useCreateProduct, useUpdateProduct, useDeleteProduct } from '../hooks/useApi';
import { Product } from '../services/api';

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

  // Form state for new/edit product
  const [productForm, setProductForm] = useState({
    productCode: '',
    name: '',
    categoryType: 'Ring' as Product['categoryType'],
    karatType: '22K' as Product['karatType'],
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
    supplierId: '',
  });

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
  
  const categories = ['Ring', 'Chain', 'Necklace', 'Earrings', 'Bangles', 'Bracelet', 'Bullion', 'Coins', 'Other'];
  const karats = ['18K', '21K', '22K', '24K'];

  // Form handling
  const resetForm = () => {
    setProductForm({
      productCode: '',
      name: '',
      categoryType: 'Ring',
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
      supplierId: '',
    });
  };

  const openEditDialog = (product: Product) => {
    setSelectedProduct(product);
    setProductForm({
      productCode: product.productCode,
      name: product.name,
      categoryType: product.categoryType,
      karatType: product.karatType,
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
      supplierId: product.supplierId?.toString() || '',
    });
    setIsEditMode(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!isManager) {
      alert('Only managers can manage products');
      return;
    }
    
    try {
      const productData = {
        productCode: productForm.productCode,
        name: productForm.name,
        categoryType: productForm.categoryType,
        karatType: productForm.karatType,
        weight: parseFloat(productForm.weight),
        brand: productForm.brand || undefined,
        designStyle: productForm.designStyle || undefined,
        subCategory: productForm.subCategory || undefined,
        shape: productForm.shape || undefined,
        purityCertificateNumber: productForm.purityCertificateNumber || undefined,
        countryOfOrigin: productForm.countryOfOrigin || undefined,
        yearOfMinting: productForm.yearOfMinting ? parseInt(productForm.yearOfMinting) : undefined,
        faceValue: productForm.faceValue ? parseFloat(productForm.faceValue) : undefined,
        hasNumismaticValue: productForm.hasNumismaticValue,
        makingChargesApplicable: productForm.makingChargesApplicable,
        supplierId: productForm.supplierId ? parseInt(productForm.supplierId) : undefined,
        isActive: true,
      };

      if (isEditMode && selectedProduct) {
        await updateProduct(selectedProduct.id, productData);
        alert('Product updated successfully!');
      } else {
        await createProduct(productData);
        alert('Product created successfully!');
      }

      // Refresh products list
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
      alert(error instanceof Error ? error.message : 'Failed to save product');
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
      
      // Refresh products list
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

  const calculatePrice = (product: Product) => {
    // Calculate current gold rate for the product
    const goldRate = product.karatType === '24K' ? 3270.5 : 
                     product.karatType === '22K' ? 2997.13 : 
                     product.karatType === '21K' ? 2727.94 : 2452.88;
    
    const makingCharges = product.makingChargesApplicable ? (product.weight * goldRate * 0.15) : 0;
    return (product.weight * goldRate) + makingCharges;
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
              <Button className="touch-target pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
                <Plus className="mr-2 h-4 w-4" />
                Add Product
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
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
                    <Select value={productForm.categoryType} onValueChange={(value) => setProductForm({...productForm, categoryType: value as any})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select category" />
                      </SelectTrigger>
                      <SelectContent>
                        {categories.map(cat => (
                          <SelectItem key={cat} value={cat}>{cat}</SelectItem>
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
                    <Select value={productForm.karatType} onValueChange={(value) => setProductForm({...productForm, karatType: value as any})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select karat" />
                      </SelectTrigger>
                      <SelectContent>
                        {karats.map(karat => (
                          <SelectItem key={karat} value={karat}>{karat}</SelectItem>
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
                  <Label htmlFor="supplierId">Supplier ID</Label>
                  <Input
                    id="supplierId"
                    value={productForm.supplierId}
                    onChange={(e) => setProductForm({...productForm, supplierId: e.target.value})}
                    placeholder="Supplier ID"
                  />
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
                <Button variant="outline" onClick={() => setIsNewProductOpen(false)}>
                  Cancel
                </Button>
                <Button 
                  type="submit" 
                  className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]"
                  disabled={creatingProduct}
                >
                  {creatingProduct ? (
                    <>
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      Creating...
                    </>
                  ) : (
                    'Add Product'
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
              <SelectContent>
                <SelectItem value="all">All Categories</SelectItem>
                {categories.map(cat => (
                  <SelectItem key={cat} value={cat}>{cat}</SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select value={karatFilter} onValueChange={setKaratFilter}>
              <SelectTrigger className="w-full md:w-32">
                <SelectValue placeholder="Karat" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Karats</SelectItem>
                {karats.map(karat => (
                  <SelectItem key={karat} value={karat}>{karat}</SelectItem>
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
                  const estimatedPrice = calculatePrice(product);
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
                          <p>{product.categoryType}</p>
                          {product.subCategory && <p className="text-sm text-muted-foreground">{product.subCategory}</p>}
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline">{product.karatType}</Badge>
                      </TableCell>
                      <TableCell>{product.weight}g</TableCell>
                      <TableCell>
                        <div>
                          <p className="font-medium">{formatCurrency(estimatedPrice)}</p>
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
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem 
                              onClick={() => {
                                setSelectedProduct(product);
                                // Could add a view details dialog here
                              }}
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
                                >
                                  <Edit className="mr-2 h-4 w-4" />
                                  Edit Product
                                </DropdownMenuItem>
                                <DropdownMenuItem 
                                  onClick={() => handleDelete(product)}
                                  className="text-red-600"
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