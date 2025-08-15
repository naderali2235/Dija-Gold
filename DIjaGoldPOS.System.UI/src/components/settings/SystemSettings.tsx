import React from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Input } from '../ui/input';
import { Label } from '../ui/label';
import { Textarea } from '../ui/textarea';
import { Switch } from '../ui/switch';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../ui/select';
import { Settings as SettingsIcon } from 'lucide-react';
import { CURRENCY_OPTIONS, TIMEZONE_OPTIONS, LANGUAGE_OPTIONS } from './constants';

interface SystemSettingsProps {
  systemSettings: Record<string, any>;
  onSystemSettingChange: (setting: string, value: string | boolean) => void;
}

export function SystemSettings({ systemSettings, onSystemSettingChange }: SystemSettingsProps) {
  return (
    <div className="space-y-6">
      {/* Company Information */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <SettingsIcon className="h-5 w-5 text-[#D4AF37]" />
            Company Information
          </CardTitle>
          <CardDescription>
            Basic company details for receipts and reports
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="companyName">Company Name</Label>
              <Input
                id="companyName"
                value={systemSettings.companyName}
                onChange={(e) => onSystemSettingChange('companyName', e.target.value)}
                placeholder="DijaGold Jewelry"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="phone">Phone Number</Label>
              <Input
                id="phone"
                value={systemSettings.phone}
                onChange={(e) => onSystemSettingChange('phone', e.target.value)}
                placeholder="+20 100 123 4567"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="email">Email Address</Label>
              <Input
                id="email"
                type="email"
                value={systemSettings.email}
                onChange={(e) => onSystemSettingChange('email', e.target.value)}
                placeholder="info@dijapos.com"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="lowStockThreshold">Low Stock Threshold</Label>
              <Input
                id="lowStockThreshold"
                type="number"
                value={systemSettings.lowStockThreshold}
                onChange={(e) => onSystemSettingChange('lowStockThreshold', e.target.value)}
                placeholder="5"
              />
            </div>
            <div className="col-span-1 md:col-span-2 space-y-2">
              <Label htmlFor="address">Address</Label>
              <Textarea
                id="address"
                value={systemSettings.address}
                onChange={(e) => onSystemSettingChange('address', e.target.value)}
                placeholder="123 Khan El-Khalili, Cairo, Egypt"
                rows={2}
              />
            </div>
            <div className="col-span-1 md:col-span-2 space-y-2">
              <Label htmlFor="receiptFooter">Receipt Footer</Label>
              <Textarea
                id="receiptFooter"
                value={systemSettings.receiptFooter}
                onChange={(e) => onSystemSettingChange('receiptFooter', e.target.value)}
                placeholder="Thank you for shopping with DijaGold!"
                rows={2}
              />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Regional Settings */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle>Regional Settings</CardTitle>
          <CardDescription>
            Configure currency, timezone, and language preferences
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="space-y-2">
              <Label htmlFor="currency">Currency</Label>
              <Select 
                value={systemSettings.currency} 
                onValueChange={(value) => onSystemSettingChange('currency', value)}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {CURRENCY_OPTIONS.map(option => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="timezone">Timezone</Label>
              <Select 
                value={systemSettings.timezone} 
                onValueChange={(value) => onSystemSettingChange('timezone', value)}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {TIMEZONE_OPTIONS.map(option => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="language">Language</Label>
              <Select 
                value={systemSettings.language} 
                onValueChange={(value) => onSystemSettingChange('language', value)}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {LANGUAGE_OPTIONS.map(option => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* System Preferences */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle>System Preferences</CardTitle>
          <CardDescription>
            Configure system behavior and features
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <div>
                <Label htmlFor="autoBackup">Automatic Backup</Label>
                <p className="text-sm text-muted-foreground">
                  Automatically backup data daily
                </p>
              </div>
              <Switch
                id="autoBackup"
                checked={systemSettings.autoBackup}
                onCheckedChange={(checked) => onSystemSettingChange('autoBackup', checked)}
              />
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}