-- Add CashDrawerBalance table to existing database
-- Run this script manually if migrations are not working

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CashDrawerBalances]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[CashDrawerBalances](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [BranchId] [int] NOT NULL,
        [BalanceDate] [datetime2](7) NOT NULL,
        [OpeningBalance] [decimal](18, 2) NOT NULL,
        [ExpectedClosingBalance] [decimal](18, 2) NOT NULL,
        [ActualClosingBalance] [decimal](18, 2) NULL,
        [OpenedByUserId] [nvarchar](450) NULL,
        [ClosedByUserId] [nvarchar](450) NULL,
        [OpenedAt] [datetime2](7) NULL,
        [ClosedAt] [datetime2](7) NULL,
        [Notes] [nvarchar](500) NULL,
        [Status] [int] NOT NULL DEFAULT(1),
        [CreatedAt] [datetime2](7) NOT NULL,
        [CreatedBy] [nvarchar](450) NULL,
        [ModifiedAt] [datetime2](7) NULL,
        [ModifiedBy] [nvarchar](450) NULL,
        [IsActive] [bit] NOT NULL DEFAULT(1),
        CONSTRAINT [PK_CashDrawerBalances] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END

-- Add indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CashDrawerBalances]') AND name = N'IX_CashDrawerBalances_BranchId_BalanceDate')
BEGIN
    CREATE UNIQUE INDEX [IX_CashDrawerBalances_BranchId_BalanceDate] ON [dbo].[CashDrawerBalances]([BranchId], [BalanceDate])
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CashDrawerBalances]') AND name = N'IX_CashDrawerBalances_BalanceDate')
BEGIN
    CREATE INDEX [IX_CashDrawerBalances_BalanceDate] ON [dbo].[CashDrawerBalances]([BalanceDate])
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CashDrawerBalances]') AND name = N'IX_CashDrawerBalances_Status')
BEGIN
    CREATE INDEX [IX_CashDrawerBalances_Status] ON [dbo].[CashDrawerBalances]([Status])
END

-- Add foreign key constraint
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CashDrawerBalances_Branches_BranchId]') AND parent_object_id = OBJECT_ID(N'[dbo].[CashDrawerBalances]'))
BEGIN
    ALTER TABLE [dbo].[CashDrawerBalances] WITH CHECK ADD CONSTRAINT [FK_CashDrawerBalances_Branches_BranchId] FOREIGN KEY([BranchId])
    REFERENCES [dbo].[Branches] ([Id])
    ON DELETE CASCADE
END

-- Add check constraint for Status values
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[dbo].[CK_CashDrawerBalances_Status]') AND parent_object_id = OBJECT_ID(N'[dbo].[CashDrawerBalances]'))
BEGIN
    ALTER TABLE [dbo].[CashDrawerBalances] WITH CHECK ADD CONSTRAINT [CK_CashDrawerBalances_Status] CHECK (([Status]=(3) OR [Status]=(2) OR [Status]=(1)))
END

PRINT 'CashDrawerBalances table created successfully!'
