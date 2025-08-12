import { Component, Input, Output, EventEmitter } from '@angular/core';

export interface PageAction {
  label: string;
  icon?: string;
  action: () => void;
  color?: 'primary' | 'accent' | 'warn';
  disabled?: boolean;
  tooltip?: string;
}

@Component({
  selector: 'app-page-header',
  template: `
    <div class="page-header d-flex align-items-center justify-content-between mb-4">
      <div class="page-title-section">
        <div class="d-flex align-items-center">
          <mat-icon *ngIf="icon" class="me-2 text-primary">{{ icon }}</mat-icon>
          <div>
            <h1 class="page-title mb-0">{{ title }}</h1>
            <p *ngIf="subtitle" class="page-subtitle text-muted mb-0">{{ subtitle }}</p>
          </div>
        </div>
      </div>
      
      <div class="page-actions" *ngIf="actions.length > 0">
        <div class="d-flex gap-2">
          <button
            *ngFor="let action of actions"
            mat-raised-button
            [color]="action.color || 'primary'"
            [disabled]="action.disabled"
            [matTooltip]="action.tooltip || ''"
            (click)="action.action()">
            <mat-icon *ngIf="action.icon" class="me-1">{{ action.icon }}</mat-icon>
            {{ action.label }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page-header {
      padding: 1rem 0;
      border-bottom: 1px solid #e9ecef;
    }
    
    .page-title {
      font-size: 1.75rem;
      font-weight: 600;
      color: #212529;
    }
    
    .page-subtitle {
      font-size: 0.95rem;
      margin-top: 0.25rem;
    }
    
    .page-actions {
      flex-shrink: 0;
    }
    
    @media (max-width: 768px) {
      .page-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 1rem;
      }
      
      .page-actions {
        width: 100%;
        
        .d-flex {
          flex-wrap: wrap;
        }
      }
      
      .page-title {
        font-size: 1.5rem;
      }
    }
  `]
})
export class PageHeaderComponent {
  @Input() title: string = '';
  @Input() subtitle?: string;
  @Input() icon?: string;
  @Input() actions: PageAction[] = [];
}