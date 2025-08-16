import React, { useState } from 'react';
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
} from 'lucide-react';
import { useAuth } from './AuthContext';
import { formatCurrency } from './utils/currency';

interface Product {
  id: string;
  sku: string;
  name: string;
  category: string;
  subcategory: string;
  karat: string;
  weight: number;
  dimensions?: string;
  description: string;
  supplier: string;
  brand: string;
  basePrice: number;
  makingCharges: number;
  currentStock: number;
  minStock: number;
  maxStock: number;
  isActive: boolean;
  images?: string[];
  createdDate: string;
}

export default function Products() {
  const { isManager } = useAuth();
  const [searchQuery, setSearchQuery] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('all');
  const [karatFilter, setKaratFilter] = useState('all');
  const [isNewProductOpen, setIsNewProductOpen] = useState(false);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [isEditMode, setIsEditMode] = useState(false);

  // Form state for new/edit product
  const [productForm, setProductForm] = useState({
    sku: '',
    name: '',
    category: '',
    subcategory: '',
    karat: '',
    weight: '',
    dimensions: '',
    description: '',
    supplier: '',
    brand: '',
    basePrice: '',
    makingCharges: '',
    minStock: '',
    maxStock: '',
  });

  // Mock data
  const products: Product[] = [
    {
      id: '1',
      sku: 'GLD-RNG-001',
      name: 'Classic Gold Ring',
      category: 'Ring',
      subcategory: 'Wedding',
      karat: '22K',
      weight: 5.5,
      dimensions: 'Size 18',
      description: 'Classic 22K gold wedding ring with traditional design',
      supplier: 'Rajesh Jewellers',
      brand: 'DijaGold Premium',
      basePrice: 33000,
      makingCharges: 500,
      currentStock: 15,
      minStock: 5,
      maxStock: 50,
      isActive: true,
      createdDate: '2024-01-10T10:00:00Z',
    },
    {
      id: '2',
      sku: 'GLD-CHN-002',
      name: 'Designer Gold Chain',
      category: 'Chain',
      subcategory: 'Designer',
      karat: '22K',
      weight: 12.3,
      dimensions: '24 inches',
      description: 'Elegant designer chain with intricate pattern work',
      supplier: 'Mumbai Gold House',
      brand: 'DijaGold Classic',
      basePrice: 73800,
      makingCharges: 800,
      currentStock: 8,
      minStock: 3,
      maxStock: 25,
      isActive: true,
      createdDate: '2024-01-08T14:30:00Z',
    },
    {
      id: '3',
      sku: 'GLD-EAR-003',
      name: 'Pearl Drop Earrings',
      category: 'Earrings',
      subcategory: 'Designer',
      karat: '18K',
      weight: 3.2,
      dimensions: '2.5cm drop',
      description: '18K gold earrings with cultured pearl drops',
      supplier: 'Chennai Pearls',
      brand: 'DijaGold Elite',
      basePrice: 15680,
      makingCharges: 400,
      currentStock: 2,
      minStock: 5,
      maxStock: 20,
      isActive: true,
      createdDate: '2024-01-05T11:15:00Z',
    },
    {
      id: '4',
      sku: 'GLD-BNG-004',
      name: 'Traditional Bangles Set',
      category: 'Bangles',
      subcategory: 'Traditional',
      karat: '22K',
      weight: 25.6,
      dimensions: 'Set of 2 - 2.4 inches',
      description: 'Traditional 22K gold bangles with carved patterns',
      supplier: 'Rajesh Jewellers',
      brand: 'DijaGold Heritage',
      basePrice: 153600,
      makingCharges: 1200,
      currentStock: 4,
      minStock: 2,
      maxStock: 15,
      isActive: true,
      createdDate: '2024-01-03T16:45:00Z',
    },
  ];

  const categories = ['Ring', 'Chain', 'Earrings', 'Bangles', 'Necklace', 'Pendant', 'Bracelet'];
  const karats = ['14K', '18K', '22K', '24K'];

  const filteredProducts = products.filter(product => {
    const matchesSearch = 
      product.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      product.sku.toLowerCase().includes(searchQuery.toLowerCase()) ||
      product.description.toLowerCase().includes(searchQuery.toLowerCase());
    
    const matchesCategory = categoryFilter === 'all' || product.category === categoryFilter;
    const matchesKarat = karatFilter === 'all' || product.karat === karatFilter;
    
    return matchesSearch && matchesCategory && matchesKarat;
  });

  const getStockStatus = (product: Product) => {
    if (product.currentStock <= product.minStock) {
      return <Badge variant="destructive">Low Stock</Badge>;
    } else if (product.currentStock >= product.maxStock * 0.8) {
      return <Badge variant="default" className="bg-green-100 text-green-800">Good Stock</Badge>;
    } else {
      return <Badge variant="outline">Normal</Badge>;
    }
  };

  const handleCreateProduct = () => {
    if (!isManager) {
      alert('Only managers can create products');
      return;
    }
    // Mock product creation
    console.log('Creating product:', productForm);
    setIsNewProductOpen(false);
    resetForm();
  };

  const handleUpdateProduct = () => {
    if (!isManager) {
      alert('Only managers can update products');
      return;
    }
    // Mock product update
    console.log('Updating product:', selectedProduct?.id, productForm);
    setSelectedProduct(null);
    setIsEditMode(false);
    resetForm();
  };

  const handleDeleteProduct = (productId: string) => {
    if (!isManager) {
      alert('Only managers can delete products');
      return;
    }
    if (confirm('Are you sure you want to delete this product?')) {
      // Mock product deletion
      console.log('Deleting product:', productId);
    }
  };

  const resetForm = () => {
    setProductForm({
      sku: '',
      name: '',
      category: '',
      subcategory: '',
      karat: '',
      weight: '',
      dimensions: '',
      description: '',
      supplier: '',
      brand: '',
      basePrice: '',
      makingCharges: '',
      minStock: '',
      maxStock: '',
    });
  };

  const populateForm = (product: Product) => {
    setProductForm({
      sku: product.sku,
      name: product.name,
      category: product.category,
      subcategory: product.subcategory,
      karat: product.karat,
      weight: product.weight.toString(),
      dimensions: product.dimensions || '',
      description: product.description,
      supplier: product.supplier,
      brand: product.brand,
      basePrice: product.basePrice.toString(),
      makingCharges: product.makingCharges.toString(),
      minStock: product.minStock.toString(),
      maxStock: product.maxStock.toString(),
    });
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
                <DialogTitle>Add New Product</DialogTitle>
                <DialogDescription>
                  Enter product details to add to inventory
                </DialogDescription>
              </DialogHeader>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="sku">SKU</Label>
                  <Input
                    id="sku"
                    value={productForm.sku}
                    onChange={(e) => setProductForm({...productForm, sku: e.target.value})}
                    placeholder="GLD-XXX-XXX"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="name">Product Name</Label>
                  <Input
                    id="name"
                    value={productForm.name}
                    onChange={(e) => setProductForm({...productForm, name: e.target.value})}
                    placeholder="Enter product name"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="category">Category</Label>
                  <Select value={productForm.category} onValueChange={(value) => setProductForm({...productForm, category: value})}>
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
                  <Label htmlFor="subcategory">Subcategory</Label>
                  <Input
                    id="subcategory"
                    value={productForm.subcategory}
                    onChange={(e) => setProductForm({...productForm, subcategory: e.target.value})}
                    placeholder="Wedding, Designer, Traditional"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="karat">Karat</Label>
                  <Select value={productForm.karat} onValueChange={(value) => setProductForm({...productForm, karat: value})}>
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
                    step="0.1"
                    value={productForm.weight}
                    onChange={(e) => setProductForm({...productForm, weight: e.target.value})}
                    placeholder="0.0"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="dimensions">Dimensions</Label>
                  <Input
                    id="dimensions"
                    value={productForm.dimensions}
                    onChange={(e) => setProductForm({...productForm, dimensions: e.target.value})}
                    placeholder="Size, length, diameter"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="supplier">Supplier</Label>
                  <Input
                    id="supplier"
                    value={productForm.supplier}
                    onChange={(e) => setProductForm({...productForm, supplier: e.target.value})}
                    placeholder="Supplier name"
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
                  <Label htmlFor="basePrice">Base Price (EGP)</Label>
                  <Input
                    id="basePrice"
                    type="number"
                    value={productForm.basePrice}
                    onChange={(e) => setProductForm({...productForm, basePrice: e.target.value})}
                    placeholder="0"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="makingCharges">Making Charges (EGP)</Label>
                  <Input
                    id="makingCharges"
                    type="number"
                    value={productForm.makingCharges}
                    onChange={(e) => setProductForm({...productForm, makingCharges: e.target.value})}
                    placeholder="0"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="minStock">Minimum Stock</Label>
                  <Input
                    id="minStock"
                    type="number"
                    value={productForm.minStock}
                    onChange={(e) => setProductForm({...productForm, minStock: e.target.value})}
                    placeholder="0"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="maxStock">Maximum Stock</Label>
                  <Input
                    id="maxStock"
                    type="number"
                    value={productForm.maxStock}
                    onChange={(e) => setProductForm({...productForm, maxStock: e.target.value})}
                    placeholder="0"
                  />
                </div>
                <div className="col-span-2 space-y-2">
                  <Label htmlFor="description">Description</Label>
                  <Input
                    id="description"
                    value={productForm.description}
                    onChange={(e) => setProductForm({...productForm, description: e.target.value})}
                    placeholder="Detailed product description"
                    className="h-20"
                  />
                </div>
              </div>
              <div className="flex justify-end gap-3 mt-6">
                <Button variant="outline" onClick={() => setIsNewProductOpen(false)}>
                  Cancel
                </Button>
                <Button onClick={handleCreateProduct} className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
                  Add Product
                </Button>
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
            {filteredProducts.length} product(s) found
          </CardDescription>
        </CardHeader>
        <CardContent>
          {filteredProducts.length === 0 ? (
            <div className="text-center py-8">
              <Package className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <p className="text-muted-foreground">No products found</p>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>SKU</TableHead>
                  <TableHead>Product</TableHead>
                  <TableHead>Category</TableHead>
                  <TableHead>Karat</TableHead>
                  <TableHead>Weight</TableHead>
                  <TableHead>Price</TableHead>
                  <TableHead>Stock</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredProducts.map((product) => (
                  <TableRow key={product.id}>
                    <TableCell className="font-medium">{product.sku}</TableCell>
                    <TableCell>
                      <div>
                        <p className="font-medium">{product.name}</p>
                        <p className="text-sm text-muted-foreground">{product.brand}</p>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div>
                        <p>{product.category}</p>
                        <p className="text-sm text-muted-foreground">{product.subcategory}</p>
                      </div>
                    </TableCell>
                    <TableCell>{product.karat}</TableCell>
                    <TableCell>{product.weight}g</TableCell>
                    <TableCell>
                      <div>
                        <p>{formatCurrency(product.basePrice + product.makingCharges)}</p>
                        <p className="text-sm text-muted-foreground">
                          Base: {formatCurrency(product.basePrice)}
                        </p>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div>
                        <p>{product.currentStock} pcs</p>
                        {getStockStatus(product)}
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
                              populateForm(product);
                            }}
                          >
                            <Eye className="mr-2 h-4 w-4" />
                            View Details
                          </DropdownMenuItem>
                          {isManager && (
                            <>
                              <DropdownMenuItem 
                                onClick={() => {
                                  setSelectedProduct(product);
                                  populateForm(product);
                                  setIsEditMode(true);
                                }}
                              >
                                <Edit className="mr-2 h-4 w-4" />
                                Edit
                              </DropdownMenuItem>
                              <DropdownMenuItem 
                                onClick={() => handleDeleteProduct(product.id)}
                                className="text-destructive"
                              >
                                <Trash2 className="mr-2 h-4 w-4" />
                                Delete
                              </DropdownMenuItem>
                            </>
                          )}
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Product Details/Edit Dialog */}
      {selectedProduct && (
        <Dialog open={!!selectedProduct} onOpenChange={() => setSelectedProduct(null)}>
          <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
            <DialogHeader>
              <DialogTitle>
                {isEditMode ? 'Edit Product' : 'Product Details'} - {selectedProduct.sku}
              </DialogTitle>
            </DialogHeader>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="sku">SKU</Label>
                <Input
                  id="sku"
                  value={productForm.sku}
                  onChange={(e) => setProductForm({...productForm, sku: e.target.value})}
                  readOnly={!isEditMode}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="name">Product Name</Label>
                <Input
                  id="name"
                  value={productForm.name}
                  onChange={(e) => setProductForm({...productForm, name: e.target.value})}
                  readOnly={!isEditMode}
                />
              </div>
              {/* Similar form fields as in new product dialog */}
              <div className="space-y-2">
                <Label htmlFor="currentStock">Current Stock</Label>
                <div className="p-2 bg-muted rounded">
                  {selectedProduct.currentStock} pieces
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="totalValue">Total Value</Label>
                <div className="p-2 bg-muted rounded">
                  {formatCurrency(selectedProduct.currentStock * (selectedProduct.basePrice + selectedProduct.makingCharges))}
                </div>
              </div>
            </div>
            <div className="flex justify-end gap-3 mt-6">
              <Button variant="outline" onClick={() => {
                setSelectedProduct(null);
                setIsEditMode(false);
              }}>
                {isEditMode ? 'Cancel' : 'Close'}
              </Button>
              {isEditMode && isManager && (
                <Button onClick={handleUpdateProduct} className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
                  Update Product
                </Button>
              )}
              {!isEditMode && isManager && (
                <Button onClick={() => setIsEditMode(true)} className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
                  <Edit className="mr-2 h-4 w-4" />
                  Edit Product
                </Button>
              )}
            </div>
          </DialogContent>
        </Dialog>
      )}
    </div>
  );
}