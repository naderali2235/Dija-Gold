-- =============================================
-- View: Sales_RevenueDaily_v1
-- Description: Daily sales revenue aggregated by branch and product category
-- Performance: Optimized with indexes on date and branch columns
-- =============================================

CREATE VIEW [Reports].[Sales_RevenueDaily_v1]
AS
SELECT 
    -- Date Information
    CAST(o.OrderDate AS DATE) as SalesDate,
    YEAR(o.OrderDate) as SalesYear,
    MONTH(o.OrderDate) as SalesMonth,
    DAY(o.OrderDate) as SalesDay,
    DATENAME(WEEKDAY, o.OrderDate) as DayOfWeek,
    
    -- Branch Information
    b.Id as BranchId,
    b.Code as BranchCode,
    b.Name as BranchName,
    
    -- Product Category Information
    pct.Id as CategoryId,
    pct.Name as CategoryName,
    pct.Code as CategoryCode,
    
    -- Karat Information
    kt.Id as KaratTypeId,
    kt.Name as KaratTypeName,
    kt.KaratValue,
    kt.Abbreviation as KaratAbbreviation,
    
    -- Sales Metrics
    COUNT(DISTINCT o.Id) as OrderCount,
    COUNT(oi.Id) as ItemCount,
    
    -- Weight Metrics
    SUM(CASE WHEN p.Weight IS NOT NULL THEN oi.Quantity * p.Weight ELSE 0 END) as TotalWeight,
    AVG(CASE WHEN p.Weight IS NOT NULL THEN p.Weight ELSE NULL END) as AverageItemWeight,
    
    -- Revenue Metrics
    SUM(oi.TotalPrice) as GrossRevenue,
    SUM(oi.DiscountAmount) as TotalDiscounts,
    SUM(oi.TaxAmount) as TotalTax,
    SUM(oi.MakingCharges) as TotalMakingCharges,
    SUM(oi.FinalPrice) as NetRevenue,
    
    -- Profitability Metrics (requires cost data)
    SUM(CASE WHEN p.StandardCost IS NOT NULL 
             THEN oi.Quantity * p.StandardCost 
             ELSE 0 END) as TotalCost,
    SUM(oi.FinalPrice) - SUM(CASE WHEN p.StandardCost IS NOT NULL 
                                  THEN oi.Quantity * p.StandardCost 
                                  ELSE 0 END) as GrossProfit,
    
    -- Average Metrics
    AVG(oi.UnitPrice) as AverageUnitPrice,
    AVG(oi.FinalPrice) as AverageItemValue,
    AVG(oi.DiscountPercentage) as AverageDiscountPercentage,
    
    -- Customer Metrics
    COUNT(DISTINCT o.CustomerId) as UniqueCustomers,
    COUNT(DISTINCT CASE WHEN c.Id IS NOT NULL THEN o.CustomerId END) as RegisteredCustomers,
    COUNT(DISTINCT CASE WHEN c.Id IS NULL THEN o.Id END) as WalkInSales,
    
    -- Payment Method Breakdown (requires financial transaction data)
    COUNT(DISTINCT CASE WHEN pm.Code = 'CASH' THEN o.Id END) as CashSales,
    COUNT(DISTINCT CASE WHEN pm.Code = 'CARD' THEN o.Id END) as CardSales,
    COUNT(DISTINCT CASE WHEN pm.Code = 'BANK_TRANSFER' THEN o.Id END) as BankTransferSales,
    
    -- Order Type Breakdown
    COUNT(DISTINCT CASE WHEN ot.Code = 'SALE' THEN o.Id END) as RegularSales,
    COUNT(DISTINCT CASE WHEN ot.Code = 'RETURN' THEN o.Id END) as Returns,
    COUNT(DISTINCT CASE WHEN ot.Code = 'EXCHANGE' THEN o.Id END) as Exchanges,
    
    -- Time-based Metrics
    MIN(o.OrderDate) as FirstSaleTime,
    MAX(o.OrderDate) as LastSaleTime,
    
    -- Quality Metrics
    COUNT(DISTINCT CASE WHEN os.Code = 'COMPLETED' THEN o.Id END) as CompletedOrders,
    COUNT(DISTINCT CASE WHEN os.Code = 'CANCELLED' THEN o.Id END) as CancelledOrders,
    CAST(COUNT(DISTINCT CASE WHEN os.Code = 'COMPLETED' THEN o.Id END) * 100.0 / 
         NULLIF(COUNT(DISTINCT o.Id), 0) AS DECIMAL(5,2)) as CompletionRate

FROM [Sales].[Orders] o
INNER JOIN [Core].[Branches] b ON o.BranchId = b.Id
INNER JOIN [Sales].[OrderItems] oi ON o.Id = oi.OrderId
INNER JOIN [Product].[Products] p ON oi.ProductId = p.Id
INNER JOIN [Lookup].[ProductCategoryTypes] pct ON p.CategoryTypeId = pct.Id
INNER JOIN [Lookup].[KaratTypes] kt ON p.KaratTypeId = kt.Id
INNER JOIN [Lookup].[OrderTypes] ot ON o.OrderTypeId = ot.Id
INNER JOIN [Lookup].[OrderStatuses] os ON o.StatusId = os.Id
LEFT JOIN [Customer].[Customers] c ON o.CustomerId = c.Id
LEFT JOIN [Financial].[FinancialTransactions] ft ON o.FinancialTransactionId = ft.Id
LEFT JOIN [Lookup].[PaymentMethods] pm ON ft.PaymentMethodId = pm.Id

WHERE 
    o.IsActive = 1
    AND oi.IsActive = 1
    AND p.IsActive = 1
    AND b.IsActive = 1
    AND pct.IsActive = 1
    AND kt.IsActive = 1
    AND ot.IsActive = 1
    AND os.IsActive = 1
    AND o.OrderDate >= DATEADD(YEAR, -2, GETDATE()) -- Limit to last 2 years for performance

GROUP BY 
    CAST(o.OrderDate AS DATE),
    YEAR(o.OrderDate),
    MONTH(o.OrderDate),
    DAY(o.OrderDate),
    DATENAME(WEEKDAY, o.OrderDate),
    b.Id, b.Code, b.Name,
    pct.Id, pct.Name, pct.Code,
    kt.Id, kt.Name, kt.KaratValue, kt.Abbreviation;

-- Create indexes for optimal performance
CREATE NONCLUSTERED INDEX IX_Sales_RevenueDaily_Date_Branch 
ON [Sales].[Orders] (OrderDate, BranchId) 
INCLUDE (CustomerId, OrderTypeId, StatusId, FinancialTransactionId)
WHERE IsActive = 1;

CREATE NONCLUSTERED INDEX IX_Sales_RevenueDaily_OrderItems 
ON [Sales].[OrderItems] (OrderId, ProductId) 
INCLUDE (Quantity, UnitPrice, TotalPrice, DiscountAmount, TaxAmount, MakingCharges, FinalPrice)
WHERE IsActive = 1;
