import React, { useState } from 'react';
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
} from 'lucide-react';
import { useAuth } from './AuthContext';

interface User {
  id: string;
  username: string;
  fullName: string;
  email: string;
  role: 'Manager' | 'Cashier';
  branchId: string;
  branchName: string;
  isActive: boolean;
  lastLogin: string;
  createdDate: string;
  permissions: {
    sales: boolean;
    returns: boolean;
    inventory: boolean;
    customers: boolean;
    reports: boolean;
    settings: boolean;
  };
}

interface UserActivity {
  id: string;
  userId: string;
  username: string;
  action: string;
  module: string;
  timestamp: string;
  ipAddress: string;
  details: string;
}

export default function Users() {
  const { isManager } = useAuth();
  const [searchQuery, setSearchQuery] = useState('');
  const [roleFilter, setRoleFilter] = useState('all');
  const [branchFilter, setBranchFilter] = useState('all');
  const [isNewUserOpen, setIsNewUserOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [isEditMode, setIsEditMode] = useState(false);
  const [isResetPasswordOpen, setIsResetPasswordOpen] = useState(false);

  // Form state for new/edit user
  const [userForm, setUserForm] = useState({
    username: '',
    fullName: '',
    email: '',
    role: 'Cashier',
    branchId: '',
    password: '',
    confirmPassword: '',
    permissions: {
      sales: true,
      returns: false,
      inventory: false,
      customers: true,
      reports: false,
      settings: false,
    },
  });

  if (!isManager) {
    return (
      <div className="space-y-6">
        <h1 className="text-3xl text-[#0D1B2A]">User Management</h1>
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="text-center py-8">
              <Shield className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <p className="text-muted-foreground">Access denied. Only managers can access user management.</p>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  // Mock data
  const users: User[] = [
    {
      id: '1',
      username: 'manager',
      fullName: 'Store Manager',
      email: 'manager@dijapos.com',
      role: 'Manager',
      branchId: 'branch1',
      branchName: 'Main Branch',
      isActive: true,
      lastLogin: '2024-01-15T09:30:00Z',
      createdDate: '2023-01-01T10:00:00Z',
      permissions: {
        sales: true,
        returns: true,
        inventory: true,
        customers: true,
        reports: true,
        settings: true,
      },
    },
    {
      id: '2',
      username: 'cashier1',
      fullName: 'Ahmed Mohamed',
      email: 'ahmed@dijapos.com',
      role: 'Cashier',
      branchId: 'branch1',
      branchName: 'Main Branch',
      isActive: true,
      lastLogin: '2024-01-15T14:20:00Z',
      createdDate: '2023-03-15T11:30:00Z',
      permissions: {
        sales: true,
        returns: true,
        inventory: false,
        customers: true,
        reports: false,
        settings: false,
      },
    },
    {
      id: '3',
      username: 'cashier2',
      fullName: 'Fatima Hassan',
      email: 'fatima@dijapos.com',
      role: 'Cashier',
      branchId: 'branch2',
      branchName: 'Mall Branch',
      isActive: true,
      lastLogin: '2024-01-14T16:45:00Z',
      createdDate: '2023-06-10T09:15:00Z',
      permissions: {
        sales: true,
        returns: false,
        inventory: false,
        customers: true,
        reports: false,
        settings: false,
      },
    },
    {
      id: '4',
      username: 'cashier3',
      fullName: 'Mohamed Ali',
      email: 'mohamed@dijapos.com',
      role: 'Cashier',
      branchId: 'branch1',
      branchName: 'Main Branch',
      isActive: false,
      lastLogin: '2023-12-20T12:30:00Z',
      createdDate: '2023-08-20T14:45:00Z',
      permissions: {
        sales: true,
        returns: false,
        inventory: false,
        customers: false,
        reports: false,
        settings: false,
      },
    },
  ];

  const userActivities: UserActivity[] = [
    {
      id: '1',
      userId: '2',
      username: 'cashier1',
      action: 'Sale Completed',
      module: 'Sales',
      timestamp: '2024-01-15T14:20:00Z',
      ipAddress: '192.168.1.101',
      details: 'Sale INV-2024-001 for EGP 25,000',
    },
    {
      id: '2',
      userId: '1',
      username: 'manager',
      action: 'Return Processed',
      module: 'Returns',
      timestamp: '2024-01-15T13:45:00Z',
      ipAddress: '192.168.1.100',
      details: 'Return RET-2024-001 for EGP 5,000',
    },
    {
      id: '3',
      userId: '3',
      username: 'cashier2',
      action: 'Customer Added',
      module: 'Customers',
      timestamp: '2024-01-15T12:30:00Z',
      ipAddress: '192.168.1.102',
      details: 'New customer: Ahmed Hassan',
    },
    {
      id: '4',
      userId: '1',
      username: 'manager',
      action: 'User Created',
      module: 'Users',
      timestamp: '2024-01-15T10:15:00Z',
      ipAddress: '192.168.1.100',
      details: 'Created new cashier user: cashier4',
    },
    {
      id: '5',
      userId: '2',
      username: 'cashier1',
      action: 'Login',
      module: 'Authentication',
      timestamp: '2024-01-15T09:30:00Z',
      ipAddress: '192.168.1.101',
      details: 'User logged in successfully',
    },
  ];

  const branches = [
    { id: 'branch1', name: 'Main Branch' },
    { id: 'branch2', name: 'Mall Branch' },
    { id: 'branch3', name: 'Downtown Branch' },
  ];

  const filteredUsers = users.filter(user => {
    const matchesSearch = 
      user.fullName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      user.username.toLowerCase().includes(searchQuery.toLowerCase()) ||
      user.email.toLowerCase().includes(searchQuery.toLowerCase());
    
    const matchesRole = roleFilter === 'all' || user.role === roleFilter;
    const matchesBranch = branchFilter === 'all' || user.branchId === branchFilter;
    
    return matchesSearch && matchesRole && matchesBranch;
  });

  const handleCreateUser = () => {
    if (userForm.password !== userForm.confirmPassword) {
      alert('Passwords do not match');
      return;
    }
    console.log('Creating user:', userForm);
    setIsNewUserOpen(false);
    resetForm();
  };

  const handleUpdateUser = () => {
    console.log('Updating user:', selectedUser?.id, userForm);
    setSelectedUser(null);
    setIsEditMode(false);
    resetForm();
  };

  const handleDeleteUser = (userId: string) => {
    if (confirm('Are you sure you want to delete this user?')) {
      console.log('Deleting user:', userId);
    }
  };

  const handleResetPassword = () => {
    console.log('Resetting password for user:', selectedUser?.id);
    setIsResetPasswordOpen(false);
    alert('Password reset email sent to user');
  };

  const toggleUserStatus = (userId: string, isActive: boolean) => {
    console.log('Toggling user status:', userId, isActive);
  };

  const resetForm = () => {
    setUserForm({
      username: '',
      fullName: '',
      email: '',
      role: 'Cashier',
      branchId: '',
      password: '',
      confirmPassword: '',
      permissions: {
        sales: true,
        returns: false,
        inventory: false,
        customers: true,
        reports: false,
        settings: false,
      },
    });
  };

  const populateForm = (user: User) => {
    setUserForm({
      username: user.username,
      fullName: user.fullName,
      email: user.email,
      role: user.role,
      branchId: user.branchId,
      password: '',
      confirmPassword: '',
      permissions: user.permissions,
    });
  };

  const userStats = {
    total: users.length,
    active: users.filter(u => u.isActive).length,
    managers: users.filter(u => u.role === 'Manager').length,
    cashiers: users.filter(u => u.role === 'Cashier').length,
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl text-[#0D1B2A]">User Management</h1>
          <p className="text-muted-foreground">Manage system users and permissions</p>
        </div>
        <Dialog open={isNewUserOpen} onOpenChange={setIsNewUserOpen}>
          <DialogTrigger asChild>
            <Button className="touch-target pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
              <Plus className="mr-2 h-4 w-4" />
              Add User
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-2xl">
            <DialogHeader>
              <DialogTitle>Add New User</DialogTitle>
              <DialogDescription>
                Create a new user account with appropriate permissions
              </DialogDescription>
            </DialogHeader>
            <Tabs defaultValue="basic" className="w-full">
              <TabsList>
                <TabsTrigger value="basic">Basic Info</TabsTrigger>
                <TabsTrigger value="permissions">Permissions</TabsTrigger>
              </TabsList>
              
              <TabsContent value="basic" className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="username">Username *</Label>
                    <Input
                      id="username"
                      value={userForm.username}
                      onChange={(e) => setUserForm({...userForm, username: e.target.value})}
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
                    <Label htmlFor="email">Email Address *</Label>
                    <Input
                      id="email"
                      type="email"
                      value={userForm.email}
                      onChange={(e) => setUserForm({...userForm, email: e.target.value})}
                      placeholder="user@dijapos.com"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="role">Role *</Label>
                    <Select value={userForm.role} onValueChange={(value) => setUserForm({...userForm, role: value})}>
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Manager">Manager</SelectItem>
                        <SelectItem value="Cashier">Cashier</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="branchId">Branch *</Label>
                    <Select value={userForm.branchId} onValueChange={(value) => setUserForm({...userForm, branchId: value})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select branch" />
                      </SelectTrigger>
                      <SelectContent>
                        {branches.map(branch => (
                          <SelectItem key={branch.id} value={branch.id}>{branch.name}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
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
                </div>
              </TabsContent>
              
              <TabsContent value="permissions" className="space-y-4">
                <div className="space-y-4">
                  <p className="text-sm text-muted-foreground">
                    Select which modules this user can access
                  </p>
                  
                  {Object.entries(userForm.permissions).map(([permission, enabled]) => (
                    <div key={permission} className="flex items-center justify-between">
                      <Label htmlFor={permission} className="capitalize">
                        {permission.replace('_', ' ')}
                      </Label>
                      <Switch
                        id={permission}
                        checked={enabled}
                        onCheckedChange={(checked) => 
                          setUserForm({
                            ...userForm,
                            permissions: {
                              ...userForm.permissions,
                              [permission]: checked,
                            },
                          })
                        }
                      />
                    </div>
                  ))}
                </div>
              </TabsContent>
            </Tabs>
            
            <div className="flex justify-end gap-3 mt-6">
              <Button variant="outline" onClick={() => setIsNewUserOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleCreateUser} className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
                Create User
              </Button>
            </div>
          </DialogContent>
        </Dialog>
      </div>

      {/* User Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total Users</p>
                <p className="text-2xl text-[#0D1B2A]">{userStats.total}</p>
              </div>
              <UserCog className="h-8 w-8 text-[#D4AF37]" />
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Active Users</p>
                <p className="text-2xl text-[#0D1B2A]">{userStats.active}</p>
              </div>
              <UserCog className="h-8 w-8 text-green-600" />
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Managers</p>
                <p className="text-2xl text-[#0D1B2A]">{userStats.managers}</p>
              </div>
              <Shield className="h-8 w-8 text-purple-600" />
            </div>
          </CardContent>
        </Card>
        
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Cashiers</p>
                <p className="text-2xl text-[#0D1B2A]">{userStats.cashiers}</p>
              </div>
              <UserCog className="h-8 w-8 text-blue-600" />
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="users">
        <TabsList>
          <TabsTrigger value="users">Users</TabsTrigger>
          <TabsTrigger value="activity">Activity Log</TabsTrigger>
        </TabsList>
        
        <TabsContent value="users" className="space-y-4">
          {/* Filters */}
          <Card className="pos-card">
            <CardContent className="pt-6">
              <div className="flex flex-col md:flex-row gap-4">
                <div className="relative flex-1">
                  <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                  <Input
                    placeholder="Search by name, username, or email..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="pl-10 touch-target"
                  />
                </div>
                <Select value={roleFilter} onValueChange={setRoleFilter}>
                  <SelectTrigger className="w-full md:w-32">
                    <SelectValue placeholder="Role" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All Roles</SelectItem>
                    <SelectItem value="Manager">Manager</SelectItem>
                    <SelectItem value="Cashier">Cashier</SelectItem>
                  </SelectContent>
                </Select>
                <Select value={branchFilter} onValueChange={setBranchFilter}>
                  <SelectTrigger className="w-full md:w-40">
                    <SelectValue placeholder="Branch" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All Branches</SelectItem>
                    {branches.map(branch => (
                      <SelectItem key={branch.id} value={branch.id}>{branch.name}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </CardContent>
          </Card>

          {/* Users Table */}
          <Card className="pos-card">
            <CardHeader>
              <CardTitle>System Users</CardTitle>
              <CardDescription>
                {filteredUsers.length} user(s) found
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>User</TableHead>
                    <TableHead>Role</TableHead>
                    <TableHead>Branch</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Last Login</TableHead>
                    <TableHead>Created</TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredUsers.map((user) => (
                    <TableRow key={user.id}>
                      <TableCell>
                        <div>
                          <p className="font-medium">{user.fullName}</p>
                          <p className="text-sm text-muted-foreground">@{user.username}</p>
                          <p className="text-sm text-muted-foreground">{user.email}</p>
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge variant={user.role === 'Manager' ? 'default' : 'secondary'}>
                          {user.role === 'Manager' && <Shield className="mr-1 h-3 w-3" />}
                          {user.role}
                        </Badge>
                      </TableCell>
                      <TableCell>{user.branchName}</TableCell>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          <Badge variant={user.isActive ? 'default' : 'secondary'}>
                            {user.isActive ? 'Active' : 'Inactive'}
                          </Badge>
                          <Switch
                            checked={user.isActive}
                            onCheckedChange={(checked) => toggleUserStatus(user.id, checked)}
                            size="sm"
                          />
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-1 text-sm">
                          <Clock className="h-3 w-3" />
                          {new Date(user.lastLogin).toLocaleDateString()}
                        </div>
                      </TableCell>
                      <TableCell>{new Date(user.createdDate).toLocaleDateString()}</TableCell>
                      <TableCell>
                        <div className="flex gap-2">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => {
                              setSelectedUser(user);
                              populateForm(user);
                            }}
                            className="touch-target"
                          >
                            <Eye className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => {
                              setSelectedUser(user);
                              populateForm(user);
                              setIsEditMode(true);
                            }}
                            className="touch-target"
                          >
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => {
                              setSelectedUser(user);
                              setIsResetPasswordOpen(true);
                            }}
                            className="touch-target"
                          >
                            <Key className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleDeleteUser(user.id)}
                            className="touch-target text-destructive"
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>
        
        <TabsContent value="activity" className="space-y-4">
          <Card className="pos-card">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Activity className="h-5 w-5 text-[#D4AF37]" />
                User Activity Log
              </CardTitle>
              <CardDescription>
                Recent user actions and system events
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Timestamp</TableHead>
                    <TableHead>User</TableHead>
                    <TableHead>Action</TableHead>
                    <TableHead>Module</TableHead>
                    <TableHead>IP Address</TableHead>
                    <TableHead>Details</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {userActivities.map((activity) => (
                    <TableRow key={activity.id}>
                      <TableCell>
                        {new Date(activity.timestamp).toLocaleString()}
                      </TableCell>
                      <TableCell>
                        <p className="font-medium">{activity.username}</p>
                      </TableCell>
                      <TableCell>{activity.action}</TableCell>
                      <TableCell>
                        <Badge variant="outline">{activity.module}</Badge>
                      </TableCell>
                      <TableCell className="font-mono text-sm">{activity.ipAddress}</TableCell>
                      <TableCell>
                        <p className="max-w-60 truncate text-sm">{activity.details}</p>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* User Details/Edit Dialog */}
      {selectedUser && !isResetPasswordOpen && (
        <Dialog open={!!selectedUser} onOpenChange={() => {
          setSelectedUser(null);
          setIsEditMode(false);
        }}>
          <DialogContent className="max-w-2xl">
            <DialogHeader>
              <DialogTitle>
                {isEditMode ? 'Edit User' : 'User Details'} - {selectedUser.username}
              </DialogTitle>
            </DialogHeader>
            
            <Tabs defaultValue="basic" className="w-full">
              <TabsList>
                <TabsTrigger value="basic">Basic Info</TabsTrigger>
                <TabsTrigger value="permissions">Permissions</TabsTrigger>
              </TabsList>
              
              <TabsContent value="basic" className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label>Username</Label>
                    <Input
                      value={userForm.username}
                      onChange={(e) => setUserForm({...userForm, username: e.target.value})}
                      readOnly={!isEditMode}
                    />
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
                    <Label>Email Address</Label>
                    <Input
                      value={userForm.email}
                      onChange={(e) => setUserForm({...userForm, email: e.target.value})}
                      readOnly={!isEditMode}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label>Role</Label>
                    {isEditMode ? (
                      <Select value={userForm.role} onValueChange={(value) => setUserForm({...userForm, role: value})}>
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="Manager">Manager</SelectItem>
                          <SelectItem value="Cashier">Cashier</SelectItem>
                        </SelectContent>
                      </Select>
                    ) : (
                      <Input value={userForm.role} readOnly />
                    )}
                  </div>
                  <div className="space-y-2">
                    <Label>Last Login</Label>
                    <div className="p-2 bg-muted rounded">
                      {new Date(selectedUser.lastLogin).toLocaleString()}
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Label>Account Created</Label>
                    <div className="p-2 bg-muted rounded">
                      {new Date(selectedUser.createdDate).toLocaleString()}
                    </div>
                  </div>
                </div>
              </TabsContent>
              
              <TabsContent value="permissions" className="space-y-4">
                <div className="space-y-4">
                  {Object.entries(userForm.permissions).map(([permission, enabled]) => (
                    <div key={permission} className="flex items-center justify-between">
                      <Label className="capitalize">
                        {permission.replace('_', ' ')}
                      </Label>
                      <Switch
                        checked={enabled}
                        onCheckedChange={(checked) => 
                          setUserForm({
                            ...userForm,
                            permissions: {
                              ...userForm.permissions,
                              [permission]: checked,
                            },
                          })
                        }
                        disabled={!isEditMode}
                      />
                    </div>
                  ))}
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
                <Button onClick={handleUpdateUser} className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
                  Update User
                </Button>
              )}
              {!isEditMode && (
                <Button onClick={() => setIsEditMode(true)} className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
                  <Edit className="mr-2 h-4 w-4" />
                  Edit User
                </Button>
              )}
            </div>
          </DialogContent>
        </Dialog>
      )}

      {/* Reset Password Dialog */}
      {isResetPasswordOpen && selectedUser && (
        <Dialog open={isResetPasswordOpen} onOpenChange={setIsResetPasswordOpen}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Reset Password</DialogTitle>
              <DialogDescription>
                Reset password for {selectedUser.fullName} ({selectedUser.username})
              </DialogDescription>
            </DialogHeader>
            
            <div className="space-y-4">
              <div className="p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
                <div className="flex items-start gap-3">
                  <AlertTriangle className="h-5 w-5 text-yellow-600 mt-0.5" />
                  <div>
                    <p className="text-sm font-medium text-yellow-800">Password Reset</p>
                    <p className="text-sm text-yellow-700">
                      A password reset email will be sent to the user's email address. 
                      They will be required to create a new password on their next login.
                    </p>
                  </div>
                </div>
              </div>
              
              <div className="space-y-2">
                <Label>User Email</Label>
                <div className="p-2 bg-muted rounded">
                  {selectedUser.email}
                </div>
              </div>
            </div>
            
            <div className="flex justify-end gap-3 mt-6">
              <Button variant="outline" onClick={() => setIsResetPasswordOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleResetPassword} className="pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]">
                <Key className="mr-2 h-4 w-4" />
                Send Reset Email
              </Button>
            </div>
          </DialogContent>
        </Dialog>
      )}
    </div>
  );
}