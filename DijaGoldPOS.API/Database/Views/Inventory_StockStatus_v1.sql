-- =============================================
-- View: Inventory_StockStatus_v1
-- Description: Current inventory status with reorder alerts and valuation
-- Performance: Optimized for real-time inventory queries
-- =============================================

CREATE VIEW [Reports].[Inventory_StockStatus_v1]
AS
SELECT 
    -- Product Information
    p.Id as ProductId,
    p.ProductCode,
    p.Name as ProductName,
    p.Brand,
    p.DesignStyle,
    p.Weight as UnitWeight,
    p.IsAvailableForSale,
    p.IsFeatured,
    
    -- Category Information
    pct.Name as CategoryName,
    pct.Code as CategoryCode,
    sc.Name as SubCategoryName,
    sc.Code as SubCategoryCode,
    
    -- Karat Information
    kt.Name as KaratTypeName,
    kt.KaratValue,
    kt.Abbreviation as KaratAbbreviation,
    kt.PurityPercentage,
    
    -- Branch Information
    b.Id as BranchId,
    b.Code as BranchCode,
    b.Name as BranchName,
    
    -- Inventory Quantities
    i.QuantityOnHand,
    i.WeightOnHand,
    i.MinimumStockLevel,
    i.MaximumStockLevel,
    i.ReorderPoint,
    
    -- Stock Status Indicators
    CASE 
        WHEN i.QuantityOnHand <= 0 THEN 'Out of Stock'
        WHEN i.QuantityOnHand <= i.ReorderPoint THEN 'Reorder Required'
        WHEN i.QuantityOnHand <= i.MinimumStockLevel THEN 'Low Stock'
        WHEN i.QuantityOnHand >= i.MaximumStockLevel THEN 'Overstock'
        ELSE 'Normal'
    END as StockStatus,
    
    -- Reorder Calculations
    CASE 
        WHEN i.MaximumStockLevel IS NOT NULL AND i.QuantityOnHand IS NOT NULL
        THEN i.MaximumStockLevel - i.QuantityOnHand
        ELSE NULL
    END as ReorderQuantity,
    
    CASE 
        WHEN i.MaximumStockLevel IS NOT NULL AND i.WeightOnHand IS NOT NULL AND p.Weight IS NOT NULL
        THEN (i.MaximumStockLevel - i.QuantityOnHand) * p.Weight
        ELSE NULL
    END as ReorderWeight,
    
    -- Valuation Information
    p.StandardCost as UnitCost,
    p.CurrentPrice as UnitPrice,
    
    -- Current Inventory Value
    CASE 
        WHEN p.StandardCost IS NOT NULL AND i.QuantityOnHand IS NOT NULL
        THEN i.QuantityOnHand * p.StandardCost
        ELSE NULL
    END as InventoryValueAtCost,
    
    CASE 
        WHEN p.CurrentPrice IS NOT NULL AND i.QuantityOnHand IS NOT NULL
        THEN i.QuantityOnHand * p.CurrentPrice
        ELSE NULL
    END as InventoryValueAtPrice,
    
    -- Potential Profit
    CASE 
        WHEN p.StandardCost IS NOT NULL AND p.CurrentPrice IS NOT NULL AND i.QuantityOnHand IS NOT NULL
        THEN i.QuantityOnHand * (p.CurrentPrice - p.StandardCost)
        ELSE NULL
    END as PotentialProfit,
    
    -- Margin Percentage
    CASE 
        WHEN p.StandardCost IS NOT NULL AND p.CurrentPrice IS NOT NULL AND p.StandardCost > 0
        THEN CAST(((p.CurrentPrice - p.StandardCost) * 100.0 / p.StandardCost) AS DECIMAL(5,2))
        ELSE NULL
    END as MarginPercentage,
    
    -- Movement Information (Last 30 days)
    ISNULL(recent_movements.TotalIncoming, 0) as IncomingQuantityLast30Days,
    ISNULL(recent_movements.TotalOutgoing, 0) as OutgoingQuantityLast30Days,
    ISNULL(recent_movements.NetMovement, 0) as NetMovementLast30Days,
    ISNULL(recent_movements.MovementCount, 0) as MovementCountLast30Days,
    
    -- Turnover Metrics
    CASE 
        WHEN i.QuantityOnHand > 0 AND recent_movements.TotalOutgoing > 0
        THEN CAST(recent_movements.TotalOutgoing * 30.0 / NULLIF(i.QuantityOnHand, 0) AS DECIMAL(10,2))
        ELSE NULL
    END as TurnoverRateAnnualized,
    
    -- Days of Stock
    CASE 
        WHEN recent_movements.AvgDailyOutgoing > 0
        THEN CAST(i.QuantityOnHand / recent_movements.AvgDailyOutgoing AS INT)
        ELSE NULL
    END as DaysOfStock,
    
    -- Supplier Information
    s.CompanyName as SupplierName,
    s.SupplierCode,
    s.LeadTimeDays as SupplierLeadTime,
    
    -- Last Transaction Information
    i.LastUpdated as LastInventoryUpdate,
    last_movement.LastMovementDate,
    last_movement.LastMovementType,
    last_movement.LastMovementQuantity,
    
    -- Gold Rate Information (for pricing)
    current_rate.RatePerGram as CurrentGoldRate,
    current_rate.EffectiveFrom as RateEffectiveFrom,
    
    -- Alerts and Flags
    CASE WHEN i.QuantityOnHand <= 0 THEN 1 ELSE 0 END as IsOutOfStock,
    CASE WHEN i.QuantityOnHand <= i.ReorderPoint THEN 1 ELSE 0 END as RequiresReorder,
    CASE WHEN i.QuantityOnHand <= i.MinimumStockLevel THEN 1 ELSE 0 END as IsLowStock,
    CASE WHEN i.QuantityOnHand >= i.MaximumStockLevel THEN 1 ELSE 0 END as IsOverstock,
    CASE WHEN last_movement.LastMovementDate < DATEADD(DAY, -30, GETDATE()) THEN 1 ELSE 0 END as IsSlowMoving,
    
    -- ABC Analysis Category (based on value and movement)
    CASE 
        WHEN i.QuantityOnHand * ISNULL(p.CurrentPrice, p.StandardCost) > 10000 
             AND recent_movements.TotalOutgoing > 5 THEN 'A'
        WHEN i.QuantityOnHand * ISNULL(p.CurrentPrice, p.StandardCost) > 5000 
             AND recent_movements.TotalOutgoing > 2 THEN 'B'
        ELSE 'C'
    END as ABCCategory

FROM [Inventory].[Inventories] i
INNER JOIN [Product].[Products] p ON i.ProductId = p.Id
INNER JOIN [Core].[Branches] b ON i.BranchId = b.Id
INNER JOIN [Lookup].[ProductCategoryTypes] pct ON p.CategoryTypeId = pct.Id
INNER JOIN [Lookup].[KaratTypes] kt ON p.KaratTypeId = kt.Id
LEFT JOIN [Lookup].[SubCategories] sc ON p.SubCategoryId = sc.Id
LEFT JOIN [Supplier].[Suppliers] s ON p.SupplierId = s.Id

-- Recent movements subquery
LEFT JOIN (
    SELECT 
        im.InventoryId,
        SUM(CASE WHEN im.QuantityChange > 0 THEN im.QuantityChange ELSE 0 END) as TotalIncoming,
        SUM(CASE WHEN im.QuantityChange < 0 THEN ABS(im.QuantityChange) ELSE 0 END) as TotalOutgoing,
        SUM(im.QuantityChange) as NetMovement,
        COUNT(*) as MovementCount,
        AVG(CASE WHEN im.QuantityChange < 0 THEN ABS(im.QuantityChange) ELSE NULL END) as AvgDailyOutgoing
    FROM [Inventory].[InventoryMovements] im
    WHERE im.MovementDate >= DATEADD(DAY, -30, GETDATE())
          AND im.IsActive = 1
    GROUP BY im.InventoryId
) recent_movements ON i.Id = recent_movements.InventoryId

-- Last movement subquery
LEFT JOIN (
    SELECT 
        im.InventoryId,
        im.MovementDate as LastMovementDate,
        im.MovementType as LastMovementType,
        im.QuantityChange as LastMovementQuantity,
        ROW_NUMBER() OVER (PARTITION BY im.InventoryId ORDER BY im.MovementDate DESC) as rn
    FROM [Inventory].[InventoryMovements] im
    WHERE im.IsActive = 1
) last_movement ON i.Id = last_movement.InventoryId AND last_movement.rn = 1

-- Current gold rate
LEFT JOIN (
    SELECT 
        gr.KaratTypeId,
        gr.RatePerGram,
        gr.EffectiveFrom,
        ROW_NUMBER() OVER (PARTITION BY gr.KaratTypeId ORDER BY gr.EffectiveFrom DESC) as rn
    FROM [Product].[GoldRates] gr
    WHERE gr.IsCurrent = 1 AND gr.IsActive = 1
) current_rate ON p.KaratTypeId = current_rate.KaratTypeId AND current_rate.rn = 1

WHERE 
    i.IsActive = 1
    AND p.IsActive = 1
    AND b.IsActive = 1
    AND pct.IsActive = 1
    AND kt.IsActive = 1;

-- Create indexes for optimal performance
CREATE NONCLUSTERED INDEX IX_Inventory_StockStatus_Product_Branch 
ON [Inventory].[Inventories] (ProductId, BranchId) 
INCLUDE (QuantityOnHand, WeightOnHand, MinimumStockLevel, MaximumStockLevel, ReorderPoint, LastUpdated)
WHERE IsActive = 1;

CREATE NONCLUSTERED INDEX IX_Inventory_StockStatus_Movements_Date 
ON [Inventory].[InventoryMovements] (InventoryId, MovementDate DESC) 
INCLUDE (QuantityChange, MovementType)
WHERE IsActive = 1;
