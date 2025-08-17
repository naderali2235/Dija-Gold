import React from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Label } from '../ui/label';
import { Badge } from '../ui/badge';
import { Button } from '../ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../ui/select';
import { Printer, Wifi, Database, Settings as SettingsIcon, Loader2 } from 'lucide-react';
import { PRINTER_OPTIONS } from './constants';

interface HardwareSettingsProps {
  hardwareSettings: Record<string, string>;
  onHardwareSettingChange?: (setting: string, value: string) => void;
}

export function HardwareSettings({ hardwareSettings }: HardwareSettingsProps) {
  const [testingDevice, setTestingDevice] = React.useState<string | null>(null);

  const testConnection = async (device: string) => {
    setTestingDevice(device);
    
    try {
      // TODO: Replace with actual hardware API calls when available
      // For printer: await testPrinterConnection()
      // For scanner: await testScannerConnection()
      // For scale: await calibrateScale()
      
      // Simulate hardware test
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      alert(`${device} test completed successfully!`);
    } catch (error) {
      alert(`${device} test failed. Please check connection.`);
      console.error(`Hardware test failed for ${device}:`, error);
    } finally {
      setTestingDevice(null);
    }
  };

  return (
    <div className="space-y-6">
      {/* Hardware Configuration */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Printer className="h-5 w-5 text-[#D4AF37]" />
            Hardware Configuration
          </CardTitle>
          <CardDescription>
            Configure connected POS hardware devices
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="receiptPrinter">Receipt Printer</Label>
                <Select value={hardwareSettings.receiptPrinter}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent className="bg-white border-gray-200 shadow-lg">
                    {PRINTER_OPTIONS.map(option => (
                      <SelectItem key={option.value} value={option.value} className="hover:bg-[#F4E9B1] focus:bg-[#F4E9B1] focus:text-[#0D1B2A]">
                        {option.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              
              <div className="space-y-2">
                <Label>Cash Drawer</Label>
                <div className="flex items-center justify-between p-2 border rounded">
                  <span>{hardwareSettings.cashDrawer}</span>
                  <Badge variant="default" className="bg-green-100 text-green-800">
                    Connected
                  </Badge>
                </div>
              </div>
              
              <div className="space-y-2">
                <Label>Barcode Scanner</Label>
                <div className="flex items-center justify-between p-2 border rounded">
                  <span>{hardwareSettings.barcodeScanner}</span>
                  <Badge variant="default" className="bg-green-100 text-green-800">
                    Ready
                  </Badge>
                </div>
              </div>
              
              <div className="space-y-2">
                <Label>Digital Scale</Label>
                <div className="flex items-center justify-between p-2 border rounded">
                  <span>{hardwareSettings.scale}</span>
                  <Badge variant="default" className="bg-green-100 text-green-800">
                    Calibrated
                  </Badge>
                </div>
              </div>
            </div>
            
            <div className="flex gap-3">
              <Button 
                variant="outline" 
                className="touch-target" 
                onClick={() => testConnection('printer')}
                disabled={testingDevice === 'printer'}
              >
                {testingDevice === 'printer' ? (
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                ) : (
                  <Printer className="mr-2 h-4 w-4" />
                )}
                {testingDevice === 'printer' ? 'Testing...' : 'Test Printer'}
              </Button>
              <Button 
                variant="outline" 
                className="touch-target" 
                onClick={() => testConnection('scanner')}
                disabled={testingDevice === 'scanner'}
              >
                {testingDevice === 'scanner' ? (
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                ) : (
                  <SettingsIcon className="mr-2 h-4 w-4" />
                )}
                {testingDevice === 'scanner' ? 'Testing...' : 'Test Scanner'}
              </Button>
              <Button 
                variant="outline" 
                className="touch-target" 
                onClick={() => testConnection('scale')}
                disabled={testingDevice === 'scale'}
              >
                {testingDevice === 'scale' ? (
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                ) : (
                  <SettingsIcon className="mr-2 h-4 w-4" />
                )}
                {testingDevice === 'scale' ? 'Calibrating...' : 'Calibrate Scale'}
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Network and Backup */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Wifi className="h-5 w-5 text-[#D4AF37]" />
            Network & Backup
          </CardTitle>
          <CardDescription>
            Network connectivity and data backup status
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <div>
                <Label>Network Status</Label>
                <p className="text-sm text-muted-foreground">
                  Internet connectivity for real-time sync
                </p>
              </div>
              <Badge variant="default" className="bg-green-100 text-green-800">
                {hardwareSettings.networkStatus}
              </Badge>
            </div>
            
            <div className="flex items-center justify-between">
              <div>
                <Label>Backup Location</Label>
                <p className="text-sm text-muted-foreground">
                  Data backup destination
                </p>
              </div>
              <Badge variant="outline">
                {hardwareSettings.backupLocation}
              </Badge>
            </div>
            
            <div className="flex gap-3 pt-2">
              <Button 
                variant="outline" 
                className="touch-target"
                onClick={() => testConnection('backup')}
                disabled={testingDevice === 'backup'}
              >
                {testingDevice === 'backup' ? (
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                ) : (
                  <Database className="mr-2 h-4 w-4" />
                )}
                {testingDevice === 'backup' ? 'Backing up...' : 'Backup Now'}
              </Button>
              <Button 
                variant="outline" 
                className="touch-target"
                onClick={() => testConnection('network')}
                disabled={testingDevice === 'network'}
              >
                {testingDevice === 'network' ? (
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                ) : (
                  <Wifi className="mr-2 h-4 w-4" />
                )}
                {testingDevice === 'network' ? 'Testing...' : 'Test Connection'}
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}