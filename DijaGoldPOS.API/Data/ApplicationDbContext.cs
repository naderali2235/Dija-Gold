using DijaGoldPOS.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DijaGoldPOS.API.Services;

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
    
    // Pricing and configuration
    public DbSet<GoldRate> GoldRates { get; set; }
    public DbSet<MakingCharges> MakingCharges { get; set; }
    public DbSet<TaxConfiguration> TaxConfigurations { get; set; }
    
    // Transactions
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionItem> TransactionItems { get; set; }
    public DbSet<TransactionTax> TransactionTaxes { get; set; }
    
    // Inventory
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<InventoryMovement> InventoryMovements { get; set; }
    
    // Purchasing
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    
    // Supplier Transactions
    public DbSet<SupplierTransaction> SupplierTransactions { get; set; }
    
    // Audit
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    // Cash Management
    public DbSet<CashDrawerBalance> CashDrawerBalances { get; set; }
    
    // Repair Jobs
    public DbSet<RepairJob> RepairJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ApplicationUser relationships
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(u => u.Branch)
                  .WithMany(b => b.Users)
                  .HasForeignKey(u => u.BranchId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Branch
        builder.Entity<Branch>(entity =>
        {
            entity.HasIndex(b => b.Code).IsUnique();
            entity.Property(b => b.Code).IsRequired().HasMaxLength(20);
            entity.Property(b => b.Name).IsRequired().HasMaxLength(100);
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
        });

        // Configure Supplier
        builder.Entity<Supplier>(entity =>
        {
            entity.Property(s => s.CompanyName).IsRequired().HasMaxLength(200);
            entity.Property(s => s.CreditLimit).HasColumnType("decimal(18,2)");
            entity.Property(s => s.CurrentBalance).HasColumnType("decimal(18,2)");
        });

        // Configure SupplierTransaction
        builder.Entity<SupplierTransaction>(entity =>
        {
            entity.HasIndex(st => st.TransactionNumber).IsUnique();
            entity.Property(st => st.TransactionNumber).IsRequired().HasMaxLength(50);
            entity.Property(st => st.TransactionType).IsRequired().HasMaxLength(50);
            entity.Property(st => st.Amount).HasColumnType("decimal(18,2)");
            entity.Property(st => st.BalanceAfterTransaction).HasColumnType("decimal(18,2)");
            entity.Property(st => st.ReferenceNumber).HasMaxLength(100);
            entity.Property(st => st.Notes).HasMaxLength(1000);
            entity.Property(st => st.CreatedByUserId).IsRequired().HasMaxLength(450);

            // Relationships
            entity.HasOne(st => st.Supplier)
                  .WithMany()
                  .HasForeignKey(st => st.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(st => st.Branch)
                  .WithMany()
                  .HasForeignKey(st => st.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure GoldRate
        builder.Entity<GoldRate>(entity =>
        {
            entity.HasIndex(gr => new { gr.KaratType, gr.EffectiveFrom });
            entity.Property(gr => gr.RatePerGram).HasColumnType("decimal(18,2)");
        });

        // Configure MakingCharges
        builder.Entity<MakingCharges>(entity =>
        {
            entity.HasIndex(mc => new { mc.ProductCategory, mc.SubCategory, mc.EffectiveFrom });
            entity.Property(mc => mc.ChargeValue).HasColumnType("decimal(10,4)");
        });

        // Configure TaxConfiguration
        builder.Entity<TaxConfiguration>(entity =>
        {
            entity.HasIndex(tc => tc.TaxCode).IsUnique();
            entity.Property(tc => tc.TaxCode).IsRequired().HasMaxLength(20);
            entity.Property(tc => tc.TaxRate).HasColumnType("decimal(10,4)");
        });

        // Configure Transaction
        builder.Entity<Transaction>(entity =>
        {
            entity.HasIndex(t => new { t.BranchId, t.TransactionNumber }).IsUnique();
            entity.Property(t => t.TransactionNumber).IsRequired().HasMaxLength(50);
            
            entity.Property(t => t.Subtotal).HasColumnType("decimal(18,2)");
            entity.Property(t => t.TotalMakingCharges).HasColumnType("decimal(18,2)");
            entity.Property(t => t.TotalTaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(t => t.DiscountAmount).HasColumnType("decimal(18,2)");
            entity.Property(t => t.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(t => t.AmountPaid).HasColumnType("decimal(18,2)");
            entity.Property(t => t.ChangeGiven).HasColumnType("decimal(18,2)");

            // Relationships
            entity.HasOne(t => t.Branch)
                  .WithMany(b => b.Transactions)
                  .HasForeignKey(t => t.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Customer)
                  .WithMany(c => c.Transactions)
                  .HasForeignKey(t => t.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.Cashier)
                  .WithMany()
                  .HasForeignKey(t => t.CashierId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.ApprovedByUser)
                  .WithMany()
                  .HasForeignKey(t => t.ApprovedBy)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.GoldRate)
                  .WithMany(gr => gr.Transactions)
                  .HasForeignKey(t => t.GoldRateId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.OriginalTransaction)
                  .WithMany(t => t.ReturnTransactions)
                  .HasForeignKey(t => t.OriginalTransactionId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure TransactionItem
        builder.Entity<TransactionItem>(entity =>
        {
            entity.Property(ti => ti.Quantity).HasColumnType("decimal(10,3)");
            entity.Property(ti => ti.UnitWeight).HasColumnType("decimal(10,3)");
            entity.Property(ti => ti.TotalWeight).HasColumnType("decimal(10,3)");
            entity.Property(ti => ti.GoldRatePerGram).HasColumnType("decimal(18,2)");
            entity.Property(ti => ti.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(ti => ti.MakingChargesAmount).HasColumnType("decimal(18,2)");
            entity.Property(ti => ti.DiscountPercentage).HasColumnType("decimal(5,2)");
            entity.Property(ti => ti.DiscountAmount).HasColumnType("decimal(18,2)");
            entity.Property(ti => ti.LineTotal).HasColumnType("decimal(18,2)");

            entity.HasOne(ti => ti.Transaction)
                  .WithMany(t => t.TransactionItems)
                  .HasForeignKey(ti => ti.TransactionId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(ti => ti.Product)
                  .WithMany(p => p.TransactionItems)
                  .HasForeignKey(ti => ti.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ti => ti.MakingCharges)
                  .WithMany(mc => mc.TransactionItems)
                  .HasForeignKey(ti => ti.MakingChargesId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure TransactionTax
        builder.Entity<TransactionTax>(entity =>
        {
            entity.Property(tt => tt.TaxRate).HasColumnType("decimal(10,4)");
            entity.Property(tt => tt.TaxableAmount).HasColumnType("decimal(18,2)");
            entity.Property(tt => tt.TaxAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(tt => tt.Transaction)
                  .WithMany(t => t.TransactionTaxes)
                  .HasForeignKey(tt => tt.TransactionId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(tt => tt.TaxConfiguration)
                  .WithMany(tc => tc.TransactionTaxes)
                  .HasForeignKey(tt => tt.TaxConfigurationId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Inventory
        builder.Entity<Inventory>(entity =>
        {
            entity.HasIndex(i => new { i.ProductId, i.BranchId }).IsUnique();
            
            entity.Property(i => i.QuantityOnHand).HasColumnType("decimal(10,3)");
            entity.Property(i => i.WeightOnHand).HasColumnType("decimal(10,3)");
            entity.Property(i => i.MinimumStockLevel).HasColumnType("decimal(10,3)");
            entity.Property(i => i.MaximumStockLevel).HasColumnType("decimal(10,3)");
            entity.Property(i => i.ReorderPoint).HasColumnType("decimal(10,3)");

            entity.HasOne(i => i.Product)
                  .WithMany(p => p.InventoryRecords)
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

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
        });

        // Configure AuditLog
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(al => al.Timestamp);
            entity.HasIndex(al => new { al.UserId, al.Timestamp });
            entity.HasIndex(al => new { al.EntityType, al.EntityId });

            entity.Property(al => al.UserId).IsRequired().HasMaxLength(450);
            entity.Property(al => al.UserName).IsRequired().HasMaxLength(100);
            entity.Property(al => al.Action).IsRequired().HasMaxLength(50);

            entity.HasOne(al => al.User)
                  .WithMany()
                  .HasForeignKey(al => al.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(al => al.Branch)
                  .WithMany()
                  .HasForeignKey(al => al.BranchId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(al => al.Transaction)
                  .WithMany()
                  .HasForeignKey(al => al.TransactionId)
                  .OnDelete(DeleteBehavior.SetNull);
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

            entity.HasOne(cdb => cdb.Branch)
                  .WithMany()
                  .HasForeignKey(cdb => cdb.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure RepairJob
        builder.Entity<RepairJob>(entity =>
        {
            entity.HasIndex(rj => rj.TransactionId).IsUnique();
            entity.HasIndex(rj => rj.Status);
            entity.HasIndex(rj => rj.Priority);
            entity.HasIndex(rj => rj.AssignedTechnicianId);

            entity.Property(rj => rj.TechnicianNotes).HasMaxLength(2000);
            entity.Property(rj => rj.MaterialsUsed).HasMaxLength(1000);
            entity.Property(rj => rj.ActualCost).HasColumnType("decimal(18,2)");
            entity.Property(rj => rj.HoursSpent).HasColumnType("decimal(5,2)");

            entity.HasOne(rj => rj.Transaction)
                  .WithOne()
                  .HasForeignKey<RepairJob>(rj => rj.TransactionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(rj => rj.AssignedTechnician)
                  .WithMany()
                  .HasForeignKey(rj => rj.AssignedTechnicianId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(rj => rj.QualityChecker)
                  .WithMany()
                  .HasForeignKey(rj => rj.QualityCheckedBy)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure soft delete filter for BaseEntity
        builder.Entity<Branch>().HasQueryFilter(e => e.IsActive);
        builder.Entity<Product>().HasQueryFilter(e => e.IsActive);
        builder.Entity<Customer>().HasQueryFilter(e => e.IsActive);
        builder.Entity<Supplier>().HasQueryFilter(e => e.IsActive);
        builder.Entity<GoldRate>().HasQueryFilter(e => e.IsActive);
        builder.Entity<MakingCharges>().HasQueryFilter(e => e.IsActive);
        builder.Entity<TaxConfiguration>().HasQueryFilter(e => e.IsActive);
        builder.Entity<Transaction>().HasQueryFilter(e => e.IsActive);
        builder.Entity<TransactionItem>().HasQueryFilter(e => e.IsActive);
        builder.Entity<TransactionTax>().HasQueryFilter(e => e.IsActive);
        builder.Entity<Inventory>().HasQueryFilter(e => e.IsActive);
        builder.Entity<InventoryMovement>().HasQueryFilter(e => e.IsActive);
        builder.Entity<PurchaseOrder>().HasQueryFilter(e => e.IsActive);
        builder.Entity<PurchaseOrderItem>().HasQueryFilter(e => e.IsActive);
        builder.Entity<CashDrawerBalance>().HasQueryFilter(e => e.IsActive);
        builder.Entity<RepairJob>().HasQueryFilter(e => e.IsActive);
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