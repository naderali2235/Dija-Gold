import React from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Input } from '../ui/input';
import { Badge } from '../ui/badge';
import { formatCurrency } from '../utils/currency';

export interface GoldRateData {
  rate: number;
  change?: number;
  changePercent?: number;
}

export interface GoldRatesMap {
  [key: string]: GoldRateData;
}

interface GoldRatesDisplayProps {
  goldRates: GoldRatesMap;
  title?: string;
  description?: string;
  loading?: boolean;
  error?: string | null;
  showInputs?: boolean;
  showChanges?: boolean;
  onRateChange?: (karat: string, value: string) => void;
  manuallyEditedRates?: Set<string>;
  disabled?: boolean;
  className?: string;
}

// Standard gold rates order and keys - will be populated dynamically from API
export const GOLD_RATES_KEYS: string[] = [];

// Transform API gold rates to the format expected by the component
export const transformGoldRates = (apiRates: any[]): Record<string, string> => {
  const transformed: Record<string, string> = {};
  
  // Clear and repopulate the global arrays
  GOLD_RATES_KEYS.length = 0;
  Object.keys(headerToKaratMapping).forEach(key => delete headerToKaratMapping[key]);

  apiRates?.forEach(rate => {
    if (rate.karatType && rate.karatType.name) {
      const karatDisplayName = rate.karatType.name.toUpperCase();
      transformed[karatDisplayName] = rate.ratePerGram.toString();
      
      // Populate the global arrays
      if (!GOLD_RATES_KEYS.includes(karatDisplayName)) {
        GOLD_RATES_KEYS.push(karatDisplayName);
      }
      headerToKaratMapping[karatDisplayName] = rate.karatTypeId;
    }
  });

  return transformed;
};

// Map fixed header names back to karat type IDs for API updates
export const headerToKaratMapping: Record<string, number> = {};

export function GoldRatesDisplay({
  goldRates,
  title = "Gold Rates",
  description = "Current gold rates in Egyptian Pounds per gram",
  loading = false,
  error = null,
  showInputs = false,
  showChanges = true,
  onRateChange,
  manuallyEditedRates = new Set(),
  disabled = false,
  className = ""
}: GoldRatesDisplayProps) {
  // Use provided global keys when available (for Settings page that calls transformGoldRates),
  // otherwise derive keys from the passed goldRates prop (for Dashboard and other read-only views)
  const derivedKeys = React.useMemo(() => {
    return GOLD_RATES_KEYS.length > 0 ? GOLD_RATES_KEYS : Object.keys(goldRates || {});
  }, [goldRates]);

  return (
    <Card className={`pos-card ${className}`}>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <span className="text-[#D4AF37]">💰</span>
          {title}
        </CardTitle>
        <CardDescription>
          {description}
          {loading && <span className="ml-2 text-blue-600">Loading...</span>}
          {error && <span className="ml-2 text-red-600">Error: {error}</span>}
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          {derivedKeys.map((karat) => {
            const rateData = goldRates[karat];
            const isManuallyEdited = manuallyEditedRates.has(karat);
            const rate = rateData?.rate || 0;
            const change = rateData?.change || 0;
            const changePercent = rateData?.changePercent || 0;

            return (
              <div 
                key={karat} 
                className={`p-4 bg-gradient-to-br from-[#D4AF37]/10 to-[#D4AF37]/5 rounded-lg border border-[#D4AF37]/20 ${
                  isManuallyEdited ? 'ring-2 ring-blue-300 bg-blue-50' : ''
                }`}
              >
                <div className="flex items-center justify-between mb-3">
                  <h3 className="font-semibold text-[#0D1B2A]">{karat}</h3>
                  {isManuallyEdited && (
                    <Badge variant="secondary" className="text-xs bg-blue-100 text-blue-800">
                      Edited
                    </Badge>
                  )}
                  {showChanges && changePercent !== 0 && (
                    <Badge 
                      variant={change >= 0 ? "default" : "destructive"} 
                      className="text-xs"
                    >
                      {change >= 0 ? '+' : ''}{changePercent.toFixed(2)}%
                    </Badge>
                  )}
                </div>
                
                <div className="space-y-3">
                  {showInputs ? (
                    <div className="relative">
                      <Input
                        id={`rate-${karat}`}
                        type="number"
                        step="0.01"
                        value={rate.toString()}
                        onChange={(e) => onRateChange?.(karat, e.target.value)}
                        className={`text-lg font-semibold text-center ${
                          isManuallyEdited ? 'border-blue-300 bg-blue-50' : ''
                        }`}
                        disabled={loading || disabled}
                        placeholder="0.00"
                      />
                      <div className="absolute left-3 top-3 text-muted-foreground text-sm font-medium">
                        EGP
                      </div>
                    </div>
                  ) : (
                    <div className="text-center">
                      <p className="text-2xl text-[#0D1B2A] font-bold">
                        {formatCurrency(rate)}
                      </p>
                    </div>
                  )}
                  
                  {showChanges && change !== 0 && (
                    <div className="text-center">
                      <p className={`text-sm ${change >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                        {change >= 0 ? '+' : ''}{formatCurrency(Math.abs(change))}
                      </p>
                    </div>
                  )}
                  
                  {showInputs && (
                    <div className="text-center">
                      <p className="text-sm text-muted-foreground">
                        Current Rate
                      </p>
                      <p className="text-lg font-bold text-[#D4AF37]">
                        {formatCurrency(rate)}
                      </p>
                    </div>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      </CardContent>
    </Card>
  );
}
