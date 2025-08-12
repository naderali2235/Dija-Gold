import { Pipe, PipeTransform } from '@angular/core';
import { environment } from '@environments/environment';

@Pipe({
  name: 'currencyFormat'
})
export class CurrencyFormatPipe implements PipeTransform {
  transform(
    value: number | null | undefined,
    showSymbol: boolean = true,
    decimalPlaces?: number
  ): string {
    if (value === null || value === undefined || isNaN(value)) {
      return showSymbol ? `0.00 ${environment.settings.currency.symbol}` : '0.00';
    }

    const decimals = decimalPlaces ?? environment.settings.currency.decimalPlaces;
    const formattedAmount = value.toFixed(decimals);
    
    // Add thousand separators
    const parts = formattedAmount.split('.');
    parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    const finalAmount = parts.join('.');

    return showSymbol 
      ? `${finalAmount} ${environment.settings.currency.symbol}`
      : finalAmount;
  }
}