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
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from './ui/dropdown-menu';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from './ui/alert-dialog';
import {
  Hammer,
  Plus,
  Search,
  Eye,
  Edit,
  MoreHorizontal,
  Loader2,
  Scale,
  DollarSign,
  Calendar,
  CheckCircle,
  XCircle,
  AlertTriangle,
  Play,
  Settings,
  Award,
  Clock,
  ArrowRight,
  Wrench,
  Package,
} from 'lucide-react';
import { useAuth } from './AuthContext';
import {
  useProducts,
  useProduct,
  useCreateProduct,
  useUpdateProduct,
  useProductManufacturingRecords,
  useProductManufacturingRecordsByProduct,
  useProductManufacturingRecordsByBatch,
  useProductManufacturingSummary,
  useCreateProductManufacturingRecord,
  useTransitionWorkflow,
  usePerformQualityCheck,
  usePerformFinalApproval,
  useWorkflowHistory,
  useAvailableTransitions,
  useAvailableRawGoldItems,
  useRemainingWeight,
  useCheckSufficientWeight,
  useDeleteProductManufacturingRecord,
  useBranches,
  useSearchTechnicians,
  useKaratTypes,
} from '../hooks/useApi';
import { Command, CommandEmpty, CommandInput, CommandItem, CommandList } from './ui/command';
import { CreateProductManufactureRawMaterialDto } from '../types/ownership';

// Extended interface for UI state
interface RawMaterialWithName extends CreateProductManufactureRawMaterialDto {
  productName: string;
}

const ProductManufacture: React.FC = () => {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState('overview');

  // State for manufacturing records
  const [manufacturingRecords, setManufacturingRecords] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [selectedRecord, setSelectedRecord] = useState<any>(null);
  const [purchaseOrderItems, setPurchaseOrderItems] = useState<any[]>([]);
  const [selectedProductPOItems, setSelectedProductPOItems] = useState<any[]>([]);
  const [technicians, setTechnicians] = useState<any[]>([]);

  // Dialog states
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const [viewDialogOpen, setViewDialogOpen] = useState(false);
  const [workflowDialogOpen, setWorkflowDialogOpen] = useState(false);
  const [qualityCheckDialogOpen, setQualityCheckDialogOpen] = useState(false);
  const [finalApprovalDialogOpen, setFinalApprovalDialogOpen] = useState(false);
  const [rawMaterialsDialogOpen, setRawMaterialsDialogOpen] = useState(false);
  const [workflowHistoryDialogOpen, setWorkflowHistoryDialogOpen] = useState(false);
  // Dialog-local filters
  const [selectedKaratTypeId, setSelectedKaratTypeId] = useState<string>('');
  const [productSearchTerm, setProductSearchTerm] = useState<string>('');
  const [debouncedProductSearch, setDebouncedProductSearch] = useState<string>('');

  // Workflow history and transitions state
  const [workflowHistory, setWorkflowHistory] = useState<any[]>([]);
  const [availableTransitions, setAvailableTransitions] = useState<string[]>([]);

  // Filtering/search state
  const [batchSearch, setBatchSearch] = useState<string>('');
  const [productFilterId, setProductFilterId] = useState<string>('');

  // Form states
  const [manufacturingForm, setManufacturingForm] = useState({
    productId: '',
    quantityToProduce: '1',
    sourceRawGoldPurchaseOrderItemId: '',
    consumedWeight: '',
    wastageWeight: '',
    manufacturingCostPerGram: '',
    batchNumber: '',
    manufacturingNotes: '',
    branchId: '',
    technicianId: 'none',
    priority: 'Normal',
    estimatedCompletionDate: ''
  });

  // Add loading state for PO items
  const [poItemsLoading, setPOItemsLoading] = useState(false);
  const [selectedItemRemainingWeight, setSelectedItemRemainingWeight] = useState<number | null>(null);

  const [workflowForm, setWorkflowForm] = useState({
    targetStatus: '',
    notes: ''
  });

  const [qualityCheckForm, setQualityCheckForm] = useState({
    passed: false,
    notes: ''
  });

  const [finalApprovalForm, setFinalApprovalForm] = useState({
    approved: false,
    notes: ''
  });

  // Raw materials state
  const [selectedRawMaterials, setSelectedRawMaterials] = useState<RawMaterialWithName[]>([]);
  const [rawMaterialForm, setRawMaterialForm] = useState({
    productId: 0,
    quantityUsed: 0,
    unitCost: 0,
    contributionPercentage: 0,
    sourceOwnershipId: 0,
    sourceType: 'PurchaseOrder' as 'PurchaseOrder' | 'Manufacturing' | 'Transfer',
    sourceId: 0,
    notes: ''
  });

  // API hooks
  const productsApi = useProducts();
  const productApi = useProduct();
  const manufacturingRecordsApi = useProductManufacturingRecords();
  const manufacturingByProductApi = useProductManufacturingRecordsByProduct();
  const manufacturingByBatchApi = useProductManufacturingRecordsByBatch();
  const manufacturingSummaryApi = useProductManufacturingSummary();
  const createRecordApi = useCreateProductManufacturingRecord();
  const transitionWorkflowApi = useTransitionWorkflow();
  const qualityCheckApi = usePerformQualityCheck();
  const finalApprovalApi = usePerformFinalApproval();
  const workflowHistoryApi = useWorkflowHistory();
  const availableTransitionsApi = useAvailableTransitions();
  const availableRawGoldItemsApi = useAvailableRawGoldItems();
  const remainingWeightApi = useRemainingWeight();
  const checkSufficientWeightApi = useCheckSufficientWeight();
  const deleteManufacturingApi = useDeleteProductManufacturingRecord();
  const branchesApi = useBranches();
  const karatTypesApi = useKaratTypes();
  const { 
    execute: searchTechnicians, 
    data: techniciansData, 
    loading: techniciansLoading 
  } = useSearchTechnicians();

  // Debounce product search term
  useEffect(() => {
    const handle = setTimeout(() => setDebouncedProductSearch(productSearchTerm.trim()), 300);
    return () => clearTimeout(handle);
  }, [productSearchTerm]);

  // Fetch karat types when create dialog opens and reset filters on close
  useEffect(() => {
    const loadKaratTypes = async () => {
      if (createDialogOpen) {
        try {
          // If hook exposes fetch, call it; otherwise assume initial load happens elsewhere
          if (typeof (karatTypesApi as any).fetchKaratTypes === 'function') {
            await (karatTypesApi as any).fetchKaratTypes();
          }
        } catch (e) {
          console.error('Failed to load karat types', e);
        }
      } else {
        setSelectedKaratTypeId('');
        setProductSearchTerm('');
        setDebouncedProductSearch('');
      }
    };
    loadKaratTypes();
  }, [createDialogOpen]);

  // Fetch products when filters change while dialog is open
  useEffect(() => {
    const fetchFilteredProducts = async () => {
      if (!createDialogOpen) return;
      try {
        await productsApi.execute({
          isActive: true,
          pageNumber: 1,
          pageSize: 50,
          ...(selectedKaratTypeId ? { karatTypeId: parseInt(selectedKaratTypeId) } : {}),
          ...(debouncedProductSearch ? { searchTerm: debouncedProductSearch } : {}),
        });
      } catch (error) {
        console.error('Error fetching filtered products:', error);
      }
    };
    fetchFilteredProducts();
  }, [createDialogOpen, selectedKaratTypeId, debouncedProductSearch]);

  // When karat type changes, clear selected product and raw gold source
  useEffect(() => {
    setManufacturingForm(prev => ({ ...prev, productId: '', sourceRawGoldPurchaseOrderItemId: '' }));
  }, [selectedKaratTypeId]);

  // Fetch raw gold purchase order items when product changes
  useEffect(() => {
    const fetchRawGoldItems = async () => {
      if (manufacturingForm.productId) {
        setPOItemsLoading(true);
        try {
          // Load selected product details to get its karat type
          const product = await productApi.execute(parseInt(manufacturingForm.productId));
          const productKaratTypeId = product?.karatTypeId ?? product?.karatType?.id;

          // Use backend endpoint for available raw gold items and filter by product karat type
          const availableItemsAll = await availableRawGoldItemsApi.execute();
          const availableItems = (availableItemsAll || []).filter((item: any) => {
            const available = (item.availableWeightForManufacturing ?? item.remainingWeight ?? 0) > 0;
            return available && (productKaratTypeId ? item.karatTypeId === productKaratTypeId : true);
          });
          setSelectedProductPOItems(availableItems);
          if (availableItems.length === 0) {
            console.log('No available raw gold items found for this product karat type');
          }
        } catch (error) {
          console.error('Error fetching raw gold items:', error);
          setSelectedProductPOItems([]);
        } finally {
          setPOItemsLoading(false);
        }
      } else {
        setSelectedProductPOItems([]);
        setManufacturingForm(prev => ({ ...prev, sourceRawGoldPurchaseOrderItemId: '' }));
        setSelectedItemRemainingWeight(null);
      }
    };
    fetchRawGoldItems();
  }, [manufacturingForm.productId]);

  // When source PO item changes, fetch remaining weight
  useEffect(() => {
    const fetchRemaining = async () => {
      try {
        if (manufacturingForm.sourceRawGoldPurchaseOrderItemId) {
          const remaining = await remainingWeightApi.execute(parseInt(manufacturingForm.sourceRawGoldPurchaseOrderItemId));
          setSelectedItemRemainingWeight(typeof remaining === 'number' ? remaining : null);
        } else {
          setSelectedItemRemainingWeight(null);
        }
      } catch (e) {
        console.error('Failed to fetch remaining weight', e);
        setSelectedItemRemainingWeight(null);
      }
    };
    fetchRemaining();
  }, [manufacturingForm.sourceRawGoldPurchaseOrderItemId]);

  // Fetch data on component mount
  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        // Fetch products, manufacturing records, branches, and technicians in parallel
        const [products, records, branches] = await Promise.all([
          productsApi.execute(),
          manufacturingRecordsApi.execute(),
          branchesApi.execute()
        ]);
        setManufacturingRecords(records || []);

      } catch (error) {
        console.error('Error fetching data:', error);
        setManufacturingRecords([]);
        setTechnicians([]);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  // Fetch technicians separately on mount
  useEffect(() => {
    const fetchTechnicians = async () => {
      try {
        console.log('Fetching technicians...');
        await searchTechnicians({
          pageSize: 100,
          isActive: true
        });
      } catch (error) {
        console.error('Error fetching technicians:', error);
        setTechnicians([]);
      }
    };

    fetchTechnicians();
  }, [searchTechnicians]);

  // Update technicians when data changes
  useEffect(() => {
    if (techniciansData?.items) {
      console.log('Technicians data received:', techniciansData.items);
      setTechnicians(techniciansData.items);
    }
  }, [techniciansData]);

  // Debug technicians state
  useEffect(() => {
    console.log('Current technicians state:', technicians);
  }, [technicians]);

  // Handle create manufacturing record
  const handleCreateRecord = async () => {
    if (!manufacturingForm.productId || !manufacturingForm.sourceRawGoldPurchaseOrderItemId) {
      alert('Please select a product and raw gold purchase order item');
      return;
    }

    if (!manufacturingForm.branchId) {
      alert('Please select a branch');
      return;
    }

    if (parseFloat(manufacturingForm.consumedWeight) <= 0) {
      alert('Please enter a valid consumed weight');
      return;
    }

    // Check sufficient weight from backend before submitting
    try {
      const sufficient = await checkSufficientWeightApi.execute(
        parseInt(manufacturingForm.sourceRawGoldPurchaseOrderItemId),
        parseFloat(manufacturingForm.consumedWeight)
      );
      if (!sufficient) {
        alert('Insufficient remaining weight on the selected raw gold item for the requested consumed weight.');
        return;
      }
    } catch (e) {
      console.error('Failed to check sufficient weight', e);
      alert('Failed to validate available weight. Please try again.');
      return;
    }

    if (isNaN(parseFloat(manufacturingForm.manufacturingCostPerGram)) || parseFloat(manufacturingForm.manufacturingCostPerGram) < 0) {
      alert('Please enter a valid manufacturing cost per gram');
      return;
    }

    setLoading(true);
    try {
      const manufacturingData = {
        ProductId: parseInt(manufacturingForm.productId),
        QuantityToProduce: parseInt(manufacturingForm.quantityToProduce) || 1,
        SourceRawGoldPurchaseOrderItemId: parseInt(manufacturingForm.sourceRawGoldPurchaseOrderItemId),
        ConsumedWeight: parseFloat(manufacturingForm.consumedWeight),
        WastageWeight: parseFloat(manufacturingForm.wastageWeight) || 0,
        ManufacturingCostPerGram: parseFloat(manufacturingForm.manufacturingCostPerGram) || 0,
        TotalManufacturingCost: 0, // Will be calculated by backend
        BatchNumber: manufacturingForm.batchNumber || '',
        ManufacturingNotes: manufacturingForm.manufacturingNotes || '',
        Status: 'Completed',
        BranchId: parseInt(manufacturingForm.branchId) || 1, // Default to branch 1 if not selected
        TechnicianId: manufacturingForm.technicianId && manufacturingForm.technicianId !== '' && manufacturingForm.technicianId !== 'none' ? parseInt(manufacturingForm.technicianId) : undefined,
        Priority: manufacturingForm.priority,
        EstimatedCompletionDate: manufacturingForm.estimatedCompletionDate || undefined
      };

      const newRecord = await createRecordApi.execute(manufacturingData);
      // Add new record to the list
      setManufacturingRecords(prev => [newRecord, ...prev]);
      setCreateDialogOpen(false);
      resetManufacturingForm();
    } catch (error) {
      console.error('Error creating manufacturing record:', error);
      alert('Failed to create manufacturing record');
    } finally {
      setLoading(false);
    }
  };

  // Reset create form
  const resetManufacturingForm = () => {
    setManufacturingForm({
      productId: '',
      quantityToProduce: '1',
      sourceRawGoldPurchaseOrderItemId: '',
      consumedWeight: '',
      wastageWeight: '',
      manufacturingCostPerGram: '',
      batchNumber: '',
      manufacturingNotes: '',
      branchId: '',
      technicianId: 'none',
      priority: 'Normal',
      estimatedCompletionDate: ''
    });
    setSelectedProductPOItems([]);
  };

  // Handle workflow transition
  const handleWorkflowTransition = async () => {
    if (!selectedRecord || !workflowForm.targetStatus) return;

    setLoading(true);
    try {
      const updatedRecord = await transitionWorkflowApi.execute(selectedRecord.id, {
        TargetStatus: workflowForm.targetStatus,
        Notes: workflowForm.notes
      });
      // Update the record in the list
      setManufacturingRecords(prev => 
        prev.map(record => 
          record.id === selectedRecord.id ? updatedRecord : record
        )
      );
      setWorkflowDialogOpen(false);
      setSelectedRecord(null);
    } catch (error) {
      console.error('Error transitioning workflow:', error);
      alert('Failed to transition workflow');
    } finally {
      setLoading(false);
    }
  };

  // Handle quality check
  const handleQualityCheck = async () => {
    if (!selectedRecord) return;

    setLoading(true);
    try {
      const updatedRecord = await qualityCheckApi.execute(selectedRecord.id, {
        Passed: qualityCheckForm.passed,
        Notes: qualityCheckForm.notes
      });
      // Update the record in the list
      setManufacturingRecords(prev => 
        prev.map(record => 
          record.id === selectedRecord.id ? updatedRecord : record
        )
      );
      setQualityCheckDialogOpen(false);
      setSelectedRecord(null);
    } catch (error) {
      console.error('Error performing quality check:', error);
      alert('Failed to perform quality check');
    } finally {
      setLoading(false);
    }
  };

  // Handle final approval
  const handleFinalApproval = async () => {
    if (!selectedRecord) return;

    setLoading(true);
    try {
      const updatedRecord = await finalApprovalApi.execute(selectedRecord.id, {
        Approved: finalApprovalForm.approved,
        Notes: finalApprovalForm.notes
      });
      // Update the record in the list
      setManufacturingRecords(prev => 
        prev.map(record => 
          record.id === selectedRecord.id ? updatedRecord : record
        )
      );
      setFinalApprovalDialogOpen(false);
      setSelectedRecord(null);
    } catch (error) {
      console.error('Error performing final approval:', error);
      alert('Failed to perform final approval');
    } finally {
      setLoading(false);
    }
  };

  // Fetch workflow history for selected record
  const fetchWorkflowHistory = async (recordId: number) => {
    try {
      const history = await workflowHistoryApi.execute(recordId);
      setWorkflowHistory(history || []);
    } catch (error) {
      console.error('Error fetching workflow history:', error);
      setWorkflowHistory([]);
    }
  };

  // Fetch available transitions for selected record
  const fetchAvailableTransitions = async (recordId: number) => {
    try {
      const transitions = await availableTransitionsApi.execute(recordId);
      setAvailableTransitions(transitions || []);
    } catch (error) {
      console.error('Error fetching available transitions:', error);
      setAvailableTransitions([]);
    }
  };

  // Handle viewing workflow history
  const handleViewWorkflowHistory = async (record: any) => {
    setSelectedRecord(record);
    await fetchWorkflowHistory(record.id);
    setWorkflowHistoryDialogOpen(true);
  };

  // Raw materials handlers
  const addRawMaterial = () => {
    if (!rawMaterialForm.productId || rawMaterialForm.quantityUsed <= 0) {
      alert('Please select a product and enter a valid quantity');
      return;
    }

    const selectedProduct = productsApi.data?.items?.find(p => p.id === rawMaterialForm.productId);
    if (!selectedProduct) return;

    const newMaterial = {
      ...rawMaterialForm,
      productName: selectedProduct.name
    };

    setSelectedRawMaterials(prev => [...prev, newMaterial]);
    setRawMaterialForm({
      productId: 0,
      quantityUsed: 0,
      unitCost: 0,
      contributionPercentage: 0,
      sourceOwnershipId: 0,
      sourceType: 'PurchaseOrder' as 'PurchaseOrder' | 'Manufacturing' | 'Transfer',
      sourceId: 0,
      notes: ''
    });
    setRawMaterialsDialogOpen(false);
  };

  const removeRawMaterial = (index: number) => {
    setSelectedRawMaterials(prev => prev.filter((_, i) => i !== index));
  };

  // Get status badge
  const getStatusBadge = (status: string) => {
    switch (status.toLowerCase()) {
      case 'draft':
        return <Badge variant="outline"><Clock className="w-3 h-3 mr-1" />Draft</Badge>;
      case 'in progress':
        return <Badge className="bg-blue-100 text-blue-800"><Play className="w-3 h-3 mr-1" />In Progress</Badge>;
      case 'quality check':
        return <Badge className="bg-yellow-100 text-yellow-800"><Settings className="w-3 h-3 mr-1" />Quality Check</Badge>;
      case 'final approval':
        return <Badge className="bg-purple-100 text-purple-800"><Award className="w-3 h-3 mr-1" />Final Approval</Badge>;
      case 'completed':
        return <Badge className="bg-green-100 text-green-800"><CheckCircle className="w-3 h-3 mr-1" />Completed</Badge>;
      case 'cancelled':
        return <Badge className="bg-red-100 text-red-800"><XCircle className="w-3 h-3 mr-1" />Cancelled</Badge>;
      default:
        return <Badge variant="outline">{status}</Badge>;
    }
  };

  // Handle opening workflow dialog with available transitions
  const handleOpenWorkflowDialog = async (record: any) => {
    setSelectedRecord(record);
    await fetchAvailableTransitions(record.id);
    setWorkflowDialogOpen(true);
  };

  // Get workflow actions based on current status
  const getWorkflowActions = (record: any) => {
    switch (record.status.toLowerCase()) {
      case 'draft':
        return ['Start Manufacturing'];
      case 'in progress':
        return ['Complete Manufacturing', 'Quality Check'];
      case 'quality check':
        return ['Approve', 'Reject'];
      case 'final approval':
        return ['Complete', 'Reject'];
      default:
        return [];
    }
  };

  // Delete manufacturing record
  const handleDeleteRecord = async (record: any) => {
    const confirmed = window.confirm('Are you sure you want to delete this manufacturing record?');
    if (!confirmed) return;
    try {
      setLoading(true);
      const ok = await deleteManufacturingApi.execute(record.id);
      if (ok) {
        setManufacturingRecords(prev => prev.filter(r => r.id !== record.id));
      } else {
        alert('Failed to delete record');
      }
    } catch (e) {
      console.error('Failed to delete record', e);
      alert('Failed to delete record');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Product Manufacturing</h1>
          <p className="text-muted-foreground">
            Manage product manufacturing processes and workflow
          </p>
        </div>
        <Button onClick={() => setCreateDialogOpen(true)}>
          <Plus className="w-4 h-4 mr-2" />
          New Manufacturing Record
        </Button>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Records</CardTitle>
            <Hammer className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{manufacturingRecords.length}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">In Progress</CardTitle>
            <Play className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {manufacturingRecords.filter(r => r.status === 'In Progress').length}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Completed</CardTitle>
            <CheckCircle className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {manufacturingRecords.filter(r => r.status === 'Completed').length}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Cost</CardTitle>
            <DollarSign className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              ${manufacturingRecords.reduce((sum, r) => sum + (r.totalManufacturingCost || 0), 0).toFixed(2)}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Main Content */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="records">Manufacturing Records</TabsTrigger>
          <TabsTrigger value="raw-materials">Raw Materials</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-4">
          {/* Manufacturing Overview */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Workflow Summary</CardTitle>
                <CardDescription>
                  Current status of manufacturing records
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {['Draft', 'In Progress', 'Quality Check', 'Final Approval', 'Completed'].map(status => {
                    const count = manufacturingRecords.filter(r => r.status === status).length;
                    return (
                      <div key={status} className="flex justify-between items-center">
                        <div className="flex items-center gap-2">
                          {getStatusBadge(status)}
                          <span className="text-sm">{status}</span>
                        </div>
                        <span className="font-medium">{count}</span>
                      </div>
                    );
                  })}
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Recent Activity</CardTitle>
                <CardDescription>
                  Latest manufacturing activities
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  {manufacturingRecords.slice(0, 5).map(record => (
                    <div key={record.id} className="flex items-center justify-between p-2 border rounded">
                      <div>
                        <p className="text-sm font-medium">{record.productName}</p>
                        <p className="text-xs text-muted-foreground">Batch: {record.batchNumber}</p>
                      </div>
                      <div className="text-right">
                        <div className="flex items-center gap-2 justify-end">
                          {getStatusBadge(record.status)}
                          <Button
                            variant="outline"
                            size="sm"
                            className="h-7 px-2 text-xs"
                            onClick={() => handleOpenWorkflowDialog(record)}
                          >
                            Change
                          </Button>
                        </div>
                        <p className="text-xs text-muted-foreground mt-1">
                          {new Date(record.createdAt).toLocaleDateString()}
                        </p>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="records" className="space-y-4">
          {/* Manufacturing Records List */}
          <Card>
            <CardHeader>
              <CardTitle>Manufacturing Records</CardTitle>
              <CardDescription>
                All manufacturing records and their current status
              </CardDescription>
            </CardHeader>
            <CardContent>
              {/* Filters */}
              <div className="flex flex-col md:flex-row gap-3 mb-4">
                <div className="flex items-center gap-2">
                  <Label htmlFor="filterProduct">Filter by Product</Label>
                  <Select value={productFilterId} onValueChange={setProductFilterId}>
                    <SelectTrigger className="w-[240px]">
                      <SelectValue placeholder="Select a product" />
                    </SelectTrigger>
                    <SelectContent>
                      {(productsApi.data?.items || []).map((p: any) => (
                        <SelectItem key={p.id} value={String(p.id)}>{p.name}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <Button
                    variant="secondary"
                    size="sm"
                    onClick={async () => {
                      try {
                        if (productFilterId) {
                          const byProd = await manufacturingByProductApi.execute(parseInt(productFilterId));
                          setManufacturingRecords(byProd || []);
                          // Optionally fetch summary
                          await manufacturingSummaryApi.execute(parseInt(productFilterId));
                        } else {
                          const all = await manufacturingRecordsApi.execute();
                          setManufacturingRecords(all || []);
                        }
                      } catch (e) {
                        console.error('Failed to filter by product', e);
                      }
                    }}
                  >
                    Apply
                  </Button>
                </div>
                <div className="flex items-center gap-2">
                  <Label htmlFor="batchSearch">Search by Batch</Label>
                  <Input
                    id="batchSearch"
                    placeholder="Enter batch number"
                    value={batchSearch}
                    onChange={e => setBatchSearch(e.target.value)}
                    className="w-[240px]"
                  />
                  <Button
                    variant="secondary"
                    size="sm"
                    onClick={async () => {
                      try {
                        if (batchSearch.trim()) {
                          const byBatch = await manufacturingByBatchApi.execute(batchSearch.trim());
                          setManufacturingRecords(byBatch || []);
                        } else {
                          const all = await manufacturingRecordsApi.execute();
                          setManufacturingRecords(all || []);
                        }
                      } catch (e) {
                        console.error('Failed to search by batch', e);
                      }
                    }}
                  >
                    Search
                  </Button>
                </div>
              </div>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Product</TableHead>
                    <TableHead>Batch #</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Weight Used</TableHead>
                    <TableHead>Total Cost</TableHead>
                    <TableHead>Created</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {manufacturingRecords.map((record) => (
                    <TableRow key={record.id}>
                      <TableCell>
                        <div>
                          <p className="font-medium">{record.productName}</p>
                          <p className="text-sm text-muted-foreground">{record.productCode}</p>
                        </div>
                      </TableCell>
                      <TableCell className="font-medium">{record.batchNumber}</TableCell>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          {getStatusBadge(record.status)}
                          <Button
                            variant="outline"
                            size="sm"
                            className="h-7 px-2 text-xs"
                            onClick={() => handleOpenWorkflowDialog(record)}
                          >
                            Change
                          </Button>
                        </div>
                      </TableCell>
                      <TableCell>{record.consumedWeight}g</TableCell>
                      <TableCell>${record.totalManufacturingCost.toFixed(2)}</TableCell>
                      <TableCell>{new Date(record.createdAt).toLocaleDateString()}</TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          <Button
                            variant="secondary"
                            size="sm"
                            onClick={() => handleOpenWorkflowDialog(record)}
                          >
                            Change Status
                          </Button>
                          <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                              <Button variant="ghost" size="sm">
                                <MoreHorizontal className="w-4 h-4" />
                              </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="end">
                              <DropdownMenuItem onClick={() => {
                                setSelectedRecord(record);
                                setViewDialogOpen(true);
                              }}>
                                <Eye className="w-4 h-4 mr-2" />
                                View Details
                              </DropdownMenuItem>
                              <DropdownMenuItem onClick={() => handleViewWorkflowHistory(record)}>
                                <Clock className="w-4 h-4 mr-2" />
                                Workflow History
                              </DropdownMenuItem>
                              <DropdownMenuItem onClick={() => handleOpenWorkflowDialog(record)}>
                                <ArrowRight className="w-4 h-4 mr-2" />
                                Transition Workflow
                              </DropdownMenuItem>
                              {record.status === 'Quality Check' && (
                                <DropdownMenuItem onClick={() => {
                                  setSelectedRecord(record);
                                  setQualityCheckDialogOpen(true);
                                }}>
                                  <Settings className="w-4 h-4 mr-2" />
                                  Quality Check
                                </DropdownMenuItem>
                              )}
                              {record.status === 'Final Approval' && (
                                <DropdownMenuItem onClick={() => {
                                  setSelectedRecord(record);
                                  setFinalApprovalDialogOpen(true);
                                }}>
                                  <Award className="w-4 h-4 mr-2" />
                                  Final Approval
                                </DropdownMenuItem>
                              )}
                              <DropdownMenuItem onClick={() => handleDeleteRecord(record)}>
                                <XCircle className="w-4 h-4 mr-2" />
                                Delete
                              </DropdownMenuItem>
                            </DropdownMenuContent>
                          </DropdownMenu>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="raw-materials" className="space-y-4">
          {/* Raw Materials Management */}
          <Card>
            <CardHeader>
              <CardTitle>Raw Materials Selection</CardTitle>
              <CardDescription>
                Select multiple raw materials for manufacturing with contribution percentages
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="flex justify-between items-center">
                  <h4 className="text-sm font-medium">Selected Raw Materials</h4>
                  <Button size="sm" onClick={() => setRawMaterialsDialogOpen(true)}>
                    <Plus className="w-4 h-4 mr-2" />
                    Add Raw Material
                  </Button>
                </div>
                
                {selectedRawMaterials.length === 0 ? (
                  <div className="text-center py-8 text-muted-foreground">
                    <Package className="w-12 h-12 mx-auto mb-4 opacity-50" />
                    <p>No raw materials selected</p>
                    <p className="text-sm mt-2">Add raw materials to track their usage in manufacturing</p>
                  </div>
                ) : (
                  <div className="space-y-3">
                    {selectedRawMaterials.map((material, index) => (
                      <div key={index} className="flex items-center justify-between p-3 border rounded">
                        <div className="flex-1">
                          <p className="font-medium">{material.productName}</p>
                          <p className="text-sm text-muted-foreground">
                            Quantity: {material.quantityUsed} | Cost: ${material.unitCost.toFixed(2)}
                          </p>
                        </div>
                        <div className="flex items-center gap-2">
                          <Badge variant="outline">{material.contributionPercentage}%</Badge>
                          <Button
                            size="sm"
                            variant="ghost"
                            onClick={() => removeRawMaterial(index)}
                          >
                            <XCircle className="w-4 h-4" />
                          </Button>
                        </div>
                      </div>
                    ))}
                    <div className="flex justify-between items-center p-2 bg-muted rounded">
                      <span className="text-sm font-medium">Total Contribution:</span>
                      <span className="text-sm font-medium">
                        {selectedRawMaterials.reduce((sum, m) => sum + m.contributionPercentage, 0)}%
                      </span>
                    </div>
                  </div>
                )}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Create Manufacturing Record Dialog */}
      <Dialog open={createDialogOpen} onOpenChange={setCreateDialogOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Create Manufacturing Record</DialogTitle>
            <DialogDescription>
              Start a new manufacturing process for a product
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              {/* Karat Type Filter */}
              <div className="space-y-2">
                <Label htmlFor="karatType">Karat Type</Label>
                <Select
                  value={selectedKaratTypeId}
                  onValueChange={(value) => setSelectedKaratTypeId(value)}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="All karat types" />
                  </SelectTrigger>
                  <SelectContent>
                    {karatTypesApi.data?.map((kt: any) => (
                      <SelectItem key={kt.id} value={kt.id.toString()}>
                        {kt.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {/* Selected Product quick info */}
                {manufacturingForm.productId && (
                  <div className="mt-2 rounded border bg-muted/30 p-2">
                    {productApi.loading ? (
                      <p className="text-xs text-muted-foreground">Loading product info...</p>
                    ) : productApi.data ? (
                      <div className="space-y-1 text-xs">
                        <div><span className="font-medium">Name:</span> {productApi.data.name}</div>
                        <div><span className="font-medium">Code:</span> {productApi.data.productCode}</div>
                        <div><span className="font-medium">Weight:</span> {productApi.data.weight} g</div>
                      </div>
                    ) : (
                      <p className="text-xs text-muted-foreground">No product details available.</p>
                    )}
                  </div>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="product">Product *</Label>
                <div className="border rounded">
                  <Command>
                    <CommandInput
                      placeholder="Search products by name or code..."
                      value={productSearchTerm}
                      onValueChange={setProductSearchTerm}
                    />
                    <CommandList>
                      <CommandEmpty>
                        {productsApi.loading ? 'Loading...' : 'No products found'}
                      </CommandEmpty>
                      {productsApi.data?.items?.map((product: any) => (
                        <CommandItem
                          key={product.id}
                          value={`${product.name} ${product.productCode}`}
                          onSelect={() => setManufacturingForm(prev => ({ ...prev, productId: product.id.toString() }))}
                        >
                          <div className="flex justify-between w-full">
                            <span>{product.name} ({product.productCode})</span>
                            <span className="text-xs text-muted-foreground">{product.karatType?.name || ''}</span>
                          </div>
                        </CommandItem>
                      ))}
                    </CommandList>
                  </Command>
                </div>
                {manufacturingForm.productId && (
                  <p className="text-xs text-muted-foreground">
                    Selected Product ID: {manufacturingForm.productId}
                  </p>
                )}
                <Select
                  value={manufacturingForm.sourceRawGoldPurchaseOrderItemId}
                  onValueChange={(value) => setManufacturingForm(prev => ({ ...prev, sourceRawGoldPurchaseOrderItemId: value }))}
                >
                  <SelectTrigger>
                    <SelectValue placeholder={
                      !manufacturingForm.productId ? "Select product first" :
                      poItemsLoading ? "Loading raw gold items..." :
                      selectedProductPOItems.length === 0 ? "No compatible raw gold items found" :
                      "Select raw gold source"
                    } />
                  </SelectTrigger>
                  <SelectContent>
                    {selectedProductPOItems.map((item) => (
                      <SelectItem key={item.id} value={item.id.toString()}>
                        {item.karatTypeName || item.karatType?.name || 'Unknown Karat'} Gold - {item.supplierName || 'Supplier'} - PO: {item.purchaseOrderNumber} ({(item.availableWeightForManufacturing ?? item.remainingWeight ?? 0)}g available)
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {selectedItemRemainingWeight !== null && manufacturingForm.sourceRawGoldPurchaseOrderItemId && (
                  <p className="text-xs text-muted-foreground mt-1">
                    Remaining weight for selected item: {selectedItemRemainingWeight} g
                  </p>
                )}
              </div>
            </div>
            
            <div className="grid grid-cols-3 gap-4">
              <div className="space-y-2">
                <Label htmlFor="quantityToProduce">Quantity to Produce *</Label>
                <Input
                  id="quantityToProduce"
                  type="number"
                  min="1"
                  value={manufacturingForm.quantityToProduce}
                  onChange={(e) => setManufacturingForm(prev => ({ ...prev, quantityToProduce: e.target.value }))}
                  placeholder="Number of pieces to produce"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="branch">Branch *</Label>
                <Select
                  value={manufacturingForm.branchId}
                  onValueChange={(value) => setManufacturingForm(prev => ({ ...prev, branchId: value }))}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select branch" />
                  </SelectTrigger>
                  <SelectContent>
                    {branchesApi.data?.items?.map((branch) => (
                      <SelectItem key={branch.id} value={branch.id.toString()}>
                        {branch.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="technician">Technician</Label>
                <Select
                  value={manufacturingForm.technicianId}
                  onValueChange={(value) => setManufacturingForm(prev => ({ ...prev, technicianId: value }))}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select technician" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="none">No technician assigned</SelectItem>
                    {technicians?.map((technician: any) => (
                      <SelectItem key={technician.id} value={technician.id.toString()}>
                        {technician.fullName}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
            
            <div className="grid grid-cols-3 gap-4">
              <div className="space-y-2">
                <Label htmlFor="batchNumber">Batch Number</Label>
                <Input
                  id="batchNumber"
                  placeholder="Enter batch number"
                  value={manufacturingForm.batchNumber}
                  onChange={(e) => setManufacturingForm(prev => ({ ...prev, batchNumber: e.target.value }))}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="priority">Priority</Label>
                <Select
                  value={manufacturingForm.priority}
                  onValueChange={(value) => setManufacturingForm(prev => ({ ...prev, priority: value }))}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select priority" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Low">Low</SelectItem>
                    <SelectItem value="Normal">Normal</SelectItem>
                    <SelectItem value="High">High</SelectItem>
                    <SelectItem value="Urgent">Urgent</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="estimatedCompletionDate">Estimated Completion Date</Label>
                <Input
                  id="estimatedCompletionDate"
                  type="date"
                  value={manufacturingForm.estimatedCompletionDate}
                  onChange={(e) => setManufacturingForm(prev => ({ ...prev, estimatedCompletionDate: e.target.value }))}
                  placeholder="Select estimated completion date"
                />
              </div>
            </div>

            <div className="grid grid-cols-3 gap-4">
              <div className="space-y-2">
                <Label htmlFor="consumedWeight">Consumed Weight (grams) *</Label>
                <Input
                  id="consumedWeight"
                  type="number"
                  step="0.001"
                  value={manufacturingForm.consumedWeight}
                  onChange={(e) => setManufacturingForm(prev => ({ ...prev, consumedWeight: e.target.value }))}
                  placeholder="Enter consumed weight in grams"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="wastageWeight">Wastage Weight (grams)</Label>
                <Input
                  id="wastageWeight"
                  type="number"
                  step="0.001"
                  value={manufacturingForm.wastageWeight}
                  onChange={(e) => setManufacturingForm(prev => ({ ...prev, wastageWeight: e.target.value }))}
                  placeholder="Enter wastage weight in grams"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="manufacturingCostPerGram">Manufacturing Cost Per Gram</Label>
                <Input
                  id="manufacturingCostPerGram"
                  type="number"
                  step="0.01"
                  value={manufacturingForm.manufacturingCostPerGram}
                  onChange={(e) => setManufacturingForm(prev => ({ ...prev, manufacturingCostPerGram: e.target.value }))}
                  placeholder="Enter manufacturing cost per gram"
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="notes">Manufacturing Notes</Label>
              <Textarea
                id="notes"
                placeholder="Enter any special instructions, quality notes, or observations for this manufacturing batch"
                value={manufacturingForm.manufacturingNotes}
                onChange={(e) => setManufacturingForm(prev => ({ ...prev, manufacturingNotes: e.target.value }))}
              />
            </div>

            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setCreateDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleCreateRecord} disabled={loading}>
                {loading && <Loader2 className="w-4 h-4 mr-2 animate-spin" />}
                Create Manufacturing Record
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* View Record Dialog */}
      <Dialog open={viewDialogOpen} onOpenChange={setViewDialogOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Manufacturing Record Details</DialogTitle>
            <DialogDescription>
              {selectedRecord?.productName} - {selectedRecord?.batchNumber}
            </DialogDescription>
          </DialogHeader>
          {selectedRecord && (
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Product</Label>
                  <p className="text-sm">{selectedRecord.productName}</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Batch Number</Label>
                  <p className="text-sm">{selectedRecord.batchNumber}</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Status</Label>
                  <div className="mt-1">{getStatusBadge(selectedRecord.status)}</div>
                </div>
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Weight Used</Label>
                  <p className="text-sm">{selectedRecord.consumedWeight}g</p>
                </div>
              </div>

              <div className="grid grid-cols-3 gap-4">
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Manufacturing Cost Per Gram</Label>
                  <p className="text-sm">${selectedRecord.manufacturingCostPerGram.toFixed(2)}</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Wastage Weight</Label>
                  <p className="text-sm">{selectedRecord.wastageWeight}g</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-muted-foreground">Total Cost</Label>
                  <p className="text-sm font-medium">${selectedRecord.totalManufacturingCost.toFixed(2)}</p>
                </div>
              </div>

              <div>
                <Label className="text-sm font-medium text-muted-foreground">Notes</Label>
                <p className="text-sm mt-1">{selectedRecord.manufacturingNotes}</p>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>

      {/* Workflow Transition Dialog */}
      <Dialog open={workflowDialogOpen} onOpenChange={setWorkflowDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Workflow Transition</DialogTitle>
            <DialogDescription>
              Move {selectedRecord?.productName} to next stage
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <Label>Current Status</Label>
              <div>{selectedRecord && getStatusBadge(selectedRecord.status)}</div>
            </div>
            <div className="space-y-2">
              <Label>Target Status</Label>
              <Select
                value={workflowForm.targetStatus}
                onValueChange={(value) => setWorkflowForm(prev => ({ ...prev, targetStatus: value }))}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select target status" />
                </SelectTrigger>
                <SelectContent>
                  {availableTransitions.map(transition => (
                    <SelectItem key={transition} value={transition}>
                      {transition}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="workflowNotes">Notes</Label>
              <Textarea
                id="workflowNotes"
                placeholder="Notes about this transition"
                value={workflowForm.notes}
                onChange={(e) => setWorkflowForm(prev => ({ ...prev, notes: e.target.value }))}
              />
            </div>
            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setWorkflowDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleWorkflowTransition} disabled={loading}>
                {loading && <Loader2 className="w-4 h-4 mr-2 animate-spin" />}
                Transition
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Quality Check Dialog */}
      <Dialog open={qualityCheckDialogOpen} onOpenChange={setQualityCheckDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Quality Check</DialogTitle>
            <DialogDescription>
              Perform quality check for {selectedRecord?.productName}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <Label>Quality Result</Label>
              <Select
                value={qualityCheckForm.passed.toString()}
                onValueChange={(value) => setQualityCheckForm(prev => ({ ...prev, passed: value === 'true' }))}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select result" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="true">Pass</SelectItem>
                  <SelectItem value="false">Fail</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="qualityNotes">Notes</Label>
              <Textarea
                id="qualityNotes"
                placeholder="Quality check notes and observations"
                value={qualityCheckForm.notes}
                onChange={(e) => setQualityCheckForm(prev => ({ ...prev, notes: e.target.value }))}
              />
            </div>
            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setQualityCheckDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleQualityCheck} disabled={loading}>
                {loading && <Loader2 className="w-4 h-4 mr-2 animate-spin" />}
                Submit Check
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Final Approval Dialog */}
      <Dialog open={finalApprovalDialogOpen} onOpenChange={setFinalApprovalDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Final Approval</DialogTitle>
            <DialogDescription>
              Final approval for {selectedRecord?.productName}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <Label>Approval Decision</Label>
              <Select
                value={finalApprovalForm.approved.toString()}
                onValueChange={(value) => setFinalApprovalForm(prev => ({ ...prev, approved: value === 'true' }))}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select decision" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="true">Approve</SelectItem>
                  <SelectItem value="false">Reject</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="approvalNotes">Notes</Label>
              <Textarea
                id="approvalNotes"
                placeholder="Final approval notes"
                value={finalApprovalForm.notes}
                onChange={(e) => setFinalApprovalForm(prev => ({ ...prev, notes: e.target.value }))}
              />
            </div>
            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setFinalApprovalDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleFinalApproval} disabled={loading}>
                {loading && <Loader2 className="w-4 h-4 mr-2 animate-spin" />}
                Submit Approval
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Raw Materials Dialog */}
      <Dialog open={rawMaterialsDialogOpen} onOpenChange={setRawMaterialsDialogOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Add Raw Material</DialogTitle>
            <DialogDescription>
              Select a raw material and specify its contribution to the manufacturing process
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="rawMaterialProduct">Product *</Label>
                <Select
                  value={rawMaterialForm.productId.toString()}
                  onValueChange={(value) => setRawMaterialForm(prev => ({ ...prev, productId: parseInt(value) }))}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select product" />
                  </SelectTrigger>
                  <SelectContent>
                    {productsApi.data?.items?.map((product) => (
                      <SelectItem key={product.id} value={product.id.toString()}>
                        {product.name} ({product.productCode})
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="sourceType">Source Type</Label>
                <Select
                  value={rawMaterialForm.sourceType}
                  onValueChange={(value: 'PurchaseOrder' | 'Manufacturing' | 'Transfer') => 
                    setRawMaterialForm(prev => ({ ...prev, sourceType: value }))}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select source type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="PurchaseOrder">Purchase Order</SelectItem>
                    <SelectItem value="Manufacturing">Manufacturing</SelectItem>
                    <SelectItem value="Transfer">Transfer</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="grid grid-cols-3 gap-4">
              <div className="space-y-2">
                <Label htmlFor="quantityUsed">Quantity Used *</Label>
                <Input
                  id="quantityUsed"
                  type="number"
                  step="0.01"
                  placeholder="0.00"
                  value={rawMaterialForm.quantityUsed}
                  onChange={(e) => setRawMaterialForm(prev => ({ ...prev, quantityUsed: parseFloat(e.target.value) || 0 }))}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="unitCost">Unit Cost</Label>
                <Input
                  id="unitCost"
                  type="number"
                  step="0.01"
                  placeholder="0.00"
                  value={rawMaterialForm.unitCost}
                  onChange={(e) => setRawMaterialForm(prev => ({ ...prev, unitCost: parseFloat(e.target.value) || 0 }))}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="contributionPercentage">Contribution %</Label>
                <Input
                  id="contributionPercentage"
                  type="number"
                  min="0"
                  max="100"
                  step="0.1"
                  placeholder="0.0"
                  value={rawMaterialForm.contributionPercentage}
                  onChange={(e) => setRawMaterialForm(prev => ({ ...prev, contributionPercentage: parseFloat(e.target.value) || 0 }))}
                />
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="sourceOwnershipId">Source Ownership ID</Label>
                <Input
                  id="sourceOwnershipId"
                  type="number"
                  placeholder="0"
                  value={rawMaterialForm.sourceOwnershipId}
                  onChange={(e) => setRawMaterialForm(prev => ({ ...prev, sourceOwnershipId: parseInt(e.target.value) || 0 }))}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="sourceId">Source ID</Label>
                <Input
                  id="sourceId"
                  type="number"
                  placeholder="0"
                  value={rawMaterialForm.sourceId}
                  onChange={(e) => setRawMaterialForm(prev => ({ ...prev, sourceId: parseInt(e.target.value) || 0 }))}
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="rawMaterialNotes">Notes</Label>
              <Textarea
                id="rawMaterialNotes"
                placeholder="Additional notes about this raw material"
                value={rawMaterialForm.notes}
                onChange={(e) => setRawMaterialForm(prev => ({ ...prev, notes: e.target.value }))}
              />
            </div>

            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setRawMaterialsDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={addRawMaterial}>
                Add Raw Material
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Workflow History Dialog */}
      <Dialog open={workflowHistoryDialogOpen} onOpenChange={setWorkflowHistoryDialogOpen}>
        <DialogContent className="max-w-4xl">
          <DialogHeader>
            <DialogTitle>Workflow History</DialogTitle>
            <DialogDescription>
              Complete workflow history for {selectedRecord?.productName}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            {workflowHistory.length > 0 ? (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Date</TableHead>
                    <TableHead>From Status</TableHead>
                    <TableHead>To Status</TableHead>
                    <TableHead>User</TableHead>
                    <TableHead>Notes</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {workflowHistory.map((entry, index) => (
                    <TableRow key={index}>
                      <TableCell>
                        {new Date(entry.transitionDate).toLocaleString()}
                      </TableCell>
                      <TableCell>
                        {entry.fromStatus && getStatusBadge(entry.fromStatus)}
                      </TableCell>
                      <TableCell>
                        {getStatusBadge(entry.toStatus)}
                      </TableCell>
                      <TableCell>{entry.userName || 'System'}</TableCell>
                      <TableCell>{entry.notes || '-'}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            ) : (
              <div className="text-center py-8 text-muted-foreground">
                No workflow history available
              </div>
            )}
            <div className="flex justify-end">
              <Button variant="outline" onClick={() => setWorkflowHistoryDialogOpen(false)}>
                Close
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
};

export default ProductManufacture;
