/**
 * Currency utility functions for EGP (Egyptian Pound) formatting
 */

/**
 * Formats a number as Egyptian Pound currency with English numerals only
 * @param amount - The amount to format
 * @param locale - The locale to use (defaults to 'en-US' for English numerals)
 * @returns Formatted currency string with English numerals
 */
export const formatCurrency = (
  amount: number | string | null | undefined,
  locale: string = 'en-US'
): string => {
  if (amount === null || amount === undefined || amount === '') {
    return 'EGP 0.00';
  }

  const numericAmount = typeof amount === 'string' ? parseFloat(amount) : amount;
  
  if (isNaN(numericAmount)) {
    return 'EGP 0.00';
  }

  try {
    // Always use en-US locale to ensure English numerals
    const formatted = new Intl.NumberFormat('en-US', {
      style: 'decimal',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(numericAmount);
    
    return `EGP ${formatted}`;
  } catch (error) {
    // Fallback formatting with English numerals
    return `EGP ${numericAmount.toFixed(2)}`;
  }
};

/**
 * Formats a number with English numerals only
 * @param number - The number to format
 * @param decimals - Number of decimal places (default: 0)
 * @returns Formatted number string with English numerals
 */
export const formatNumber = (
  number: number | string | null | undefined,
  decimals: number = 0
): string => {
  if (number === null || number === undefined || number === '') {
    return '0';
  }

  const numericValue = typeof number === 'string' ? parseFloat(number) : number;
  
  if (isNaN(numericValue)) {
    return '0';
  }

  return new Intl.NumberFormat('en-US', {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  }).format(numericValue);
};

/**
 * Formats a percentage with English numerals only
 * @param number - The number to format as percentage
 * @param decimals - Number of decimal places (default: 1)
 * @returns Formatted percentage string with English numerals
 */
export const formatPercentage = (
  number: number | string | null | undefined,
  decimals: number = 1
): string => {
  if (number === null || number === undefined || number === '') {
    return '0%';
  }

  const numericValue = typeof number === 'string' ? parseFloat(number) : number;
  
  if (isNaN(numericValue)) {
    return '0%';
  }

  return `${formatNumber(numericValue, decimals)}%`;
};

/**
 * Converts any Arabic numerals to English numerals in a string
 * @param input - The string containing potential Arabic numerals
 * @returns String with all numerals converted to English
 */
export const convertToEnglishNumerals = (input: string): string => {
  const arabicNumerals = ['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩'];
  const englishNumerals = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
  
  let result = input;
  arabicNumerals.forEach((arabic, index) => {
    result = result.replace(new RegExp(arabic, 'g'), englishNumerals[index]);
  });
  
  return result;
};

/**
 * Validates that a string contains only English numerals and allowed characters
 * @param input - The input string to validate
 * @param allowDecimal - Whether to allow decimal points (default: true)
 * @param allowNegative - Whether to allow negative numbers (default: false)
 * @returns True if input contains only English numerals and allowed characters
 */
export const isEnglishNumeralsOnly = (
  input: string,
  allowDecimal: boolean = true,
  allowNegative: boolean = false
): boolean => {
  let pattern = '[0-9]';
  if (allowDecimal) pattern += '\\.';
  if (allowNegative) pattern += '\\-';
  
  const regex = new RegExp(`^[${pattern}]*$`);
  return regex.test(input);
};

/**
 * Formats a date string or Date object with English numerals
 * @param date - The date to format (can be Date object, ISO string, or date string)
 * @param locale - The locale to use (defaults to 'en-US' for English numerals)
 * @param options - Intl.DateTimeFormatOptions for custom formatting
 * @returns Formatted date string with English numerals
 */
export const formatDate = (
  date: Date | string | null | undefined,
  locale: string = 'en-US',
  options: Intl.DateTimeFormatOptions = {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }
): string => {
  if (!date) {
    return '';
  }

  try {
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    
    if (isNaN(dateObj.getTime())) {
      return '';
    }

    // Always use en-US locale to ensure English numerals
    const formatted = new Intl.DateTimeFormat('en-US', options).format(dateObj);
    return convertToEnglishNumerals(formatted);
  } catch (error) {
    // Fallback formatting with English numerals
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    return convertToEnglishNumerals(dateObj.toLocaleDateString('en-US'));
  }
};

/**
 * Formats a date for display in short format (DD/MM/YYYY) with English numerals
 * @param date - The date to format
 * @returns Formatted date string with English numerals
 */
export const formatShortDate = (date: Date | string | null | undefined): string => {
  return formatDate(date, 'en-US', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  });
};

/**
 * Formats a date for display with time (DD/MM/YYYY HH:MM)
 * @param date - The date to format
 * @returns Formatted date string with time
 */
export const formatDateTime = (date: Date | string | null | undefined): string => {
  return formatDate(date, 'ar-EG', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  });
};

/**
 * Parses a currency string back to a number
 * @param currencyString - The formatted currency string
 * @returns The numeric value
 */
export const parseCurrency = (currencyString: string): number => {
  if (!currencyString) {
    return 0;
  }

  // Remove currency symbol and any non-numeric characters except decimal point
  const numericString = currencyString.replace(/[^\d.-]/g, '');
  const parsed = parseFloat(numericString);
  
  return isNaN(parsed) ? 0 : parsed;
};

/**
 * Validates if a string is a valid currency amount
 * @param amount - The amount to validate
 * @returns True if valid, false otherwise
 */
export const isValidCurrency = (amount: string): boolean => {
  if (!amount) {
    return false;
  }

  const numericAmount = parseFloat(amount);
  return !isNaN(numericAmount) && numericAmount >= 0;
};
