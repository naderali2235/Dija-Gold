import React, { useState } from 'react';
import { Card, CardContent } from './ui/card';
import { Button } from './ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from './ui/tabs';
import { Shield, Save, AlertTriangle, RotateCcw } from 'lucide-react';
import { useAuth } from './AuthContext';
import { PricingSettings } from './settings/PricingSettings';
import { SystemSettings } from './settings/SystemSettings';
import { SecuritySettings } from './settings/SecuritySettings';
import { NotificationSettings } from './settings/NotificationSettings';
import { HardwareSettings } from './settings/HardwareSettings';
import {
  DEFAULT_GOLD_RATES,
  DEFAULT_TAX_SETTINGS,
  DEFAULT_SYSTEM_SETTINGS,
  DEFAULT_SECURITY_SETTINGS,
  DEFAULT_NOTIFICATION_SETTINGS,
  DEFAULT_HARDWARE_SETTINGS,
} from './settings/constants';

export default function Settings() {
  const { isManager } = useAuth();
  const [activeTab, setActiveTab] = useState('pricing');
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);

  // Settings states
  const [goldRates, setGoldRates] = useState(DEFAULT_GOLD_RATES);
  const [taxSettings, setTaxSettings] = useState(DEFAULT_TAX_SETTINGS);
  const [systemSettings, setSystemSettings] = useState(DEFAULT_SYSTEM_SETTINGS);
  const [securitySettings, setSecuritySettings] = useState(DEFAULT_SECURITY_SETTINGS);
  const [notificationSettings, setNotificationSettings] = useState(DEFAULT_NOTIFICATION_SETTINGS);
  const [hardwareSettings] = useState(DEFAULT_HARDWARE_SETTINGS);

  if (!isManager) {
    return (
      <div className="space-y-6">
        <h1 className="text-3xl text-[#0D1B2A]">System Settings</h1>
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="text-center py-8">
              <Shield className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <p className="text-muted-foreground">Access denied. Only managers can access system settings.</p>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  const handleGoldRateChange = (karat: string, value: string) => {
    setGoldRates({ ...goldRates, [karat]: value });
    setHasUnsavedChanges(true);
  };

  const handleTaxSettingChange = (setting: string, value: string) => {
    setTaxSettings({ ...taxSettings, [setting]: value });
    setHasUnsavedChanges(true);
  };

  const handleSystemSettingChange = (setting: string, value: string | boolean) => {
    setSystemSettings({ ...systemSettings, [setting]: value });
    setHasUnsavedChanges(true);
  };

  const handleSecuritySettingChange = (setting: string, value: string | boolean) => {
    setSecuritySettings({ ...securitySettings, [setting]: value });
    setHasUnsavedChanges(true);
  };

  const handleNotificationSettingChange = (setting: string, value: boolean) => {
    setNotificationSettings({ ...notificationSettings, [setting]: value });
    setHasUnsavedChanges(true);
  };

  const handleSaveSettings = () => {
    console.log('Saving settings:', {
      goldRates,
      taxSettings,
      systemSettings,
      securitySettings,
      notificationSettings,
    });
    setHasUnsavedChanges(false);
    alert('Settings saved successfully!');
  };

  const resetToDefaults = () => {
    if (window.confirm('Are you sure you want to reset to default settings? This action cannot be undone.')) {
      setGoldRates(DEFAULT_GOLD_RATES);
      setTaxSettings(DEFAULT_TAX_SETTINGS);
      setSystemSettings(DEFAULT_SYSTEM_SETTINGS);
      setSecuritySettings(DEFAULT_SECURITY_SETTINGS);
      setNotificationSettings(DEFAULT_NOTIFICATION_SETTINGS);
      setHasUnsavedChanges(false);
      alert('Settings reset to defaults!');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl text-[#0D1B2A]">System Settings</h1>
          <p className="text-muted-foreground">Configure system preferences and business rules</p>
        </div>
        <div className="flex gap-3">
          <Button variant="outline" onClick={resetToDefaults}>
            <RotateCcw className="mr-2 h-4 w-4" />
            Reset to Defaults
          </Button>
          <Button 
            onClick={handleSaveSettings}
            disabled={!hasUnsavedChanges}
            className="touch-target pos-button-primary bg-[#D4AF37] hover:bg-[#B8941F] text-[#0D1B2A]"
          >
            <Save className="mr-2 h-4 w-4" />
            Save Changes
          </Button>
        </div>
      </div>

      {hasUnsavedChanges && (
        <Card className="pos-card">
          <CardContent className="pt-6">
            <div className="flex items-start gap-3 p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
              <AlertTriangle className="h-5 w-5 text-yellow-600 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-yellow-800">Unsaved Changes</p>
                <p className="text-sm text-yellow-700">
                  You have unsaved changes. Don't forget to save your settings before leaving this page.
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="grid w-full grid-cols-5">
          <TabsTrigger value="pricing">Pricing</TabsTrigger>
          <TabsTrigger value="system">System</TabsTrigger>
          <TabsTrigger value="security">Security</TabsTrigger>
          <TabsTrigger value="notifications">Notifications</TabsTrigger>
          <TabsTrigger value="hardware">Hardware</TabsTrigger>
        </TabsList>
        
        <TabsContent value="pricing">
          <PricingSettings
            goldRates={goldRates}
            taxSettings={taxSettings}
            onGoldRateChange={handleGoldRateChange}
            onTaxSettingChange={handleTaxSettingChange}
          />
        </TabsContent>
        
        <TabsContent value="system">
          <SystemSettings
            systemSettings={systemSettings}
            onSystemSettingChange={handleSystemSettingChange}
          />
        </TabsContent>
        
        <TabsContent value="security">
          <SecuritySettings
            securitySettings={securitySettings}
            onSecuritySettingChange={handleSecuritySettingChange}
          />
        </TabsContent>
        
        <TabsContent value="notifications">
          <NotificationSettings
            notificationSettings={notificationSettings}
            onNotificationSettingChange={handleNotificationSettingChange}
          />
        </TabsContent>
        
        <TabsContent value="hardware">
          <HardwareSettings hardwareSettings={hardwareSettings} />
        </TabsContent>
      </Tabs>
    </div>
  );
}