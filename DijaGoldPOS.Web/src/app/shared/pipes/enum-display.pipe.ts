import { Pipe, PipeTransform } from '@angular/core';
import { EnumHelper } from '@core/models/enums';

@Pipe({
  name: 'enumDisplay'
})
export class EnumDisplayPipe implements PipeTransform {
  transform(value: any, enumType: string): string {
    if (value === null || value === undefined) {
      return '';
    }

    switch (enumType.toLowerCase()) {
      case 'karattype':
        return EnumHelper.getKaratTypeDisplay(value);
      case 'productcategory':
        return EnumHelper.getProductCategoryDisplay(value);
      case 'transactiontype':
        return EnumHelper.getTransactionTypeDisplay(value);
      case 'transactionstatus':
        return EnumHelper.getTransactionStatusDisplay(value);
      case 'chargetype':
        return EnumHelper.getChargeTypeDisplay(value);
      default:
        return value.toString();
    }
  }
}