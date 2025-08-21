/**
 * Number input component that enforces English numerals only
 * Prevents Arabic numerals and provides validation
 */

import React, { forwardRef, useCallback } from 'react';
import { Input } from './input';
import { cn } from './utils';
import { convertToEnglishNumerals, isEnglishNumeralsOnly } from '../../utils/currency';

export interface NumberInputProps
  extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'type' | 'onChange'> {
  value?: string | number;
  onChange?: (value: string) => void;
  allowDecimal?: boolean;
  allowNegative?: boolean;
  maxDecimals?: number;
  onValueChange?: (numericValue: number | null) => void;
}

const NumberInput = forwardRef<HTMLInputElement, NumberInputProps>(
  (
    {
      className,
      value = '',
      onChange,
      onValueChange,
      allowDecimal = true,
      allowNegative = false,
      maxDecimals = 2,
      onKeyDown,
      onPaste,
      ...props
    },
    ref
  ) => {
    const stringValue = typeof value === 'number' ? value.toString() : value;

    const validateAndFormatInput = useCallback(
      (input: string): string => {
        // Convert Arabic numerals to English
        let processed = convertToEnglishNumerals(input);

        // Remove any characters that aren't allowed
        let allowedChars = '0-9';
        if (allowDecimal) allowedChars += '\\.';
        if (allowNegative) allowedChars += '\\-';

        const regex = new RegExp(`[^${allowedChars}]`, 'g');
        processed = processed.replace(regex, '');

        // Handle decimal places limit
        if (allowDecimal && maxDecimals >= 0) {
          const decimalIndex = processed.indexOf('.');
          if (decimalIndex !== -1) {
            const beforeDecimal = processed.substring(0, decimalIndex + 1);
            const afterDecimal = processed.substring(decimalIndex + 1, decimalIndex + 1 + maxDecimals);
            processed = beforeDecimal + afterDecimal;
          }
        }

        // Handle negative sign (only at the beginning)
        if (allowNegative) {
          const negativeCount = (processed.match(/-/g) || []).length;
          if (negativeCount > 1) {
            processed = processed.replace(/-/g, '');
            if (processed.length > 0) {
              processed = '-' + processed;
            }
          } else if (processed.includes('-') && !processed.startsWith('-')) {
            processed = processed.replace(/-/g, '');
          }
        }

        // Handle multiple decimal points
        if (allowDecimal) {
          const decimalCount = (processed.match(/\./g) || []).length;
          if (decimalCount > 1) {
            const firstDecimalIndex = processed.indexOf('.');
            processed = processed.substring(0, firstDecimalIndex + 1) + 
                       processed.substring(firstDecimalIndex + 1).replace(/\./g, '');
          }
        }

        return processed;
      },
      [allowDecimal, allowNegative, maxDecimals]
    );

    const handleInputChange = useCallback(
      (e: React.ChangeEvent<HTMLInputElement>) => {
        const rawValue = e.target.value;
        const processedValue = validateAndFormatInput(rawValue);

        if (onChange) {
          onChange(processedValue);
        }

        if (onValueChange) {
          const numericValue = processedValue === '' ? null : parseFloat(processedValue);
          onValueChange(isNaN(numericValue!) ? null : numericValue);
        }
      },
      [onChange, onValueChange, validateAndFormatInput]
    );

    const handleKeyDown = useCallback(
      (e: React.KeyboardEvent<HTMLInputElement>) => {
        // Block Arabic numerals at keyboard level
        const arabicNumerals = ['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩'];
        if (arabicNumerals.includes(e.key)) {
          e.preventDefault();
          return;
        }

        // Allow: backspace, delete, tab, escape, enter
        if ([8, 9, 27, 13, 46].includes(e.keyCode)) {
          return;
        }

        // Allow: Ctrl+A, Ctrl+C, Ctrl+V, Ctrl+X, Ctrl+Z
        if (e.ctrlKey && [65, 67, 86, 88, 90].includes(e.keyCode)) {
          return;
        }

        // Allow: home, end, left, right, down, up
        if ([35, 36, 37, 39, 40, 38].includes(e.keyCode)) {
          return;
        }

        // Ensure that it is a number and stop the keypress
        if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
          // Allow decimal point
          if (allowDecimal && (e.keyCode === 190 || e.keyCode === 110)) {
            // Only allow one decimal point
            const currentValue = (e.target as HTMLInputElement).value;
            if (currentValue.includes('.')) {
              e.preventDefault();
            }
            return;
          }

          // Allow minus sign
          if (allowNegative && (e.keyCode === 189 || e.keyCode === 109)) {
            // Only allow minus at the beginning
            const currentValue = (e.target as HTMLInputElement).value;
            const selectionStart = (e.target as HTMLInputElement).selectionStart || 0;
            if (selectionStart !== 0 || currentValue.includes('-')) {
              e.preventDefault();
            }
            return;
          }

          e.preventDefault();
        }

        if (onKeyDown) {
          onKeyDown(e);
        }
      },
      [allowDecimal, allowNegative, onKeyDown]
    );

    const handlePaste = useCallback(
      (e: React.ClipboardEvent<HTMLInputElement>) => {
        e.preventDefault();
        const pastedText = e.clipboardData.getData('text');
        const processedValue = validateAndFormatInput(pastedText);

        if (onChange) {
          onChange(processedValue);
        }

        if (onValueChange) {
          const numericValue = processedValue === '' ? null : parseFloat(processedValue);
          onValueChange(isNaN(numericValue!) ? null : numericValue);
        }

        if (onPaste) {
          onPaste(e);
        }
      },
      [onChange, onValueChange, validateAndFormatInput, onPaste]
    );

    return (
      <Input
        {...props}
        ref={ref}
        type="text"
        inputMode="decimal"
        value={stringValue}
        onChange={handleInputChange}
        onKeyDown={handleKeyDown}
        onPaste={handlePaste}
        className={cn(
          "font-mono", // Use monospace font for better number readability
          className
        )}
        autoComplete="off"
        spellCheck={false}
      />
    );
  }
);

NumberInput.displayName = 'NumberInput';

export { NumberInput };
