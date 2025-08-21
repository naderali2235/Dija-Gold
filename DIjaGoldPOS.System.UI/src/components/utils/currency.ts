/**
 * Currency utility for Egyptian Pound (EGP) formatting with Western numerals
 */

export const formatCurrency = (amount: number, showCurrency: boolean = true): string => {
  const formatted = new Intl.NumberFormat('en-US', {
    style: 'decimal',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount);
  
  return showCurrency ? `EGP ${formatted}` : formatted;
};

export const formatCurrencyShort = (amount: number): string => {
  if (amount >= 1000000) {
    return `EGP ${(amount / 1000000).toFixed(1)}M`;
  } else if (amount >= 1000) {
    return `EGP ${(amount / 1000).toFixed(1)}K`;
  }
  return formatCurrency(amount);
};

export const parseCurrency = (value: string): number => {
  return parseFloat(value.replace(/[^0-9.-]+/g, '')) || 0;
};

// Date formatting utility
export const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString();
};

// Egyptian Pound symbol
export const EGP_SYMBOL = 'EGP';

// Gold rate conversions - now fetched from API
// Legacy rates removed - use API data instead

// Format numbers with Western numerals
export const formatNumber = (number: number): string => {
  return new Intl.NumberFormat('en-US').format(number);
};

// Format percentage with Western numerals
export const formatPercentage = (number: number, decimals: number = 1): string => {
  return new Intl.NumberFormat('en-US', {
    style: 'percent',
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  }).format(number / 100);
};