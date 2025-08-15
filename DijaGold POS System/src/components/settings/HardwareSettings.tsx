import React from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Label } from '../ui/label';
import { Badge } from '../ui/badge';
import { Button } from '../ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../ui/select';
import { Printer, Wifi, Database, Settings as SettingsIcon } from 'lucide-react';
import { PRINTER_OPTIONS } from './constants';

interface HardwareSettingsProps {
  hardwareSettings: Record<string, string>;
  onHardwareSettingChange?: (setting: string, value: string) => void;
}

export function HardwareSettings({ hardwareSettings }: HardwareSettingsProps) {
  const testConnection = (device: string) => {
    // Mock test functionality
    alert(`Testing connection to ${device}...`);
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
                  <SelectContent>
                    {PRINTER_OPTIONS.map(option => (
                      <SelectItem key={option.value} value={option.value}>
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
              <Button variant="outline" onClick={() => testConnection('printer')}>
                <Printer className="mr-2 h-4 w-4" />
                Test Printer
              </Button>
              <Button variant="outline" onClick={() => testConnection('scanner')}>
                <SettingsIcon className="mr-2 h-4 w-4" />
                Test Scanner
              </Button>
              <Button variant="outline" onClick={() => testConnection('scale')}>
                <SettingsIcon className="mr-2 h-4 w-4" />
                Calibrate Scale
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
              <Button variant="outline">
                <Database className="mr-2 h-4 w-4" />
                Backup Now
              </Button>
              <Button variant="outline">
                <Wifi className="mr-2 h-4 w-4" />
                Test Connection
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}