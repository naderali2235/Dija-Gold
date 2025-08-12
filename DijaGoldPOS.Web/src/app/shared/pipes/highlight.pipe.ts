import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'highlight'
})
export class HighlightPipe implements PipeTransform {
  transform(
    text: string | null | undefined,
    searchTerm: string | null | undefined,
    cssClass: string = 'highlight'
  ): string {
    if (!text || !searchTerm) {
      return text || '';
    }

    const term = searchTerm.trim();
    if (!term) {
      return text;
    }

    // Escape special regex characters
    const escapedTerm = term.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    const regex = new RegExp(`(${escapedTerm})`, 'gi');
    
    return text.replace(regex, `<span class="${cssClass}">$1</span>`);
  }
}