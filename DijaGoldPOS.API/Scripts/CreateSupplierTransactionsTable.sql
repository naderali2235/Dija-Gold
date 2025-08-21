-- Create SupplierTransactions table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SupplierTransactions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SupplierTransactions](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [TransactionNumber] [nvarchar](50) NOT NULL,
        [SupplierId] [int] NOT NULL,
        [TransactionDate] [datetime2](7) NOT NULL,
        [TransactionType] [nvarchar](50) NOT NULL,
        [Amount] [decimal](18,2) NOT NULL,
        [BalanceAfterTransaction] [decimal](18,2) NOT NULL,
        [ReferenceNumber] [nvarchar](100) NULL,
        [Notes] [nvarchar](1000) NULL,
        [CreatedByUserId] [nvarchar](450) NOT NULL,
        [BranchId] [int] NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [ModifiedAt] [datetime2](7) NULL,
        [CreatedBy] [nvarchar](450) NOT NULL,
        [ModifiedBy] [nvarchar](450) NULL,
        [IsActive] [bit] NOT NULL,
        CONSTRAINT [PK_SupplierTransactions] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END

-- Create unique index on TransactionNumber
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SupplierTransactions]') AND name = N'IX_SupplierTransactions_TransactionNumber')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_SupplierTransactions_TransactionNumber] ON [dbo].[SupplierTransactions]
    (
        [TransactionNumber] ASC
    )
END

-- Create foreign key constraint for SupplierId
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_SupplierTransactions_Suppliers_SupplierId]') AND parent_object_id = OBJECT_ID(N'[dbo].[SupplierTransactions]'))
BEGIN
    ALTER TABLE [dbo].[SupplierTransactions] WITH CHECK ADD CONSTRAINT [FK_SupplierTransactions_Suppliers_SupplierId] 
    FOREIGN KEY([SupplierId]) REFERENCES [dbo].[Suppliers] ([Id])
END

-- Create foreign key constraint for BranchId
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_SupplierTransactions_Branches_BranchId]') AND parent_object_id = OBJECT_ID(N'[dbo].[SupplierTransactions]'))
BEGIN
    ALTER TABLE [dbo].[SupplierTransactions] WITH CHECK ADD CONSTRAINT [FK_SupplierTransactions_Branches_BranchId] 
    FOREIGN KEY([BranchId]) REFERENCES [dbo].[Branches] ([Id])
END

-- Add migration record to __EFMigrationsHistory if not exists
IF NOT EXISTS (SELECT * FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = '20250820171742_InitialCreate')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250820171742_InitialCreate', '8.0.0')
END
