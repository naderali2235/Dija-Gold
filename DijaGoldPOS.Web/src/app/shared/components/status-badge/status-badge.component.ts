import { Component, Input } from '@angular/core';
import { TransactionStatus, TransactionType } from '@core/models/enums';

@Component({
  selector: 'app-status-badge',
  template: `
    <span class="badge" [class]="getBadgeClass()">
      <i *ngIf="showIcon" [class]="getIconClass()" class="me-1"></i>
      {{ getDisplayText() }}
    </span>
  `,
  styles: [`
    .badge {
      font-size: 0.75rem;
      font-weight: 500;
      padding: 0.375rem 0.75rem;
      border-radius: 0.375rem;
      display: inline-flex;
      align-items: center;
    }
    
    .badge i {
      font-size: 0.875em;
    }
    
    .bg-success { background-color: #28a745 !important; color: white; }
    .bg-warning { background-color: #ffc107 !important; color: #212529; }
    .bg-danger { background-color: #dc3545 !important; color: white; }
    .bg-info { background-color: #17a2b8 !important; color: white; }
    .bg-secondary { background-color: #6c757d !important; color: white; }
    .bg-primary { background-color: #007bff !important; color: white; }
  `]
})
export class StatusBadgeComponent {
  @Input() status?: TransactionStatus | string;
  @Input() type?: TransactionType | string;
  @Input() customText?: string;
  @Input() customClass?: string;
  @Input() showIcon: boolean = true;

  getBadgeClass(): string {
    if (this.customClass) {
      return `badge ${this.customClass}`;
    }

    if (this.status !== undefined) {
      switch (this.status) {
        case TransactionStatus.Completed:
        case 'Completed':
        case 'Active':
        case 'Received':
          return 'badge bg-success';
        case TransactionStatus.Pending:
        case 'Pending':
        case 'Processing':
          return 'badge bg-warning';
        case TransactionStatus.Cancelled:
        case 'Cancelled':
        case 'Inactive':
          return 'badge bg-danger';
        case TransactionStatus.Refunded:
        case 'Refunded':
        case 'Returned':
          return 'badge bg-info';
        default:
          return 'badge bg-secondary';
      }
    }

    if (this.type !== undefined) {
      switch (this.type) {
        case TransactionType.Sale:
        case 'Sale':
          return 'badge bg-success';
        case TransactionType.Return:
        case 'Return':
          return 'badge bg-warning';
        case TransactionType.Repair:
        case 'Repair':
          return 'badge bg-info';
        default:
          return 'badge bg-primary';
      }
    }

    return 'badge bg-secondary';
  }

  getIconClass(): string {
    if (this.status !== undefined) {
      switch (this.status) {
        case TransactionStatus.Completed:
        case 'Completed':
        case 'Active':
        case 'Received':
          return 'bi bi-check-circle';
        case TransactionStatus.Pending:
        case 'Pending':
        case 'Processing':
          return 'bi bi-clock';
        case TransactionStatus.Cancelled:
        case 'Cancelled':
        case 'Inactive':
          return 'bi bi-x-circle';
        case TransactionStatus.Refunded:
        case 'Refunded':
        case 'Returned':
          return 'bi bi-arrow-return-left';
        default:
          return 'bi bi-question-circle';
      }
    }

    if (this.type !== undefined) {
      switch (this.type) {
        case TransactionType.Sale:
        case 'Sale':
          return 'bi bi-cart-check';
        case TransactionType.Return:
        case 'Return':
          return 'bi bi-arrow-return-left';
        case TransactionType.Repair:
        case 'Repair':
          return 'bi bi-tools';
        default:
          return 'bi bi-receipt';
      }
    }

    return 'bi bi-info-circle';
  }

  getDisplayText(): string {
    if (this.customText) {
      return this.customText;
    }

    if (this.status !== undefined) {
      if (typeof this.status === 'string') {
        return this.status;
      }
      
      switch (this.status) {
        case TransactionStatus.Completed:
          return 'Completed';
        case TransactionStatus.Pending:
          return 'Pending';
        case TransactionStatus.Cancelled:
          return 'Cancelled';
        case TransactionStatus.Refunded:
          return 'Refunded';
        default:
          return 'Unknown';
      }
    }

    if (this.type !== undefined) {
      if (typeof this.type === 'string') {
        return this.type;
      }
      
      switch (this.type) {
        case TransactionType.Sale:
          return 'Sale';
        case TransactionType.Return:
          return 'Return';
        case TransactionType.Repair:
          return 'Repair';
        default:
          return 'Transaction';
      }
    }

    return 'Status';
  }
}