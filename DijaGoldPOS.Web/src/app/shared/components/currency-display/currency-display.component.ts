import { Component, Input } from '@angular/core';
import { environment } from '@environments/environment';

@Component({
  selector: 'app-currency-display',
  template: `
    <span class="currency-display" [class]="cssClass">
      <span *ngIf="showSymbol && symbolPosition === 'before'" class="currency-symbol">
        {{ currencySymbol }}
      </span>
      <span class="currency-amount">
        {{ formattedAmount }}
      </span>
      <span *ngIf="showSymbol && symbolPosition === 'after'" class="currency-symbol">
        {{ currencySymbol }}
      </span>
    </span>
  `,
  styles: [`
    .currency-display {
      font-weight: 500;
    }

    .currency-symbol {
      margin: 0 0.25rem;
      font-size: 0.9em;
      opacity: 0.8;
    }

    .currency-amount {
      font-variant-numeric: tabular-nums;
    }

    .positive {
      color: var(--bs-success);
    }

    .negative {
      color: var(--bs-danger);
    }

    .zero {
      color: var(--bs-secondary);
    }

    .large {
      font-size: 1.25em;
      font-weight: 600;
    }

    .small {
      font-size: 0.875em;
    }
  `]
})
export class CurrencyDisplayComponent {
  @Input() amount: number = 0;
  @Input() showSymbol: boolean = true;
  @Input() symbolPosition: 'before' | 'after' = 'after';
  @Input() decimalPlaces?: number;
  @Input() size: 'small' | 'normal' | 'large' = 'normal';
  @Input() colorize: boolean = false;
  @Input() customClass?: string;

  get currencySymbol(): string {
    return environment.settings.currency.symbol;
  }

  get formattedAmount(): string {
    const decimals = this.decimalPlaces ?? environment.settings.currency.decimalPlaces;
    return this.amount.toFixed(decimals);
  }

  get cssClass(): string {
    let classes: string[] = [];
    
    if (this.colorize) {
      if (this.amount > 0) {
        classes.push('positive');
      } else if (this.amount < 0) {
        classes.push('negative');
      } else {
        classes.push('zero');
      }
    }
    
    if (this.size !== 'normal') {
      classes.push(this.size);
    }
    
    if (this.customClass) {
      classes.push(this.customClass);
    }
    
    return classes.join(' ');
  }
}