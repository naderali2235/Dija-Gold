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
  Wrench,
  Plus,
  Search,
  Clock,
  CheckCircle,
  AlertTriangle,
  Calendar,
  DollarSign,
  User,
  Phone,
} from 'lucide-react';
import { formatCurrency } from './utils/currency';
import { useAuth } from './AuthContext';
import api, { transactionsApi, customersApi, RepairRequest, Transaction } from '../services/api';
import { toast } from 'sonner';

interface RepairJob {
  id: string;
  jobNumber: string;
  customerName: string;
  customerPhone: string;
  itemDescription: string;
  repairType: string;
  status: 'pending' | 'in_progress' | 'completed' | 'ready_for_pickup' | 'delivered';
  priority: 'low' | 'medium' | 'high';
  estimatedCost: number;
  actualCost?: number;
  receivedDate: string;
  estimatedCompletion: string;
  completedDate?: string;
  notes: string;
  images?: string[];
}

export default function Repairs() {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState('all');
  const [isNewRepairOpen, setIsNewRepairOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedJob, setSelectedJob] = useState<RepairJob | null>(null);
  const [customers, setCustomers] = useState<any[]>([]);

  // Form state for new repair
  const [newRepair, setNewRepair] = useState({
    customerId: '',
    customerName: '',
    customerPhone: '',
    itemDescription: '',
    repairType: '',
    priority: 'medium',
    estimatedCost: '',
    estimatedCompletion: '',
    notes: '',
    amountPaid: '',
    paymentMethod: '1', // Cash
  });

  // State for repair jobs - now using real transactions
  const [repairJobs, setRepairJobs] = useState<RepairJob[]>([]);
  const [repairJobsLoading, setRepairJobsLoading] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Fetch repair transactions from API
  const fetchRepairJobs = async () => {
    try {
      setRepairJobsLoading(true);
      
      // Search for repair transactions
      const result = await transactionsApi.searchTransactions({
        transactionType: 'Repair',
        branchId: user?.branch?.id,
        pageSize: 100, // Get all repairs for now
      });

      // Get unique customer IDs for phone number lookup
      const customerIds = result.items
        .filter(t => t.customerId)
        .map(t => t.customerId!)
        .filter((id, index, arr) => arr.indexOf(id) === index);
      
      // Fetch customer details for phone numbers
      const customerDetails = new Map<number, any>();
      if (customerIds.length > 0) {
        try {
          const customersResult = await customersApi.getCustomers({ pageSize: 1000 });
          customersResult.items.forEach(customer => {
            if (customerIds.includes(customer.id)) {
              customerDetails.set(customer.id, customer);
            }
          });
        } catch (error) {
          console.warn('Failed to fetch customer details for phone numbers:', error);
        }
      }

      // Convert Transaction objects to RepairJob format
      const jobs: RepairJob[] = result.items.map((transaction: Transaction) => {
        // Parse repair description to extract repair type
        const repairDescription = transaction.repairDescription || 'General Repair';
        const parts = repairDescription.split(' - ');
        const itemDescription = parts[0] || 'Repair Service';
        const repairTypePart = parts[1] || '';
        
        // Extract repair type from description
        let repairType = 'General Repair';
        if (repairTypePart.includes('cleaning')) repairType = 'Cleaning & Polishing';
        else if (repairTypePart.includes('resizing')) repairType = 'Ring Resizing';
        else if (repairTypePart.includes('clasp')) repairType = 'Clasp Repair';
        else if (repairTypePart.includes('chain')) repairType = 'Chain Repair';
        else if (repairTypePart.includes('stone')) repairType = 'Stone Setting';
        else if (repairTypePart.includes('prong')) repairType = 'Prong Repair';
        else if (repairTypePart.includes('component')) repairType = 'Component Replacement';
        
        // Extract notes from description
        const notesPart = repairDescription.split(' | Notes: ')[1] || '';
        
        // Get customer phone from fetched details
        const customerDetail = transaction.customerId ? customerDetails.get(transaction.customerId) : null;
        const customerPhone = customerDetail?.mobileNumber || customerDetail?.phone || '';
        
        // Determine priority based on repair type (enhanced logic)
        let priority: 'low' | 'medium' | 'high' = 'medium';
        if (repairTypePart.includes('prong') || repairTypePart.includes('stone')) {
          priority = 'high'; // Structural repairs are high priority
        } else if (repairTypePart.includes('cleaning') || repairTypePart.includes('polishing')) {
          priority = 'low'; // Cosmetic repairs are lower priority
        }
        
        return {
          id: transaction.id.toString(),
          jobNumber: transaction.transactionNumber,
          customerName: transaction.customerName || 'Walk-in Customer',
          customerPhone: customerPhone,
          itemDescription: itemDescription,
          repairType: repairType,
          status: mapTransactionStatusToJobStatus(transaction.status),
          priority: priority,
          estimatedCost: transaction.totalAmount,
          actualCost: transaction.amountPaid > 0 ? transaction.totalAmount : undefined,
          receivedDate: transaction.transactionDate,
          estimatedCompletion: transaction.estimatedCompletionDate || transaction.transactionDate,
          completedDate: transaction.status === 'Completed' ? transaction.transactionDate : undefined,
          notes: notesPart,
        };
      });

      setRepairJobs(jobs);
    } catch (error) {
      console.error('Failed to fetch repair jobs:', error);
      
      toast.error('Failed to Load Repair Jobs', {
        description: 'Unable to fetch repair jobs. Please check your connection and try again.',
        duration: 5000
      });
      
      // Fall back to empty array
      setRepairJobs([]);
    } finally {
      setRepairJobsLoading(false);
    }
  };

  // Fetch customers for dropdown
  const fetchCustomers = async () => {
    try {
      const result = await customersApi.getCustomers({ pageSize: 100 });
      setCustomers(result.items);
    } catch (error) {
      console.error('Failed to fetch customers:', error);
      
      toast.warning('Customer List Unavailable', {
        description: 'Unable to load customer list. You can still create repairs for walk-in customers.',
        duration: 4000
      });
    }
  };

  // Map transaction status to repair job status
  const mapTransactionStatusToJobStatus = (status: string): 'pending' | 'in_progress' | 'completed' | 'ready_for_pickup' | 'delivered' => {
    switch (status) {
      case 'Pending': return 'pending';
      case 'Completed': return 'completed';
      case 'Cancelled': return 'pending'; // Could be mapped differently
      default: return 'pending';
    }
  };

  useEffect(() => {
    fetchRepairJobs();
    fetchCustomers();
  }, [user?.branch?.id]);

  const filteredJobs = repairJobs.filter(job => {
    const matchesSearch = job.jobNumber.toLowerCase().includes(searchQuery.toLowerCase()) ||
                         job.customerName.toLowerCase().includes(searchQuery.toLowerCase()) ||
                         job.itemDescription.toLowerCase().includes(searchQuery.toLowerCase());
    
    if (activeTab === 'all') return matchesSearch;
    return matchesSearch && job.status === activeTab;
  });

  const getStatusBadge = (status: string) => {
    const variants = {
      pending: { variant: 'outline' as const, className: 'bg-gray-100 text-gray-800', icon: Clock },
      in_progress: { variant: 'default' as const, className: 'bg-blue-100 text-blue-800', icon: Wrench },
      completed: { variant: 'default' as const, className: 'bg-green-100 text-green-800', icon: CheckCircle },
      ready_for_pickup: { variant: 'default' as const, className: 'bg-yellow-100 text-yellow-800', icon: AlertTriangle },
      delivered: { variant: 'secondary' as const, className: 'bg-purple-100 text-purple-800', icon: CheckCircle },
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

  const getPriorityBadge = (priority: string) => {
    const variants = {
      low: 'bg-green-100 text-green-800',
      medium: 'bg-yellow-100 text-yellow-800',
      high: 'bg-red-100 text-red-800',
    };

    return (
      <Badge variant="outline" className={variants[priority as keyof typeof variants]}>
        {priority.toUpperCase()}
      </Badge>
    );
  };

  const validateRepairForm = () => {
    const errors: string[] = [];
    
    if (!newRepair.itemDescription.trim()) {
      errors.push('Item description is required');
    }
    
    if (!newRepair.repairType) {
      errors.push('Repair type is required');
    }
    
    if (!newRepair.estimatedCost || parseFloat(newRepair.estimatedCost) <= 0) {
      errors.push('Valid estimated cost is required (must be greater than 0)');
    }
    
    if (!newRepair.amountPaid || parseFloat(newRepair.amountPaid) < 0) {
      errors.push('Amount paid is required (cannot be negative)');
    }
    
    if (parseFloat(newRepair.amountPaid || '0') > parseFloat(newRepair.estimatedCost || '0')) {
      errors.push('Amount paid cannot exceed estimated cost');
    }
    
    if (newRepair.estimatedCompletion) {
      const completionDate = new Date(newRepair.estimatedCompletion);
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      
      if (completionDate < today) {
        errors.push('Estimated completion date cannot be in the past');
      }
    }
    
    return errors;
  };

  const handleNewRepairSubmit = async () => {
    try {
      setIsSubmitting(true);
      
      // Validate form
      const validationErrors = validateRepairForm();
      if (validationErrors.length > 0) {
        toast.error('Validation Failed', {
          description: validationErrors.join('\n'),
          duration: 5000
        });
        return;
      }

      // Prepare repair request for API
      const repairRequest: RepairRequest = {
        branchId: user?.branch?.id || 1, // Default to branch 1 if not available
        customerId: newRepair.customerId ? parseInt(newRepair.customerId) : undefined,
        repairDescription: `${newRepair.itemDescription} - ${newRepair.repairType}${newRepair.notes ? ` | Notes: ${newRepair.notes}` : ''}`,
        repairAmount: parseFloat(newRepair.estimatedCost),
        estimatedCompletionDate: newRepair.estimatedCompletion ? new Date(newRepair.estimatedCompletion).toISOString() : undefined,
        amountPaid: parseFloat(newRepair.amountPaid || '0'),
        paymentMethod: parseInt(newRepair.paymentMethod), // 1 = Cash
      };

      console.log('Submitting repair request:', repairRequest);
      
      // Call the repair API
      const result = await transactionsApi.processRepair(repairRequest);
      
      console.log('Repair transaction created:', result);
      
      toast.success('Success!', {
        description: `Repair job ${result.transactionNumber} created successfully!`,
        duration: 4000
      });
      
      setIsNewRepairOpen(false);
      setNewRepair({
        customerId: '',
        customerName: '',
        customerPhone: '',
        itemDescription: '',
        repairType: '',
        priority: 'medium',
        estimatedCost: '',
        estimatedCompletion: '',
        notes: '',
        amountPaid: '',
        paymentMethod: '1',
      });
      
      // Refresh repair jobs list
      await fetchRepairJobs();
    } catch (error) {
      console.error('Failed to create repair job:', error);
      
      toast.error('Failed to Create Repair Job', {
        description: error instanceof Error ? error.message : 'An unexpected error occurred. Please try again.',
        duration: 6000
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleStatusUpdate = async (jobId: string, newStatus: string) => {
    try {
      // Since repairs are now transactions, status updates would require
      // additional API endpoints for repair job management
      // For now, we'll show a message that this feature needs backend support
      toast.info('Status Update Noted', {
        description: `Status update to "${newStatus.replace('_', ' ')}" has been noted. Enhanced status tracking will be available in a future update.`,
        duration: 5000
      });
      
      console.log(`Status update requested for job ${jobId} to ${newStatus}`);
    } catch (error) {
      console.error('Failed to update repair job status:', error);
      
      toast.error('Status Update Failed', {
        description: 'Failed to update job status. Please try again.',
        duration: 4000
      });
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl text-[#0D1B2A]">Repair Services</h1>
          <p className="text-muted-foreground">Track and manage jewelry repair jobs</p>
        </div>
        <Dialog open={isNewRepairOpen} onOpenChange={setIsNewRepairOpen}>
          <DialogTrigger asChild>
            <Button className="touch-target" variant="golden">
              <Plus className="mr-2 h-4 w-4" />
              New Repair Job
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-2xl bg-white border-gray-200 shadow-lg">
            <DialogHeader>
              <DialogTitle>Create New Repair Job</DialogTitle>
              <DialogDescription>
                Fill in the details for the new repair job. Fields marked with <span className="text-red-500">*</span> are required.
              </DialogDescription>
            </DialogHeader>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="customer">Customer (Optional)</Label>
                <Select value={newRepair.customerId} onValueChange={(value) => {
                  if (value === 'walk-in') {
                    setNewRepair({
                      ...newRepair, 
                      customerId: value,
                      customerName: 'Walk-in Customer',
                      customerPhone: ''
                    });
                  } else {
                    const customer = customers.find(c => c.id.toString() === value);
                    setNewRepair({
                      ...newRepair, 
                      customerId: value,
                      customerName: customer?.fullName || '',
                      customerPhone: customer?.mobileNumber || ''
                    });
                  }
                }}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select customer (optional)" />
                  </SelectTrigger>
                  <SelectContent className="bg-white border-gray-200 shadow-lg">
                    <SelectItem value="walk-in" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Walk-in Customer</SelectItem>
                    {customers.map((customer) => (
                      <SelectItem 
                        key={customer.id} 
                        value={customer.id.toString()}
                        className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]"
                      >
                        {customer.fullName} - {customer.mobileNumber}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="customerPhone">Customer Phone</Label>
                <Input
                  id="customerPhone"
                  value={newRepair.customerPhone}
                  onChange={(e) => setNewRepair({...newRepair, customerPhone: e.target.value})}
                  placeholder="Enter phone number"
                  disabled={!!newRepair.customerId}
                />
              </div>
              <div className="col-span-2 space-y-2">
                <Label htmlFor="itemDescription">Item Description <span className="text-red-500">*</span></Label>
                <Textarea
                  id="itemDescription"
                  value={newRepair.itemDescription}
                  onChange={(e) => setNewRepair({...newRepair, itemDescription: e.target.value})}
                  placeholder="e.g., Gold ring with loose stone, Chain with broken clasp"
                  rows={3}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="repairType">Repair Type <span className="text-red-500">*</span></Label>
                <Select value={newRepair.repairType} onValueChange={(value) => setNewRepair({...newRepair, repairType: value})}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select repair type" />
                  </SelectTrigger>
                  <SelectContent className="bg-white border-gray-200 shadow-lg">
                    <SelectItem value="cleaning" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Cleaning & Polishing</SelectItem>
                    <SelectItem value="resizing" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Ring Resizing</SelectItem>
                    <SelectItem value="clasp_repair" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Clasp Repair</SelectItem>
                    <SelectItem value="chain_repair" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Chain Repair</SelectItem>
                    <SelectItem value="stone_setting" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Stone Setting</SelectItem>
                    <SelectItem value="prong_repair" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Prong Repair</SelectItem>
                    <SelectItem value="component_replacement" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Component Replacement</SelectItem>
                    <SelectItem value="other" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Other</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="priority">Priority</Label>
                <Select value={newRepair.priority} onValueChange={(value) => setNewRepair({...newRepair, priority: value})}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent className="bg-white border-gray-200 shadow-lg">
                    <SelectItem value="low" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Low</SelectItem>
                    <SelectItem value="medium" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Medium</SelectItem>
                    <SelectItem value="high" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">High</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="estimatedCost">Estimated Cost (EGP) <span className="text-red-500">*</span></Label>
                <Input
                  id="estimatedCost"
                  type="number"
                  value={newRepair.estimatedCost}
                  onChange={(e) => setNewRepair({...newRepair, estimatedCost: e.target.value})}
                  placeholder="0"
                  step="0.01"
                  min="0.01"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="estimatedCompletion">Estimated Completion</Label>
                <Input
                  id="estimatedCompletion"
                  type="date"
                  value={newRepair.estimatedCompletion}
                  onChange={(e) => setNewRepair({...newRepair, estimatedCompletion: e.target.value})}
                  min={new Date().getFullYear() + '-' + 
                    String(new Date().getMonth() + 1).padStart(2, '0') + '-' + 
                    String(new Date().getDate()).padStart(2, '0')}
                  title="Select the estimated completion date for this repair"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="amountPaid">Amount Paid (EGP) <span className="text-red-500">*</span></Label>
                <Input
                  id="amountPaid"
                  type="number"
                  value={newRepair.amountPaid}
                  onChange={(e) => setNewRepair({...newRepair, amountPaid: e.target.value})}
                  placeholder="0"
                  step="0.01"
                  min="0"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="paymentMethod">Payment Method</Label>
                <Select value={newRepair.paymentMethod} onValueChange={(value) => setNewRepair({...newRepair, paymentMethod: value})}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent className="bg-white border-gray-200 shadow-lg">
                    <SelectItem value="1" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Cash</SelectItem>
                    <SelectItem value="2" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Card</SelectItem>
                    <SelectItem value="3" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Bank Transfer</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="col-span-2 space-y-2">
                <Label htmlFor="notes">Notes</Label>
                <Textarea
                  id="notes"
                  value={newRepair.notes}
                  onChange={(e) => setNewRepair({...newRepair, notes: e.target.value})}
                  placeholder="Special handling instructions, customer preferences, etc."
                  rows={2}
                />
              </div>
            </div>
            <div className="flex justify-end gap-3 mt-6">
              <Button variant="outline" className="touch-target" onClick={() => setIsNewRepairOpen(false)}>
                Cancel
              </Button>
              <Button 
                onClick={handleNewRepairSubmit} 
                variant="golden" 
                disabled={isSubmitting || !newRepair.itemDescription.trim() || !newRepair.repairType || !newRepair.estimatedCost || !newRepair.amountPaid}
                className="touch-target"
              >
                {isSubmitting ? (
                  <>
                    <div className="animate-spin h-4 w-4 border-2 border-white border-t-transparent rounded-full mr-2"></div>
                    Creating...
                  </>
                ) : 'Create Job'}
              </Button>
            </div>
          </DialogContent>
        </Dialog>
      </div>

      {/* Search Bar */}
      <Card className="pos-card">
        <CardContent className="pt-6">
          <div className="relative">
            <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search by job number, customer name, or item description..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-10 touch-target"
            />
          </div>
        </CardContent>
      </Card>

      {/* Status Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="grid w-full grid-cols-6">
          <TabsTrigger value="all">All Jobs</TabsTrigger>
          <TabsTrigger value="pending">Pending</TabsTrigger>
          <TabsTrigger value="in_progress">In Progress</TabsTrigger>
          <TabsTrigger value="completed">Completed</TabsTrigger>
          <TabsTrigger value="ready_for_pickup">Ready</TabsTrigger>
          <TabsTrigger value="delivered">Delivered</TabsTrigger>
        </TabsList>

        <TabsContent value={activeTab} className="mt-6">
          <Card className="pos-card">
            <CardHeader>
              <CardTitle>Repair Jobs</CardTitle>
              <CardDescription>
                {filteredJobs.length} job(s) found
              </CardDescription>
            </CardHeader>
            <CardContent>
              {repairJobsLoading ? (
                <div className="text-center py-8">
                  <div className="animate-spin h-8 w-8 border-2 border-[#D4AF37] border-t-transparent rounded-full mx-auto mb-4"></div>
                  <p className="text-muted-foreground">Loading repair jobs...</p>
                </div>
              ) : filteredJobs.length === 0 ? (
                <div className="text-center py-8">
                  <Wrench className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                  <p className="text-muted-foreground">
                    {searchQuery ? 'No repair jobs match your search' : 'No repair jobs found'}
                  </p>
                  {searchQuery && (
                    <Button 
                      variant="outline" 
                      className="mt-4"
                      onClick={() => setSearchQuery('')}
                    >
                      Clear Search
                    </Button>
                  )}
                </div>
              ) : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Job #</TableHead>
                      <TableHead>Customer</TableHead>
                      <TableHead>Item</TableHead>
                      <TableHead>Type</TableHead>
                      <TableHead>Status</TableHead>
                      <TableHead>Priority</TableHead>
                      <TableHead>Cost</TableHead>
                      <TableHead>Due Date</TableHead>
                      <TableHead>Actions</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {filteredJobs.map((job) => (
                      <TableRow key={job.id}>
                        <TableCell className="font-medium">{job.jobNumber}</TableCell>
                        <TableCell>
                          <div>
                            <p className="font-medium">{job.customerName}</p>
                            <p className="text-sm text-muted-foreground">{job.customerPhone}</p>
                          </div>
                        </TableCell>
                        <TableCell>
                          <p className="max-w-32 truncate">{job.itemDescription}</p>
                        </TableCell>
                        <TableCell>{job.repairType}</TableCell>
                        <TableCell>{getStatusBadge(job.status)}</TableCell>
                        <TableCell>{getPriorityBadge(job.priority)}</TableCell>
                        <TableCell>
                          <div>
                            <p>{formatCurrency(job.estimatedCost)}</p>
                            {job.actualCost && (
                              <p className="text-sm text-muted-foreground">
                                (Actual: {formatCurrency(job.actualCost)})
                              </p>
                            )}
                          </div>
                        </TableCell>
                        <TableCell>
                          {new Date(job.estimatedCompletion).toLocaleDateString()}
                        </TableCell>
                        <TableCell>
                          <div className="flex gap-2">
                            <Dialog>
                              <DialogTrigger asChild>
                                <Button variant="outline" size="sm" className="touch-target hover:bg-[#F4E9B1] transition-colors">
                                  View
                                </Button>
                              </DialogTrigger>
                              <DialogContent className="bg-white border-gray-200 shadow-lg">
                                <DialogHeader>
                                  <DialogTitle>{job.jobNumber} - Details</DialogTitle>
                                </DialogHeader>
                                <div className="space-y-4">
                                  <div className="grid grid-cols-2 gap-4">
                                    <div>
                                      <Label>Customer</Label>
                                      <p>{job.customerName}</p>
                                      <p className="text-sm text-muted-foreground">{job.customerPhone}</p>
                                    </div>
                                    <div>
                                      <Label>Status</Label>
                                      <div className="mt-1">{getStatusBadge(job.status)}</div>
                                    </div>
                                    <div>
                                      <Label>Priority</Label>
                                      <div className="mt-1">{getPriorityBadge(job.priority)}</div>
                                    </div>
                                    <div>
                                      <Label>Repair Type</Label>
                                      <p>{job.repairType}</p>
                                    </div>
                                  </div>
                                  <div>
                                    <Label>Item Description</Label>
                                    <p>{job.itemDescription}</p>
                                  </div>
                                  <div>
                                    <Label>Notes</Label>
                                    <p>{job.notes}</p>
                                  </div>
                                  <div className="grid grid-cols-2 gap-4">
                                    <div>
                                      <Label>Received Date</Label>
                                      <p>{new Date(job.receivedDate).toLocaleDateString()}</p>
                                    </div>
                                    <div>
                                      <Label>Due Date</Label>
                                      <p>{new Date(job.estimatedCompletion).toLocaleDateString()}</p>
                                    </div>
                                  </div>
                                  {job.status !== 'delivered' && (
                                    <div className="flex gap-2 pt-4">
                                      {job.status === 'pending' && (
                                        <Button
                                          onClick={() => handleStatusUpdate(job.id, 'in_progress')}
                                          size="sm"
                                        >
                                          Start Work
                                        </Button>
                                      )}
                                      {job.status === 'in_progress' && (
                                        <Button
                                          onClick={() => handleStatusUpdate(job.id, 'completed')}
                                          size="sm"
                                        >
                                          Mark Complete
                                        </Button>
                                      )}
                                      {job.status === 'completed' && (
                                        <Button
                                          onClick={() => handleStatusUpdate(job.id, 'ready_for_pickup')}
                                          size="sm"
                                        >
                                          Ready for Pickup
                                        </Button>
                                      )}
                                      {job.status === 'ready_for_pickup' && (
                                        <Button
                                          onClick={() => handleStatusUpdate(job.id, 'delivered')}
                                          size="sm"
                                        >
                                          Mark Delivered
                                        </Button>
                                      )}
                                    </div>
                                  )}
                                </div>
                              </DialogContent>
                            </Dialog>
                          </div>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}