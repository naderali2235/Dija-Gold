# DijaGold POS Angular Development Plan

## Overview

This document outlines the comprehensive development plan for creating a modern Angular 20 frontend application for the DijaGold POS System. The application will integrate with the existing ASP.NET Core Web API and follow Angular best practices including standalone components, signals-based state management, and performance optimization.

## Project Architecture

### Technology Stack
- **Framework**: Angular 20 (Latest)
- **Styling**: SCSS with Angular Material
- **State Management**: Angular Signals
- **HTTP Client**: Angular HttpClient with interceptors
- **Forms**: Reactive Forms with typed FormControls
- **Testing**: Jasmine & Karma for unit tests, Cypress for E2E
- **Build Tool**: Angular CLI with Webpack
- **Package Manager**: npm

### Project Structure
```
src/app/
├── core/                   # Singleton services, guards, interceptors
│   ├── guards/
│   │   ├── auth.guard.ts
│   │   ├── manager.guard.ts
│   │   └── cashier.guard.ts
│   ├── interceptors/
│   │   ├── auth.interceptor.ts
│   │   ├── error.interceptor.ts
│   │   └── loading.interceptor.ts
│   ├── services/
│   │   ├── auth.service.ts
│   │   ├── token.service.ts
│   │   ├── notification.service.ts
│   │   └── api.service.ts
│   └── models/
│       ├── user.model.ts
│       ├── api-response.model.ts
│       └── common.model.ts
├── shared/                 # Reusable components, pipes, directives
│   ├── components/
│   │   ├── loading-spinner/
│   │   ├── confirmation-dialog/
│   │   ├── data-table/
│   │   ├── search-input/
│   │   └── currency-display/
│   ├── pipes/
│   │   ├── currency-format.pipe.ts
│   │   ├── weight-format.pipe.ts
│   │   └── date-format.pipe.ts
│   └── directives/
│       ├── number-only.directive.ts
│       └── decimal-precision.directive.ts
├── features/               # Feature modules
│   ├── auth/
│   ├── dashboard/
│   ├── products/
│   ├── transactions/
│   ├── inventory/
│   ├── customers/
│   ├── suppliers/
│   ├── reports/
│   ├── pricing/
│   └── settings/
└── layouts/               # Application layouts
    ├── main-layout/
    ├── auth-layout/
    └── print-layout/
```

## Development Phases

### Phase 1: Project Setup & Foundation

#### Project Initialization
- Create new Angular 20 project with standalone components
- Configure TypeScript strict mode
- Set up SCSS with Angular Material
- Configure ESLint and Prettier
- Set up environment configurations

#### Dependencies Installation
```json
{
  "dependencies": {
    "@angular/animations": "^20.0.0",
    "@angular/cdk": "^20.0.0",
    "@angular/material": "^20.0.0",
    "@angular/common": "^20.0.0",
    "@angular/forms": "^20.0.0",
    "rxjs": "^7.8.0",
    "date-fns": "^3.0.0",
    "chart.js": "^4.0.0",
    "ng2-charts": "^6.0.0"
  },
  "devDependencies": {
    "@types/node": "^20.0.0",
    "cypress": "^13.0.0",
    "@typescript-eslint/eslint-plugin": "^7.0.0"
  }
}
```

#### Theming & Localization
- Create custom Angular Material theme based on Figma design
- Set up RTL support for Arabic language
- Configure typography scale
- Set up internationalization (i18n) for Arabic/English

### Phase 2: Core Infrastructure

#### Authentication System
```typescript
// Core authentication components
- LoginComponent (standalone)
- ChangePasswordComponent
- AuthGuard with role-based access
- JWT token management
- Session timeout handling
```

#### HTTP Infrastructure
```typescript
// HTTP interceptors and services
- AuthInterceptor (automatic JWT attachment)
- ErrorInterceptor (global error handling)
- LoadingInterceptor (loading state management)
- CorrelationIdInterceptor (request tracking)
- ApiService (base HTTP service with typed responses)
```

#### State Management with Signals
```typescript
// Signal-based stores
- AuthStore (user state, permissions, login status)
- AppStore (global application state)
- NotificationStore (toast messages, alerts)
- LoadingStore (loading states across the app)
```

### Phase 3: Layout & Navigation

#### Main Layout Components
- **HeaderComponent**: User info, notifications, quick actions, logout
- **SidebarComponent**: Role-based navigation menu with collapsible sections
- **BreadcrumbComponent**: Dynamic navigation breadcrumbs
- **FooterComponent**: System information and status
- **MainLayoutComponent**: Container for authenticated pages

#### Navigation Structure
```typescript
// Role-based menu structure
Manager Menu:
- Dashboard
- Products (CRUD)
- Transactions (All types)
- Inventory Management
- Customer Management
- Supplier Management
- Pricing Management
- Reports & Analytics
- User Management
- System Settings

Cashier Menu:
- Dashboard (limited)
- POS Interface
- Transaction History
- Customer Lookup
- Basic Reports
```

#### Responsive Design
- Mobile-first approach
- Tablet-optimized POS interface
- Sidebar collapse/expand for mobile
- Touch-friendly controls
- Adaptive grid layouts

### Phase 4: Feature Modules Implementation

#### Authentication Module
```typescript
Features:
- Login with username/password
- Remember me functionality
- Password change
- Session management
- Auto-logout on token expiry
- Role-based redirects

Components:
- LoginComponent
- ChangePasswordComponent
- SessionTimeoutDialogComponent
```

#### Dashboard Module
```typescript
Manager Dashboard:
- Sales summary widgets
- Inventory alerts
- Recent transactions
- Profit analysis charts
- Quick action buttons
- System notifications

Cashier Dashboard:
- Today's sales summary
- Quick POS access
- Recent transactions
- Customer lookup
- Basic inventory status

Components:
- DashboardComponent
- SalesSummaryWidget
- InventoryStatusWidget
- QuickActionsWidget
- RecentTransactionsWidget
- ChartsComponent (using Chart.js)
```

#### Products Module
```typescript
Features:
- Product catalog with search and filters
- CRUD operations (Manager only)
- Barcode generation and scanning
- Category management
- Weight-based inventory tracking
- Karat type management
- Making charges configuration

Components:
- ProductListComponent (with virtual scrolling)
- ProductFormComponent (reactive forms)
- ProductDetailsComponent
- ProductSearchComponent
- BarcodeGeneratorComponent
- CategoryManagementComponent
- ProductImportComponent
```

#### POS/Transactions Module
```typescript
Core POS Features:
- Product selection with barcode scanning
- Shopping cart with real-time pricing
- Customer selection/creation
- Payment processing
- Receipt generation and printing
- Transaction history
- Return processing (Manager approval)
- Repair transaction handling

Components:
- POSComponent (main sales interface)
- ProductSelectorComponent
- ShoppingCartComponent
- CustomerSelectorComponent
- PaymentProcessorComponent
- ReceiptComponent
- TransactionHistoryComponent
- ReturnProcessingComponent
- RepairTransactionComponent
```

#### Inventory Module
```typescript
Features:
- Real-time inventory tracking
- Stock adjustments (Manager only)
- Inventory transfers between branches
- Low stock alerts
- Inventory movement history
- Batch inventory updates
- Inventory valuation reports

Components:
- InventoryListComponent
- StockAdjustmentComponent
- InventoryTransferComponent
- InventoryMovementHistoryComponent
- LowStockAlertsComponent
- InventoryValuationComponent
```

#### Customer Management Module
```typescript
Features:
- Customer database with search
- Customer profiles and history
- Loyalty program management
- Customer transaction history
- Customer analytics
- Contact management

Components:
- CustomerListComponent
- CustomerFormComponent
- CustomerDetailsComponent
- CustomerSearchComponent
- CustomerHistoryComponent
- LoyaltyManagementComponent
```

#### Supplier Management Module
```typescript
Features:
- Supplier database
- Purchase order management
- Supplier performance tracking
- Payment management
- Contact management

Components:
- SupplierListComponent
- SupplierFormComponent
- SupplierDetailsComponent
- PurchaseOrderComponent
- SupplierPerformanceComponent
```

#### Reports Module
```typescript
Available Reports:
- Daily sales summary
- Cash reconciliation
- Inventory movement
- Profit analysis (Manager only)
- Customer analysis (Manager only)
- Supplier balance (Manager only)
- Inventory valuation (Manager only)
- Tax reports (Manager only)
- Transaction logs
- Export to Excel/PDF

Components:
- ReportDashboardComponent
- SalesReportComponent
- InventoryReportComponent
- ProfitAnalysisComponent
- CustomerAnalyticsComponent
- TaxReportComponent
- ReportExportComponent
- ReportSchedulerComponent
```

#### Pricing Management Module
```typescript
Features:
- Daily gold rate management
- Making charges configuration
- Tax configuration
- Discount management
- Pricing calculator
- Historical pricing data

Components:
- GoldRateManagementComponent
- MakingChargesComponent
- TaxConfigurationComponent
- PricingCalculatorComponent
- PricingHistoryComponent
```

#### Settings & Administration Module
```typescript
Features:
- User management (Manager only)
- Branch management
- System settings
- Audit logs
- Backup management
- System maintenance

Components:
- UserManagementComponent
- BranchManagementComponent
- SystemSettingsComponent
- AuditLogComponent
- BackupManagementComponent
- SystemMaintenanceComponent
```

### Phase 5: Advanced Features

#### Print Functionality
```typescript
Features:
- Receipt printing with Egyptian formatting
- Label printing for products
- Barcode printing
- Report printing
- Custom print templates
- Print preview

Components:
- PrintService
- ReceiptTemplateComponent
- LabelTemplateComponent
- BarcodeTemplateComponent
- PrintPreviewComponent
```

#### Offline Capabilities (PWA)
```typescript
Features:
- Service worker implementation
- Offline data caching
- Sync when online
- Offline transaction queue
- App manifest for mobile installation

Components:
- OfflineIndicatorComponent
- SyncStatusComponent
- OfflineQueueComponent
```

#### Real-time Features
```typescript
Features:
- Real-time inventory updates
- Live gold rate updates
- Multi-user transaction notifications
- System alerts and notifications

Implementation:
- SignalR integration
- WebSocket connections
- Real-time state synchronization
```

### Phase 6: Optimization & Testing

#### Performance Optimization
- **Lazy Loading**: All feature modules loaded on demand
- **Deferrable Views**: Non-critical components loaded after main content
- **Virtual Scrolling**: Large lists (products, transactions, customers)
- **OnPush Change Detection**: Optimized change detection strategy
- **TrackBy Functions**: Efficient list rendering
- **Image Optimization**: NgOptimizedImage for product images
- **Bundle Analysis**: Webpack bundle analyzer for size optimization

#### Testing Strategy
```typescript
Unit Testing:
- Services with 90%+ coverage
- Components with signal testing
- Pipes and directives
- Guards and interceptors

Integration Testing:
- API integration tests
- Form validation tests
- Navigation flow tests

E2E Testing:
- Critical user journeys
- POS transaction flow
- Manager workflows
- Cross-browser testing
```

#### Accessibility Implementation
- **ARIA Labels**: Comprehensive screen reader support
- **Keyboard Navigation**: Full keyboard accessibility
- **Color Contrast**: WCAG 2.1 AA compliance
- **Focus Management**: Proper focus handling
- **RTL Support**: Right-to-left layout for Arabic

### Phase 7: Production Readiness

#### Build & Deployment Configuration
```typescript
Environments:
- development.ts
- staging.ts
- production.ts

Features:
- API endpoint configuration
- Feature flags
- Error tracking integration
- Analytics integration
- Performance monitoring
```

#### Security Implementation
- **Content Security Policy**: XSS protection
- **Input Sanitization**: All user inputs sanitized
- **Role-based Guards**: Route protection
- **HTTPS Enforcement**: Secure communication
- **Token Security**: Secure token storage and handling

#### Monitoring & Analytics
- **Error Tracking**: Comprehensive error logging
- **Performance Monitoring**: Core Web Vitals tracking
- **User Analytics**: Usage patterns and behavior
- **API Monitoring**: Request/response tracking

## Key Technical Decisions

### Angular 20 Features Utilization

#### Signals for State Management
```typescript
@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly http = inject(HttpClient);
  
  // Signal-based reactive state
  products = signal<Product[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  selectedProduct = signal<Product | null>(null);
  
  // Computed signals for derived state
  filteredProducts = computed(() => 
    this.products().filter(p => p.isActive)
  );
  
  totalProducts = computed(() => this.products().length);
}
```

#### Standalone Components
```typescript
@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, MaterialModule, SharedModule],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductListComponent {
  private readonly productService = inject(ProductService);
  
  products = this.productService.products;
  loading = this.productService.loading;
}
```

#### Deferrable Views for Performance
```html
<div class="dashboard">
  <div class="critical-content">
    <!-- Critical above-the-fold content -->
    <app-sales-summary />
  </div>
  
  @defer (on viewport) {
    <app-charts />
  } @placeholder {
    <div class="chart-skeleton">Loading charts...</div>
  }
  
  @defer (on idle) {
    <app-recent-transactions />
  }
</div>
```

### Component Architecture Patterns

#### Composition over Inheritance
```typescript
// Base table functionality
@Component({
  selector: 'app-data-table',
  standalone: true,
  template: `
    <table mat-table [dataSource]="dataSource()">
      <ng-content></ng-content>
    </table>
  `
})
export class DataTableComponent<T> {
  dataSource = input.required<T[]>();
  loading = input(false);
}

// Specific product table
@Component({
  selector: 'app-product-table',
  standalone: true,
  imports: [DataTableComponent, MatTableModule],
  template: `
    <app-data-table [dataSource]="products">
      <ng-container matColumnDef="name">
        <th mat-header-cell *matHeaderCellDef>Product Name</th>
        <td mat-cell *matCellDef="let product">{{product.name}}</td>
      </ng-container>
    </app-data-table>
  `
})
export class ProductTableComponent {
  products = input.required<Product[]>();
}
```

### API Integration Patterns

#### Typed HTTP Services
```typescript
@Injectable({ providedIn: 'root' })
export class ProductApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'api/products';
  
  getProducts(params: ProductSearchParams): Observable<PagedResult<Product>> {
    return this.http.get<ApiResponse<PagedResult<Product>>>(
      this.baseUrl, 
      { params: this.buildParams(params) }
    ).pipe(
      map(response => response.data),
      catchError(this.handleError)
    );
  }
  
  createProduct(product: CreateProductDto): Observable<Product> {
    return this.http.post<ApiResponse<Product>>(
      this.baseUrl, 
      product
    ).pipe(
      map(response => response.data),
      catchError(this.handleError)
    );
  }
  
  private handleError = (error: HttpErrorResponse) => {
    // Centralized error handling
    return throwError(() => error);
  };
}
```

### Form Handling with Reactive Forms
```typescript
@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [ReactiveFormsModule, MaterialModule],
  templateUrl: './product-form.component.html'
})
export class ProductFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly productService = inject(ProductService);
  
  form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(3)]],
    weight: [0, [Validators.required, Validators.min(0.001)]],
    karatType: ['', Validators.required],
    category: ['', Validators.required],
    makingCharges: this.fb.group({
      type: ['percentage'],
      value: [0, Validators.min(0)]
    })
  });
  
  onSubmit() {
    if (this.form.valid) {
      const product = this.form.getRawValue();
      this.productService.createProduct(product);
    }
  }
}
```

## Integration with Existing API

### API Endpoint Mapping
The Angular services will directly map to your existing API endpoints:

- **AuthService** → `/api/auth/*`
- **ProductService** → `/api/products/*`
- **TransactionService** → `/api/transactions/*`
- **InventoryService** → `/api/inventory/*`
- **CustomerService** → `/api/customers/*`
- **SupplierService** → `/api/suppliers/*`
- **ReportService** → `/api/reports/*`
- **PricingService** → `/api/pricing/*`
- **UserService** → `/api/users/*`
- **BranchService** → `/api/branches/*`

### Data Models Synchronization
TypeScript interfaces will mirror your C# DTOs:

```typescript
// Matching your existing DTOs
export interface Product {
  id: string;
  name: string;
  weight: number;
  karatType: KaratType;
  category: ProductCategoryType;
  isActive: boolean;
  branchId: string;
  // ... other properties matching your API
}

export interface CreateProductDto {
  name: string;
  weight: number;
  karatType: KaratType;
  category: ProductCategoryType;
  // ... other properties
}
```

## Deployment Considerations

### Build Configuration
```json
{
  "scripts": {
    "build:dev": "ng build --configuration development",
    "build:staging": "ng build --configuration staging",
    "build:prod": "ng build --configuration production --optimization",
    "analyze": "ng build --stats-json && webpack-bundle-analyzer dist/stats.json"
  }
}
```

### Environment Configuration
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://api.dijagold.com',
  enableAnalytics: true,
  enableErrorTracking: true,
  version: '1.0.0'
};
```

## Success Metrics

### Performance Targets
- **First Contentful Paint (FCP)**: < 1.5s
- **Largest Contentful Paint (LCP)**: < 2.5s
- **Interaction to Next Paint (INP)**: < 200ms
- **Cumulative Layout Shift (CLS)**: < 0.1

### Code Quality Targets
- **Test Coverage**: > 80% for critical paths
- **TypeScript Strict Mode**: 100% compliance
- **ESLint**: Zero warnings/errors
- **Accessibility**: WCAG 2.1 AA compliance

### User Experience Goals
- **Mobile Responsiveness**: 100% feature parity
- **Offline Capability**: Core POS functions work offline
- **Load Time**: < 3s on 3G networks
- **Error Handling**: User-friendly error messages for all scenarios

## Maintenance & Updates

### Code Maintenance
- Regular dependency updates
- Angular version migrations
- Performance monitoring and optimization
- Security vulnerability assessments

### Documentation
- Technical documentation for developers
- User guides for end users
- API integration documentation
- Deployment and maintenance procedures

This comprehensive plan provides a solid foundation for building a modern, scalable, and maintainable Angular application that integrates seamlessly with your existing DijaGold POS API while following Angular 20 best practices and performance optimization techniques.
