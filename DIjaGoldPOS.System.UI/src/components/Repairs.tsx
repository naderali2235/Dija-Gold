import React, { useState } from 'react';
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
  const [activeTab, setActiveTab] = useState('all');
  const [isNewRepairOpen, setIsNewRepairOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedJob, setSelectedJob] = useState<RepairJob | null>(null);

  // Form state for new repair
  const [newRepair, setNewRepair] = useState({
    customerName: '',
    customerPhone: '',
    itemDescription: '',
    repairType: '',
    priority: 'medium',
    estimatedCost: '',
    estimatedCompletion: '',
    notes: '',
  });

  // State for repair jobs (will be replaced with API data when available)
  const [repairJobs, setRepairJobs] = useState<RepairJob[]>([]);
  const [repairJobsLoading, setRepairJobsLoading] = useState(false);

  // TODO: Replace with actual repair API when available
  // const { execute: fetchRepairJobs, loading: repairJobsLoading } = useRepairJobs();
  
  // Mock data for now - will be replaced with API call
  React.useEffect(() => {
    setRepairJobsLoading(true);
    // Simulate API call
    setTimeout(() => {
      const mockJobs: RepairJob[] = [
        {
          id: '1',
          jobNumber: 'REP-2024-001',
          customerName: 'Rajesh Kumar',
          customerPhone: '+91 98765 43210',
          itemDescription: 'Gold chain with broken clasp',
          repairType: 'Clasp Repair',
          status: 'in_progress',
          priority: 'medium',
          estimatedCost: 800,
          receivedDate: '2024-01-15T10:00:00Z',
          estimatedCompletion: '2024-01-20T10:00:00Z',
          notes: 'Customer wants original clasp design maintained',
        },
        {
          id: '2',
          jobNumber: 'REP-2024-002',
          customerName: 'Priya Sharma',
          customerPhone: '+91 87654 32109',
          itemDescription: 'Ring resizing from 16 to 18',
          repairType: 'Resizing',
          status: 'completed',
          priority: 'high',
          estimatedCost: 1200,
          actualCost: 1100,
          receivedDate: '2024-01-12T14:30:00Z',
          estimatedCompletion: '2024-01-18T14:30:00Z',
          completedDate: '2024-01-17T16:45:00Z',
          notes: 'Ring resized successfully, customer satisfied',
        },
        {
          id: '3',
          jobNumber: 'REP-2024-003',
          customerName: 'Amit Patel',
          customerPhone: '+91 76543 21098',
          itemDescription: 'Earring back replacement',
          repairType: 'Component Replacement',
          status: 'ready_for_pickup',
          priority: 'low',
          estimatedCost: 300,
          actualCost: 250,
          receivedDate: '2024-01-14T11:15:00Z',
          estimatedCompletion: '2024-01-16T11:15:00Z',
          completedDate: '2024-01-16T09:30:00Z',
          notes: 'New earring backs fitted, matching originals',
        },
      ];
      setRepairJobs(mockJobs);
      setRepairJobsLoading(false);
    }, 1000);
  }, []);

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

  const handleNewRepairSubmit = async () => {
    try {
      // TODO: Replace with actual repair API when available
      // const repairRequest = {
      //   customerName: newRepair.customerName,
      //   customerPhone: newRepair.customerPhone,
      //   itemDescription: newRepair.itemDescription,
      //   repairType: newRepair.repairType,
      //   priority: newRepair.priority,
      //   estimatedCost: parseFloat(newRepair.estimatedCost),
      //   estimatedCompletion: newRepair.estimatedCompletion,
      //   notes: newRepair.notes,
      //   branchId: user?.branch?.id,
      // };
      // await createRepairJob(repairRequest);
      
      // Simulate API call for now
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      console.log('New repair submitted:', newRepair);
      setIsNewRepairOpen(false);
      setNewRepair({
        customerName: '',
        customerPhone: '',
        itemDescription: '',
        repairType: '',
        priority: 'medium',
        estimatedCost: '',
        estimatedCompletion: '',
        notes: '',
      });
      
      // Refresh repair jobs list
      // await fetchRepairJobs();
    } catch (error) {
      console.error('Failed to create repair job:', error);
      alert('Failed to create repair job. Please try again.');
    }
  };

  const handleStatusUpdate = async (jobId: string, newStatus: string) => {
    try {
      // TODO: Replace with actual repair API when available
      // await updateRepairJobStatus(jobId, newStatus);
      
      // Simulate API call for now
      await new Promise(resolve => setTimeout(resolve, 500));
      
      console.log(`Updating job ${jobId} to status ${newStatus}`);
      alert(`Job status updated to ${newStatus}`);
      
      // Refresh repair jobs list
      // await fetchRepairJobs();
    } catch (error) {
      console.error('Failed to update repair job status:', error);
      alert('Failed to update job status. Please try again.');
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
                Fill in the details for the new repair job
              </DialogDescription>
            </DialogHeader>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="customerName">Customer Name</Label>
                <Input
                  id="customerName"
                  value={newRepair.customerName}
                  onChange={(e) => setNewRepair({...newRepair, customerName: e.target.value})}
                  placeholder="Enter customer name"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="customerPhone">Customer Phone</Label>
                <Input
                  id="customerPhone"
                  value={newRepair.customerPhone}
                  onChange={(e) => setNewRepair({...newRepair, customerPhone: e.target.value})}
                  placeholder="Enter phone number"
                />
              </div>
              <div className="col-span-2 space-y-2">
                <Label htmlFor="itemDescription">Item Description</Label>
                <Textarea
                  id="itemDescription"
                  value={newRepair.itemDescription}
                  onChange={(e) => setNewRepair({...newRepair, itemDescription: e.target.value})}
                  placeholder="Describe the item to be repaired"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="repairType">Repair Type</Label>
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
                <Label htmlFor="estimatedCost">Estimated Cost (EGP)</Label>
                <Input
                  id="estimatedCost"
                  type="number"
                  value={newRepair.estimatedCost}
                  onChange={(e) => setNewRepair({...newRepair, estimatedCost: e.target.value})}
                  placeholder="0"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="estimatedCompletion">Estimated Completion</Label>
                <Input
                  id="estimatedCompletion"
                  type="date"
                  value={newRepair.estimatedCompletion}
                  onChange={(e) => setNewRepair({...newRepair, estimatedCompletion: e.target.value})}
                />
              </div>
              <div className="col-span-2 space-y-2">
                <Label htmlFor="notes">Notes</Label>
                <Textarea
                  id="notes"
                  value={newRepair.notes}
                  onChange={(e) => setNewRepair({...newRepair, notes: e.target.value})}
                  placeholder="Additional notes or special instructions"
                />
              </div>
            </div>
            <div className="flex justify-end gap-3 mt-6">
              <Button variant="outline" className="touch-target" onClick={() => setIsNewRepairOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleNewRepairSubmit} variant="golden">
                Create Job
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
              {filteredJobs.length === 0 ? (
                <div className="text-center py-8">
                  <Wrench className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                  <p className="text-muted-foreground">No repair jobs found</p>
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