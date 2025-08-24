import React from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Input } from '../ui/input';
import { Label } from '../ui/label';
import { Button } from '../ui/button';
import { Badge } from '../ui/badge';
import { DollarSign, TrendingUp, Plus, Edit, Trash2, Loader2 } from 'lucide-react';
import { formatCurrency } from '../utils/currency';
import { useGoldRates, useMakingCharges, useTaxConfigurations, useProductCategoryTypes, useChargeTypes } from '../../hooks/useApi';
import { GoldRate, MakingCharges, TaxConfigurationDto } from '../../services/api';
import { LookupHelper } from '../../types/lookups';
import { GoldRatesDisplay, transformGoldRates, GoldRatesMap } from '../shared/GoldRatesDisplay';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '../ui/dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '../ui/select';
import { Textarea } from '../ui/textarea';
import { Switch } from '../ui/switch';

interface PricingSettingsProps {
  goldRates: Record<string, string>;
  onGoldRateChange: (karat: string, value: string) => void;
  onTaxSettingChange: (setting: string, value: string) => void;
  taxSettings: Record<string, string>;
  onResetManuallyEdited?: () => number;
}



export function PricingSettings({
  goldRates,
  onGoldRateChange,
  onTaxSettingChange,
  taxSettings,
  onResetManuallyEdited,
}: PricingSettingsProps) {
  const { data: goldRatesData, loading, error, fetchRates } = useGoldRates();
  const { data: makingChargesData, loading: makingChargesLoading, error: makingChargesError, updateCharges, fetchCharges } = useMakingCharges();
  const { data: taxConfigurationsData, loading: taxConfigurationsLoading, error: taxConfigurationsError, updateTaxConfiguration, fetchTaxConfigurations } = useTaxConfigurations();
  const { data: productCategoryTypesData, loading: categoriesLoading, error: categoriesError, fetchCategories } = useProductCategoryTypes();
  const { data: chargeTypesData, loading: chargeTypesLoading, error: chargeTypesError, fetchChargeTypes } = useChargeTypes();


  // Track which rates have been manually edited by the user
  const [manuallyEditedRates, setManuallyEditedRates] = React.useState<Set<string>>(new Set());

  // Fetch all required data when component mounts
  React.useEffect(() => {
    fetchRates();
    fetchCharges();
    fetchCategories();
    fetchChargeTypes();
    fetchTaxConfigurations();
  }, []); // Empty dependency array - only run on mount

  // Making charges form state
  const [isMakingChargesDialogOpen, setIsMakingChargesDialogOpen] = React.useState(false);
  const [editingCharge, setEditingCharge] = React.useState<MakingCharges | null>(null);
  // Helper function to format date as YYYY-MM-DD in local timezone
  const formatDateLocal = (date: Date) => {
    return date.getFullYear() + '-' + 
      String(date.getMonth() + 1).padStart(2, '0') + '-' + 
      String(date.getDate()).padStart(2, '0');
  };

  const [makingChargesForm, setMakingChargesForm] = React.useState({
    name: '',
    productCategory: 1, // GoldJewelry default
    subCategory: '',
    chargeType: 1, // Percentage default
    chargeValue: '',
    effectiveFrom: formatDateLocal(new Date()),
  });
  const [isSubmitting, setIsSubmitting] = React.useState(false);
  
  // Tax configuration form state
  const [isTaxConfigDialogOpen, setIsTaxConfigDialogOpen] = React.useState(false);
  const [editingTaxConfig, setEditingTaxConfig] = React.useState<TaxConfigurationDto | null>(null);
  const [taxConfigForm, setTaxConfigForm] = React.useState({
    taxName: '',
    taxCode: '',
    taxType: 1, // Percentage default
    taxRate: '',
    isMandatory: true,
    effectiveFrom: formatDateLocal(new Date()),
    displayOrder: 1,
  });
  const [isTaxSubmitting, setIsTaxSubmitting] = React.useState(false);
  
  // Sync API data with local state when API data changes
  React.useEffect(() => {
    if (goldRatesData && goldRatesData.length > 0) {
      const transformedRates = transformGoldRates(goldRatesData);
      // Only update rates that haven't been manually edited by the user
      Object.entries(transformedRates).forEach(([karat, rate]) => {
        if (!manuallyEditedRates.has(karat)) {
          onGoldRateChange(karat, rate);
        }
      });
    }
  }, [goldRatesData, manuallyEditedRates]); // Removed karatTypesData dependency since we use fixed headers

  // Track the reset trigger value to avoid dependency on function reference
  const resetTriggerValue = onResetManuallyEdited ? onResetManuallyEdited() : 0;
  
  // Refresh rates from database when requested (after save)
  React.useEffect(() => {
    if (resetTriggerValue > 0) {
      // Clear manually edited state to allow fresh data from API
      setManuallyEditedRates(new Set());
      // Fetch fresh data from API
      fetchRates();
    }
  }, [resetTriggerValue, fetchRates]);

  // Handle gold rate changes
  const handleGoldRateChange = (karat: string, value: string) => {
    // Mark this rate as manually edited by the user
    setManuallyEditedRates(prev => new Set(prev).add(karat));
    onGoldRateChange(karat, value);
  };

  // Making charges form handlers
  const resetMakingChargesForm = () => {
    setMakingChargesForm({
      name: '',
      productCategory: 1,
      subCategory: '',
      chargeType: 1,
      chargeValue: '',
      effectiveFrom: formatDateLocal(new Date()),
    });
    setEditingCharge(null);
  };

  const openMakingChargesDialog = (charge?: MakingCharges) => {
    if (charge) {
      setEditingCharge(charge);
      setMakingChargesForm({
        name: charge.name,
        productCategory: charge.productCategory,
        subCategory: charge.subCategory || '',
        chargeType: charge.chargeType,
        chargeValue: charge.chargeValue.toString(),
        effectiveFrom: formatDateLocal(new Date()),
      });
    } else {
      resetMakingChargesForm();
    }
    setIsMakingChargesDialogOpen(true);
  };

  const handleMakingChargesSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!makingChargesForm.name || !makingChargesForm.chargeValue) {
      alert('Please fill in all required fields');
      return;
    }

    try {
      setIsSubmitting(true);
      
      await updateCharges({
        id: editingCharge?.id,
        name: makingChargesForm.name,
        productCategory: makingChargesForm.productCategory,
        subCategory: makingChargesForm.subCategory || undefined,
        chargeType: makingChargesForm.chargeType,
        chargeValue: parseFloat(makingChargesForm.chargeValue),
        effectiveFrom: makingChargesForm.effectiveFrom,
      });

      // Refresh charges data after update
      await fetchCharges();

      setIsMakingChargesDialogOpen(false);
      resetMakingChargesForm();
    } catch (error) {
      console.error('Error updating making charges:', error);
      alert('Failed to update making charges. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  // Tax configuration form handlers
  const resetTaxConfigForm = () => {
    setTaxConfigForm({
      taxName: '',
      taxCode: '',
      taxType: 1,
      taxRate: '',
      isMandatory: true,
      effectiveFrom: formatDateLocal(new Date()),
      displayOrder: 1,
    });
    setEditingTaxConfig(null);
  };

  const openTaxConfigDialog = (taxConfig?: TaxConfigurationDto) => {
    if (taxConfig) {
      setEditingTaxConfig(taxConfig);
      setTaxConfigForm({
        taxName: taxConfig.taxName,
        taxCode: taxConfig.taxCode,
        taxType: taxConfig.taxType,
        taxRate: taxConfig.taxRate.toString(),
        isMandatory: taxConfig.isMandatory,
        effectiveFrom: formatDateLocal(new Date()),
        displayOrder: taxConfig.displayOrder,
      });
    } else {
      resetTaxConfigForm();
    }
    setIsTaxConfigDialogOpen(true);
  };

  const handleTaxConfigSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!taxConfigForm.taxName || !taxConfigForm.taxCode || !taxConfigForm.taxRate) {
      alert('Please fill in all required fields');
      return;
    }

    try {
      setIsTaxSubmitting(true);
      
      await updateTaxConfiguration({
        id: editingTaxConfig?.id,
        taxName: taxConfigForm.taxName,
        taxCode: taxConfigForm.taxCode,
        taxType: taxConfigForm.taxType,
        taxRate: parseFloat(taxConfigForm.taxRate),
        isMandatory: taxConfigForm.isMandatory,
        effectiveFrom: taxConfigForm.effectiveFrom,
        displayOrder: taxConfigForm.displayOrder,
      });

      // Refresh tax configurations data after update
      await fetchTaxConfigurations();

      setIsTaxConfigDialogOpen(false);
      resetTaxConfigForm();
    } catch (error) {
      console.error('Error updating tax configuration:', error);
      alert('Failed to update tax configuration. Please try again.');
    } finally {
      setIsTaxSubmitting(false);
    }
  };

  // Convert goldRates to the format expected by GoldRatesDisplay
  const goldRatesForDisplay: GoldRatesMap = React.useMemo(() => {
    const rates: GoldRatesMap = {};
    Object.entries(goldRates).forEach(([karat, rate]) => {
      rates[karat] = {
        rate: parseFloat(rate) || 0
      };
    });
    return rates;
  }, [goldRates]);

  return (
    <div className="space-y-6">
      {/* Gold Rates */}
      <GoldRatesDisplay
        goldRates={goldRatesForDisplay}
        title="Gold Rates (per gram)"
        description="Current gold rates in Egyptian Pounds per gram"
        loading={loading}
        error={error}
        showInputs={true}
        showChanges={false}
        onRateChange={handleGoldRateChange}
        manuallyEditedRates={manuallyEditedRates}
        disabled={false}
      />

      {/* Making Charges */}
      <Card className="pos-card">
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="flex items-center gap-2">
                <DollarSign className="h-5 w-5 text-[#D4AF37]" />
                Making Charges
              </CardTitle>
              <CardDescription>
                Manage making charges configuration
                {makingChargesLoading && <span className="ml-2 text-blue-600">Loading...</span>}
                {makingChargesError && <span className="ml-2 text-red-600">Error: {makingChargesError}</span>}
              </CardDescription>
            </div>
            <Dialog open={isMakingChargesDialogOpen} onOpenChange={setIsMakingChargesDialogOpen}>
              <DialogTrigger asChild>
                <Button 
                  size="sm" 
                  variant="golden"
                  onClick={() => openMakingChargesDialog()}
                  className="touch-target"
                >
                  <Plus className="mr-2 h-4 w-4" />
                  Add Charge
                </Button>
              </DialogTrigger>
              <DialogContent className="max-w-md">
                <DialogHeader>
                  <DialogTitle>
                    {editingCharge ? 'Edit Making Charge' : 'Add New Making Charge'}
                  </DialogTitle>
                  <DialogDescription>
                    Configure making charges for different product categories
                  </DialogDescription>
                </DialogHeader>
                <form onSubmit={handleMakingChargesSubmit} className="space-y-4">
                  <div className="space-y-2">
                    <Label htmlFor="chargeName">Charge Name *</Label>
                    <Input
                      id="chargeName"
                      value={makingChargesForm.name}
                      onChange={(e) => setMakingChargesForm({...makingChargesForm, name: e.target.value})}
                      placeholder="e.g., Wedding Ring Making Charge"
                      required
                    />
                  </div>
                  
                  <div className="space-y-2">
                    <Label htmlFor="productCategory">Product Category *</Label>
                    <Select 
                      value={makingChargesForm.productCategory.toString()} 
                      onValueChange={(value) => setMakingChargesForm({...makingChargesForm, productCategory: parseInt(value)})}
                      disabled={categoriesLoading}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder={categoriesLoading ? "Loading..." : "Select category"} />
                      </SelectTrigger>
                      <SelectContent>
                        {categoriesLoading ? (
                          <div className="flex items-center justify-center p-2">
                            <Loader2 className="h-4 w-4 animate-spin mr-2" />
                            Loading categories...
                          </div>
                        ) : categoriesError ? (
                          <div className="text-red-500 p-2">Error loading categories</div>
                        ) : (
                          productCategoryTypesData?.map((category) => (
                            <SelectItem key={category.id} value={category.id.toString()}>
                              {category.name}
                            </SelectItem>
                          ))
                        )}
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="subCategory">Subcategory (Optional)</Label>
                    <Input
                      id="subCategory"
                      value={makingChargesForm.subCategory}
                      onChange={(e) => setMakingChargesForm({...makingChargesForm, subCategory: e.target.value})}
                      placeholder="e.g., rings, necklaces"
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="chargeType">Charge Type *</Label>
                    <Select 
                      value={makingChargesForm.chargeType.toString()} 
                      onValueChange={(value) => setMakingChargesForm({...makingChargesForm, chargeType: parseInt(value)})}
                      disabled={chargeTypesLoading}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder={chargeTypesLoading ? "Loading..." : "Select charge type"} />
                      </SelectTrigger>
                      <SelectContent>
                        {chargeTypesLoading ? (
                          <div className="flex items-center justify-center p-2">
                            <Loader2 className="h-4 w-4 animate-spin mr-2" />
                            Loading charge types...
                          </div>
                        ) : chargeTypesError ? (
                          <div className="text-red-500 p-2">Error loading charge types</div>
                        ) : (
                          chargeTypesData?.map((type) => (
                            <SelectItem key={type.id} value={type.id.toString()}>
                              {type.name}
                            </SelectItem>
                          ))
                        )}
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="chargeValue">
                      Charge Value * 
                      {makingChargesForm.chargeType === 1 ? ' (%)' : ' (EGP)'}
                    </Label>
                    <Input
                      id="chargeValue"
                      type="number"
                      step="0.01"
                      value={makingChargesForm.chargeValue}
                      onChange={(e) => setMakingChargesForm({...makingChargesForm, chargeValue: e.target.value})}
                      placeholder={makingChargesForm.chargeType === 1 ? "15.5" : "500"}
                      required
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="effectiveFrom">Effective From *</Label>
                    <Input
                      id="effectiveFrom"
                      type="date"
                      value={makingChargesForm.effectiveFrom}
                      onChange={(e) => setMakingChargesForm({...makingChargesForm, effectiveFrom: e.target.value})}
                      required
                    />
                  </div>

                  <div className="flex justify-end gap-2 pt-4">
                    <Button 
                      type="button" 
                      variant="outline" 
                      onClick={() => setIsMakingChargesDialogOpen(false)}
                      disabled={isSubmitting}
                    >
                      Cancel
                    </Button>
                    <Button 
                      type="submit" 
                      variant="golden"
                      disabled={isSubmitting}
                    >
                      {isSubmitting ? (
                        <>
                          <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                          Saving...
                        </>
                      ) : (
                        editingCharge ? 'Update Charge' : 'Add Charge'
                      )}
                    </Button>
                  </div>
                </form>
              </DialogContent>
            </Dialog>
          </div>
        </CardHeader>
        <CardContent>
          {makingChargesData && makingChargesData.length > 0 ? (
            <div className="space-y-4">
              {makingChargesData.map((charge) => (
                <div key={charge.id} className="flex items-center justify-between p-3 border rounded-lg">
                  <div>
                    <h4 className="font-medium">{charge.name}</h4>
                    <p className="text-sm text-muted-foreground">
                      Category: {productCategoryTypesData?.find(c => c.id === charge.productCategory)?.name || charge.productCategory}
                      {charge.subCategory && ` - ${charge.subCategory}`}
                    </p>
                    <p className="text-xs text-muted-foreground">
                      Effective: {new Date(charge.effectiveFrom).toLocaleDateString()}
                    </p>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="text-right">
                      <p className="font-medium">
                        {charge.chargeType === 1 ? `${charge.chargeValue}%` : `${formatCurrency(charge.chargeValue)}`}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {chargeTypesData?.find(t => t.id === charge.chargeType)?.name || 'Unknown Type'}
                      </p>
                    </div>
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => openMakingChargesDialog(charge)}
                      className="hover:bg-[#F4E9B1]"
                    >
                      <Edit className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-8">
              <p className="text-muted-foreground mb-4">No making charges configured</p>
              <Button 
                size="sm" 
                variant="outline"
                onClick={() => openMakingChargesDialog()}
              >
                <Plus className="mr-2 h-4 w-4" />
                Add First Charge
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Tax Configurations */}
      <Card className="pos-card">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <DollarSign className="h-5 w-5 text-[#D4AF37]" />
            Tax Configurations
          </CardTitle>
          <CardDescription>
            Manage tax rates and configurations (no waiving option available)
            {taxConfigurationsLoading && <span className="ml-2 text-blue-600">Loading...</span>}
            {taxConfigurationsError && <span className="ml-2 text-red-600">Error: {taxConfigurationsError}</span>}
          </CardDescription>
          <div className="flex justify-end">
            <Button 
              size="sm" 
              variant="golden"
              onClick={() => openTaxConfigDialog()}
            >
              <Plus className="mr-2 h-4 w-4" />
              Add Tax Configuration
            </Button>
          </div>

          {/* Tax Configuration Dialog */}
          <Dialog open={isTaxConfigDialogOpen} onOpenChange={setIsTaxConfigDialogOpen}>
            <DialogContent className="max-w-md">
              <DialogHeader>
                <DialogTitle>
                  {editingTaxConfig ? 'Edit Tax Configuration' : 'Add Tax Configuration'}
                </DialogTitle>
                <DialogDescription>
                  Configure tax settings with versioning support
                </DialogDescription>
              </DialogHeader>
              <form onSubmit={handleTaxConfigSubmit} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="taxName">Tax Name *</Label>
                  <Input
                    id="taxName"
                    value={taxConfigForm.taxName}
                    onChange={(e) => setTaxConfigForm({...taxConfigForm, taxName: e.target.value})}
                    placeholder="e.g., VAT, Sales Tax"
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="taxCode">Tax Code *</Label>
                  <Input
                    id="taxCode"
                    value={taxConfigForm.taxCode}
                    onChange={(e) => setTaxConfigForm({...taxConfigForm, taxCode: e.target.value})}
                    placeholder="e.g., VAT001, ST001"
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="taxType">Tax Type *</Label>
                  <Select 
                    value={taxConfigForm.taxType.toString()} 
                    onValueChange={(value) => setTaxConfigForm({...taxConfigForm, taxType: parseInt(value)})}
                    disabled={chargeTypesLoading}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder={chargeTypesLoading ? "Loading..." : "Select tax type"} />
                    </SelectTrigger>
                    <SelectContent>
                      {chargeTypesLoading ? (
                        <div className="flex items-center justify-center p-2">
                          <Loader2 className="h-4 w-4 animate-spin mr-2" />
                          Loading tax types...
                        </div>
                      ) : chargeTypesError ? (
                        <div className="text-red-500 p-2">Error loading tax types</div>
                      ) : (
                        chargeTypesData?.map((type) => (
                          <SelectItem key={type.id} value={type.id.toString()}>
                            {type.name}
                          </SelectItem>
                        ))
                      )}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="taxRate">
                    Tax Rate * 
                    {taxConfigForm.taxType === 1 ? ' (%)' : ' (EGP)'}
                  </Label>
                  <Input
                    id="taxRate"
                    type="number"
                    step="0.01"
                    value={taxConfigForm.taxRate}
                    onChange={(e) => setTaxConfigForm({...taxConfigForm, taxRate: e.target.value})}
                    placeholder={taxConfigForm.taxType === 1 ? "14.0" : "50.0"}
                    required
                  />
                </div>

                <div className="flex items-center space-x-2">
                  <Switch
                    id="isMandatory"
                    checked={taxConfigForm.isMandatory}
                    onCheckedChange={(checked) => setTaxConfigForm({...taxConfigForm, isMandatory: checked})}
                  />
                  <Label htmlFor="isMandatory">Mandatory Tax</Label>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="displayOrder">Display Order</Label>
                  <Input
                    id="displayOrder"
                    type="number"
                    min="1"
                    value={taxConfigForm.displayOrder}
                    onChange={(e) => setTaxConfigForm({...taxConfigForm, displayOrder: parseInt(e.target.value) || 1})}
                    placeholder="1"
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="taxEffectiveFrom">Effective From *</Label>
                  <Input
                    id="taxEffectiveFrom"
                    type="date"
                    value={taxConfigForm.effectiveFrom}
                    onChange={(e) => setTaxConfigForm({...taxConfigForm, effectiveFrom: e.target.value})}
                    required
                  />
                </div>

                <div className="flex justify-end gap-2 pt-4">
                  <Button 
                    type="button" 
                    variant="outline" 
                    onClick={() => setIsTaxConfigDialogOpen(false)}
                    disabled={isTaxSubmitting}
                  >
                    Cancel
                  </Button>
                  <Button 
                    type="submit" 
                    variant="golden"
                    disabled={isTaxSubmitting}
                  >
                    {isTaxSubmitting ? (
                      <>
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                        Saving...
                      </>
                    ) : (
                      editingTaxConfig ? 'Update Tax' : 'Add Tax'
                    )}
                  </Button>
                </div>
              </form>
            </DialogContent>
          </Dialog>
        </CardHeader>
        <CardContent>
          {taxConfigurationsData && taxConfigurationsData.length > 0 ? (
            <div className="space-y-4">
              {taxConfigurationsData.map((taxConfig) => (
                <div key={taxConfig.id} className="flex items-center justify-between p-3 border rounded-lg">
                  <div>
                    <h4 className="font-medium">{taxConfig.taxName}</h4>
                    <p className="text-sm text-muted-foreground">
                      Code: {taxConfig.taxCode} | Order: {taxConfig.displayOrder}
                    </p>
                    <p className="text-xs text-muted-foreground">
                      Effective: {new Date(taxConfig.effectiveFrom).toLocaleDateString()}
                      {taxConfig.isMandatory && <span className="ml-2 text-green-600">• Mandatory</span>}
                    </p>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="text-right">
                      <p className="font-medium">
                        {taxConfig.taxType === 1 ? `${taxConfig.taxRate}%` : `${formatCurrency(taxConfig.taxRate)}`}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {chargeTypesData?.find(t => t.id === taxConfig.taxType)?.name || 'Unknown Type'}
                      </p>
                    </div>
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => openTaxConfigDialog(taxConfig)}
                      className="hover:bg-[#F4E9B1]"
                    >
                      <Edit className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-8">
              <p className="text-muted-foreground mb-4">No tax configurations set up</p>
              <Button 
                size="sm" 
                variant="outline"
                onClick={() => openTaxConfigDialog()}
              >
                <Plus className="mr-2 h-4 w-4" />
                Add First Tax Configuration
              </Button>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}