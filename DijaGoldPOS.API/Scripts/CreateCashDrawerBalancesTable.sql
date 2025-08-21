-- Create CashDrawerBalances table
-- This script creates the CashDrawerBalances table with all required fields including settlement tracking

-- Check if table doesn't exist before creating it
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CashDrawerBalances' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[CashDrawerBalances] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [BranchId] int NOT NULL,
        [BalanceDate] datetime2(7) NOT NULL,
        [OpeningBalance] decimal(18,2) NOT NULL,
        [ExpectedClosingBalance] decimal(18,2) NOT NULL,
        [ActualClosingBalance] decimal(18,2) NULL,
        [OpenedByUserId] nvarchar(450) NULL,
        [ClosedByUserId] nvarchar(450) NULL,
        [OpenedAt] datetime2(7) NULL,
        [ClosedAt] datetime2(7) NULL,
        [Notes] nvarchar(500) NULL,
        [Status] int NOT NULL DEFAULT 1,
        [SettledAmount] decimal(18,2) NULL,
        [CarriedForwardAmount] decimal(18,2) NULL,
        [SettlementNotes] nvarchar(500) NULL,
        [CreatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [ModifiedAt] datetime2(7) NULL,
        [CreatedBy] nvarchar(450) NOT NULL DEFAULT 'System',
        [ModifiedBy] nvarchar(450) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        
        CONSTRAINT [PK_CashDrawerBalances] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CashDrawerBalances_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE CASCADE
    );

    -- Create indexes for better performance
    CREATE INDEX [IX_CashDrawerBalances_BranchId] ON [CashDrawerBalances] ([BranchId]);
    CREATE INDEX [IX_CashDrawerBalances_BalanceDate] ON [CashDrawerBalances] ([BalanceDate]);
    CREATE INDEX [IX_CashDrawerBalances_BranchId_BalanceDate] ON [CashDrawerBalances] ([BranchId], [BalanceDate]);
    CREATE INDEX [IX_CashDrawerBalances_Status] ON [CashDrawerBalances] ([Status]);
    CREATE INDEX [IX_CashDrawerBalances_Settlement] ON [CashDrawerBalances] ([BranchId], [BalanceDate], [SettledAmount], [CarriedForwardAmount]);

    PRINT 'CashDrawerBalances table created successfully with settlement fields!';
END
ELSE
BEGIN
    PRINT 'CashDrawerBalances table already exists. Checking for settlement fields...';
    
    -- Add settlement fields if they don't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CashDrawerBalances') AND name = 'SettledAmount')
    BEGIN
        ALTER TABLE [CashDrawerBalances] ADD [SettledAmount] decimal(18,2) NULL;
        PRINT 'Added SettledAmount field.';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CashDrawerBalances') AND name = 'CarriedForwardAmount')
    BEGIN
        ALTER TABLE [CashDrawerBalances] ADD [CarriedForwardAmount] decimal(18,2) NULL;
        PRINT 'Added CarriedForwardAmount field.';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CashDrawerBalances') AND name = 'SettlementNotes')
    BEGIN
        ALTER TABLE [CashDrawerBalances] ADD [SettlementNotes] nvarchar(500) NULL;
        PRINT 'Added SettlementNotes field.';
    END
    
    -- Create settlement index if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('CashDrawerBalances') AND name = 'IX_CashDrawerBalances_Settlement')
    BEGIN
        CREATE INDEX [IX_CashDrawerBalances_Settlement] ON [CashDrawerBalances] ([BranchId], [BalanceDate], [SettledAmount], [CarriedForwardAmount]);
        PRINT 'Created settlement index.';
    END
END
