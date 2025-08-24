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
  Eye,
  Edit,
  Trash2,
  Bell,
  FileText,
  Timer,
  Package,
  Shield,
  Send,
  Filter,
  BarChart3,
  TrendingUp,
  Users,
  MapPin,
} from 'lucide-react';
import { formatCurrency } from './utils/currency';
import { useAuth } from './AuthContext';
import { 
  CreateRepairJobRequestDto, 
  CreateRepairOrderRequestDto,
  Transaction,
  RepairJobDto,
  TechnicianDto,
  RepairJobStatisticsDto
} from '../services/api';
import { 
  usePaginatedCustomers,
  useSearchRepairJobs,
  useRepairJobStatistics,
  useActiveTechnicians,
  useRepairStatuses,
  useRepairPriorities,
  usePaymentMethods,
  useCreateRepairOrder,
  useUpdateRepairJobStatus,
  useAssignTechnician,
  useCompleteRepair
} from '../hooks/useApi';
import { EnumLookupDto } from '../types/lookups';
import { toast } from 'sonner';

export default function Repairs() {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState('all');
  const [isNewRepairOpen, setIsNewRepairOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedJob, setSelectedJob] = useState<RepairJobDto | null>(null);
  // Use hooks for data fetching
  const { data: customersData, loading: customersLoading } = usePaginatedCustomers({
    pageNumber: 1,
    pageSize: 1000,
    isActive: true
  });
  
  const { execute: searchRepairJobs, data: repairJobsData, loading: repairJobsLoading } = useSearchRepairJobs();
  const { execute: getRepairJobStatistics, data: statistics, loading: statisticsLoading } = useRepairJobStatistics();
  const { execute: getActiveTechnicians, data: technicians, loading: techniciansLoading } = useActiveTechnicians();
  const { execute: getRepairStatuses, data: repairStatuses, loading: statusesLoading } = useRepairStatuses();
  const { execute: getRepairPriorities, data: repairPriorities, loading: prioritiesLoading } = useRepairPriorities();
  const { execute: getPaymentMethods, data: paymentMethods, loading: paymentMethodsLoading } = usePaymentMethods();
  
  const { execute: createRepairOrder, loading: isCreating } = useCreateRepairOrder();
  const { execute: updateRepairJobStatus, loading: isUpdatingStatus } = useUpdateRepairJobStatus();
  const { execute: assignTechnician, loading: isAssigningTechnician } = useAssignTechnician();
  const { execute: completeRepair, loading: isCompletingRepair } = useCompleteRepair();

  const [showStatistics, setShowStatistics] = useState(false);

  // Form state for new repair
  const [newRepair, setNewRepair] = useState({
    customerId: '',
    customerName: '',
    customerPhone: '',
    itemDescription: '',
    repairType: '',
    priority: 'Medium',
    estimatedCost: '',
    estimatedCompletion: '',
    notes: '',
    amountPaid: '',
  });

  // State for job details dialog
  const [jobDetailsOpen, setJobDetailsOpen] = useState(false);
  const [selectedJobForDetails, setSelectedJobForDetails] = useState<RepairJobDto | null>(null);

  // State for status update dialog
  const [statusUpdateOpen, setStatusUpdateOpen] = useState(false);
  const [statusUpdateJob, setStatusUpdateJob] = useState<RepairJobDto | null>(null);
  const [statusUpdateForm, setStatusUpdateForm] = useState({
    status: '',
    notes: '',
    materialsUsed: '',
    hoursSpent: '',
    additionalPaymentAmount: '',
    paymentMethod: 'Cash',
  });

  // State for technician assignment dialog
  const [assignTechnicianOpen, setAssignTechnicianOpen] = useState(false);
  const [assignTechnicianJob, setAssignTechnicianJob] = useState<RepairJobDto | null>(null);
  const [assignTechnicianForm, setAssignTechnicianForm] = useState({
    technicianId: '',
    technicianNotes: '',
  });

  // Fetch data on component mount
  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      await Promise.all([
        searchRepairJobs({ pageSize: 1000 }),
        getRepairJobStatistics(),
        getActiveTechnicians(),
        getRepairStatuses(),
        getRepairPriorities(),
        getPaymentMethods()
      ]);
    } catch (error) {
      console.error('Error fetching data:', error);
      toast.error('Failed to load repair jobs data');
    }
  };

  // Compute loading state
  const loading = customersLoading || repairJobsLoading || statisticsLoading || techniciansLoading || 
                 statusesLoading || prioritiesLoading || paymentMethodsLoading;

  const handleCreateRepair = async () => {
    if (!newRepair.itemDescription || !newRepair.repairType) {
      toast.error('Please fill in all required fields');
      return;
    }

    try {
      // Create repair order using the new architecture
      console.log('User data for repair:', user);
      console.log('User branch:', user?.branch);
      
      if (!user?.branch?.id) {
        toast.error('Branch information not available. Please contact your administrator to assign you to a branch.');
        return;
      }
      
      const branchId = user.branch.id; // Use user's numeric branch ID
      console.log('Using branch ID:', branchId);
      
      // Get priority enum value
      const priorityLookup = repairPriorities?.find(p => p.name === newRepair.priority);
      const priorityValue = priorityLookup?.id || 2; // Default to Medium priority
      
      const repairOrderRequest: CreateRepairOrderRequestDto = {
        branchId: branchId,
        customerId: newRepair.customerId && newRepair.customerId !== 'walk-in' ? parseInt(newRepair.customerId) : undefined,
        repairDescription: newRepair.itemDescription,
        repairAmount: parseFloat(newRepair.estimatedCost) || 0,
        amountPaid: parseFloat(newRepair.amountPaid) || 0,
        paymentMethodId: 1, // Cash payment method
        estimatedCompletionDate: newRepair.estimatedCompletion ? 
          new Date(Date.now() + parseInt(newRepair.estimatedCompletion) * 24 * 60 * 60 * 1000).toISOString() : 
          new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(), // Default to 7 days
        priorityId: priorityValue,
        assignedTechnicianId: undefined, // Will be assigned later
        technicianNotes: newRepair.notes
      };

      // Create the repair order (this will automatically create the order, financial transaction, and repair job)
      await createRepairOrder(repairOrderRequest);
      
      toast.success('Repair job created successfully');
      setIsNewRepairOpen(false);
      resetNewRepairForm();
      fetchData();
    } catch (error) {
      console.error('Error creating repair job:', error);
      toast.error('Failed to create repair job');
    }
  };

  const resetNewRepairForm = () => {
    setNewRepair({
      customerId: '',
      customerName: '',
      customerPhone: '',
      itemDescription: '',
      repairType: '',
      priority: 'Medium',
      estimatedCost: '',
      estimatedCompletion: '',
      notes: '',
      amountPaid: '',
    });
  };

  const handleStatusUpdate = async (jobId: number, status: string) => {
    try {
      // Find the current job to validate transition
      const currentJob = repairJobsData?.items?.find(job => job.id === jobId);
      if (!currentJob) {
        toast.error('Repair job not found');
        return;
      }

      // Find the status enum value from the lookup data
      const statusLookup = repairStatuses?.find(s => s.name === status);
      if (!statusLookup) {
        toast.error('Invalid status selected');
        return;
      }

      // Validate status transition based on backend rules
      const validTransitions: { [key: number]: number[] } = {
        1: [2, 6], // Pending -> InProgress, Cancelled
        2: [3, 6], // InProgress -> Completed, Cancelled  
        3: [4, 6], // Completed -> ReadyForPickup, Cancelled
        4: [5, 6], // ReadyForPickup -> Delivered, Cancelled
        5: [],     // Delivered -> No further transitions
        6: []      // Cancelled -> No further transitions
      };

      const currentStatus = currentJob.statusId;
      const newStatus = statusLookup.id;
      
      if (!validTransitions[currentStatus]?.includes(newStatus)) {
        const currentStatusName = repairStatuses?.find(s => s.id === currentStatus)?.name || 'Unknown';
        toast.error(`Cannot transition from ${currentStatusName} to ${status}. Please follow the proper workflow.`);
        return;
      }

      // Get payment method enum value
      const paymentMethodLookup = paymentMethods?.find(m => m.name === statusUpdateForm.paymentMethod);
      const paymentMethodValue = paymentMethodLookup?.id || 1; // Default to Cash

      await updateRepairJobStatus(jobId, {
        statusId: statusLookup.id,
        technicianNotes: statusUpdateForm.notes,
        materialsUsed: statusUpdateForm.materialsUsed,
        hoursSpent: statusUpdateForm.hoursSpent ? parseFloat(statusUpdateForm.hoursSpent) : undefined,
        additionalPaymentAmount: statusUpdateForm.additionalPaymentAmount ? parseFloat(statusUpdateForm.additionalPaymentAmount) : undefined,
        paymentMethodId: paymentMethodValue,
      });
      toast.success('Repair job status updated successfully');
      setStatusUpdateOpen(false);
      resetStatusUpdateForm();
      fetchData();
    } catch (error) {
      console.error('Error updating repair job status:', error);
      const errorMessage = error instanceof Error ? error.message : 'Failed to update repair job status';
      toast.error(errorMessage);
    }
  };

  const handleCompleteRepair = async (jobId: number) => {
    try {
      // Find the current job to validate
      const currentJob = repairJobsData?.items?.find(job => job.id === jobId);
      if (!currentJob) {
        toast.error('Repair job not found');
        return;
      }

      // Validate that the job is in the right status to be completed
      if (currentJob.statusId !== 2) { // InProgress = 2
        const currentStatusName = repairStatuses?.find(s => s.id === currentJob.statusId)?.name || 'Unknown';
        toast.error(`Cannot complete repair from ${currentStatusName} status. Job must be In Progress first.`);
        return;
      }

      // Get actual cost from form or use estimated cost as fallback
      const actualCost = statusUpdateForm.materialsUsed ? 
        parseFloat(statusUpdateForm.materialsUsed) || currentJob.repairAmount || 0 :
        currentJob.repairAmount || 0;

      // Get payment method enum value
      const paymentMethodLookup = paymentMethods?.find(m => m.name === statusUpdateForm.paymentMethod);
      const paymentMethodValue = paymentMethodLookup?.id || 1; // Default to Cash

      await completeRepair(jobId, {
        actualCost: actualCost,
        technicianNotes: statusUpdateForm.notes,
        materialsUsed: statusUpdateForm.materialsUsed,
        hoursSpent: statusUpdateForm.hoursSpent ? parseFloat(statusUpdateForm.hoursSpent) : undefined,
        additionalPaymentAmount: statusUpdateForm.additionalPaymentAmount ? parseFloat(statusUpdateForm.additionalPaymentAmount) : undefined,
        paymentMethodId: paymentMethodValue,
      });
      toast.success('Repair job completed successfully');
      setStatusUpdateOpen(false);
      resetStatusUpdateForm();
      fetchData();
    } catch (error) {
      console.error('Error completing repair job:', error);
      const errorMessage = error instanceof Error ? error.message : 'Failed to complete repair job';
      toast.error(errorMessage);
    }
  };

  const handleAssignTechnician = async (jobId: number) => {
    try {
      await assignTechnician(jobId, {
        technicianId: parseInt(assignTechnicianForm.technicianId),
        technicianNotes: assignTechnicianForm.technicianNotes,
      });
      toast.success('Technician assigned successfully');
      setAssignTechnicianOpen(false);
      resetAssignTechnicianForm();
      fetchData();
    } catch (error) {
      console.error('Error assigning technician:', error);
      toast.error('Failed to assign technician');
    }
  };

  const resetStatusUpdateForm = () => {
    setStatusUpdateForm({
      status: '',
      notes: '',
      materialsUsed: '',
      hoursSpent: '',
      additionalPaymentAmount: '',
      paymentMethod: 'Cash',
    });
    setStatusUpdateJob(null);
  };

  const resetAssignTechnicianForm = () => {
    setAssignTechnicianForm({
      technicianId: '',
      technicianNotes: '',
    });
    setAssignTechnicianJob(null);
  };

  const openStatusUpdateDialog = (job: RepairJobDto, status: string) => {
    setStatusUpdateJob(job);
    setStatusUpdateForm({
      status,
      notes: '',
      materialsUsed: '',
      hoursSpent: '',
      additionalPaymentAmount: '',
      paymentMethod: 'Cash',
    });
    setStatusUpdateOpen(true);
  };

  const openAssignTechnicianDialog = (job: RepairJobDto) => {
    setAssignTechnicianJob(job);
    setAssignTechnicianForm({
      technicianId: '',
      technicianNotes: '',
    });
    setAssignTechnicianOpen(true);
  };

  const openJobDetailsDialog = (job: RepairJobDto) => {
    setSelectedJobForDetails(job);
    setJobDetailsOpen(true);
  };

  // Filter repair jobs based on active tab and search query
  const filteredJobs = repairJobsData?.items?.filter(job => {
    // Get status display name for comparison
    const statusLookup = repairStatuses?.find(s => s.id === job.statusId);
    const statusDisplayName = statusLookup?.name?.toLowerCase() || '';
    
    const matchesTab = activeTab === 'all' || statusDisplayName === activeTab;
    const matchesSearch = searchQuery === '' || 
      job.financialTransactionNumber?.toLowerCase().includes(searchQuery.toLowerCase()) ||
      job.customerName?.toLowerCase().includes(searchQuery.toLowerCase()) ||
      job.repairDescription.toLowerCase().includes(searchQuery.toLowerCase());
    return matchesTab && matchesSearch;
  }) || [];

  const getStatusBadgeVariant = (status: number) => {
    const statusLookup = repairStatuses?.find(s => s.id === status);
    const statusName = statusLookup?.name?.toLowerCase() || '';
    
    switch (statusName) {
      case 'pending': return 'secondary';
      case 'in progress': return 'default';
      case 'completed': return 'default';
      case 'ready for pickup': return 'default';
      case 'delivered': return 'default';
      case 'cancelled': return 'destructive';
      default: return 'secondary';
    }
  };

  const getStatusBadgeColor = (status: number) => {
    const statusLookup = repairStatuses?.find(s => s.id === status);
    const statusName = statusLookup?.name?.toLowerCase() || '';
    
    switch (statusName) {
      case 'pending': return 'bg-yellow-100 text-yellow-800';
      case 'in progress': return 'bg-blue-100 text-blue-800';
      case 'completed': return 'bg-green-100 text-green-800';
      case 'ready for pickup': return 'bg-orange-100 text-orange-800';
      case 'delivered': return 'bg-purple-100 text-purple-800';
      case 'cancelled': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-[#0D1B2A]"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl text-[#0D1B2A]">Repair Services</h1>
          <p className="text-muted-foreground">Track and manage jewelry repair jobs</p>
        </div>
        <div className="flex gap-3">
          <Button 
            variant="outline" 
            onClick={() => setShowStatistics(!showStatistics)}
            className="touch-target"
          >
            <BarChart3 className="mr-2 h-4 w-4" />
            {showStatistics ? 'Hide' : 'Show'} Statistics
          </Button>
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
                      const customer = customersData?.items?.find(c => c.id.toString() === value);
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
                      {(customersData?.items || []).map((customer) => (
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
                      <SelectItem value="stone_setting" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Stone Setting</SelectItem>
                      <SelectItem value="chain_repair" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Chain Repair</SelectItem>
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
                       {(repairPriorities || []).map((priority) => (
                         <SelectItem 
                           key={priority.id} 
                           value={priority.name}
                           className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]"
                         >
                           {priority.name}
                         </SelectItem>
                       ))}
                     </SelectContent>
                   </Select>
                 </div>
                <div className="space-y-2">
                  <Label htmlFor="estimatedCost">Estimated Cost</Label>
                  <Input
                    id="estimatedCost"
                    type="number"
                    value={newRepair.estimatedCost}
                    onChange={(e) => setNewRepair({...newRepair, estimatedCost: e.target.value})}
                    placeholder="Enter estimated cost"
                    step="0.01"
                    min="0"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="estimatedCompletion">Estimated Completion (days)</Label>
                  <Input
                    id="estimatedCompletion"
                    type="number"
                    value={newRepair.estimatedCompletion}
                    onChange={(e) => setNewRepair({...newRepair, estimatedCompletion: e.target.value})}
                    placeholder="Enter estimated days"
                    min="1"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="amountPaid">Amount Paid</Label>
                  <Input
                    id="amountPaid"
                    type="number"
                    value={newRepair.amountPaid}
                    onChange={(e) => setNewRepair({...newRepair, amountPaid: e.target.value})}
                    placeholder="Enter amount paid"
                    step="0.01"
                    min="0"
                  />
                </div>
                <div className="col-span-2 space-y-2">
                  <Label htmlFor="notes">Notes</Label>
                  <Textarea
                    id="notes"
                    value={newRepair.notes}
                    onChange={(e) => setNewRepair({...newRepair, notes: e.target.value})}
                    placeholder="Additional notes or special instructions..."
                    rows={3}
                  />
                </div>
              </div>
              <div className="flex justify-end gap-3 mt-6">
                <Button variant="outline" className="touch-target" onClick={() => setIsNewRepairOpen(false)}>
                  Cancel
                </Button>
                <Button 
                  onClick={handleCreateRepair}
                  variant="golden" 
                  disabled={!newRepair.itemDescription || !newRepair.repairType || isCreating}
                  className="touch-target"
                >
                  {isCreating ? (
                    <>
                      <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                      Creating...
                    </>
                  ) : 'Create Job'}
                </Button>
              </div>
            </DialogContent>
          </Dialog>
        </div>
      </div>

      {/* Statistics Dashboard */}
      {showStatistics && statistics && (
        <Card className="pos-card">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <BarChart3 className="h-5 w-5" />
              Repair Job Statistics
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-4">
              <div className="text-center p-4 bg-blue-50 rounded-lg">
                <div className="text-2xl font-bold text-blue-600">{statistics.totalJobs}</div>
                <div className="text-sm text-gray-600">Total Jobs</div>
              </div>
              <div className="text-center p-4 bg-yellow-50 rounded-lg">
                <div className="text-2xl font-bold text-yellow-600">{statistics.pendingJobs}</div>
                <div className="text-sm text-gray-600">Pending</div>
              </div>
              <div className="text-center p-4 bg-blue-50 rounded-lg">
                <div className="text-2xl font-bold text-blue-600">{statistics.inProgressJobs}</div>
                <div className="text-sm text-gray-600">In Progress</div>
              </div>
              <div className="text-center p-4 bg-green-50 rounded-lg">
                <div className="text-2xl font-bold text-green-600">{statistics.completedJobs}</div>
                <div className="text-sm text-gray-600">Completed</div>
              </div>
              <div className="text-center p-4 bg-orange-50 rounded-lg">
                <div className="text-2xl font-bold text-orange-600">{statistics.readyForPickupJobs}</div>
                <div className="text-sm text-gray-600">Ready for Pickup</div>
              </div>
              <div className="text-center p-4 bg-purple-50 rounded-lg">
                <div className="text-2xl font-bold text-purple-600">{statistics.deliveredJobs}</div>
                <div className="text-sm text-gray-600">Delivered</div>
              </div>
            </div>
            <div className="mt-6 grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <h4 className="font-semibold mb-3">Revenue Overview</h4>
                <div className="space-y-2">
                  <div className="flex justify-between">
                    <span>Total Revenue:</span>
                    <span className="font-semibold">{formatCurrency(statistics.totalRevenue)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Average Completion Time:</span>
                    <span className="font-semibold">{statistics.averageCompletionTime.toFixed(1)} hours</span>
                  </div>
                </div>
              </div>
              <div>
                <h4 className="font-semibold mb-3">Jobs by Priority</h4>
                <div className="space-y-2">
                  {Object.entries(statistics.jobsByPriority).map(([priority, count]) => (
                    <div key={priority} className="flex justify-between">
                      <span className="capitalize">{priority}:</span>
                      <span className="font-semibold">{count}</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

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
        <TabsList className="grid w-full grid-cols-7">
          <TabsTrigger value="all">All Jobs</TabsTrigger>
          <TabsTrigger value="pending">Pending</TabsTrigger>
          <TabsTrigger value="inprogress">In Progress</TabsTrigger>
          <TabsTrigger value="completed">Completed</TabsTrigger>
          <TabsTrigger value="readyforpickup">Ready</TabsTrigger>
          <TabsTrigger value="delivered">Delivered</TabsTrigger>
          <TabsTrigger value="cancelled">Cancelled</TabsTrigger>
        </TabsList>

        <TabsContent value={activeTab} className="mt-6">
          <Card className="pos-card">
            <CardContent className="pt-6">
              <div className="overflow-x-auto">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Job #</TableHead>
                      <TableHead>Customer</TableHead>
                      <TableHead>Item Description</TableHead>
                      <TableHead>Repair Type</TableHead>
                      <TableHead>Priority</TableHead>
                      <TableHead>Status</TableHead>
                      <TableHead>Technician</TableHead>
                      <TableHead>Created</TableHead>
                      <TableHead>Actions</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {filteredJobs.map((job) => (
                      <TableRow key={job.id}>
                        <TableCell className="font-medium">{job.financialTransactionNumber || `RJ-${job.id}`}</TableCell>
                        <TableCell>
                          <div>
                            <div className="font-medium">{job.customerName || 'Walk-in Customer'}</div>
                            {job.customerPhone && (
                              <div className="text-sm text-gray-500">{job.customerPhone}</div>
                            )}
                          </div>
                        </TableCell>
                        <TableCell className="max-w-xs truncate">{job.repairDescription}</TableCell>
                        <TableCell>Repair Service</TableCell>
                                                 <TableCell>
                           <Badge variant="outline" className="capitalize">
                             {repairPriorities?.find(p => p.id === job.priorityId)?.name || 'Unknown'}
                           </Badge>
                         </TableCell>
                        <TableCell>
                          <Badge 
                            variant={getStatusBadgeVariant(job.statusId)}
                            className={getStatusBadgeColor(job.statusId)}
                          >
                            {job.statusDisplayName || job.statusId}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          {job.assignedTechnicianName || (
                            <span className="text-gray-400">Unassigned</span>
                          )}
                        </TableCell>
                        <TableCell>
                          {new Date(job.createdAt).toLocaleDateString()}
                        </TableCell>
                        <TableCell>
                          <div className="flex gap-2">
                            <Button
                              size="sm"
                              variant="outline"
                              onClick={() => openJobDetailsDialog(job)}
                              className="touch-target"
                            >
                              <Eye className="h-4 w-4" />
                            </Button>
                            {!job.assignedTechnicianId && (
                              <Button
                                size="sm"
                                variant="outline"
                                onClick={() => openAssignTechnicianDialog(job)}
                                className="touch-target"
                              >
                                <User className="h-4 w-4" />
                              </Button>
                            )}
                                                         {job.statusId === 1 && ( // Pending
                               <Button
                                 size="sm"
                                 variant="outline"
                                 onClick={() => openStatusUpdateDialog(job, 'In Progress')}
                                 className="touch-target"
                               >
                                 <Wrench className="h-4 w-4" />
                               </Button>
                             )}
                             {job.statusId === 2 && ( // InProgress
                               <Button
                                 size="sm"
                                 variant="outline"
                                 onClick={() => openStatusUpdateDialog(job, 'Completed')}
                                 className="touch-target"
                               >
                                 <CheckCircle className="h-4 w-4" />
                               </Button>
                             )}
                             {job.statusId === 3 && ( // Completed
                               <Button
                                 size="sm"
                                 variant="outline"
                                 onClick={() => openStatusUpdateDialog(job, 'Ready for Pickup')}
                                 className="touch-target"
                               >
                                 <Package className="h-4 w-4" />
                               </Button>
                             )}
                             {job.statusId === 4 && ( // ReadyForPickup
                               <Button
                                 size="sm"
                                 variant="outline"
                                 onClick={() => openStatusUpdateDialog(job, 'Delivered')}
                                 className="touch-target"
                               >
                                 <Send className="h-4 w-4" />
                               </Button>
                             )}
                          </div>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
                {filteredJobs.length === 0 && (
                  <div className="text-center py-8 text-gray-500">
                    No repair jobs found matching your criteria.
                  </div>
                )}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Job Details Dialog */}
      <Dialog open={jobDetailsOpen} onOpenChange={setJobDetailsOpen}>
        <DialogContent className="max-w-2xl bg-white border-gray-200 shadow-lg">
          <DialogHeader>
            <DialogTitle>{selectedJobForDetails?.financialTransactionNumber || `RJ-${selectedJobForDetails?.id}`} - Repair Job Details</DialogTitle>
          </DialogHeader>
          {selectedJobForDetails && (
            <div className="space-y-6">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label className="text-sm font-medium text-gray-500">Customer</Label>
                  <p className="text-sm">{selectedJobForDetails.customerName || 'Walk-in Customer'}</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-gray-500">Phone</Label>
                  <p className="text-sm">{selectedJobForDetails.customerPhone || 'N/A'}</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-gray-500">Repair Type</Label>
                  <p className="text-sm">Repair Service</p>
                </div>
                                 <div>
                   <Label className="text-sm font-medium text-gray-500">Priority</Label>
                   <Badge variant="outline" className="capitalize">
                     {repairPriorities?.find(p => p.id === selectedJobForDetails.priorityId)?.name || 'Unknown'}
                   </Badge>
                 </div>
                <div>
                  <Label className="text-sm font-medium text-gray-500">Status</Label>
                  <Badge 
                    variant={getStatusBadgeVariant(selectedJobForDetails.statusId)}
                    className={getStatusBadgeColor(selectedJobForDetails.statusId)}
                  >
                    {selectedJobForDetails.statusDisplayName || selectedJobForDetails.statusId}
                  </Badge>
                </div>
                <div>
                  <Label className="text-sm font-medium text-gray-500">Technician</Label>
                  <p className="text-sm">{selectedJobForDetails.assignedTechnicianName || 'Unassigned'}</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-gray-500">Created</Label>
                  <p className="text-sm">{new Date(selectedJobForDetails.createdAt).toLocaleString()}</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-gray-500">Estimated Cost</Label>
                  <p className="text-sm">{selectedJobForDetails.actualCost ? formatCurrency(selectedJobForDetails.actualCost) : 'N/A'}</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-gray-500">Amount Paid</Label>
                  <p className="text-sm">{selectedJobForDetails.amountPaid ? formatCurrency(selectedJobForDetails.amountPaid) : 'N/A'}</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-gray-500">Estimated Completion</Label>
                  <p className="text-sm">{selectedJobForDetails.estimatedCompletionDate ? new Date(selectedJobForDetails.estimatedCompletionDate).toLocaleDateString() : 'N/A'}</p>
                </div>
                <div>
                  <Label className="text-sm font-medium text-gray-500">Completed At</Label>
                  <p className="text-sm">{selectedJobForDetails.completedDate ? new Date(selectedJobForDetails.completedDate).toLocaleString() : 'N/A'}</p>
                </div>
              </div>
              
              <div>
                <Label className="text-sm font-medium text-gray-500">Item Description</Label>
                <p className="text-sm mt-1 p-3 bg-gray-50 rounded-md">{selectedJobForDetails.repairDescription}</p>
              </div>
              
              {selectedJobForDetails.technicianNotes && (
                <div>
                  <Label className="text-sm font-medium text-gray-500">Technician Notes</Label>
                  <p className="text-sm mt-1 p-3 bg-gray-50 rounded-md">{selectedJobForDetails.technicianNotes}</p>
                </div>
              )}
              
              {selectedJobForDetails.materialsUsed && (
                <div>
                  <Label className="text-sm font-medium text-gray-500">Materials Used</Label>
                  <p className="text-sm mt-1 p-3 bg-gray-50 rounded-md">{selectedJobForDetails.materialsUsed}</p>
                </div>
              )}
              
              {selectedJobForDetails.hoursSpent && (
                <div>
                  <Label className="text-sm font-medium text-gray-500">Hours Spent</Label>
                  <p className="text-sm mt-1 p-3 bg-gray-50 rounded-md">{selectedJobForDetails.hoursSpent} hours</p>
                </div>
              )}
            </div>
          )}
        </DialogContent>
      </Dialog>

      {/* Status Update Dialog */}
      <Dialog open={statusUpdateOpen} onOpenChange={setStatusUpdateOpen}>
        <DialogContent className="max-w-md bg-white border-gray-200 shadow-lg">
          <DialogHeader>
            <DialogTitle>Update Repair Job Status</DialogTitle>
            <DialogDescription>
              Update the status for repair job {statusUpdateJob?.financialTransactionNumber || `RJ-${statusUpdateJob?.id}`}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
                         <div className="space-y-2">
               <Label htmlFor="status">New Status</Label>
               <Select value={statusUpdateForm.status} onValueChange={(value) => setStatusUpdateForm({...statusUpdateForm, status: value})}>
                 <SelectTrigger>
                   <SelectValue placeholder="Select new status" />
                 </SelectTrigger>
                 <SelectContent className="bg-white border-gray-200 shadow-lg">
                   {(repairStatuses || []).map((status) => (
                     <SelectItem 
                       key={status.id} 
                       value={status.name}
                       className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]"
                     >
                       {status.name}
                     </SelectItem>
                   ))}
                 </SelectContent>
               </Select>
             </div>
            
            <div className="space-y-2">
              <Label htmlFor="statusNotes">Notes</Label>
              <Textarea
                id="statusNotes"
                value={statusUpdateForm.notes}
                onChange={(e) => setStatusUpdateForm({...statusUpdateForm, notes: e.target.value})}
                placeholder="Add any notes about the status update..."
                rows={3}
              />
            </div>
            
            <div className="space-y-2">
              <Label htmlFor="materialsUsed">Materials Used</Label>
              <Input
                id="materialsUsed"
                value={statusUpdateForm.materialsUsed}
                onChange={(e) => setStatusUpdateForm({...statusUpdateForm, materialsUsed: e.target.value})}
                placeholder="e.g., Gold wire, solder, stones"
              />
            </div>
            
            <div className="space-y-2">
              <Label htmlFor="hoursSpent">Hours Spent</Label>
              <Input
                id="hoursSpent"
                type="number"
                value={statusUpdateForm.hoursSpent}
                onChange={(e) => setStatusUpdateForm({...statusUpdateForm, hoursSpent: e.target.value})}
                placeholder="Enter hours spent"
                step="0.5"
                min="0"
              />
            </div>
            
            {/* Payment fields - only show for completion or delivery */}
            {(statusUpdateForm.status === 'Completed' || statusUpdateForm.status === 'Ready for Pickup') && (
              <>
                <div className="space-y-2">
                  <Label htmlFor="additionalPayment">Additional Payment Amount</Label>
                  <Input
                    id="additionalPayment"
                    type="number"
                    value={statusUpdateForm.additionalPaymentAmount}
                    onChange={(e) => setStatusUpdateForm({...statusUpdateForm, additionalPaymentAmount: e.target.value})}
                    placeholder="Enter additional payment amount"
                    step="0.01"
                    min="0"
                  />
                </div>
                
                <div className="space-y-2">
                  <Label htmlFor="paymentMethod">Payment Method</Label>
                  <Select value={statusUpdateForm.paymentMethod} onValueChange={(value) => setStatusUpdateForm({...statusUpdateForm, paymentMethod: value})}>
                    <SelectTrigger>
                      <SelectValue placeholder="Select payment method" />
                    </SelectTrigger>
                    <SelectContent className="bg-white border-gray-200 shadow-lg">
                      {(paymentMethods || []).map((method) => (
                        <SelectItem 
                          key={method.id} 
                          value={method.name}
                          className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]"
                        >
                          {method.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </>
            )}
          </div>
          <div className="flex justify-end gap-3 mt-6">
            <Button variant="outline" className="touch-target" onClick={() => setStatusUpdateOpen(false)}>
              Cancel
            </Button>
            <Button 
              onClick={() => statusUpdateForm.status === 'Completed' ? 
                handleCompleteRepair(statusUpdateJob!.id) : 
                handleStatusUpdate(statusUpdateJob!.id, statusUpdateForm.status)
              }
              variant="golden" 
              disabled={!statusUpdateForm.status}
              className="touch-target"
            >
              Update Status
            </Button>
          </div>
        </DialogContent>
      </Dialog>

      {/* Assign Technician Dialog */}
      <Dialog open={assignTechnicianOpen} onOpenChange={setAssignTechnicianOpen}>
        <DialogContent className="max-w-md bg-white border-gray-200 shadow-lg">
          <DialogHeader>
            <DialogTitle>Assign Technician</DialogTitle>
            <DialogDescription>
              Assign a technician to repair job {assignTechnicianJob?.financialTransactionNumber || `RJ-${assignTechnicianJob?.id}`}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="technicianId">Technician</Label>
              <Select value={assignTechnicianForm.technicianId} onValueChange={(value) => setAssignTechnicianForm({...assignTechnicianForm, technicianId: value})}>
                <SelectTrigger>
                  <SelectValue placeholder="Select a technician" />
                </SelectTrigger>
                <SelectContent className="bg-white border-gray-200 shadow-lg">
                  {(technicians || []).map((technician) => (
                    <SelectItem 
                      key={technician.id} 
                      value={technician.id.toString()}
                      className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]"
                    >
                      {technician.fullName} {technician.specialization && `(${technician.specialization})`}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            
            <div className="space-y-2">
              <Label htmlFor="assignTechnicianNotes">Notes</Label>
              <Textarea
                id="assignTechnicianNotes"
                value={assignTechnicianForm.technicianNotes}
                onChange={(e) => setAssignTechnicianForm({...assignTechnicianForm, technicianNotes: e.target.value})}
                placeholder="Add any special instructions for the technician..."
                rows={3}
              />
            </div>
          </div>
          <div className="flex justify-end gap-3 mt-6">
            <Button variant="outline" className="touch-target" onClick={() => setAssignTechnicianOpen(false)}>
              Cancel
            </Button>
            <Button 
              onClick={() => handleAssignTechnician(assignTechnicianJob!.id)}
              variant="golden" 
              disabled={!assignTechnicianForm.technicianId}
              className="touch-target"
            >
              Assign Technician
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
