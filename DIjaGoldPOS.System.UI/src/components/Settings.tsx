import React, { useState, useEffect, useCallback } from 'react';
import { Card, CardContent } from './ui/card';
import { Button } from './ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from './ui/tabs';
import { Shield, Save, AlertTriangle, Settings as SettingsIcon } from 'lucide-react';
import { useAuth } from './AuthContext';
import { useGoldRates } from '../hooks/useApi';
import { productOwnershipApi } from '../services/api';
import { PricingSettings } from './settings/PricingSettings';
import { GoldRatesDisplay, transformGoldRates, headerToKaratMapping, GoldRatesMap } from './shared/GoldRatesDisplay';
import { SystemSettings } from './settings/SystemSettings';
import { SecuritySettings } from './settings/SecuritySettings';
import { NotificationSettings } from './settings/NotificationSettings';
import { OwnershipSettings } from './settings/OwnershipSettings';
import {
  DEFAULT_TAX_SETTINGS,
  DEFAULT_SYSTEM_SETTINGS,
  DEFAULT_SECURITY_SETTINGS,
  DEFAULT_NOTIFICATION_SETTINGS,
  DEFAULT_OWNERSHIP_SETTINGS,
} from './settings/constants';

export default function Settings() {
  const { isManager } = useAuth();
  const [activeTab, setActiveTab] = useState('pricing');
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);
  const { data: goldRatesData, updateRates, fetchRates } = useGoldRates();


  // Original values to compare against for detecting changes
  const [originalGoldRates, setOriginalGoldRates] = useState<Record<string, string>>({
    '18K GOLD': '0',
    '21K GOLD': '0',
    '22K GOLD': '0',
    '24K GOLD': '0'
  });
  const [originalTaxSettings, setOriginalTaxSettings] = useState(DEFAULT_TAX_SETTINGS);
  const [originalSystemSettings, setOriginalSystemSettings] = useState(DEFAULT_SYSTEM_SETTINGS);
  const [originalSecuritySettings, setOriginalSecuritySettings] = useState(DEFAULT_SECURITY_SETTINGS);
  const [originalNotificationSettings, setOriginalNotificationSettings] = useState(DEFAULT_NOTIFICATION_SETTINGS);
  const [originalOwnershipSettings, setOriginalOwnershipSettings] = useState(DEFAULT_OWNERSHIP_SETTINGS);

  // Settings states
  const [goldRates, setGoldRates] = useState<Record<string, string>>({
    '18K GOLD': '0',
    '21K GOLD': '0',
    '22K GOLD': '0',
    '24K GOLD': '0'
  });
  const [taxSettings, setTaxSettings] = useState(DEFAULT_TAX_SETTINGS);
  const [systemSettings, setSystemSettings] = useState(DEFAULT_SYSTEM_SETTINGS);
  const [securitySettings, setSecuritySettings] = useState(DEFAULT_SECURITY_SETTINGS);
  const [notificationSettings, setNotificationSettings] = useState(DEFAULT_NOTIFICATION_SETTINGS);
  const [ownershipSettings, setOwnershipSettings] = useState(DEFAULT_OWNERSHIP_SETTINGS);

  const [resetManuallyEditedTrigger, setResetManuallyEditedTrigger] = useState(0);
  const [isSaving, setIsSaving] = useState(false);

  // Function to check if there are any unsaved changes
  const checkForChanges = useCallback(() => {
    const goldRatesChanged = JSON.stringify(goldRates) !== JSON.stringify(originalGoldRates);
    const taxSettingsChanged = JSON.stringify(taxSettings) !== JSON.stringify(originalTaxSettings);
    const systemSettingsChanged = JSON.stringify(systemSettings) !== JSON.stringify(originalSystemSettings);
    const securitySettingsChanged = JSON.stringify(securitySettings) !== JSON.stringify(originalSecuritySettings);
    const notificationSettingsChanged = JSON.stringify(notificationSettings) !== JSON.stringify(originalNotificationSettings);
    const ownershipSettingsChanged = JSON.stringify(ownershipSettings) !== JSON.stringify(originalOwnershipSettings);
    
    const hasChanges = goldRatesChanged || taxSettingsChanged || systemSettingsChanged || 
                      securitySettingsChanged || notificationSettingsChanged || ownershipSettingsChanged;
    
    setHasUnsavedChanges(hasChanges);
  }, [goldRates, originalGoldRates, taxSettings, originalTaxSettings, systemSettings, originalSystemSettings, securitySettings, originalSecuritySettings, notificationSettings, originalNotificationSettings, ownershipSettings, originalOwnershipSettings]);



  // Fetch data when component mounts
  useEffect(() => {
    if (isManager) {
      fetchRates();
    }
  }, [isManager]); // Only depend on isManager, fetchRates is stable

  // Update gold rates when API data is fetched

  useEffect(() => {
    if (goldRatesData && goldRatesData.length > 0) {
      const transformedRates = transformGoldRates(goldRatesData);
      setGoldRates(transformedRates);
      setOriginalGoldRates(transformedRates);
    }
  }, [goldRatesData]); // Removed karatTypesData dependency since we use fixed headers

  // Check for changes whenever any setting changes
  useEffect(() => {
    checkForChanges();
  }, [checkForChanges]);

  // Handler functions - must be defined before any conditional returns
  const handleGoldRateChange = useCallback((karat: string, value: string) => {
    setGoldRates(prev => ({ ...prev, [karat]: value }));
  }, []);

  const handleTaxSettingChange = useCallback((setting: string, value: string) => {
    setTaxSettings({ ...taxSettings, [setting]: value });
  }, [taxSettings]);

  const handleSystemSettingChange = useCallback((setting: string, value: string | boolean) => {
    setSystemSettings({ ...systemSettings, [setting]: value });
  }, [systemSettings]);

  const handleSecuritySettingChange = useCallback((setting: string, value: string | boolean) => {
    setSecuritySettings({ ...securitySettings, [setting]: value });
  }, [securitySettings]);

  const handleNotificationSettingChange = useCallback((setting: string, value: boolean) => {
    setNotificationSettings({ ...notificationSettings, [setting]: value });
  }, [notificationSettings]);

  const handleOwnershipSettingChange = useCallback((settings: typeof DEFAULT_OWNERSHIP_SETTINGS) => {
    setOwnershipSettings(settings);
  }, []);

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

  const handleSaveSettings = async () => {
    try {
      setIsSaving(true);
      
      // Convert gold rates to the format expected by the API
      const goldRateUpdates = Object.entries(goldRates).map(([karatDisplayName, rate]) => {
        const karatTypeId = headerToKaratMapping[karatDisplayName] || 1; // Default to 1 if not found
        
        return {
          karatTypeId: karatTypeId, // Use correct property name expected by backend
          ratePerGram: parseFloat(rate),
          effectiveFrom: new Date().toISOString()
        };
      });

      // Save gold rates to API
      await updateRates(goldRateUpdates);

      console.log('Settings saved successfully:', {
        goldRates,
        taxSettings,
        systemSettings,
        securitySettings,
        notificationSettings,
        ownershipSettings,
      });
      
      // Update original values to current values after successful save
      setOriginalGoldRates({ ...goldRates });
      setOriginalTaxSettings({ ...taxSettings });
      setOriginalSystemSettings({ ...systemSettings });
      setOriginalSecuritySettings({ ...securitySettings });
      setOriginalNotificationSettings({ ...notificationSettings });
      setOriginalOwnershipSettings({ ...ownershipSettings });
      
      setHasUnsavedChanges(false);
      // Trigger refresh of rates from database after saving
      setResetManuallyEditedTrigger(prev => prev + 1);
      alert('Settings saved successfully!');
    } catch (error) {
      console.error('Error saving settings:', error);
      alert('Failed to save settings. Please try again.');
    } finally {
      setIsSaving(false);
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
          <Button 
            onClick={handleSaveSettings}
            disabled={!hasUnsavedChanges || isSaving}
            className="touch-target"
            variant="golden"
          >
            <Save className="mr-2 h-4 w-4" />
            {isSaving ? 'Saving...' : 'Save Changes'}
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
          <TabsTrigger value="ownership">Ownership</TabsTrigger>
        </TabsList>
        
        <TabsContent value="pricing">
          <PricingSettings
            goldRates={goldRates}
            taxSettings={taxSettings}
            onGoldRateChange={handleGoldRateChange}
            onTaxSettingChange={handleTaxSettingChange}
            onResetManuallyEdited={() => resetManuallyEditedTrigger}
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
        
        <TabsContent value="ownership">
          <OwnershipSettings
            settings={ownershipSettings}
            onSettingsChange={handleOwnershipSettingChange}
            isManager={isManager}
          />
        </TabsContent>
      </Tabs>
    </div>
  );
}