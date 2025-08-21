-- Add settlement fields to CashDrawerBalance table
-- This script adds new columns to track shift settlements and carry forward amounts

-- Check if columns don't exist before adding them
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CashDrawerBalances') AND name = 'SettledAmount')
BEGIN
    ALTER TABLE CashDrawerBalances 
    ADD SettledAmount decimal(18,2) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CashDrawerBalances') AND name = 'CarriedForwardAmount')
BEGIN
    ALTER TABLE CashDrawerBalances 
    ADD CarriedForwardAmount decimal(18,2) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CashDrawerBalances') AND name = 'SettlementNotes')
BEGIN
    ALTER TABLE CashDrawerBalances 
    ADD SettlementNotes nvarchar(500) NULL;
END

-- Create index on settlement fields for better query performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('CashDrawerBalances') AND name = 'IX_CashDrawerBalances_Settlement')
BEGIN
    CREATE INDEX IX_CashDrawerBalances_Settlement 
    ON CashDrawerBalances (BranchId, BalanceDate, SettledAmount, CarriedForwardAmount);
END

PRINT 'Settlement fields added to CashDrawerBalance table successfully!';
