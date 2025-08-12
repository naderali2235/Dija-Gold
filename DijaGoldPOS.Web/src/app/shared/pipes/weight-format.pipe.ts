import { Pipe, PipeTransform } from '@angular/core';
import { environment } from '@environments/environment';

@Pipe({
  name: 'weightFormat'
})
export class WeightFormatPipe implements PipeTransform {
  transform(
    value: number | null | undefined,
    showUnit: boolean = true,
    decimalPlaces?: number
  ): string {
    if (value === null || value === undefined || isNaN(value)) {
      return showUnit ? `0.000 ${environment.settings.weight.unit}` : '0.000';
    }

    const decimals = decimalPlaces ?? environment.settings.weight.decimalPlaces;
    const formattedWeight = value.toFixed(decimals);

    return showUnit 
      ? `${formattedWeight} ${environment.settings.weight.unit}`
      : formattedWeight;
  }
}