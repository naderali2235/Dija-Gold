import React from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { Label } from '../ui/label';
import { Switch } from '../ui/switch';
import { Badge } from '../ui/badge';
import { Separator } from '../ui/separator';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '../ui/select';
import {
  AlertTriangle,
  Shield,
  Bell,
  Settings,
  CheckCircle,
  XCircle,
  Info,
} from 'lucide-react';
import {
  DEFAULT_OWNERSHIP_SETTINGS,
  OWNERSHIP_SEVERITY_OPTIONS,
  VALIDATION_RULE_OPTIONS,
} from './constants';

interface OwnershipSettingsProps {
  settings: typeof DEFAULT_OWNERSHIP_SETTINGS;
  onSettingsChange: (settings: typeof DEFAULT_OWNERSHIP_SETTINGS) => void;
  isManager: boolean;
}

export function OwnershipSettings({ settings, onSettingsChange, isManager }: OwnershipSettingsProps) {
  const handleChange = (key: string, value: any) => {
    onSettingsChange({
      ...settings,
      [key]: value,
    });
  };

  const handleNestedChange = (parentKey: string, childKey: string, value: any) => {
    const parentValue = settings[parentKey as keyof typeof settings];
    onSettingsChange({
      ...settings,
      [parentKey]: {
        ...(typeof parentValue === 'object' && parentValue !== null ? parentValue : {}),
        [childKey]: value,
      },
    });
  };

  const getThresholdColor = (threshold: string) => {
    const value = parseFloat(threshold);
    if (value >= 80) return 'bg-green-100 text-green-800';
    if (value >= 50) return 'bg-yellow-100 text-yellow-800';
    if (value >= 30) return 'bg-orange-100 text-orange-800';
    return 'bg-red-100 text-red-800';
  };

  const getThresholdLabel = (threshold: string) => {
    const value = parseFloat(threshold);
    if (value >= 80) return 'Excellent';
    if (value >= 50) return 'Good';
    if (value >= 30) return 'Fair';
    return 'Poor';
  };

  if (!isManager) {
    return (
      <Card className="pos-card">
        <CardContent className="pt-6">
          <div className="text-center py-8">
            <Shield className="h-12 w-12 mx-auto mb-4 text-muted-foreground" />
            <h3 className="text-lg font-medium mb-2">Access Restricted</h3>
            <p className="text-muted-foreground">
              Only managers can configure ownership settings.
            </p>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-6">
      {/* Ownership Thresholds */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Settings className="h-5 w-5" />
            Ownership Thresholds
          </CardTitle>
          <CardDescription>
            Configure ownership percentage thresholds for different alert levels
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="space-y-2">
              <Label htmlFor="lowOwnershipThreshold">Low Ownership Threshold (%)</Label>
              <Input
                id="lowOwnershipThreshold"
                type="number"
                min="0"
                max="100"
                value={settings.lowOwnershipThreshold}
                onChange={(e) => handleChange('lowOwnershipThreshold', e.target.value)}
                placeholder="50"
              />
              <div className="flex items-center gap-2">
                <Badge className={getThresholdColor(settings.lowOwnershipThreshold)}>
                  {getThresholdLabel(settings.lowOwnershipThreshold)}
                </Badge>
                <Info className="h-4 w-4 text-muted-foreground" />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="highOwnershipThreshold">High Ownership Threshold (%)</Label>
              <Input
                id="highOwnershipThreshold"
                type="number"
                min="0"
                max="100"
                value={settings.highOwnershipThreshold}
                onChange={(e) => handleChange('highOwnershipThreshold', e.target.value)}
                placeholder="80"
              />
              <div className="flex items-center gap-2">
                <Badge className={getThresholdColor(settings.highOwnershipThreshold)}>
                  {getThresholdLabel(settings.highOwnershipThreshold)}
                </Badge>
                <Info className="h-4 w-4 text-muted-foreground" />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="criticalOwnershipThreshold">Critical Threshold (%)</Label>
              <Input
                id="criticalOwnershipThreshold"
                type="number"
                min="0"
                max="100"
                value={settings.criticalOwnershipThreshold}
                onChange={(e) => handleChange('criticalOwnershipThreshold', e.target.value)}
                placeholder="30"
              />
              <div className="flex items-center gap-2">
                <Badge className={getThresholdColor(settings.criticalOwnershipThreshold)}>
                  {getThresholdLabel(settings.criticalOwnershipThreshold)}
                </Badge>
                <Info className="h-4 w-4 text-muted-foreground" />
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Core Features */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Shield className="h-5 w-5" />
            Core Features
          </CardTitle>
          <CardDescription>
            Enable or disable core ownership management features
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <Label htmlFor="enableOwnershipValidation">Ownership Validation</Label>
              <p className="text-sm text-muted-foreground">
                Validate ownership before allowing sales and inventory operations
              </p>
            </div>
            <Switch
              id="enableOwnershipValidation"
              checked={settings.enableOwnershipValidation}
              onCheckedChange={(checked) => handleChange('enableOwnershipValidation', checked)}
            />
          </div>

          <Separator />

          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <Label htmlFor="enableOwnershipAlerts">Ownership Alerts</Label>
              <p className="text-sm text-muted-foreground">
                Generate alerts for ownership issues and violations
              </p>
            </div>
            <Switch
              id="enableOwnershipAlerts"
              checked={settings.enableOwnershipAlerts}
              onCheckedChange={(checked) => handleChange('enableOwnershipAlerts', checked)}
            />
          </div>

          <Separator />

          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <Label htmlFor="enablePaymentTracking">Payment Tracking</Label>
              <p className="text-sm text-muted-foreground">
                Track outstanding payments and payment confirmations
              </p>
            </div>
            <Switch
              id="enablePaymentTracking"
              checked={settings.enablePaymentTracking}
              onCheckedChange={(checked) => handleChange('enablePaymentTracking', checked)}
            />
          </div>

          <Separator />

          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <Label htmlFor="autoGenerateAlerts">Auto-Generate Alerts</Label>
              <p className="text-sm text-muted-foreground">
                Automatically generate alerts based on ownership thresholds
              </p>
            </div>
            <Switch
              id="autoGenerateAlerts"
              checked={settings.autoGenerateAlerts}
              onCheckedChange={(checked) => handleChange('autoGenerateAlerts', checked)}
            />
          </div>
        </CardContent>
      </Card>

      {/* Validation Rules */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <CheckCircle className="h-5 w-5" />
            Validation Rules
          </CardTitle>
          <CardDescription>
            Configure specific validation rules for ownership management
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <Label htmlFor="preventSaleBelowThreshold">Prevent Sales Below Threshold</Label>
              <p className="text-sm text-muted-foreground">
                Block sales when ownership percentage is below the low threshold
              </p>
            </div>
            <Switch
              id="preventSaleBelowThreshold"
              checked={settings.validationRules.preventSaleBelowThreshold}
              onCheckedChange={(checked) => handleNestedChange('validationRules', 'preventSaleBelowThreshold', checked)}
            />
          </div>

          <Separator />

          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <Label htmlFor="requirePaymentConfirmation">Require Payment Confirmation</Label>
              <p className="text-sm text-muted-foreground">
                Require payment confirmation before updating ownership records
              </p>
            </div>
            <Switch
              id="requirePaymentConfirmation"
              checked={settings.validationRules.requirePaymentConfirmation}
              onCheckedChange={(checked) => handleNestedChange('validationRules', 'requirePaymentConfirmation', checked)}
            />
          </div>

          <Separator />

          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <Label htmlFor="enableTransferValidation">Transfer Validation</Label>
              <p className="text-sm text-muted-foreground">
                Validate ownership during inventory transfers between branches
              </p>
            </div>
            <Switch
              id="enableTransferValidation"
              checked={settings.validationRules.enableTransferValidation}
              onCheckedChange={(checked) => handleNestedChange('validationRules', 'enableTransferValidation', checked)}
            />
          </div>

          <Separator />

          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <Label htmlFor="enableInventoryValidation">Inventory Validation</Label>
              <p className="text-sm text-muted-foreground">
                Validate ownership during inventory adjustments and movements
              </p>
            </div>
            <Switch
              id="enableInventoryValidation"
              checked={settings.validationRules.enableInventoryValidation}
              onCheckedChange={(checked) => handleNestedChange('validationRules', 'enableInventoryValidation', checked)}
            />
          </div>
        </CardContent>
      </Card>

      {/* Alert Severity Levels */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5" />
            Alert Severity Levels
          </CardTitle>
          <CardDescription>
            Configure ownership percentage thresholds for different alert severity levels
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="space-y-2">
              <Label htmlFor="lowSeverity">Low Severity (%)</Label>
              <Input
                id="lowSeverity"
                type="number"
                min="0"
                max="100"
                value={settings.alertSeverityLevels.low}
                onChange={(e) => handleNestedChange('alertSeverityLevels', 'low', e.target.value)}
                placeholder="20"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="mediumSeverity">Medium Severity (%)</Label>
              <Input
                id="mediumSeverity"
                type="number"
                min="0"
                max="100"
                value={settings.alertSeverityLevels.medium}
                onChange={(e) => handleNestedChange('alertSeverityLevels', 'medium', e.target.value)}
                placeholder="50"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="highSeverity">High Severity (%)</Label>
              <Input
                id="highSeverity"
                type="number"
                min="0"
                max="100"
                value={settings.alertSeverityLevels.high}
                onChange={(e) => handleNestedChange('alertSeverityLevels', 'high', e.target.value)}
                placeholder="80"
              />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Notification Settings */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Bell className="h-5 w-5" />
            Notification Settings
          </CardTitle>
          <CardDescription>
            Configure how and where ownership alerts are displayed
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <Label htmlFor="emailAlerts">Email Alerts</Label>
              <p className="text-sm text-muted-foreground">
                Send ownership alerts via email
              </p>
            </div>
            <Switch
              id="emailAlerts"
              checked={settings.notificationSettings.emailAlerts}
              onCheckedChange={(checked) => handleNestedChange('notificationSettings', 'emailAlerts', checked)}
            />
          </div>

          <Separator />

          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <Label htmlFor="dashboardAlerts">Dashboard Alerts</Label>
              <p className="text-sm text-muted-foreground">
                Show ownership alerts on the main dashboard
              </p>
            </div>
            <Switch
              id="dashboardAlerts"
              checked={settings.notificationSettings.dashboardAlerts}
              onCheckedChange={(checked) => handleNestedChange('notificationSettings', 'dashboardAlerts', checked)}
            />
          </div>

          <Separator />

          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <Label htmlFor="reportAlerts">Report Alerts</Label>
              <p className="text-sm text-muted-foreground">
                Include ownership alerts in reports
              </p>
            </div>
            <Switch
              id="reportAlerts"
              checked={settings.notificationSettings.reportAlerts}
              onCheckedChange={(checked) => handleNestedChange('notificationSettings', 'reportAlerts', checked)}
            />
          </div>

          <Separator />

          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <Label htmlFor="lowOwnershipAlerts">Low Ownership Alerts</Label>
              <p className="text-sm text-muted-foreground">
                Generate alerts for products with low ownership
              </p>
            </div>
            <Switch
              id="lowOwnershipAlerts"
              checked={settings.notificationSettings.lowOwnershipAlerts}
              onCheckedChange={(checked) => handleNestedChange('notificationSettings', 'lowOwnershipAlerts', checked)}
            />
          </div>

          <Separator />

          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <Label htmlFor="outstandingPaymentAlerts">Outstanding Payment Alerts</Label>
              <p className="text-sm text-muted-foreground">
                Generate alerts for products with outstanding payments
              </p>
            </div>
            <Switch
              id="outstandingPaymentAlerts"
              checked={settings.notificationSettings.outstandingPaymentAlerts}
              onCheckedChange={(checked) => handleNestedChange('notificationSettings', 'outstandingPaymentAlerts', checked)}
            />
          </div>
        </CardContent>
      </Card>

      {/* Information Card */}
      <Card className="pos-card border-blue-200 bg-blue-50">
        <CardContent className="pt-6">
          <div className="flex items-start space-x-3">
            <Info className="h-5 w-5 text-blue-600 mt-0.5" />
            <div className="space-y-2">
              <h3 className="font-medium text-blue-900">Ownership Management</h3>
              <p className="text-sm text-blue-700">
                These settings control how the system manages product ownership, validates operations, 
                and generates alerts. Changes will affect sales, inventory, and reporting functionality.
              </p>
              <div className="text-xs text-blue-600">
                <p>• Low ownership threshold: Products below this percentage will trigger alerts</p>
                <p>• High ownership threshold: Products above this percentage are considered well-owned</p>
                <p>• Critical threshold: Products below this percentage may be blocked from sales</p>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
