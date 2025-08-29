using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Models.LookupTables;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DijaGoldPOS.API.Services;
using System.Linq.Expressions;

namespace DijaGoldPOS.API.Data;

/// <summary>
/// Main database context for the Dija Gold POS application
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    // Core business entities
    public DbSet<Branch> Branches { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Technician> Technicians { get; set; }

    // Pricing and configuration
    public DbSet<GoldRate> GoldRates { get; set; }
    public DbSet<MakingCharges> MakingCharges { get; set; }
    public DbSet<TaxConfiguration> TaxConfigurations { get; set; }

    // Orders and transactions
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<FinancialTransaction> FinancialTransactions { get; set; }

    // Inventory management
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<InventoryMovement> InventoryMovements { get; set; }

    // Purchasing system
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    public DbSet<SupplierTransaction> SupplierTransactions { get; set; }

    // Raw gold purchasing system
    public DbSet<RawGoldPurchaseOrder> RawGoldPurchaseOrders { get; set; }
    public DbSet<RawGoldPurchaseOrderItem> RawGoldPurchaseOrderItems { get; set; }
    public DbSet<RawGoldInventory> RawGoldInventories { get; set; }
    public DbSet<RawGoldInventoryMovement> RawGoldInventoryMovements { get; set; }

    // Product ownership tracking
    public DbSet<ProductOwnership> ProductOwnerships { get; set; }
    public DbSet<OwnershipMovement> OwnershipMovements { get; set; }

    // Customer purchases
    public DbSet<CustomerPurchase> CustomerPurchases { get; set; }
    public DbSet<CustomerPurchaseItem> CustomerPurchaseItems { get; set; }

    // Repair management
    public DbSet<RepairJob> RepairJobs { get; set; }

    // Cash management
    public DbSet<CashDrawerBalance> CashDrawerBalances { get; set; }

    // Manufacturing
    public DbSet<ProductManufacture> ProductManufactures { get; set; }
    public DbSet<ProductManufactureRawMaterial> ProductManufactureRawMaterials { get; set; }
    public DbSet<ManufacturingWorkflowHistory> ManufacturingWorkflowHistories { get; set; }

    // Audit trail
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    // Lookup Tables
    public DbSet<OrderTypeLookup> OrderTypeLookups { get; set; }
    public DbSet<OrderStatusLookup> OrderStatusLookups { get; set; }
    public DbSet<FinancialTransactionTypeLookup> FinancialTransactionTypeLookups { get; set; }
    public DbSet<FinancialTransactionStatusLookup> FinancialTransactionStatusLookups { get; set; }
    public DbSet<BusinessEntityTypeLookup> BusinessEntityTypeLookups { get; set; }
    public DbSet<RepairStatusLookup> RepairStatusLookups { get; set; }
    public DbSet<RepairPriorityLookup> RepairPriorityLookups { get; set; }
    public DbSet<KaratTypeLookup> KaratTypeLookups { get; set; }
    public DbSet<ProductCategoryTypeLookup> ProductCategoryTypeLookups { get; set; }
    public DbSet<TransactionTypeLookup> TransactionTypeLookups { get; set; }
    public DbSet<PaymentMethodLookup> PaymentMethodLookups { get; set; }
    public DbSet<TransactionStatusLookup> TransactionStatusLookups { get; set; }
    public DbSet<ChargeTypeLookup> ChargeTypeLookups { get; set; }
    public DbSet<SubCategoryLookup> SubCategoryLookups { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Identity and User relationships
        ConfigureUserRelationships(builder);

        // Configure core business entities
        ConfigureCoreBusinessEntities(builder);

        // Configure pricing and configuration entities
        ConfigurePricingAndConfiguration(builder);

        // Configure orders and transactions
        ConfigureOrdersAndTransactions(builder);

        // Configure inventory management
        ConfigureInventoryManagement(builder);

        // Configure purchasing system
        ConfigurePurchasingSystem(builder);

        // Configure raw gold purchasing system
        ConfigureRawGoldPurchasingSystem(builder);



        // Configure lookup tables
        ConfigureLookupTables(builder);

        // Configure audit and soft delete filters
        ConfigureAuditAndSoftDelete(builder);
    }

    /// <summary>
    /// Configure user and identity relationships
    /// </summary>
    private static void ConfigureUserRelationships(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(u => u.Branch)
                  .WithMany(b => b.Users)
                  .HasForeignKey(u => u.BranchId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    /// <summary>
    /// Configure core business entities (Branch, Product, Customer, Supplier, Technician)
    /// </summary>
    private static void ConfigureCoreBusinessEntities(ModelBuilder builder)
    {
        // Configure Branch
        builder.Entity<Branch>(entity =>
        {
            entity.HasIndex(b => b.Code).IsUnique();
            entity.Property(b => b.Code).IsRequired().HasMaxLength(20);
            entity.Property(b => b.Name).IsRequired().HasMaxLength(100);
            entity.Property(b => b.Address).HasMaxLength(500);
            entity.Property(b => b.Phone).HasMaxLength(20);
            entity.Property(b => b.ManagerName).HasMaxLength(100);

            // Explicitly configure navigation properties to avoid shadow properties

            // Primary users relationship
            entity.HasMany(b => b.Users)
                  .WithOne(u => u.Branch)
                  .HasForeignKey(u => u.BranchId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Financial transactions relationship
            entity.HasMany(b => b.FinancialTransactions)
                  .WithOne(ft => ft.Branch)
                  .HasForeignKey(ft => ft.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Orders relationship
            entity.HasMany(b => b.Orders)
                  .WithOne(o => o.Branch)
                  .HasForeignKey(o => o.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Cash drawer balances relationship
            entity.HasMany(b => b.CashDrawerBalances)
                  .WithOne(cdb => cdb.Branch)
                  .HasForeignKey(cdb => cdb.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Product ownerships relationship
            entity.HasMany(b => b.ProductOwnerships)
                  .WithOne(po => po.Branch)
                  .HasForeignKey(po => po.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);


        });

        // Configure Product
        builder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.ProductCode).IsUnique();
            entity.Property(p => p.ProductCode).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Weight).HasColumnType("decimal(10,3)");

            entity.HasOne(p => p.Supplier)
                  .WithMany(s => s.Products)
                  .HasForeignKey(p => p.SupplierId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Explicitly configure navigation properties to avoid shadow properties
            entity.HasMany(p => p.OrderItems)
                  .WithOne(oi => oi.Product)
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(p => p.InventoryRecords)
                  .WithOne(i => i.Product)
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(p => p.ManufacturingRecords)
                  .WithOne(pm => pm.Product)
                  .HasForeignKey(pm => pm.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Lookup table relationships
            entity.HasOne(p => p.CategoryType)
                  .WithMany()
                  .HasForeignKey(p => p.CategoryTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.KaratType)
                  .WithMany()
                  .HasForeignKey(p => p.KaratTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.SubCategoryLookup)
                  .WithMany()
                  .HasForeignKey(p => p.SubCategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Customer
        builder.Entity<Customer>(entity =>
        {
            entity.HasIndex(c => c.NationalId).IsUnique().HasFilter("[NationalId] IS NOT NULL");
            entity.HasIndex(c => c.MobileNumber).IsUnique().HasFilter("[MobileNumber] IS NOT NULL");
            entity.HasIndex(c => c.Email).IsUnique().HasFilter("[Email] IS NOT NULL");

            entity.Property(c => c.FullName).IsRequired().HasMaxLength(100);
            entity.Property(c => c.TotalPurchaseAmount).HasColumnType("decimal(18,2)");
            entity.Property(c => c.DefaultDiscountPercentage).HasColumnType("decimal(5,2)");

            // Explicitly configure navigation properties to avoid shadow properties
            entity.HasMany(c => c.Orders)
                  .WithOne(o => o.Customer)
                  .HasForeignKey(o => o.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Supplier
        builder.Entity<Supplier>(entity =>
        {
            entity.Property(s => s.CompanyName).IsRequired().HasMaxLength(200);
            entity.Property(s => s.CreditLimit).HasColumnType("decimal(18,2)");
            entity.Property(s => s.CurrentBalance).HasColumnType("decimal(18,2)");

            // Explicitly configure navigation properties to avoid shadow properties
            entity.HasMany(s => s.Products)
                  .WithOne(p => p.Supplier)
                  .HasForeignKey(p => p.SupplierId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Technician
        builder.Entity<Technician>(entity =>
        {
            entity.HasIndex(t => t.PhoneNumber).IsUnique().HasFilter("[PhoneNumber] IS NOT NULL");
            entity.HasIndex(t => t.Email).IsUnique().HasFilter("[Email] IS NOT NULL");

            entity.Property(t => t.FullName).IsRequired().HasMaxLength(100);
            entity.Property(t => t.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(t => t.Email).HasMaxLength(100);
            entity.Property(t => t.Specialization).HasMaxLength(500);

            entity.HasOne(t => t.Branch)
                  .WithMany()
                  .HasForeignKey(t => t.BranchId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    /// <summary>
    /// Configure pricing and configuration entities
    /// </summary>
    private static void ConfigurePricingAndConfiguration(ModelBuilder builder)
    {

        // Configure GoldRate
        builder.Entity<GoldRate>(entity =>
        {
            entity.HasIndex(gr => new { gr.KaratTypeId, gr.EffectiveFrom });
            entity.Property(gr => gr.RatePerGram).HasColumnType("decimal(18,2)");

            // Navigation properties
            entity.HasMany(gr => gr.Orders)
                  .WithOne()
                  .HasForeignKey(o => o.GoldRateId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Lookup table relationships
            entity.HasOne(gr => gr.KaratType)
                  .WithMany()
                  .HasForeignKey(gr => gr.KaratTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure MakingCharges
        builder.Entity<MakingCharges>(entity =>
        {
            entity.HasIndex(mc => new { mc.ProductCategoryId, mc.SubCategoryId, mc.EffectiveFrom });
            entity.Property(mc => mc.ChargeValue).HasColumnType("decimal(10,4)");

            // Lookup table relationships
            entity.HasOne(mc => mc.ProductCategory)
                  .WithMany()
                  .HasForeignKey(mc => mc.ProductCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(mc => mc.ChargeType)
                  .WithMany()
                  .HasForeignKey(mc => mc.ChargeTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(mc => mc.SubCategoryLookup)
                  .WithMany()
                  .HasForeignKey(mc => mc.SubCategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure TaxConfiguration
        builder.Entity<TaxConfiguration>(entity =>
        {
            entity.HasIndex(tc => tc.TaxCode).IsUnique();
            entity.Property(tc => tc.TaxCode).IsRequired().HasMaxLength(20);
            entity.Property(tc => tc.TaxRate).HasColumnType("decimal(10,4)");

            // Lookup table relationships
            entity.HasOne(tc => tc.TaxType)
                  .WithMany()
                  .HasForeignKey(tc => tc.TaxTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// Configure orders and transactions
    /// </summary>
    private static void ConfigureOrdersAndTransactions(ModelBuilder builder)
    {

    }

    /// <summary>
    /// Configure inventory management
    /// </summary>
    private static void ConfigureInventoryManagement(ModelBuilder builder)
    {
        // Configure Inventory
        builder.Entity<Inventory>(entity =>
        {
            entity.HasIndex(i => new { i.ProductId, i.BranchId }).IsUnique();

            entity.Property(i => i.QuantityOnHand).HasColumnType("decimal(10,3)");
            entity.Property(i => i.WeightOnHand).HasColumnType("decimal(10,3)");
            entity.Property(i => i.MinimumStockLevel).HasColumnType("decimal(10,3)");
            entity.Property(i => i.MaximumStockLevel).HasColumnType("decimal(10,3)");
            entity.Property(i => i.ReorderPoint).HasColumnType("decimal(10,3)");
            entity.Property(i => i.Notes).HasMaxLength(1000);

            // Explicitly configure relationships to avoid shadow properties
            entity.HasOne(i => i.Product)
                  .WithMany(p => p.InventoryRecords)
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Use the primary InventoryItems relationship from Branch
            entity.HasOne(i => i.Branch)
                  .WithMany(b => b.InventoryItems)
                  .HasForeignKey(i => i.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure InventoryMovement
        builder.Entity<InventoryMovement>(entity =>
        {
            entity.Property(im => im.QuantityChange).HasColumnType("decimal(10,3)");
            entity.Property(im => im.WeightChange).HasColumnType("decimal(10,3)");
            entity.Property(im => im.QuantityBalance).HasColumnType("decimal(10,3)");
            entity.Property(im => im.WeightBalance).HasColumnType("decimal(10,3)");
            entity.Property(im => im.UnitCost).HasColumnType("decimal(18,2)");

            entity.HasOne(im => im.Inventory)
                  .WithMany(i => i.InventoryMovements)
                  .HasForeignKey(im => im.InventoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// Configure purchasing system
    /// </summary>
    private static void ConfigurePurchasingSystem(ModelBuilder builder)
    {

        // Configure PurchaseOrder
        builder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasIndex(po => po.PurchaseOrderNumber).IsUnique();
            entity.Property(po => po.PurchaseOrderNumber).IsRequired().HasMaxLength(50);
            
            entity.Property(po => po.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(po => po.AmountPaid).HasColumnType("decimal(18,2)");
            entity.Property(po => po.OutstandingBalance).HasColumnType("decimal(18,2)");

            entity.HasOne(po => po.Supplier)
                  .WithMany(s => s.PurchaseOrders)
                  .HasForeignKey(po => po.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(po => po.Branch)
                  .WithMany()
                  .HasForeignKey(po => po.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure PurchaseOrderItem
        builder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.Property(poi => poi.QuantityOrdered).HasColumnType("decimal(10,3)");
            entity.Property(poi => poi.QuantityReceived).HasColumnType("decimal(10,3)");
            entity.Property(poi => poi.WeightOrdered).HasColumnType("decimal(10,3)");
            entity.Property(poi => poi.WeightReceived).HasColumnType("decimal(10,3)");
            entity.Property(poi => poi.UnitCost).HasColumnType("decimal(18,2)");
            entity.Property(poi => poi.LineTotal).HasColumnType("decimal(18,2)");

            entity.HasOne(poi => poi.PurchaseOrder)
                  .WithMany(po => po.PurchaseOrderItems)
                  .HasForeignKey(poi => poi.PurchaseOrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(poi => poi.Product)
                  .WithMany()
                  .HasForeignKey(poi => poi.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(poi => poi.Status).HasMaxLength(50);
        });

        // Configure CashDrawerBalance
        builder.Entity<CashDrawerBalance>(entity =>
        {
            entity.HasIndex(cdb => new { cdb.BranchId, cdb.BalanceDate }).IsUnique();
            entity.HasIndex(cdb => cdb.BalanceDate);
            entity.HasIndex(cdb => cdb.Status);

            entity.Property(cdb => cdb.OpeningBalance).HasColumnType("decimal(18,2)");
            entity.Property(cdb => cdb.ExpectedClosingBalance).HasColumnType("decimal(18,2)");
            entity.Property(cdb => cdb.ActualClosingBalance).HasColumnType("decimal(18,2)");
            entity.Property(cdb => cdb.CarriedForwardAmount).HasColumnType("decimal(18,2)");
            entity.Property(cdb => cdb.SettledAmount).HasColumnType("decimal(18,2)");
            entity.Property(cdb => cdb.Notes).HasMaxLength(1000);
            entity.Property(cdb => cdb.SettlementNotes).HasMaxLength(500);

            entity.HasOne(cdb => cdb.Branch)
                  .WithMany(b => b.CashDrawerBalances)
                  .HasForeignKey(cdb => cdb.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure RepairJob
        builder.Entity<RepairJob>(entity =>
        {
            entity.HasIndex(rj => rj.FinancialTransactionId).IsUnique().HasFilter("[FinancialTransactionId] IS NOT NULL");
            entity.HasIndex(rj => rj.StatusId);
            entity.HasIndex(rj => rj.PriorityId);
            entity.HasIndex(rj => rj.AssignedTechnicianId);
            entity.HasIndex(rj => rj.QualityCheckedBy);

            entity.Property(rj => rj.TechnicianNotes).HasMaxLength(2000);
            entity.Property(rj => rj.MaterialsUsed).HasMaxLength(1000);
            entity.Property(rj => rj.ActualCost).HasColumnType("decimal(18,2)");
            entity.Property(rj => rj.EstimatedCost).HasColumnType("decimal(18,2)");
            entity.Property(rj => rj.HoursSpent).HasColumnType("decimal(5,2)");
            entity.Property(rj => rj.Notes).HasMaxLength(2000);

            // Financial transaction relationship (one-to-one)
            entity.HasOne(rj => rj.FinancialTransaction)
                  .WithOne()
                  .HasForeignKey<RepairJob>(rj => rj.FinancialTransactionId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Explicitly configure the AssignedTechnician relationship
            entity.HasOne(rj => rj.AssignedTechnician)
                  .WithMany(t => t.RepairJobs)
                  .HasForeignKey(rj => rj.AssignedTechnicianId)
                  .OnDelete(DeleteBehavior.NoAction);

            // Explicitly configure the QualityChecker relationship
            entity.HasOne(rj => rj.QualityChecker)
                  .WithMany(t => t.QualityCheckedRepairJobs)
                  .HasForeignKey(rj => rj.QualityCheckedBy)
                  .OnDelete(DeleteBehavior.NoAction);

            // Lookup table relationships
            entity.HasOne(rj => rj.Status)
                  .WithMany()
                  .HasForeignKey(rj => rj.StatusId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(rj => rj.Priority)
                  .WithMany()
                  .HasForeignKey(rj => rj.PriorityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure FinancialTransaction
        builder.Entity<FinancialTransaction>(entity =>
        {
            entity.HasIndex(ft => new { ft.BranchId, ft.TransactionNumber }).IsUnique();
            entity.HasIndex(ft => ft.TransactionDate);
            entity.HasIndex(ft => ft.TransactionTypeId);
            entity.HasIndex(ft => ft.StatusId);
            entity.HasIndex(ft => ft.BusinessEntityId);
            entity.HasIndex(ft => ft.BusinessEntityTypeId);

            entity.Property(ft => ft.TransactionNumber).IsRequired().HasMaxLength(50);
            entity.Property(ft => ft.ProcessedByUserId).IsRequired().HasMaxLength(450);
            entity.Property(ft => ft.ApprovedByUserId).HasMaxLength(450);
            entity.Property(ft => ft.ReversalReason).HasMaxLength(500);
            entity.Property(ft => ft.Notes).HasMaxLength(2000);

            entity.Property(ft => ft.Subtotal).HasColumnType("decimal(18,2)");
            entity.Property(ft => ft.TotalTaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(ft => ft.TotalDiscountAmount).HasColumnType("decimal(18,2)");
            entity.Property(ft => ft.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(ft => ft.AmountPaid).HasColumnType("decimal(18,2)");
            entity.Property(ft => ft.ChangeGiven).HasColumnType("decimal(18,2)");

            // Explicitly configure relationships to avoid shadow properties
            entity.HasOne(ft => ft.Branch)
                  .WithMany(b => b.FinancialTransactions)
                  .HasForeignKey(ft => ft.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ft => ft.ProcessedByUser)
                  .WithMany(u => u.FinancialTransactions)
                  .HasForeignKey(ft => ft.ProcessedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ft => ft.ApprovedByUser)
                  .WithMany(u => u.ApprovedFinancialTransactions)
                  .HasForeignKey(ft => ft.ApprovedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(ft => ft.OriginalTransaction)
                  .WithMany(ft => ft.ReversalTransactions)
                  .HasForeignKey(ft => ft.OriginalTransactionId)
                  .OnDelete(DeleteBehavior.NoAction);
                  
            // Lookup table relationships
            entity.HasOne(ft => ft.TransactionType)
                  .WithMany()
                  .HasForeignKey(ft => ft.TransactionTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(ft => ft.BusinessEntityType)
                  .WithMany()
                  .HasForeignKey(ft => ft.BusinessEntityTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(ft => ft.PaymentMethod)
                  .WithMany()
                  .HasForeignKey(ft => ft.PaymentMethodId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(ft => ft.Status)
                  .WithMany()
                  .HasForeignKey(ft => ft.StatusId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Order
        builder.Entity<Order>(entity =>
        {
            entity.HasIndex(o => new { o.BranchId, o.OrderNumber }).IsUnique();
            entity.HasIndex(o => o.OrderDate);
            entity.HasIndex(o => o.OrderTypeId);
            entity.HasIndex(o => o.StatusId);
            entity.HasIndex(o => o.CustomerId);
            entity.HasIndex(o => o.CashierId);
            entity.HasIndex(o => o.ApprovedByUserId);

            entity.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(o => o.CashierId).IsRequired().HasMaxLength(450);
            entity.Property(o => o.ApprovedByUserId).HasMaxLength(450);
            entity.Property(o => o.ReturnReason).HasMaxLength(500);
            entity.Property(o => o.Notes).HasMaxLength(2000);

            // Relationships
            entity.HasOne(o => o.Branch)
                  .WithMany(b => b.Orders)
                  .HasForeignKey(o => o.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.Customer)
                  .WithMany(c => c.Orders)
                  .HasForeignKey(o => o.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Explicitly configure the Cashier relationship
            entity.HasOne(o => o.Cashier)
                  .WithMany(u => u.Orders)
                  .HasForeignKey(o => o.CashierId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Explicitly configure the ApprovedByUser relationship
            entity.HasOne(o => o.ApprovedByUser)
                  .WithMany(u => u.ApprovedOrders)
                  .HasForeignKey(o => o.ApprovedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(o => o.GoldRate)
                  .WithMany(gr => gr.Orders)
                  .HasForeignKey(o => o.GoldRateId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(o => o.FinancialTransaction)
                  .WithOne()
                  .HasForeignKey<Order>(o => o.FinancialTransactionId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(o => o.OriginalOrder)
                  .WithMany(o => o.RelatedOrders)
                  .HasForeignKey(o => o.OriginalOrderId)
                  .OnDelete(DeleteBehavior.NoAction);

            // Lookup table relationships
            entity.HasOne(o => o.OrderType)
                  .WithMany()
                  .HasForeignKey(o => o.OrderTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.Status)
                  .WithMany()
                  .HasForeignKey(o => o.StatusId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure OrderItem
        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(oi => oi.Quantity).HasColumnType("decimal(10,3)");
            entity.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(oi => oi.TotalPrice).HasColumnType("decimal(18,2)");
            entity.Property(oi => oi.DiscountPercentage).HasColumnType("decimal(5,2)");
            entity.Property(oi => oi.DiscountAmount).HasColumnType("decimal(18,2)");
            entity.Property(oi => oi.FinalPrice).HasColumnType("decimal(18,2)");
            entity.Property(oi => oi.MakingCharges).HasColumnType("decimal(18,2)");
            entity.Property(oi => oi.TaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(oi => oi.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(oi => oi.Notes).HasMaxLength(500);

            // Explicitly configure relationships to avoid shadow properties
            entity.HasOne(oi => oi.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.Product)
                  .WithMany(p => p.OrderItems)
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Technician
        builder.Entity<Technician>(entity =>
        {
            entity.HasIndex(t => t.PhoneNumber).IsUnique().HasFilter("[PhoneNumber] IS NOT NULL");
            entity.HasIndex(t => t.Email).IsUnique().HasFilter("[Email] IS NOT NULL");

            entity.Property(t => t.FullName).IsRequired().HasMaxLength(100);
            entity.Property(t => t.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(t => t.Email).HasMaxLength(100);
            entity.Property(t => t.Specialization).HasMaxLength(500);

            // Explicitly configure relationships to avoid shadow properties
            entity.HasOne(t => t.Branch)
                  .WithMany()
                  .HasForeignKey(t => t.BranchId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Configure assigned repair jobs relationship
            entity.HasMany(t => t.RepairJobs)
                  .WithOne(rj => rj.AssignedTechnician)
                  .HasForeignKey(rj => rj.AssignedTechnicianId)
                  .OnDelete(DeleteBehavior.NoAction);

            // Configure quality checked repair jobs relationship
            entity.HasMany(t => t.QualityCheckedRepairJobs)
                  .WithOne(rj => rj.QualityChecker)
                  .HasForeignKey(rj => rj.QualityCheckedBy)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure ProductOwnership
        builder.Entity<ProductOwnership>(entity =>
        {
            entity.HasIndex(po => new { po.ProductId, po.BranchId, po.SupplierId, po.PurchaseOrderId });
            entity.HasIndex(po => po.OwnershipPercentage);
            entity.HasIndex(po => po.OutstandingAmount);

            entity.Property(po => po.TotalQuantity).HasColumnType("decimal(10,3)");
            entity.Property(po => po.TotalWeight).HasColumnType("decimal(10,3)");
            entity.Property(po => po.OwnedQuantity).HasColumnType("decimal(10,3)");
            entity.Property(po => po.OwnedWeight).HasColumnType("decimal(10,3)");
            entity.Property(po => po.OwnershipPercentage).HasColumnType("decimal(5,4)");
            entity.Property(po => po.TotalCost).HasColumnType("decimal(18,2)");
            entity.Property(po => po.AmountPaid).HasColumnType("decimal(18,2)");
            entity.Property(po => po.OutstandingAmount).HasColumnType("decimal(18,2)");
            entity.Property(po => po.Notes).HasMaxLength(1000);

            // Explicitly configure relationships to avoid shadow properties
            entity.HasOne(po => po.Product)
                  .WithMany(p => p.ProductOwnerships)
                  .HasForeignKey(po => po.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Use the ProductOwnerships relationship from Branch
            entity.HasOne(po => po.Branch)
                  .WithMany(b => b.ProductOwnerships)
                  .HasForeignKey(po => po.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(po => po.Supplier)
                  .WithMany(s => s.ProductOwnerships)
                  .HasForeignKey(po => po.SupplierId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(po => po.PurchaseOrder)
                  .WithMany(po => po.ProductOwnerships)
                  .HasForeignKey(po => po.PurchaseOrderId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Note: CustomerPurchase relationship removed as customer purchases now represent gold amounts, not specific products
        });

        // Configure OwnershipMovement
        builder.Entity<OwnershipMovement>(entity =>
        {
            entity.HasIndex(om => om.MovementDate);
            entity.HasIndex(om => om.MovementType);
            entity.HasIndex(om => om.ReferenceNumber);
            
            entity.Property(om => om.MovementType).IsRequired().HasMaxLength(50);
            entity.Property(om => om.ReferenceNumber).HasMaxLength(100);
            entity.Property(om => om.QuantityChange).HasColumnType("decimal(10,3)");
            entity.Property(om => om.WeightChange).HasColumnType("decimal(10,3)");
            entity.Property(om => om.AmountChange).HasColumnType("decimal(18,2)");
            entity.Property(om => om.OwnedQuantityAfter).HasColumnType("decimal(10,3)");
            entity.Property(om => om.OwnedWeightAfter).HasColumnType("decimal(10,3)");
            entity.Property(om => om.AmountPaidAfter).HasColumnType("decimal(18,2)");
            entity.Property(om => om.OwnershipPercentageAfter).HasColumnType("decimal(5,4)");
            entity.Property(om => om.Notes).HasMaxLength(500);
            entity.Property(om => om.CreatedByUserId).IsRequired().HasMaxLength(450);

            // Relationships
            entity.HasOne(om => om.ProductOwnership)
                  .WithMany(po => po.OwnershipMovements)
                  .HasForeignKey(om => om.ProductOwnershipId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure CustomerPurchase
        builder.Entity<CustomerPurchase>(entity =>
        {
            entity.HasIndex(cp => cp.PurchaseNumber).IsUnique();
            entity.HasIndex(cp => cp.CustomerId);
            entity.HasIndex(cp => cp.BranchId);
            entity.HasIndex(cp => cp.PurchaseDate);
            entity.HasIndex(cp => cp.CreatedByUserId);

            entity.Property(cp => cp.PurchaseNumber).IsRequired().HasMaxLength(50);
            entity.Property(cp => cp.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(cp => cp.AmountPaid).HasColumnType("decimal(18,2)");
            entity.Property(cp => cp.Notes).HasMaxLength(1000);
            entity.Property(cp => cp.CreatedByUserId).IsRequired().HasMaxLength(450);

            // Explicitly configure relationships to avoid shadow properties
            entity.HasOne(cp => cp.Customer)
                  .WithMany(c => c.CustomerPurchases)
                  .HasForeignKey(cp => cp.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(cp => cp.Branch)
                  .WithMany(b => b.CustomerPurchases)
                  .HasForeignKey(cp => cp.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(cp => cp.PaymentMethod)
                  .WithMany()
                  .HasForeignKey(cp => cp.PaymentMethodId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Customer purchase items relationship
            entity.HasMany(cp => cp.CustomerPurchaseItems)
                  .WithOne(cpi => cpi.CustomerPurchase)
                  .HasForeignKey(cpi => cpi.CustomerPurchaseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure CustomerPurchaseItem
        builder.Entity<CustomerPurchaseItem>(entity =>
        {
            entity.Property(cpi => cpi.Weight).HasColumnType("decimal(10,3)");
            entity.Property(cpi => cpi.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(cpi => cpi.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(cpi => cpi.Notes).HasMaxLength(500);

            // Relationships
            entity.HasOne(cpi => cpi.CustomerPurchase)
                  .WithMany(cp => cp.CustomerPurchaseItems)
                  .HasForeignKey(cpi => cpi.CustomerPurchaseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(cpi => cpi.KaratType)
                  .WithMany()
                  .HasForeignKey(cpi => cpi.KaratTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Lookup Tables
        builder.Entity<OrderTypeLookup>(entity =>
        {
            entity.HasIndex(ot => ot.Name).IsUnique();
            entity.Property(ot => ot.Name).IsRequired().HasMaxLength(50);
            entity.Property(ot => ot.Description).HasMaxLength(200);
        });

        builder.Entity<OrderStatusLookup>(entity =>
        {
            entity.HasIndex(os => os.Name).IsUnique();
            entity.Property(os => os.Name).IsRequired().HasMaxLength(50);
            entity.Property(os => os.Description).HasMaxLength(200);
        });

        builder.Entity<FinancialTransactionTypeLookup>(entity =>
        {
            entity.HasIndex(ftt => ftt.Name).IsUnique();
            entity.Property(ftt => ftt.Name).IsRequired().HasMaxLength(50);
            entity.Property(ftt => ftt.Description).HasMaxLength(200);
        });

        builder.Entity<FinancialTransactionStatusLookup>(entity =>
        {
            entity.HasIndex(fts => fts.Name).IsUnique();
            entity.Property(fts => fts.Name).IsRequired().HasMaxLength(50);
            entity.Property(fts => fts.Description).HasMaxLength(200);
        });

        builder.Entity<BusinessEntityTypeLookup>(entity =>
        {
            entity.HasIndex(bet => bet.Name).IsUnique();
            entity.Property(bet => bet.Name).IsRequired().HasMaxLength(50);
            entity.Property(bet => bet.Description).HasMaxLength(200);
        });

        builder.Entity<RepairStatusLookup>(entity =>
        {
            entity.HasIndex(rs => rs.Name).IsUnique();
            entity.Property(rs => rs.Name).IsRequired().HasMaxLength(50);
            entity.Property(rs => rs.Description).HasMaxLength(200);
        });

        builder.Entity<RepairPriorityLookup>(entity =>
        {
            entity.HasIndex(rp => rp.Name).IsUnique();
            entity.Property(rp => rp.Name).IsRequired().HasMaxLength(50);
            entity.Property(rp => rp.Description).HasMaxLength(200);
        });

        builder.Entity<KaratTypeLookup>(entity =>
        {
            entity.HasIndex(kt => kt.Name).IsUnique();
            entity.Property(kt => kt.Name).IsRequired().HasMaxLength(50);
            entity.Property(kt => kt.Description).HasMaxLength(200);
        });

        builder.Entity<ProductCategoryTypeLookup>(entity =>
        {
            entity.HasIndex(pct => pct.Name).IsUnique();
            entity.Property(pct => pct.Name).IsRequired().HasMaxLength(50);
            entity.Property(pct => pct.Description).HasMaxLength(200);
        });

        builder.Entity<TransactionTypeLookup>(entity =>
        {
            entity.HasIndex(tt => tt.Name).IsUnique();
            entity.Property(tt => tt.Name).IsRequired().HasMaxLength(50);
            entity.Property(tt => tt.Description).HasMaxLength(200);
        });

        builder.Entity<PaymentMethodLookup>(entity =>
        {
            entity.HasIndex(pm => pm.Name).IsUnique();
            entity.Property(pm => pm.Name).IsRequired().HasMaxLength(50);
            entity.Property(pm => pm.Description).HasMaxLength(200);
        });

        builder.Entity<TransactionStatusLookup>(entity =>
        {
            entity.HasIndex(ts => ts.Name).IsUnique();
            entity.Property(ts => ts.Name).IsRequired().HasMaxLength(50);
            entity.Property(ts => ts.Description).HasMaxLength(200);
        });

        builder.Entity<ChargeTypeLookup>(entity =>
        {
            entity.HasIndex(ct => ct.Name).IsUnique();
            entity.Property(ct => ct.Name).IsRequired().HasMaxLength(50);
            entity.Property(ct => ct.Description).HasMaxLength(200);
        });

        builder.Entity<SubCategoryLookup>(entity =>
        {
            entity.HasIndex(sc => sc.Name).IsUnique();
            entity.Property(sc => sc.Name).IsRequired().HasMaxLength(50);
            entity.Property(sc => sc.Description).HasMaxLength(200);
            
            // Relationship to product category type
            entity.HasOne(sc => sc.CategoryType)
                  .WithMany()
                  .HasForeignKey(sc => sc.CategoryTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ProductManufacture entity
        builder.Entity<ProductManufacture>(entity =>
        {
            // Explicitly configure primary key
            entity.HasKey(pm => pm.Id);

            // Ignore automatic relationship discovery to prevent shadow properties
            entity.Ignore("PurchaseOrderItemId1");

            // Explicitly configure relationships to avoid shadow properties
            entity.HasOne(pm => pm.Product)
                  .WithMany(p => p.ManufacturingRecords)
                  .HasForeignKey(pm => pm.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Primary relationship: Raw gold purchase order item as source material
            entity.HasOne(pm => pm.SourceRawGoldPurchaseOrderItem)
                  .WithMany(rgpoi => rgpoi.SourceManufacturingRecords)
                  .HasForeignKey(pm => pm.SourceRawGoldPurchaseOrderItemId)
                  .HasConstraintName("FK_ProductManufacture_RawGoldPurchaseOrderItem")
                  .OnDelete(DeleteBehavior.Restrict);

            // Optional relationship: Regular purchase order item as additional material
            entity.HasOne(pm => pm.PurchaseOrderItem)
                  .WithMany(poi => poi.AdditionalManufacturingRecords)
                  .HasForeignKey(pm => pm.PurchaseOrderItemId)
                  .HasConstraintName("FK_ProductManufacture_PurchaseOrderItem_Additional")
                  .OnDelete(DeleteBehavior.SetNull)
                  .IsRequired(false);

            entity.HasOne(pm => pm.Branch)
                  .WithMany()
                  .HasForeignKey(pm => pm.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(pm => pm.Technician)
                  .WithMany()
                  .HasForeignKey(pm => pm.TechnicianId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(pm => pm.ProductId);
            entity.HasIndex(pm => pm.SourceRawGoldPurchaseOrderItemId);
            entity.HasIndex(pm => pm.PurchaseOrderItemId);
            entity.HasIndex(pm => pm.BatchNumber);
            entity.HasIndex(pm => pm.ManufactureDate);
            entity.HasIndex(pm => pm.BranchId);
            entity.HasIndex(pm => pm.TechnicianId);

            // Properties
            entity.Property(pm => pm.QuantityProduced).IsRequired().HasDefaultValueSql("0");
            entity.Property(pm => pm.ConsumedWeight).HasColumnType("decimal(10,3)").HasDefaultValueSql("0");
            entity.Property(pm => pm.WastageWeight).HasColumnType("decimal(10,3)").HasDefaultValueSql("0");
            entity.Property(pm => pm.ManufacturingCostPerGram).HasColumnType("decimal(18,2)").HasDefaultValueSql("0");
            entity.Property(pm => pm.TotalManufacturingCost).HasColumnType("decimal(18,2)").HasDefaultValueSql("0");
            entity.Property(pm => pm.BatchNumber).HasMaxLength(50);
            entity.Property(pm => pm.ManufacturingNotes).HasMaxLength(1000);
            entity.Property(pm => pm.Status).HasMaxLength(20);
            
            // Explicitly configure foreign key properties to prevent shadow properties
            entity.Property(pm => pm.SourceRawGoldPurchaseOrderItemId).IsRequired();
            entity.Property(pm => pm.PurchaseOrderItemId).IsRequired(false);
        });

        // Configure ManufacturingWorkflowHistory entity
        builder.Entity<ManufacturingWorkflowHistory>(entity =>
        {
            // Relationships
            entity.HasOne(mwh => mwh.ProductManufacture)
                  .WithMany(pm => pm.WorkflowHistory)
                  .HasForeignKey(mwh => mwh.ProductManufactureId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(mwh => mwh.ProductManufactureId);
            entity.HasIndex(mwh => mwh.ActionByUserId);
            entity.HasIndex(mwh => new { mwh.ProductManufactureId, mwh.CreatedAt });

            // Properties
            entity.Property(mwh => mwh.FromStatus).HasMaxLength(30);
            entity.Property(mwh => mwh.ToStatus).HasMaxLength(30);
            entity.Property(mwh => mwh.Action).HasMaxLength(50);
            entity.Property(mwh => mwh.ActionByUserId).HasMaxLength(450);
            entity.Property(mwh => mwh.ActionByUserName).HasMaxLength(100);
            entity.Property(mwh => mwh.Notes).HasMaxLength(1000);
        });

        // Configure soft delete filter for BaseEntity
        builder.Entity<Branch>().HasQueryFilter(e => e.IsActive);
        builder.Entity<Product>().HasQueryFilter(e => e.IsActive);
        builder.Entity<Customer>().HasQueryFilter(e => e.IsActive);
        builder.Entity<Supplier>().HasQueryFilter(e => e.IsActive);
        builder.Entity<GoldRate>().HasQueryFilter(e => e.IsActive);
        builder.Entity<MakingCharges>().HasQueryFilter(e => e.IsActive);
        builder.Entity<TaxConfiguration>().HasQueryFilter(e => e.IsActive);
        // Obsolete Transaction models removed from query filters
        builder.Entity<Inventory>().HasQueryFilter(e => e.IsActive);
        builder.Entity<InventoryMovement>().HasQueryFilter(e => e.IsActive);
        builder.Entity<PurchaseOrder>().HasQueryFilter(e => e.IsActive);
        builder.Entity<PurchaseOrderItem>().HasQueryFilter(e => e.IsActive);
        builder.Entity<RawGoldPurchaseOrder>().HasQueryFilter(e => e.IsActive);
        builder.Entity<RawGoldPurchaseOrderItem>().HasQueryFilter(e => e.IsActive);
        builder.Entity<RawGoldInventory>().HasQueryFilter(e => e.IsActive);
        builder.Entity<RawGoldInventoryMovement>().HasQueryFilter(e => e.IsActive);
        builder.Entity<CashDrawerBalance>().HasQueryFilter(e => e.IsActive);
        builder.Entity<RepairJob>().HasQueryFilter(e => e.IsActive);
        builder.Entity<Technician>().HasQueryFilter(e => e.IsActive);
        builder.Entity<FinancialTransaction>().HasQueryFilter(e => e.IsActive);
        builder.Entity<Order>().HasQueryFilter(e => e.IsActive);
        builder.Entity<OrderItem>().HasQueryFilter(e => e.IsActive);
        builder.Entity<ProductOwnership>().HasQueryFilter(e => e.IsActive);
        builder.Entity<OwnershipMovement>().HasQueryFilter(e => e.IsActive);
        builder.Entity<CustomerPurchase>().HasQueryFilter(e => e.IsActive);
        builder.Entity<CustomerPurchaseItem>().HasQueryFilter(e => e.IsActive);
        builder.Entity<ProductManufacture>().HasQueryFilter(e => e.IsActive);
        builder.Entity<ManufacturingWorkflowHistory>().HasQueryFilter(e => e.IsActive);
        
        // Configure soft delete filter for Lookup Tables
        builder.Entity<OrderTypeLookup>().HasQueryFilter(e => e.IsActive);
        builder.Entity<OrderStatusLookup>().HasQueryFilter(e => e.IsActive);
        builder.Entity<FinancialTransactionTypeLookup>().HasQueryFilter(e => e.IsActive);
        builder.Entity<FinancialTransactionStatusLookup>().HasQueryFilter(e => e.IsActive);
        builder.Entity<BusinessEntityTypeLookup>().HasQueryFilter(e => e.IsActive);
        builder.Entity<RepairStatusLookup>().HasQueryFilter(e => e.IsActive);
        builder.Entity<RepairPriorityLookup>().HasQueryFilter(e => e.IsActive);
        builder.Entity<KaratTypeLookup>().HasQueryFilter(e => e.IsActive);
        builder.Entity<ProductCategoryTypeLookup>().HasQueryFilter(e => e.IsActive);
        builder.Entity<TransactionTypeLookup>().HasQueryFilter(e => e.IsActive);
        builder.Entity<PaymentMethodLookup>().HasQueryFilter(e => e.IsActive);
        builder.Entity<TransactionStatusLookup>().HasQueryFilter(e => e.IsActive);
        builder.Entity<ChargeTypeLookup>().HasQueryFilter(e => e.IsActive);
        builder.Entity<SubCategoryLookup>().HasQueryFilter(e => e.IsActive);
    }

    /// <summary>
    /// Configure raw gold purchasing system
    /// </summary>
    private static void ConfigureRawGoldPurchasingSystem(ModelBuilder builder)
    {
        // Configure RawGoldPurchaseOrder
        builder.Entity<RawGoldPurchaseOrder>(entity =>
        {
            entity.HasIndex(rgpo => rgpo.PurchaseOrderNumber).IsUnique();
            entity.Property(rgpo => rgpo.PurchaseOrderNumber).IsRequired().HasMaxLength(50);
            
            entity.Property(rgpo => rgpo.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(rgpo => rgpo.AmountPaid).HasColumnType("decimal(18,2)");
            entity.Property(rgpo => rgpo.OutstandingBalance).HasColumnType("decimal(18,2)");
            entity.Property(rgpo => rgpo.Notes).HasMaxLength(1000);

            entity.HasOne(rgpo => rgpo.Supplier)
                  .WithMany(s => s.RawGoldPurchaseOrders)
                  .HasForeignKey(rgpo => rgpo.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(rgpo => rgpo.Branch)
                  .WithMany()
                  .HasForeignKey(rgpo => rgpo.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure RawGoldPurchaseOrderItem
        builder.Entity<RawGoldPurchaseOrderItem>(entity =>
        {
            entity.Property(rgpoi => rgpoi.WeightOrdered).HasColumnType("decimal(10,3)");
            entity.Property(rgpoi => rgpoi.WeightReceived).HasColumnType("decimal(10,3)");
            entity.Property(rgpoi => rgpoi.UnitCostPerGram).HasColumnType("decimal(18,2)");
            entity.Property(rgpoi => rgpoi.LineTotal).HasColumnType("decimal(18,2)");
            entity.Property(rgpoi => rgpoi.WeightConsumedInManufacturing).HasColumnType("decimal(10,3)");
            entity.Property(rgpoi => rgpoi.AvailableWeightForManufacturing).HasColumnType("decimal(10,3)");
            entity.Property(rgpoi => rgpoi.Description).HasMaxLength(500);
            entity.Property(rgpoi => rgpoi.Notes).HasMaxLength(1000);
            entity.Property(rgpoi => rgpoi.Status).HasMaxLength(50);

            entity.HasOne(rgpoi => rgpoi.RawGoldPurchaseOrder)
                  .WithMany(rgpo => rgpo.RawGoldPurchaseOrderItems)
                  .HasForeignKey(rgpoi => rgpoi.RawGoldPurchaseOrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rgpoi => rgpoi.KaratType)
                  .WithMany()
                  .HasForeignKey(rgpoi => rgpoi.KaratTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ProductManufactureRawMaterial
        builder.Entity<ProductManufactureRawMaterial>(entity =>
        {
            entity.Property(pmrm => pmrm.ConsumedWeight).HasColumnType("decimal(10,3)");
            entity.Property(pmrm => pmrm.WastageWeight).HasColumnType("decimal(10,3)");
            entity.Property(pmrm => pmrm.CostPerGram).HasColumnType("decimal(18,2)");
            entity.Property(pmrm => pmrm.TotalRawMaterialCost).HasColumnType("decimal(18,2)");
            entity.Property(pmrm => pmrm.ContributionPercentage).HasColumnType("decimal(5,4)");
            entity.Property(pmrm => pmrm.Notes).HasMaxLength(1000);

            entity.HasOne(pmrm => pmrm.ProductManufacture)
                  .WithMany(pm => pm.RawMaterials)
                  .HasForeignKey(pmrm => pmrm.ProductManufactureId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pmrm => pmrm.RawGoldPurchaseOrderItem)
                  .WithMany()
                  .HasForeignKey(pmrm => pmrm.RawGoldPurchaseOrderItemId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure RawGoldInventory
        builder.Entity<RawGoldInventory>(entity =>
        {
            entity.HasIndex(rgi => new { rgi.KaratTypeId, rgi.BranchId }).IsUnique();

            entity.Property(rgi => rgi.WeightOnHand).HasColumnType("decimal(10,3)");
            entity.Property(rgi => rgi.WeightReserved).HasColumnType("decimal(10,3)");
            entity.Property(rgi => rgi.WeightAvailable).HasColumnType("decimal(10,3)");
            entity.Property(rgi => rgi.AverageCostPerGram).HasColumnType("decimal(18,2)");
            entity.Property(rgi => rgi.TotalValue).HasColumnType("decimal(18,2)");
            entity.Property(rgi => rgi.MinimumStockLevel).HasColumnType("decimal(10,3)");
            entity.Property(rgi => rgi.ReorderPoint).HasColumnType("decimal(10,3)");
            entity.Property(rgi => rgi.Notes).HasMaxLength(1000);

            entity.HasOne(rgi => rgi.KaratType)
                  .WithMany()
                  .HasForeignKey(rgi => rgi.KaratTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(rgi => rgi.Branch)
                  .WithMany()
                  .HasForeignKey(rgi => rgi.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure RawGoldInventoryMovement
        builder.Entity<RawGoldInventoryMovement>(entity =>
        {
            entity.Property(rgim => rgim.WeightChange).HasColumnType("decimal(10,3)");
            entity.Property(rgim => rgim.WeightBalance).HasColumnType("decimal(10,3)");
            entity.Property(rgim => rgim.UnitCostPerGram).HasColumnType("decimal(18,2)");
            entity.Property(rgim => rgim.TotalCost).HasColumnType("decimal(18,2)");
            entity.Property(rgim => rgim.MovementType).IsRequired().HasMaxLength(50);
            entity.Property(rgim => rgim.ReferenceNumber).HasMaxLength(100);
            entity.Property(rgim => rgim.Notes).HasMaxLength(500);

            entity.HasOne(rgim => rgim.RawGoldInventory)
                  .WithMany(rgi => rgi.RawGoldInventoryMovements)
                  .HasForeignKey(rgim => rgim.RawGoldInventoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(rgim => rgim.RawGoldPurchaseOrderItem)
                  .WithMany()
                  .HasForeignKey(rgim => rgim.RawGoldPurchaseOrderItemId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    /// <summary>
    /// Override SaveChanges to automatically update audit fields
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically update audit fields
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }





    /// <summary>
    /// Configure lookup tables
    /// </summary>
    private static void ConfigureLookupTables(ModelBuilder builder)
    {
        // Configure all lookup entities with common properties
        var lookupEntities = new[]
        {
            typeof(BusinessEntityTypeLookup),
            typeof(ChargeTypeLookup),
            typeof(FinancialTransactionStatusLookup),
            typeof(FinancialTransactionTypeLookup),
            typeof(KaratTypeLookup),
            typeof(OrderStatusLookup),
            typeof(OrderTypeLookup),
            typeof(PaymentMethodLookup),
            typeof(ProductCategoryTypeLookup),
            typeof(RepairPriorityLookup),
            typeof(RepairStatusLookup),
            typeof(SubCategoryLookup),
            typeof(TransactionStatusLookup),
            typeof(TransactionTypeLookup)
        };

        foreach (var entityType in lookupEntities)
        {
            builder.Entity(entityType, entity =>
            {
                entity.Property("Name")
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property("Description")
                      .HasMaxLength(500);

                entity.Property("IsActive")
                      .HasDefaultValue(true);

                entity.HasIndex("Name")
                      .IsUnique()
                      .HasFilter("[IsActive] = 1");
            });
        }
    }

    /// <summary>
    /// Configure audit and soft delete filters
    /// </summary>
    private static void ConfigureAuditAndSoftDelete(ModelBuilder builder)
    {
        // Configure AuditLog entity
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(al => al.Id);

            entity.Property(al => al.Timestamp)
                  .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(al => al.UserId)
                  .HasMaxLength(450); // ASP.NET Identity User Id length

            entity.Property(al => al.UserName)
                  .HasMaxLength(256)
                  .IsRequired(false);

            entity.Property(al => al.Action)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(al => al.EntityType)
                  .HasMaxLength(100);

            entity.Property(al => al.EntityId)
                  .HasMaxLength(100);

            entity.Property(al => al.Description)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(al => al.OldValues)
                  .HasColumnType("nvarchar(max)");

            entity.Property(al => al.NewValues)
                  .HasColumnType("nvarchar(max)");

            entity.Property(al => al.IpAddress)
                  .HasMaxLength(50);

            entity.Property(al => al.UserAgent)
                  .HasMaxLength(500);

            entity.Property(al => al.SessionId)
                  .HasMaxLength(100);

            entity.Property(al => al.ErrorMessage)
                  .HasMaxLength(500);

            entity.Property(al => al.Details)
                  .HasMaxLength(1000);

            entity.Property(al => al.BranchName)
                  .HasMaxLength(100);

            // Configure navigation properties
            entity.HasOne(al => al.User)
                  .WithMany()
                  .HasForeignKey(al => al.UserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(al => al.Branch)
                  .WithMany()
                  .HasForeignKey(al => al.BranchId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(al => al.FinancialTransaction)
                  .WithMany()
                  .HasForeignKey(al => al.FinancialTransactionId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(al => al.Order)
                  .WithMany()
                  .HasForeignKey(al => al.OrderId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(al => al.Timestamp);
            entity.HasIndex(al => new { al.EntityType, al.EntityId });
            entity.HasIndex(al => al.UserId);
        });

        // Configure global soft delete filter
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, "IsActive");
                var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(true)), parameter);
                entityType.SetQueryFilter(filter);
            }
        }
    }

    /// <summary>
    /// Update audit fields for entities being saved
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var currentUserId = _currentUserService?.UserId ?? "system";

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = currentUserId;
                    entry.Entity.IsActive = true;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    entry.Entity.ModifiedBy = currentUserId;
                    break;
            }
        }
    }
}
