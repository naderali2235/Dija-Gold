import { Component, Input } from '@angular/core';
import { environment } from '@environments/environment';

@Component({
  selector: 'app-weight-display',
  template: `
    <span class="weight-display" [class]="cssClass">
      <span class="weight-amount">
        {{ formattedWeight }}
      </span>
      <span *ngIf="showUnit" class="weight-unit">
        {{ weightUnit }}
      </span>
    </span>
  `,
  styles: [`
    .weight-display {
      font-weight: 500;
      font-variant-numeric: tabular-nums;
    }

    .weight-unit {
      margin-left: 0.25rem;
      font-size: 0.9em;
      opacity: 0.8;
    }

    .large {
      font-size: 1.25em;
      font-weight: 600;
    }

    .small {
      font-size: 0.875em;
    }

    .highlight {
      background-color: var(--bs-warning-bg);
      padding: 0.125rem 0.25rem;
      border-radius: 0.25rem;
    }

    .text-gold {
      color: #d4af37;
      font-weight: 600;
    }
  `]
})
export class WeightDisplayComponent {
  @Input() weight: number = 0;
  @Input() showUnit: boolean = true;
  @Input() decimalPlaces?: number;
  @Input() size: 'small' | 'normal' | 'large' = 'normal';
  @Input() highlight: boolean = false;
  @Input() goldTheme: boolean = false;
  @Input() customClass?: string;

  get weightUnit(): string {
    return environment.settings.weight.unit;
  }

  get formattedWeight(): string {
    const decimals = this.decimalPlaces ?? environment.settings.weight.decimalPlaces;
    return this.weight.toFixed(decimals);
  }

  get cssClass(): string {
    let classes: string[] = [];
    
    if (this.size !== 'normal') {
      classes.push(this.size);
    }
    
    if (this.highlight) {
      classes.push('highlight');
    }
    
    if (this.goldTheme) {
      classes.push('text-gold');
    }
    
    if (this.customClass) {
      classes.push(this.customClass);
    }
    
    return classes.join(' ');
  }
}