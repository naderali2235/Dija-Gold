import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

// Material Modules (re-export for convenience)
import { MaterialModules } from '../app.module';

// Shared Components
import { LoadingSpinnerComponent } from './components/loading-spinner/loading-spinner.component';
import { ConfirmDialogComponent } from './components/confirm-dialog/confirm-dialog.component';
import { PageHeaderComponent } from './components/page-header/page-header.component';
import { EmptyStateComponent } from './components/empty-state/empty-state.component';
import { StatusBadgeComponent } from './components/status-badge/status-badge.component';
import { CurrencyDisplayComponent } from './components/currency-display/currency-display.component';
import { WeightDisplayComponent } from './components/weight-display/weight-display.component';

// Pipes
import { CurrencyFormatPipe } from './pipes/currency-format.pipe';
import { WeightFormatPipe } from './pipes/weight-format.pipe';
import { DateTimeFormatPipe } from './pipes/datetime-format.pipe';
import { EnumDisplayPipe } from './pipes/enum-display.pipe';
import { FilterPipe } from './pipes/filter.pipe';
import { HighlightPipe } from './pipes/highlight.pipe';

// Directives
import { AutofocusDirective } from './directives/autofocus.directive';
import { ClickOutsideDirective } from './directives/click-outside.directive';
import { NumberOnlyDirective } from './directives/number-only.directive';
import { PermissionDirective } from './directives/permission.directive';

@NgModule({
  declarations: [
    // Components
    LoadingSpinnerComponent,
    ConfirmDialogComponent,
    PageHeaderComponent,
    EmptyStateComponent,
    StatusBadgeComponent,
    CurrencyDisplayComponent,
    WeightDisplayComponent,
    
    // Pipes
    CurrencyFormatPipe,
    WeightFormatPipe,
    DateTimeFormatPipe,
    EnumDisplayPipe,
    FilterPipe,
    HighlightPipe,
    
    // Directives
    AutofocusDirective,
    ClickOutsideDirective,
    NumberOnlyDirective,
    PermissionDirective
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule,
    ...MaterialModules
  ],
  exports: [
    // Angular modules
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule,
    
    // Material modules
    ...MaterialModules,
    
    // Shared components
    LoadingSpinnerComponent,
    ConfirmDialogComponent,
    PageHeaderComponent,
    EmptyStateComponent,
    StatusBadgeComponent,
    CurrencyDisplayComponent,
    WeightDisplayComponent,
    
    // Pipes
    CurrencyFormatPipe,
    WeightFormatPipe,
    DateTimeFormatPipe,
    EnumDisplayPipe,
    FilterPipe,
    HighlightPipe,
    
    // Directives
    AutofocusDirective,
    ClickOutsideDirective,
    NumberOnlyDirective,
    PermissionDirective
  ]
})
export class SharedModule { }