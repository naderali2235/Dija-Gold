import React from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Input } from '../ui/input';
import { Label } from '../ui/label';
import { Switch } from '../ui/switch';
import { Shield, AlertTriangle } from 'lucide-react';

interface SecuritySettingsProps {
  securitySettings: Record<string, any>;
  onSecuritySettingChange: (setting: string, value: string | boolean) => void;
}

export function SecuritySettings({ securitySettings, onSecuritySettingChange }: SecuritySettingsProps) {
  return (
    <div className="space-y-6">
      {/* Password Policy */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Shield className="h-5 w-5 text-[#D4AF37]" />
            Password Policy
          </CardTitle>
          <CardDescription>
            Configure password requirements and expiration
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="passwordExpiry">Password Expiry (days)</Label>
              <Input
                id="passwordExpiry"
                type="number"
                value={securitySettings.passwordExpiry}
                onChange={(e) => onSecuritySettingChange('passwordExpiry', e.target.value)}
                placeholder="90"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="maxLoginAttempts">Max Login Attempts</Label>
              <Input
                id="maxLoginAttempts"
                type="number"
                value={securitySettings.maxLoginAttempts}
                onChange={(e) => onSecuritySettingChange('maxLoginAttempts', e.target.value)}
                placeholder="3"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="sessionTimeout">Session Timeout (minutes)</Label>
              <Input
                id="sessionTimeout"
                type="number"
                value={securitySettings.sessionTimeout}
                onChange={(e) => onSecuritySettingChange('sessionTimeout', e.target.value)}
                placeholder="30"
              />
            </div>
          </div>
          
          <div className="mt-6 space-y-4">
            <div className="flex items-center justify-between">
              <div>
                <Label htmlFor="requireStrongPassword">Require Strong Passwords</Label>
                <p className="text-sm text-muted-foreground">
                  Enforce minimum 8 characters with mixed case, numbers, and symbols
                </p>
              </div>
              <Switch
                id="requireStrongPassword"
                checked={securitySettings.requireStrongPassword}
                onCheckedChange={(checked) => onSecuritySettingChange('requireStrongPassword', checked)}
              />
            </div>
            
            <div className="flex items-center justify-between">
              <div>
                <Label htmlFor="enableTwoFactor">Two-Factor Authentication</Label>
                <p className="text-sm text-muted-foreground">
                  Require SMS or email verification for login
                </p>
              </div>
              <Switch
                id="enableTwoFactor"
                checked={securitySettings.enableTwoFactor}
                onCheckedChange={(checked) => onSecuritySettingChange('enableTwoFactor', checked)}
              />
            </div>
            
            <div className="flex items-center justify-between">
              <div>
                <Label htmlFor="auditLogging">Audit Logging</Label>
                <p className="text-sm text-muted-foreground">
                  Log all user actions for security monitoring
                </p>
              </div>
              <Switch
                id="auditLogging"
                checked={securitySettings.auditLogging}
                onCheckedChange={(checked) => onSecuritySettingChange('auditLogging', checked)}
              />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Security Notice */}
      <Card className="pos-card">
        <CardContent className="pt-6">
          <div className="flex items-start gap-3 p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
            <AlertTriangle className="h-5 w-5 text-yellow-600 mt-0.5" />
            <div>
              <p className="text-sm font-medium text-yellow-800">Security Reminder</p>
              <p className="text-sm text-yellow-700 mt-1">
                Changes to security settings will take effect after the next user login. 
                Ensure all users are notified of any policy changes.
              </p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}