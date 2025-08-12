import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  template: `
    <div class="empty-state text-center py-5">
      <div class="empty-state-icon mb-3">
        <mat-icon class="large-icon text-muted">{{ icon }}</mat-icon>
      </div>
      
      <h4 class="empty-state-title text-muted mb-2">{{ title }}</h4>
      
      <p *ngIf="message" class="empty-state-message text-muted mb-3">
        {{ message }}
      </p>
      
      <div *ngIf="actionLabel && actionCallback" class="empty-state-action">
        <button 
          mat-raised-button 
          color="primary" 
          (click)="actionCallback()">
          <mat-icon *ngIf="actionIcon" class="me-1">{{ actionIcon }}</mat-icon>
          {{ actionLabel }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .empty-state {
      padding: 3rem 1rem;
    }
    
    .large-icon {
      font-size: 4rem;
      width: 4rem;
      height: 4rem;
      opacity: 0.5;
    }
    
    .empty-state-title {
      font-size: 1.25rem;
      font-weight: 500;
    }
    
    .empty-state-message {
      max-width: 400px;
      margin: 0 auto 1.5rem;
      line-height: 1.5;
    }
  `]
})
export class EmptyStateComponent {
  @Input() icon: string = 'inbox';
  @Input() title: string = 'No data available';
  @Input() message?: string;
  @Input() actionLabel?: string;
  @Input() actionIcon?: string;
  @Input() actionCallback?: () => void;
}