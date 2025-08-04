import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  template: `
    <div class="loading-spinner-container" [class.overlay]="overlay">
      <div class="text-center">
        <div 
          class="spinner-border" 
          [class]="'text-' + color"
          [style.width.rem]="size"
          [style.height.rem]="size"
          role="status">
          <span class="visually-hidden">{{ message }}</span>
        </div>
        <div *ngIf="showMessage" class="mt-2">
          <small class="text-muted">{{ message }}</small>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .loading-spinner-container {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 2rem;
    }

    .loading-spinner-container.overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background-color: rgba(255, 255, 255, 0.8);
      z-index: 1000;
    }
  `]
})
export class LoadingSpinnerComponent {
  @Input() message: string = 'Loading...';
  @Input() showMessage: boolean = false;
  @Input() size: number = 2;
  @Input() color: string = 'primary';
  @Input() overlay: boolean = false;
}