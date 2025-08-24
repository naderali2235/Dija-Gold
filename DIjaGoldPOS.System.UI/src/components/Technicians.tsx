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
import { 
  useSearchTechnicians, 
  useCreateTechnician, 
  useUpdateTechnician, 
  useDeleteTechnician,
  usePaginatedBranches
} from '../hooks/useApi';
import { 
  TechnicianDto, 
  CreateTechnicianRequestDto, 
  UpdateTechnicianRequestDto, 
  BranchDto 
} from '../services/api';
import { toast } from 'sonner';

export default function Technicians() {
  const { user } = useAuth();
  const [searchQuery, setSearchQuery] = useState('');
  const [isNewTechnicianOpen, setIsNewTechnicianOpen] = useState(false);
  const [editingTechnician, setEditingTechnician] = useState<TechnicianDto | null>(null);

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

  // API hooks
  const { 
    execute: searchTechnicians, 
    data: techniciansData, 
    loading: techniciansLoading, 
    error: techniciansError 
  } = useSearchTechnicians();

  const { 
    execute: createTechnician, 
    loading: createLoading, 
    error: createError 
  } = useCreateTechnician();

  const { 
    execute: updateTechnician, 
    loading: updateLoading, 
    error: updateError 
  } = useUpdateTechnician();

  const { 
    execute: deleteTechnician, 
    loading: deleteLoading, 
    error: deleteError 
  } = useDeleteTechnician();

  const { 
    data: branchesData, 
    loading: branchesLoading, 
    error: branchesError 
  } = usePaginatedBranches({ pageSize: 100 });

  // Fetch technicians on component mount
  useEffect(() => {
    searchTechnicians({
      pageSize: 100,
      isActive: true,
    });
  }, [searchTechnicians]);

  // Handle API errors
  useEffect(() => {
    if (techniciansError) {
      toast.error('Failed to Load Technicians', {
        description: 'Unable to fetch technicians. Please check your connection and try again.',
        duration: 5000
      });
    }
  }, [techniciansError]);

  useEffect(() => {
    if (branchesError) {
      toast.warning('Branch List Unavailable', {
        description: 'Unable to load branch list. Some features may be limited.',
        duration: 4000
      });
    }
  }, [branchesError]);

  useEffect(() => {
    if (createError) {
      toast.error('Failed to Create Technician', {
        description: createError,
        duration: 6000
      });
    }
  }, [createError]);

  useEffect(() => {
    if (updateError) {
      toast.error('Failed to Update Technician', {
        description: updateError,
        duration: 6000
      });
    }
  }, [updateError]);

  useEffect(() => {
    if (deleteError) {
      toast.error('Failed to Delete Technician', {
        description: deleteError,
        duration: 6000
      });
    }
  }, [deleteError]);

  const technicians = techniciansData?.items || [];
  const branches = branchesData?.items || [];

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

      await createTechnician(request);
      
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
      
      // Refresh technicians list
      await searchTechnicians({
        pageSize: 100,
        isActive: true,
      });
    } catch (error) {
      // Error is handled by the useEffect above
      console.error('Failed to create technician:', error);
    }
  };

  const handleEditTechnician = async () => {
    if (!editingTechnician) return;
    
    try {
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

      await updateTechnician(editingTechnician.id, request);
      
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
      
      // Refresh technicians list
      await searchTechnicians({
        pageSize: 100,
        isActive: true,
      });
    } catch (error) {
      // Error is handled by the useEffect above
      console.error('Failed to update technician:', error);
    }
  };

  const handleDeleteTechnician = async (technician: TechnicianDto) => {
    if (!window.confirm(`Are you sure you want to delete technician "${technician.fullName}"?`)) {
      return;
    }

    try {
      await deleteTechnician(technician.id);
      
      toast.success('Success!', {
        description: 'Technician deleted successfully!',
        duration: 4000
      });
      
      // Refresh technicians list
      await searchTechnicians({
        pageSize: 100,
        isActive: true,
      });
    } catch (error) {
      // Error is handled by the useEffect above
      console.error('Failed to delete technician:', error);
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

  const isLoading = techniciansLoading || branchesLoading;
  const isSubmitting = createLoading || updateLoading || deleteLoading;

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
                {createLoading ? (
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
          {isLoading ? (
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
                        <span className="text-sm">
                          {branches.find(b => b.id === technician.branchId)?.name || 'Unknown'}
                        </span>
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
                          disabled={isSubmitting}
                        >
                          <Edit className="h-3 w-3" />
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => handleDeleteTechnician(technician)}
                          className="touch-target hover:bg-red-50 hover:text-red-600 transition-colors"
                          disabled={isSubmitting}
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
              {updateLoading ? (
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
