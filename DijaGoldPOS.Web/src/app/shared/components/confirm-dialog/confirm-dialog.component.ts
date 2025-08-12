import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  type?: 'warning' | 'danger' | 'info';
}

@Component({
  selector: 'app-confirm-dialog',
  template: `
    <div class="confirm-dialog">
      <h2 mat-dialog-title class="d-flex align-items-center">
        <mat-icon [class]="getIconClass()" class="me-2">{{ getIcon() }}</mat-icon>
        {{ data.title }}
      </h2>
      
      <mat-dialog-content class="py-3">
        <p class="mb-0">{{ data.message }}</p>
      </mat-dialog-content>
      
      <mat-dialog-actions align="end" class="gap-2">
        <button mat-button (click)="onCancel()" [disabled]="false">
          {{ data.cancelText || 'Cancel' }}
        </button>
        <button 
          mat-raised-button 
          [color]="getButtonColor()" 
          (click)="onConfirm()"
          [disabled]="false">
          {{ data.confirmText || 'Confirm' }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .confirm-dialog {
      min-width: 300px;
      max-width: 500px;
    }
    
    .text-warning { color: #ffc107; }
    .text-danger { color: #dc3545; }
    .text-info { color: #0dcaf0; }
    
    mat-dialog-actions {
      padding: 16px 24px;
    }
  `]
})
export class ConfirmDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogData
  ) {}

  onConfirm(): void {
    this.dialogRef.close(true);
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  getIcon(): string {
    switch (this.data.type) {
      case 'warning':
        return 'warning';
      case 'danger':
        return 'error';
      case 'info':
        return 'info';
      default:
        return 'help';
    }
  }

  getIconClass(): string {
    switch (this.data.type) {
      case 'warning':
        return 'text-warning';
      case 'danger':
        return 'text-danger';
      case 'info':
        return 'text-info';
      default:
        return 'text-primary';
    }
  }

  getButtonColor(): string {
    switch (this.data.type) {
      case 'danger':
        return 'warn';
      default:
        return 'primary';
    }
  }
}