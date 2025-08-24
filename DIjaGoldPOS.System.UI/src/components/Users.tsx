import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Badge } from './ui/badge';
import { Switch } from './ui/switch';
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
  UserCog,
  Plus,
  Search,
  Shield,
  Clock,
  Eye,
  Edit,
  Trash2,
  Key,
  Activity,
  AlertTriangle,
  Loader2,
  CheckCircle,
  XCircle,
  User as UserIcon,
  Mail,
  MapPin,
} from 'lucide-react';
import { useAuth } from './AuthContext';
import { toast } from 'sonner';
import {
  usePaginatedUsers,
  useCreateUser,
  useUpdateUser,
  useUpdateUserRoles,
  useUpdateUserStatus,
  useUserActivity,
  useResetPassword,
  useUserPermissions,
  useUpdateUserPermissions,
  usePaginatedBranches,
} from '../hooks/useApi';
import {
  UserDto,
  CreateUserRequest,
  UpdateUserRequest,
  UserActivityDto,
  UserPermissionsDto,
  BranchDto,
} from '../services/api';

interface UserFormData {
  userName: string;
  fullName: string;
  email: string;
  employeeCode: string;
  password: string;
  confirmPassword: string;
  roles: string[];
  branchId: number | null;
  isActive: boolean;
}

const defaultUserForm: UserFormData = {
  userName: '',
  fullName: '',
  email: '',
  employeeCode: '',
  password: '',
  confirmPassword: '',
  roles: ['Cashier'],
  branchId: null,
  isActive: true,
};

const availableRoles = ['Manager', 'Cashier'];

export default function Users() {
  const { user: currentUser, isManager } = useAuth();
  
  // State management
  const [searchQuery, setSearchQuery] = useState('');
  const [roleFilter, setRoleFilter] = useState('');
  const [branchFilter, setBranchFilter] = useState('');
  const [isNewUserOpen, setIsNewUserOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<UserDto | null>(null);
  const [isEditMode, setIsEditMode] = useState(false);
  const [isResetPasswordOpen, setIsResetPasswordOpen] = useState(false);
  const [userForm, setUserForm] = useState<UserFormData>(defaultUserForm);
  const [newPassword, setNewPassword] = useState('');
  const [confirmNewPassword, setConfirmNewPassword] = useState('');

  // Branches API hook
  const {
    data: branchesData,
    loading: branchesLoading,
    error: branchesError,
  } = usePaginatedBranches({
    isActive: true,
    pageSize: 100, // Get all branches
  });

  // API Hooks
  const {
    data: usersData,
    loading: usersLoading,
    error: usersError,
    fetchData: refetchUsers,
    updateParams: updateUsersParams,
    hasNextPage,
    hasPrevPage,
    nextPage,
    prevPage,
    setPage,
  } = usePaginatedUsers({
    searchTerm: searchQuery,
    Role: roleFilter === 'all' ? undefined : roleFilter,
    branchId: branchFilter === 'all' ? undefined : (branchFilter ? parseInt(branchFilter) : undefined),
    isActive: true,
  });

  const { execute: createUser, loading: createLoading } = useCreateUser();
  const { execute: updateUser, loading: updateLoading } = useUpdateUser();
  const { execute: updateUserRoles, loading: rolesLoading } = useUpdateUserRoles();
  const { execute: updateUserStatus, loading: statusLoading } = useUpdateUserStatus();
  const { execute: resetUserPassword, loading: resetLoading } = useResetPassword();
  const { execute: fetchUserActivity, loading: activityLoading } = useUserActivity();
  const { execute: fetchUserPermissions, loading: permissionsLoading } = useUserPermissions();
  const { execute: updateUserPermissions, loading: updatePermissionsLoading } = useUpdateUserPermissions();

  // Local state for user activity and permissions
  const [userActivity, setUserActivity] = useState<UserActivityDto | null>(null);
  const [userPermissions, setUserPermissions] = useState<UserPermissionsDto | null>(null);

  // Update search parameters with debounce
  useEffect(() => {
    const timeoutId = setTimeout(() => {
      updateUsersParams({
        searchTerm: searchQuery || undefined,
        Role: roleFilter === 'all' ? undefined : roleFilter,
        branchId: branchFilter === 'all' ? undefined : (branchFilter ? parseInt(branchFilter) : undefined),
        pageNumber: 1,
      });
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [searchQuery, roleFilter, branchFilter, updateUsersParams]);

  // Check permissions - only managers and admins can manage users
  const canManageUsers = isManager || currentUser?.roles?.includes('Admin') || currentUser?.roles?.includes('Manager');
  
  if (!canManageUsers) {
    return (
      <div className="space-y-6">
        <h1 className="text-3xl text-[#0D1B2A]">User Management</h1>
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="text-center py-8">
              <Shield className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <p className="text-muted-foreground">Access denied. Only managers and administrators can access user management.</p>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  // Handlers
  const handleCreateUser = async () => {
    if (userForm.password !== userForm.confirmPassword) {
      toast.error('Passwords do not match');
      return;
    }

    if (!userForm.userName || !userForm.fullName || !userForm.email || !userForm.password) {
      toast.error('Please fill in all required fields');
      return;
    }

    try {
      const createUserData: CreateUserRequest = {
        UserName: userForm.userName,
        FullName: userForm.fullName,
        Email: userForm.email,
        EmployeeCode: userForm.employeeCode || undefined,
        Password: userForm.password,
        Roles: userForm.roles,
        BranchId: userForm.branchId || undefined,
        IsActive: userForm.isActive,
      };

      await createUser(createUserData);
      toast.success('User created successfully');
      setIsNewUserOpen(false);
      resetForm();
      refetchUsers();
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to create user');
    }
  };

  const handleUpdateUser = async () => {
    if (!selectedUser) return;

    try {
      const updateUserData: UpdateUserRequest = {
        Id: selectedUser.Id,
        FullName: userForm.fullName,
        Email: userForm.email,
        EmployeeCode: userForm.employeeCode || undefined,
        BranchId: userForm.branchId || undefined,
      };

      await updateUser(selectedUser.Id, updateUserData);
      
      // Update roles if changed
      if (JSON.stringify(userForm.roles.sort()) !== JSON.stringify(selectedUser.Roles.sort())) {
        await updateUserRoles(selectedUser.Id, {
          UserId: selectedUser.Id,
          Roles: userForm.roles,
        });
      }

      toast.success('User updated successfully');
      setSelectedUser(null);
      setIsEditMode(false);
      resetForm();
      refetchUsers();
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to update user');
    }
  };

  const handleToggleUserStatus = async (userId: string, isActive: boolean) => {
    try {
      await updateUserStatus(userId, {
        UserId: userId,
        IsActive: isActive,
        Reason: isActive ? 'Account reactivated' : 'Account deactivated',
      });
      
      toast.success(`User ${isActive ? 'activated' : 'deactivated'} successfully`);
      refetchUsers();
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to update user status');
    }
  };

  const handleResetPassword = async () => {
    if (!selectedUser) return;

    if (newPassword !== confirmNewPassword) {
      toast.error('Passwords do not match');
      return;
    }

    if (!newPassword || newPassword.length < 6) {
      toast.error('Password must be at least 6 characters long');
      return;
    }

    try {
      await resetUserPassword(selectedUser.Id, {
        UserId: selectedUser.Id,
        NewPassword: newPassword,
        ForcePasswordChange: true,
      });

      toast.success('Password reset successfully');
      setIsResetPasswordOpen(false);
      setNewPassword('');
      setConfirmNewPassword('');
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to reset password');
    }
  };

  const handleViewUserDetails = async (user: UserDto) => {
    setSelectedUser(user);
    populateForm(user);

    // Fetch user activity and permissions
    try {
      const [activity, permissions] = await Promise.all([
        fetchUserActivity(user.Id, { pageSize: 10 }),
        fetchUserPermissions(user.Id),
      ]);
      setUserActivity(activity);
      setUserPermissions(permissions);
    } catch (error) {
      console.error('Failed to fetch user details:', error);
    }
  };

  const resetForm = () => {
    setUserForm(defaultUserForm);
  };

  const populateForm = (user: UserDto) => {
    setUserForm({
      userName: user.UserName,
      fullName: user.FullName,
      email: user.Email,
      employeeCode: user.EmployeeCode || '',
      password: '',
      confirmPassword: '',
      roles: user.Roles,
      branchId: user.BranchId || null,
      isActive: user.IsActive,
    });
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  const getRoleBadgeColor = (roles: string[]) => {
    if (roles.includes('Admin')) return 'bg-red-100 text-red-800';
    if (roles.includes('Manager')) return 'bg-blue-100 text-blue-800';
    return 'bg-green-100 text-green-800';
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <h1 className="text-3xl text-[#0D1B2A]">User Management</h1>
        <Button 
          onClick={() => setIsNewUserOpen(true)}
          variant="golden"
        >
          <Plus className="mr-2 h-4 w-4" />
          Add User
        </Button>
      </div>

      {/* Filters */}
      <Card className="pos-card">
        <CardContent className="pt-6">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div className="relative">
              <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search users..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-10"
              />
            </div>
            <Select value={roleFilter} onValueChange={setRoleFilter}>
              <SelectTrigger>
                <SelectValue placeholder="Filter by role" />
              </SelectTrigger>
              <SelectContent className="bg-white border-gray-200 shadow-lg">
                <SelectItem value="all" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">All Roles</SelectItem>
                {availableRoles.map(role => (
                  <SelectItem key={role} value={role} className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">{role}</SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select value={branchFilter} onValueChange={setBranchFilter}>
              <SelectTrigger>
                <SelectValue placeholder="Filter by branch" />
              </SelectTrigger>
              <SelectContent className="bg-white border-gray-200 shadow-lg">
                <SelectItem value="all" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">All Branches</SelectItem>
                {branchesLoading ? (
                  <SelectItem value="loading" disabled>Loading branches...</SelectItem>
                ) : branchesError ? (
                  <SelectItem value="error" disabled>Error loading branches</SelectItem>
                ) : branchesData?.items?.map((branch: BranchDto) => (
                  <SelectItem key={branch.id} value={branch.id.toString()} className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">
                    {branch.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Button 
              variant="outline" 
              onClick={() => {
                setSearchQuery('');
                setRoleFilter('all');
                setBranchFilter('all');
              }}
            >
              Clear Filters
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Users Table */}
      <Card className="pos-card">
        <CardContent className="p-0">
          {usersLoading && (
            <div className="flex justify-center items-center py-12">
              <Loader2 className="h-8 w-8 animate-spin" />
            </div>
          )}

          {usersError && (
            <div className="text-center py-12 text-red-600">
              <p>Error loading users: {usersError}</p>
              <Button onClick={refetchUsers} className="mt-4">
                Try Again
              </Button>
            </div>
          )}

          {usersData && usersData.items && (
            <>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>User</TableHead>
                    <TableHead>Role</TableHead>
                    <TableHead>Branch</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Last Login</TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {usersData.items.map((user) => (
                    <TableRow key={user.Id}>
                      <TableCell>
                        <div className="flex items-center space-x-3">
                          <UserIcon className="h-8 w-8 text-muted-foreground" />
                          <div>
                            <div className="font-medium">{user.FullName}</div>
                            <div className="text-sm text-muted-foreground">
                              @{user.UserName}
                            </div>
                            <div className="text-sm text-muted-foreground">
                              {user.Email}
                            </div>
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="space-y-1">
                          {user.Roles.map((role: string) => (
                            <Badge 
                              key={role} 
                              variant="secondary" 
                              className={getRoleBadgeColor(user.Roles)}
                            >
                              {role}
                            </Badge>
                          ))}
                        </div>
                      </TableCell>
                      <TableCell>
                        {user.BranchName || 'No Branch'}
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center space-x-2">
                          <Switch
                            checked={user.IsActive}
                            onCheckedChange={(checked: boolean) => 
                              handleToggleUserStatus(user.Id, checked)
                            }
                            disabled={statusLoading}
                          />
                          <span className="text-sm">
                            {user.IsActive ? (
                              <span className="flex items-center text-green-600">
                                <CheckCircle className="h-4 w-4 mr-1" />
                                Active
                              </span>
                            ) : (
                              <span className="flex items-center text-red-600">
                                <XCircle className="h-4 w-4 mr-1" />
                                Inactive
                              </span>
                            )}
                          </span>
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="text-sm">
                          {user.LastLoginAt ? formatDate(user.LastLoginAt) : 'Never'}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="flex space-x-2">
                          <Button
                            size="sm"
                            variant="ghost"
                            onClick={() => handleViewUserDetails(user)}
                          >
                            <Eye className="h-4 w-4" />
                          </Button>
                          <Button
                            size="sm"
                            variant="ghost"
                            onClick={() => {
                              setSelectedUser(user);
                              setIsResetPasswordOpen(true);
                            }}
                          >
                            <Key className="h-4 w-4" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>

              {/* Pagination */}
              <div className="flex items-center justify-between px-6 py-4 border-t">
                <div className="text-sm text-muted-foreground">
                  Showing {usersData.items.length} of {usersData.totalCount} users
                </div>
                <div className="flex space-x-2">
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={prevPage}
                    disabled={!hasPrevPage}
                  >
                    Previous
                  </Button>
                  <span className="flex items-center px-3 text-sm">
                    Page {usersData.pageNumber} of {Math.ceil(usersData.totalCount / usersData.pageSize)}
                  </span>
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={nextPage}
                    disabled={!hasNextPage}
                  >
                    Next
                  </Button>
                </div>
              </div>
            </>
          )}
        </CardContent>
      </Card>

      {/* Create User Dialog */}
      <Dialog open={isNewUserOpen} onOpenChange={setIsNewUserOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto bg-white border-gray-200 shadow-lg">
          <DialogHeader>
            <DialogTitle>Create New User</DialogTitle>
            <DialogDescription>
              Add a new user to the system with appropriate roles and permissions.
            </DialogDescription>
          </DialogHeader>
          
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="userName">Username *</Label>
              <Input
                id="userName"
                value={userForm.userName}
                onChange={(e) => setUserForm({...userForm, userName: e.target.value})}
                placeholder="Enter username"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="fullName">Full Name *</Label>
              <Input
                id="fullName"
                value={userForm.fullName}
                onChange={(e) => setUserForm({...userForm, fullName: e.target.value})}
                placeholder="Enter full name"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="email">Email *</Label>
              <Input
                id="email"
                type="email"
                value={userForm.email}
                onChange={(e) => setUserForm({...userForm, email: e.target.value})}
                placeholder="Enter email address"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="employeeCode">Employee Code</Label>
              <Input
                id="employeeCode"
                value={userForm.employeeCode}
                onChange={(e) => setUserForm({...userForm, employeeCode: e.target.value})}
                placeholder="Enter employee code"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="password">Password *</Label>
              <Input
                id="password"
                type="password"
                value={userForm.password}
                onChange={(e) => setUserForm({...userForm, password: e.target.value})}
                placeholder="Enter password"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="confirmPassword">Confirm Password *</Label>
              <Input
                id="confirmPassword"
                type="password"
                value={userForm.confirmPassword}
                onChange={(e) => setUserForm({...userForm, confirmPassword: e.target.value})}
                placeholder="Confirm password"
              />
            </div>
            <div className="space-y-2">
              <Label>Roles *</Label>
              <Select
                value={userForm.roles[0] || 'Cashier'}
                onValueChange={(value: string) => setUserForm({...userForm, roles: [value]})}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select role" />
                </SelectTrigger>
                <SelectContent className="bg-white border-gray-200 shadow-lg">
                  {availableRoles.map(role => (
                    <SelectItem key={role} value={role} className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">{role}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Branch</Label>
              <Select
                value={userForm.branchId?.toString() || 'none'}
                onValueChange={(value: string) => setUserForm({...userForm, branchId: value && value !== 'none' ? parseInt(value) : null})}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select branch" />
                </SelectTrigger>
                <SelectContent className="bg-white border-gray-200 shadow-lg">
                  <SelectItem value="none" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">No Branch</SelectItem>
                  {branchesLoading ? (
                    <SelectItem value="loading" disabled>Loading branches...</SelectItem>
                  ) : branchesError ? (
                    <SelectItem value="error" disabled>Error loading branches</SelectItem>
                  ) : branchesData?.items?.map((branch: BranchDto) => (
                    <SelectItem key={branch.id} value={branch.id.toString()} className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">
                      {branch.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
          
          <div className="flex items-center space-x-2 mt-4">
            <Switch
              checked={userForm.isActive}
              onCheckedChange={(checked: boolean) => setUserForm({...userForm, isActive: checked})}
            />
            <Label>Active Account</Label>
          </div>
          
          <div className="flex justify-end gap-3 mt-6">
            <Button variant="outline" onClick={() => {
              setIsNewUserOpen(false);
              resetForm();
            }}>
              Cancel
            </Button>
            <Button 
              onClick={handleCreateUser} 
              disabled={createLoading}
              variant="golden"
            >
              {createLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Create User
            </Button>
          </div>
        </DialogContent>
      </Dialog>

      {/* User Details Dialog */}
      {selectedUser && (
        <Dialog open={!!selectedUser} onOpenChange={() => {
          setSelectedUser(null);
          setIsEditMode(false);
          setUserActivity(null);
          setUserPermissions(null);
        }}>
          <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto bg-white border-gray-200 shadow-lg">
            <DialogHeader>
              <DialogTitle className="flex items-center gap-2">
                <UserIcon className="h-5 w-5" />
                {selectedUser.FullName}
                {!selectedUser.IsActive && (
                  <Badge variant="secondary" className="bg-red-100 text-red-800">
                    Inactive
                  </Badge>
                )}
              </DialogTitle>
              <DialogDescription>
                Manage user details, roles, and permissions
              </DialogDescription>
            </DialogHeader>
            
            <Tabs defaultValue="details" className="w-full">
              <TabsList className="grid w-full grid-cols-4">
                <TabsTrigger value="details">Details</TabsTrigger>
                <TabsTrigger value="permissions">Permissions</TabsTrigger>
                <TabsTrigger value="activity">Activity</TabsTrigger>
                <TabsTrigger value="settings">Settings</TabsTrigger>
              </TabsList>
              
              <TabsContent value="details" className="space-y-4 mt-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label>Username</Label>
                    <Input value={userForm.userName} readOnly />
                  </div>
                  <div className="space-y-2">
                    <Label>Full Name</Label>
                    <Input
                      value={userForm.fullName}
                      onChange={(e) => setUserForm({...userForm, fullName: e.target.value})}
                      readOnly={!isEditMode}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label>Email</Label>
                    <Input
                      value={userForm.email}
                      onChange={(e) => setUserForm({...userForm, email: e.target.value})}
                      readOnly={!isEditMode}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label>Employee Code</Label>
                    <Input
                      value={userForm.employeeCode}
                      onChange={(e) => setUserForm({...userForm, employeeCode: e.target.value})}
                      readOnly={!isEditMode}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label>Roles</Label>
                    {isEditMode ? (
                      <Select
                        value={userForm.roles[0] || 'Cashier'}
                        onValueChange={(value: string) => setUserForm({...userForm, roles: [value]})}
                      >
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent className="bg-white border-gray-200 shadow-lg">
                          {availableRoles.map(role => (
                            <SelectItem key={role} value={role} className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">{role}</SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    ) : (
                      <div className="flex gap-2">
                        {selectedUser.Roles.map(role => (
                          <Badge key={role} variant="secondary" className={getRoleBadgeColor(selectedUser.Roles)}>
                            {role}
                          </Badge>
                        ))}
                      </div>
                    )}
                  </div>
                  <div className="space-y-2">
                    <Label>Branch</Label>
                    {isEditMode ? (
                      <Select
                        value={userForm.branchId?.toString() || 'none'}
                        onValueChange={(value: string) => setUserForm({...userForm, branchId: value && value !== 'none' ? parseInt(value) : null})}
                      >
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent className="bg-white border-gray-200 shadow-lg">
                          <SelectItem value="none" className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">No Branch</SelectItem>
                          {branchesLoading ? (
                            <SelectItem value="loading" disabled>Loading branches...</SelectItem>
                          ) : branchesError ? (
                            <SelectItem value="error" disabled>Error loading branches</SelectItem>
                          ) : branchesData?.items?.map((branch: BranchDto) => (
                            <SelectItem key={branch.id} value={branch.id.toString()} className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">
                              {branch.name}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    ) : (
                      <Input value={selectedUser.BranchName || 'No Branch'} readOnly />
                    )}
                  </div>
                  <div className="space-y-2">
                    <Label>Last Login</Label>
                    <div className="p-2 bg-muted rounded">
                      {selectedUser.LastLoginAt ? formatDate(selectedUser.LastLoginAt) : 'Never'}
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Label>Account Created</Label>
                    <div className="p-2 bg-muted rounded">
                      {formatDate(selectedUser.CreatedAt)}
                    </div>
                  </div>
                </div>
              </TabsContent>
              
              <TabsContent value="permissions" className="space-y-4 mt-4">
                {permissionsLoading ? (
                  <div className="flex justify-center py-8">
                    <Loader2 className="h-6 w-6 animate-spin" />
                  </div>
                ) : userPermissions ? (
                  <div className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                      {Object.entries(userPermissions.featureAccess).map(([feature, hasAccess]) => (
                        <div key={feature} className="flex items-center justify-between p-3 border rounded">
                          <Label className="capitalize font-medium">
                            {feature.replace(/([A-Z])/g, ' $1').trim()}
                          </Label>
                          <Badge variant={hasAccess ? "default" : "secondary"}>
                            {hasAccess ? 'Allowed' : 'Denied'}
                          </Badge>
                        </div>
                      ))}
                    </div>
                  </div>
                ) : (
                  <p className="text-muted-foreground">No permission data available</p>
                )}
              </TabsContent>
              
              <TabsContent value="activity" className="space-y-4 mt-4">
                {activityLoading ? (
                  <div className="flex justify-center py-8">
                    <Loader2 className="h-6 w-6 animate-spin" />
                  </div>
                ) : userActivity && userActivity.activities.length > 0 ? (
                  <div className="space-y-3">
                    {userActivity.activities.map((activity) => (
                      <div key={activity.Id} className="flex items-start space-x-3 p-3 border rounded">
                        <Activity className="h-5 w-5 text-muted-foreground mt-0.5" />
                        <div className="flex-1 space-y-1">
                          <div className="flex items-center justify-between">
                            <p className="text-sm font-medium">{activity.Action}</p>
                            <span className="text-xs text-muted-foreground">
                              {formatDate(activity.Timestamp)}
                            </span>
                          </div>
                          <p className="text-xs text-muted-foreground">
                            {activity.Details}
                          </p>
                          <div className="flex items-center gap-2 text-xs text-muted-foreground">
                            <span>{activity.EntityType}</span>
                            <span>•</span>
                            <span>{activity.IpAddress}</span>
                            {activity.BranchName && (
                              <>
                                <span>•</span>
                                <span>{activity.BranchName}</span>
                              </>
                            )}
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-muted-foreground">No activity records found</p>
                )}
              </TabsContent>
              
              <TabsContent value="settings" className="space-y-4 mt-4">
                <div className="space-y-6">
                  <div className="flex items-center justify-between p-4 border rounded">
                    <div>
                      <Label className="text-base font-medium">Account Status</Label>
                      <p className="text-sm text-muted-foreground">
                        {selectedUser.IsActive ? 'Account is active and can log in' : 'Account is disabled'}
                      </p>
                    </div>
                    <Switch
                      checked={selectedUser.IsActive}
                      onCheckedChange={(checked: boolean) => 
                        handleToggleUserStatus(selectedUser.Id, checked)
                      }
                      disabled={statusLoading}
                    />
                  </div>
                  
                  <div className="flex items-center justify-between p-4 border rounded">
                    <div>
                      <Label className="text-base font-medium">Email Confirmed</Label>
                      <p className="text-sm text-muted-foreground">
                        {selectedUser.EmailConfirmed ? 'Email address is verified' : 'Email needs verification'}
                      </p>
                    </div>
                    <Badge variant={selectedUser.EmailConfirmed ? "default" : "secondary"}>
                      {selectedUser.EmailConfirmed ? 'Verified' : 'Unverified'}
                    </Badge>
                  </div>
                  
                  <div className="flex items-center justify-between p-4 border rounded">
                    <div>
                      <Label className="text-base font-medium">Account Lockout</Label>
                      <p className="text-sm text-muted-foreground">
                        {selectedUser.LockoutEnd ? 'Account is temporarily locked' : 'Account is not locked'}
                      </p>
                    </div>
                    <Badge variant={selectedUser.LockoutEnd ? "destructive" : "default"}>
                      {selectedUser.LockoutEnd ? 'Locked' : 'Unlocked'}
                    </Badge>
                  </div>
                </div>
              </TabsContent>
            </Tabs>
            
            <div className="flex justify-end gap-3 mt-6">
              <Button variant="outline" onClick={() => {
                setSelectedUser(null);
                setIsEditMode(false);
              }}>
                {isEditMode ? 'Cancel' : 'Close'}
              </Button>
              {isEditMode && (
                <Button 
                  onClick={handleUpdateUser} 
                  disabled={updateLoading || rolesLoading}
                  variant="golden"
                >
                  {(updateLoading || rolesLoading) && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                  Update User
                </Button>
              )}
              {!isEditMode && (
                <Button 
                  onClick={() => setIsEditMode(true)} 
                  variant="golden"
                >
                  <Edit className="mr-2 h-4 w-4" />
                  Edit User
                </Button>
              )}
            </div>
          </DialogContent>
        </Dialog>
      )}

      {/* Reset Password Dialog */}
      <Dialog open={isResetPasswordOpen} onOpenChange={setIsResetPasswordOpen}>
        <DialogContent className="bg-white border-gray-200 shadow-lg">
          <DialogHeader>
            <DialogTitle>Reset Password</DialogTitle>
            <DialogDescription>
              {selectedUser && `Set a new password for ${selectedUser.FullName} (${selectedUser.UserName})`}
            </DialogDescription>
          </DialogHeader>
          
          <div className="space-y-4">
            <div className="p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
              <div className="flex items-start gap-3">
                <AlertTriangle className="h-5 w-5 text-yellow-600 mt-0.5" />
                <div>
                  <p className="text-sm font-medium text-yellow-800">Password Reset</p>
                  <p className="text-sm text-yellow-700">
                    The user will be required to change this password on their next login.
                  </p>
                </div>
              </div>
            </div>
            
            <div className="space-y-2">
              <Label>New Password</Label>
              <Input
                type="password"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                placeholder="Enter new password"
              />
            </div>
            
            <div className="space-y-2">
              <Label>Confirm New Password</Label>
              <Input
                type="password"
                value={confirmNewPassword}
                onChange={(e) => setConfirmNewPassword(e.target.value)}
                placeholder="Confirm new password"
              />
            </div>
          </div>
          
          <div className="flex justify-end gap-3 mt-6">
            <Button variant="outline" onClick={() => {
              setIsResetPasswordOpen(false);
              setNewPassword('');
              setConfirmNewPassword('');
            }}>
              Cancel
            </Button>
            <Button 
              onClick={handleResetPassword} 
              disabled={resetLoading}
              variant="golden"
            >
              {resetLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              <Key className="mr-2 h-4 w-4" />
              Reset Password
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}