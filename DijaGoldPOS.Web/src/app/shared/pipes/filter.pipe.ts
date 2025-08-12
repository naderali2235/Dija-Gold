import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'filter'
})
export class FilterPipe implements PipeTransform {
  transform<T>(items: T[] | null | undefined, searchTerm: string, searchFields?: string[]): T[] {
    if (!items || !searchTerm) {
      return items || [];
    }

    const term = searchTerm.toLowerCase().trim();
    
    return items.filter(item => {
      if (searchFields && searchFields.length > 0) {
        // Search in specific fields
        return searchFields.some(field => {
          const value = this.getNestedProperty(item, field);
          return value && value.toString().toLowerCase().includes(term);
        });
      } else {
        // Search in all string properties
        return this.searchInAllProperties(item, term);
      }
    });
  }

  private getNestedProperty(obj: any, path: string): any {
    return path.split('.').reduce((current, prop) => current?.[prop], obj);
  }

  private searchInAllProperties(obj: any, searchTerm: string): boolean {
    if (obj === null || obj === undefined) {
      return false;
    }

    if (typeof obj === 'string' || typeof obj === 'number') {
      return obj.toString().toLowerCase().includes(searchTerm);
    }

    if (typeof obj === 'object') {
      return Object.values(obj).some(value => 
        this.searchInAllProperties(value, searchTerm)
      );
    }

    return false;
  }
}