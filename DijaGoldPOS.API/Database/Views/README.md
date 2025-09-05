# Database Views for Reporting

This directory contains SQL scripts for creating optimized database views for all reporting scenarios. Views provide better performance, security, and maintainability compared to direct table queries.

## View Categories

### 1. Sales & Revenue Views
- **SalesRevenueDaily**: Daily sales revenue aggregated by branch and product category
- **SalesRevenueTrends**: Monthly and yearly sales trends with comparative analysis
- **CustomerSalesAnalysis**: Customer purchase patterns and loyalty metrics
- **ProductPerformance**: Product-wise sales performance and profitability

### 2. Inventory Views
- **InventoryStatus**: Current stock levels with reorder alerts
- **InventoryMovements**: Stock movement history with transaction details
- **ProductOwnershipSummary**: Supplier-wise product ownership tracking
- **GoldBalanceByKarat**: Gold balance summary by karat type and branch

### 3. Financial Views
- **CashFlowSummary**: Daily cash flow with opening/closing balances
- **OutstandingPayables**: Supplier payment obligations
- **OutstandingReceivables**: Customer payment obligations
- **ProfitabilityAnalysis**: Gross profit analysis by product and time period

### 4. Operational Views
- **BranchPerformance**: Branch-wise operational metrics
- **UserActivity**: User transaction summary and performance metrics
- **RepairJobStatus**: Repair job progress and completion metrics
- **ManufacturingEfficiency**: Production efficiency and waste analysis

### 5. Compliance & Audit Views
- **AuditTrailSummary**: Comprehensive audit trail for compliance reporting
- **TransactionAnomalies**: Unusual transaction patterns for review
- **KYCComplianceStatus**: Customer KYC status and compliance metrics
- **RegulatoryReporting**: Views for regulatory compliance reports

## Performance Optimization

### Indexing Strategy
- All views have corresponding indexes on frequently filtered columns
- Composite indexes for multi-column filters
- Covering indexes for SELECT-only operations

### Materialized Views
- High-frequency reports use materialized views with scheduled refresh
- Real-time views for operational dashboards
- Historical views for trend analysis

### Caching Strategy
- Application-level caching for frequently accessed views
- Database-level query plan caching
- Result set caching for expensive aggregations

## Security Features

### Row-Level Security
- Branch-based data filtering for multi-tenant scenarios
- User role-based data access restrictions
- Sensitive data masking for non-privileged users

### Data Privacy
- PII masking in reporting views
- Audit trail for view access
- Compliance with data protection regulations

## Maintenance

### Refresh Schedule
- Real-time views: No refresh needed
- Near real-time: 5-minute refresh
- Daily aggregations: Overnight refresh
- Monthly/Yearly: Weekly refresh

### Monitoring
- View performance monitoring
- Usage statistics tracking
- Automated alerting for performance degradation

## Usage Guidelines

1. **Always use views** for reporting instead of direct table queries
2. **Filter early** - apply WHERE clauses to reduce data volume
3. **Use appropriate indexes** - ensure proper indexing for your filters
4. **Monitor performance** - track query execution times
5. **Cache results** - implement caching for frequently accessed data

## View Naming Convention

- Prefix with module name (e.g., `Sales_`, `Inventory_`)
- Descriptive names indicating purpose
- Suffix with aggregation level (`_Daily`, `_Monthly`, `_Summary`)
- Version suffix for schema changes (`_v1`, `_v2`)

Examples:
- `Sales_RevenueDaily_v1`
- `Inventory_StockStatus_v1`
- `Financial_CashFlow_Summary_v1`
