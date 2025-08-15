import React from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Input } from '../ui/input';
import { Label } from '../ui/label';
import { Button } from '../ui/button';
import { DollarSign, TrendingUp } from 'lucide-react';
import { formatCurrency } from '../utils/currency';

interface PricingSettingsProps {
  goldRates: Record<string, string>;
  taxSettings: Record<string, string>;
  onGoldRateChange: (karat: string, value: string) => void;
  onTaxSettingChange: (setting: string, value: string) => void;
}

export function PricingSettings({
  goldRates,
  taxSettings,
  onGoldRateChange,
  onTaxSettingChange,
}: PricingSettingsProps) {
  return (
    <div className="space-y-6">
      {/* Gold Rates */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <TrendingUp className="h-5 w-5 text-[#D4AF37]" />
            Gold Rates (per gram)
          </CardTitle>
          <CardDescription>
            Current gold rates in Egyptian Pounds per gram
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {Object.entries(goldRates).map(([karat, rate]) => (
              <div key={karat} className="space-y-2">
                <Label htmlFor={`rate-${karat}`}>{karat.toUpperCase()}</Label>
                <div className="relative">
                  <Input
                    id={`rate-${karat}`}
                    type="number"
                    step="0.01"
                    value={rate}
                    onChange={(e) => onGoldRateChange(karat, e.target.value)}
                    className="pl-12"
                  />
                  <div className="absolute left-3 top-3 text-muted-foreground text-sm">
                    EGP
                  </div>
                </div>
                <p className="text-xs text-muted-foreground">
                  Current: {formatCurrency(parseFloat(rate) || 0)}
                </p>
              </div>
            ))}
          </div>
          <div className="flex justify-end mt-4">
            <Button variant="outline" size="sm">
              <TrendingUp className="mr-2 h-4 w-4" />
              Update from Market
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Tax and Charges */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <DollarSign className="h-5 w-5 text-[#D4AF37]" />
            Tax & Charges
          </CardTitle>
          <CardDescription>
            Configure tax rates and making charges
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="gst">GST Rate (%)</Label>
              <Input
                id="gst"
                type="number"
                step="0.1"
                value={taxSettings.gst}
                onChange={(e) => onTaxSettingChange('gst', e.target.value)}
                placeholder="14"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="loyaltyPointsRate">Loyalty Points Rate (%)</Label>
              <Input
                id="loyaltyPointsRate"
                type="number"
                step="0.01"
                value={taxSettings.loyaltyPointsRate}
                onChange={(e) => onTaxSettingChange('loyaltyPointsRate', e.target.value)}
                placeholder="0.1"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="makingChargesMin">Min Making Charges (EGP)</Label>
              <Input
                id="makingChargesMin"
                type="number"
                value={taxSettings.makingChargesMin}
                onChange={(e) => onTaxSettingChange('makingChargesMin', e.target.value)}
                placeholder="300"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="makingChargesMax">Max Making Charges (EGP)</Label>
              <Input
                id="makingChargesMax"
                type="number"
                value={taxSettings.makingChargesMax}
                onChange={(e) => onTaxSettingChange('makingChargesMax', e.target.value)}
                placeholder="2000"
              />
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}