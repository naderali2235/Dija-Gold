import { Directive, ElementRef, HostListener, Input } from '@angular/core';

@Directive({
  selector: '[appNumberOnly]'
})
export class NumberOnlyDirective {
  @Input() allowDecimal: boolean = true;
  @Input() allowNegative: boolean = false;
  @Input() decimalPlaces?: number;

  constructor(private elementRef: ElementRef) {}

  @HostListener('keydown', ['$event'])
  onKeyDown(event: KeyboardEvent): void {
    const input = this.elementRef.nativeElement as HTMLInputElement;
    const key = event.key;
    const currentValue = input.value;
    const selectionStart = input.selectionStart || 0;
    const selectionEnd = input.selectionEnd || 0;

    // Allow special keys
    if (this.isSpecialKey(key)) {
      return;
    }

    // Allow numbers
    if (/^\d$/.test(key)) {
      // Check decimal places limit
      if (this.decimalPlaces !== undefined && this.allowDecimal) {
        const decimalIndex = currentValue.indexOf('.');
        if (decimalIndex !== -1) {
          const afterDecimal = currentValue.substring(decimalIndex + 1);
          if (afterDecimal.length >= this.decimalPlaces && selectionStart > decimalIndex) {
            event.preventDefault();
            return;
          }
        }
      }
      return;
    }

    // Allow decimal point
    if (key === '.' && this.allowDecimal) {
      if (currentValue.includes('.')) {
        event.preventDefault();
        return;
      }
      return;
    }

    // Allow negative sign
    if (key === '-' && this.allowNegative) {
      if (currentValue.includes('-') || selectionStart !== 0) {
        event.preventDefault();
        return;
      }
      return;
    }

    // Block all other keys
    event.preventDefault();
  }

  @HostListener('paste', ['$event'])
  onPaste(event: ClipboardEvent): void {
    const clipboardData = event.clipboardData?.getData('text') || '';
    
    if (!this.isValidNumber(clipboardData)) {
      event.preventDefault();
    }
  }

  private isSpecialKey(key: string): boolean {
    const specialKeys = [
      'Backspace', 'Delete', 'Tab', 'Escape', 'Enter',
      'Home', 'End', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown',
      'F1', 'F2', 'F3', 'F4', 'F5', 'F6', 'F7', 'F8', 'F9', 'F10', 'F11', 'F12'
    ];

    // Allow Ctrl+A, Ctrl+C, Ctrl+V, Ctrl+X, Ctrl+Z
    if (event.ctrlKey && ['a', 'c', 'v', 'x', 'z'].includes(key.toLowerCase())) {
      return true;
    }

    return specialKeys.includes(key);
  }

  private isValidNumber(value: string): boolean {
    if (!value) return true;

    let pattern = '^';
    
    if (this.allowNegative) {
      pattern += '-?';
    }
    
    pattern += '\\d*';
    
    if (this.allowDecimal) {
      pattern += '(\\.\\d*)?';
    }
    
    pattern += '$';

    const regex = new RegExp(pattern);
    return regex.test(value);
  }
}