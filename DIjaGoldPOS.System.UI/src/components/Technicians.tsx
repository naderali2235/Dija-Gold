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
  Plus,
  Search,
  Edit,
  Trash2,
  User,
  Phone,
  Mail,
  Briefcase,
  Building,
  Calendar,
  AlertTriangle,
} from 'lucide-react';
import { useAuth } from './AuthContext';
import { techniciansApi, branchesApi, TechnicianDto, CreateTechnicianRequestDto, UpdateTechnicianRequestDto, BranchDto } from '../services/api';
import { toast } from 'sonner';

export default function Technicians() {
  const { user } = useAuth();
  const [technicians, setTechnicians] = useState<TechnicianDto[]>([]);
  const [branches, setBranches] = useState<BranchDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [isNewTechnicianOpen, setIsNewTechnicianOpen] = useState(false);
  const [editingTechnician, setEditingTechnician] = useState<TechnicianDto | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Form state for new technician
  const [newTechnician, setNewTechnician] = useState({
    fullName: '',
    phoneNumber: '',
    email: '',
    specialization: '',
    branchId: user?.branch?.id || 1,
  });

  // Form state for editing technician
  const [editTechnician, setEditTechnician] = useState({
    fullName: '',
    phoneNumber: '',
    email: '',
    specialization: '',
    isActive: true,
    branchId: 1,
  });

  // Fetch technicians
  const fetchTechnicians = async () => {
    try {
      setLoading(true);
      const result = await techniciansApi.searchTechnicians({
        pageSize: 100,
        isActive: true,
      });
      setTechnicians(result.items);
    } catch (error) {
      console.error('Failed to fetch technicians:', error);
      toast.error('Failed to Load Technicians', {
        description: 'Unable to fetch technicians. Please check your connection and try again.',
        duration: 5000
      });
    } finally {
      setLoading(false);
    }
  };

  // Fetch branches
  const fetchBranches = async () => {
    try {
      const result = await branchesApi.getBranches({ pageSize: 100 });
      setBranches(result.items);
    } catch (error) {
      console.error('Failed to fetch branches:', error);
      toast.warning('Branch List Unavailable', {
        description: 'Unable to load branch list. Some features may be limited.',
        duration: 4000
      });
    }
  };

  useEffect(() => {
    fetchTechnicians();
    fetchBranches();
  }, []);

  const filteredTechnicians = technicians.filter(technician =>
    technician.fullName.toLowerCase().includes(searchQuery.toLowerCase()) ||
    technician.phoneNumber.includes(searchQuery) ||
    (technician.email && technician.email.toLowerCase().includes(searchQuery.toLowerCase())) ||
    (technician.specialization && technician.specialization.toLowerCase().includes(searchQuery.toLowerCase()))
  );

  const validateTechnicianForm = (data: any) => {
    const errors: string[] = [];
    
    if (!data.fullName.trim()) {
      errors.push('Full name is required');
    }
    
    if (!data.phoneNumber.trim()) {
      errors.push('Phone number is required');
    }
    
    if (data.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(data.email)) {
      errors.push('Please enter a valid email address');
    }
    
    return errors;
  };

  const handleCreateTechnician = async () => {
    try {
      setIsSubmitting(true);
      
      const validationErrors = validateTechnicianForm(newTechnician);
      if (validationErrors.length > 0) {
        toast.error('Validation Failed', {
          description: validationErrors.join('\n'),
          duration: 5000
        });
        return;
      }

      const request: CreateTechnicianRequestDto = {
        fullName: newTechnician.fullName.trim(),
        phoneNumber: newTechnician.phoneNumber.trim(),
        email: newTechnician.email.trim() || undefined,
        specialization: newTechnician.specialization.trim() || undefined,
        branchId: newTechnician.branchId,
      };

      await techniciansApi.createTechnician(request);
      
      toast.success('Success!', {
        description: 'Technician created successfully!',
        duration: 4000
      });
      
      setIsNewTechnicianOpen(false);
      setNewTechnician({
        fullName: '',
        phoneNumber: '',
        email: '',
        specialization: '',
        branchId: user?.branch?.id || 1,
      });
      
      await fetchTechnicians();
    } catch (error) {
      console.error('Failed to create technician:', error);
      toast.error('Failed to Create Technician', {
        description: error instanceof Error ? error.message : 'An unexpected error occurred. Please try again.',
        duration: 6000
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleEditTechnician = async () => {
    if (!editingTechnician) return;
    
    try {
      setIsSubmitting(true);
      
      const validationErrors = validateTechnicianForm(editTechnician);
      if (validationErrors.length > 0) {
        toast.error('Validation Failed', {
          description: validationErrors.join('\n'),
          duration: 5000
        });
        return;
      }

      const request: UpdateTechnicianRequestDto = {
        fullName: editTechnician.fullName.trim(),
        phoneNumber: editTechnician.phoneNumber.trim(),
        email: editTechnician.email.trim() || undefined,
        specialization: editTechnician.specialization.trim() || undefined,
        isActive: editTechnician.isActive,
        branchId: editTechnician.branchId,
      };

      await techniciansApi.updateTechnician(editingTechnician.id, request);
      
      toast.success('Success!', {
        description: 'Technician updated successfully!',
        duration: 4000
      });
      
      setEditingTechnician(null);
      setEditTechnician({
        fullName: '',
        phoneNumber: '',
        email: '',
        specialization: '',
        isActive: true,
        branchId: 1,
      });
      
      await fetchTechnicians();
    } catch (error) {
      console.error('Failed to update technician:', error);
      toast.error('Failed to Update Technician', {
        description: error instanceof Error ? error.message : 'An unexpected error occurred. Please try again.',
        duration: 6000
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeleteTechnician = async (technician: TechnicianDto) => {
    if (!window.confirm(`Are you sure you want to delete technician "${technician.fullName}"?`)) {
      return;
    }

    try {
      await techniciansApi.deleteTechnician(technician.id);
      
      toast.success('Success!', {
        description: 'Technician deleted successfully!',
        duration: 4000
      });
      
      await fetchTechnicians();
    } catch (error) {
      console.error('Failed to delete technician:', error);
      toast.error('Failed to Delete Technician', {
        description: error instanceof Error ? error.message : 'An unexpected error occurred. Please try again.',
        duration: 6000
      });
    }
  };

  const openEditDialog = (technician: TechnicianDto) => {
    setEditingTechnician(technician);
    setEditTechnician({
      fullName: technician.fullName,
      phoneNumber: technician.phoneNumber,
      email: technician.email || '',
      specialization: technician.specialization || '',
      isActive: technician.isActive,
      branchId: technician.branchId,
    });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl text-[#0D1B2A]">Technician Management</h1>
          <p className="text-muted-foreground">Manage technicians for repair jobs</p>
        </div>
        <Dialog open={isNewTechnicianOpen} onOpenChange={setIsNewTechnicianOpen}>
          <DialogTrigger asChild>
            <Button className="touch-target" variant="golden">
              <Plus className="mr-2 h-4 w-4" />
              New Technician
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-md bg-white border-gray-200 shadow-lg">
            <DialogHeader>
              <DialogTitle>Add New Technician</DialogTitle>
              <DialogDescription>
                Fill in the details for the new technician. Fields marked with <span className="text-red-500">*</span> are required.
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="fullName">Full Name <span className="text-red-500">*</span></Label>
                <Input
                  id="fullName"
                  value={newTechnician.fullName}
                  onChange={(e) => setNewTechnician({...newTechnician, fullName: e.target.value})}
                  placeholder="Enter full name"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="phoneNumber">Phone Number <span className="text-red-500">*</span></Label>
                <Input
                  id="phoneNumber"
                  value={newTechnician.phoneNumber}
                  onChange={(e) => setNewTechnician({...newTechnician, phoneNumber: e.target.value})}
                  placeholder="Enter phone number"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="email">Email</Label>
                <Input
                  id="email"
                  type="email"
                  value={newTechnician.email}
                  onChange={(e) => setNewTechnician({...newTechnician, email: e.target.value})}
                  placeholder="Enter email address"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="specialization">Specialization</Label>
                <Input
                  id="specialization"
                  value={newTechnician.specialization}
                  onChange={(e) => setNewTechnician({...newTechnician, specialization: e.target.value})}
                  placeholder="e.g., Ring repairs, Stone setting"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="branchId">Branch</Label>
                <Select value={newTechnician.branchId.toString()} onValueChange={(value) => setNewTechnician({...newTechnician, branchId: parseInt(value)})}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent className="bg-white border-gray-200 shadow-lg">
                    {branches.map((branch) => (
                      <SelectItem 
                        key={branch.id} 
                        value={branch.id.toString()}
                        className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]"
                      >
                        {branch.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
            <div className="flex justify-end gap-3 mt-6">
              <Button variant="outline" className="touch-target" onClick={() => setIsNewTechnicianOpen(false)}>
                Cancel
              </Button>
              <Button 
                onClick={handleCreateTechnician} 
                variant="golden" 
                disabled={isSubmitting || !newTechnician.fullName.trim() || !newTechnician.phoneNumber.trim()}
                className="touch-target"
              >
                {isSubmitting ? (
                  <>
                    <div className="animate-spin h-4 w-4 border-2 border-white border-t-transparent rounded-full mr-2"></div>
                    Creating...
                  </>
                ) : 'Create Technician'}
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
              placeholder="Search by name, phone, email, or specialization..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-10 touch-target"
            />
          </div>
        </CardContent>
      </Card>

      {/* Technicians Table */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle>Technicians</CardTitle>
          <CardDescription>
            {filteredTechnicians.length} technician(s) found
          </CardDescription>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="text-center py-8">
              <div className="animate-spin h-8 w-8 border-2 border-[#D4AF37] border-t-transparent rounded-full mx-auto mb-4"></div>
              <p className="text-muted-foreground">Loading technicians...</p>
            </div>
          ) : filteredTechnicians.length === 0 ? (
            <div className="text-center py-8">
              <User className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <p className="text-muted-foreground">
                {searchQuery ? 'No technicians match your search' : 'No technicians found'}
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
                  <TableHead>Name</TableHead>
                  <TableHead>Contact</TableHead>
                  <TableHead>Specialization</TableHead>
                  <TableHead>Branch</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredTechnicians.map((technician) => (
                  <TableRow key={technician.id}>
                    <TableCell>
                      <div>
                        <p className="font-medium">{technician.fullName}</p>
                        <p className="text-sm text-muted-foreground">ID: {technician.id}</p>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="space-y-1">
                        <div className="flex items-center gap-2">
                          <Phone className="h-3 w-3 text-muted-foreground" />
                          <span className="text-sm">{technician.phoneNumber}</span>
                        </div>
                        {technician.email && (
                          <div className="flex items-center gap-2">
                            <Mail className="h-3 w-3 text-muted-foreground" />
                            <span className="text-sm">{technician.email}</span>
                          </div>
                        )}
                      </div>
                    </TableCell>
                    <TableCell>
                      {technician.specialization ? (
                        <div className="flex items-center gap-2">
                          <Briefcase className="h-3 w-3 text-muted-foreground" />
                          <span className="text-sm">{technician.specialization}</span>
                        </div>
                      ) : (
                        <span className="text-sm text-muted-foreground">General</span>
                      )}
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <Building className="h-3 w-3 text-muted-foreground" />
                        <span className="text-sm">{technician.branchName || 'Unknown'}</span>
                      </div>
                    </TableCell>
                    <TableCell>
                      <Badge variant={technician.isActive ? "default" : "secondary"}>
                        {technician.isActive ? 'Active' : 'Inactive'}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <div className="flex gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => openEditDialog(technician)}
                          className="touch-target hover:bg-[#F4E9B1] transition-colors"
                        >
                          <Edit className="h-3 w-3" />
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => handleDeleteTechnician(technician)}
                          className="touch-target hover:bg-red-50 hover:text-red-600 transition-colors"
                        >
                          <Trash2 className="h-3 w-3" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Edit Technician Dialog */}
      <Dialog open={!!editingTechnician} onOpenChange={(open) => !open && setEditingTechnician(null)}>
        <DialogContent className="max-w-md bg-white border-gray-200 shadow-lg">
          <DialogHeader>
            <DialogTitle>Edit Technician</DialogTitle>
            <DialogDescription>
              Update the technician's information. Fields marked with <span className="text-red-500">*</span> are required.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="editFullName">Full Name <span className="text-red-500">*</span></Label>
              <Input
                id="editFullName"
                value={editTechnician.fullName}
                onChange={(e) => setEditTechnician({...editTechnician, fullName: e.target.value})}
                placeholder="Enter full name"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="editPhoneNumber">Phone Number <span className="text-red-500">*</span></Label>
              <Input
                id="editPhoneNumber"
                value={editTechnician.phoneNumber}
                onChange={(e) => setEditTechnician({...editTechnician, phoneNumber: e.target.value})}
                placeholder="Enter phone number"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="editEmail">Email</Label>
              <Input
                id="editEmail"
                type="email"
                value={editTechnician.email}
                onChange={(e) => setEditTechnician({...editTechnician, email: e.target.value})}
                placeholder="Enter email address"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="editSpecialization">Specialization</Label>
              <Input
                id="editSpecialization"
                value={editTechnician.specialization}
                onChange={(e) => setEditTechnician({...editTechnician, specialization: e.target.value})}
                placeholder="e.g., Ring repairs, Stone setting"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="editBranchId">Branch</Label>
              <Select value={editTechnician.branchId.toString()} onValueChange={(value) => setEditTechnician({...editTechnician, branchId: parseInt(value)})}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent className="bg-white border-gray-200 shadow-lg">
                  {branches.map((branch) => (
                    <SelectItem 
                      key={branch.id} 
                      value={branch.id.toString()}
                      className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]"
                    >
                      {branch.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="editIsActive">Status</Label>
              <Select value={editTechnician.isActive.toString()} onValueChange={(value) => setEditTechnician({...editTechnician, isActive: value === 'true'})}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent className="bg-white border-gray-200 shadow-lg">
                  <SelectItem value="true" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Active</SelectItem>
                  <SelectItem value="false" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">Inactive</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
          <div className="flex justify-end gap-3 mt-6">
            <Button variant="outline" className="touch-target" onClick={() => setEditingTechnician(null)}>
              Cancel
            </Button>
            <Button 
              onClick={handleEditTechnician} 
              variant="golden" 
              disabled={isSubmitting || !editTechnician.fullName.trim() || !editTechnician.phoneNumber.trim()}
              className="touch-target"
            >
              {isSubmitting ? (
                <>
                  <div className="animate-spin h-4 w-4 border-2 border-white border-t-transparent rounded-full mr-2"></div>
                  Updating...
                </>
              ) : 'Update Technician'}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
