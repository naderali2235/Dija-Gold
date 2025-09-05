using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Models.CoreModels;
using DijaGoldPOS.API.Models.LookupModels;
using DijaGoldPOS.API.Models.ProductModels;

namespace DijaGoldPOS.API.Data;

/// <summary>
/// Enhanced database context with module-based schemas and comprehensive configuration
/// </summary>
public class EnhancedApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ICurrentUserService _currentUserService;

    public EnhancedApplicationDbContext(DbContextOptions<EnhancedApplicationDbContext> options, ICurrentUserService currentUserService) 
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    #region Core Module DbSets
    public DbSet<Branch> Branches { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    #endregion

    #region Lookup Module DbSets
    // Product Lookups
    public DbSet<KaratTypeLookup> KaratTypes { get; set; }
    public DbSet<ProductCategoryTypeLookup> ProductCategoryTypes { get; set; }
    public DbSet<SubCategoryLookup> SubCategories { get; set; }
    public DbSet<ChargeTypeLookup> ChargeTypes { get; set; }

    // Business Lookups
    public DbSet<BusinessEntityTypeLookup> BusinessEntityTypes { get; set; }
    public DbSet<PaymentMethodLookup> PaymentMethods { get; set; }

    // Transaction Lookups
    public DbSet<OrderTypeLookup> OrderTypes { get; set; }
    public DbSet<OrderStatusLookup> OrderStatuses { get; set; }
    public DbSet<FinancialTransactionTypeLookup> FinancialTransactionTypes { get; set; }
    public DbSet<FinancialTransactionStatusLookup> FinancialTransactionStatuses { get; set; }
    public DbSet<TransactionTypeLookup> TransactionTypes { get; set; }
    public DbSet<TransactionStatusLookup> TransactionStatuses { get; set; }

    // Repair Lookups
    public DbSet<RepairStatusLookup> RepairStatuses { get; set; }
    public DbSet<RepairPriorityLookup> RepairPriorities { get; set; }
    #endregion

    #region Product Module DbSets
    public DbSet<Product> Products { get; set; }
    public DbSet<GoldRate> GoldRates { get; set; }
    public DbSet<MakingCharges> MakingCharges { get; set; }
    public DbSet<TaxConfiguration> TaxConfigurations { get; set; }
    #endregion

    #region Customer Module DbSets (to be created)
    // Will be added when Customer module is created
    #endregion

    #region Supplier Module DbSets (to be created)
    // Will be added when Supplier module is created
    #endregion

    #region Inventory Module DbSets (to be created)
    // Will be added when Inventory module is created
    #endregion

    #region Sales Module DbSets (to be created)
    // Will be added when Sales module is created
    #endregion

    #region Financial Module DbSets (to be created)
    // Will be added when Financial module is created
    #endregion

    #region Manufacturing Module DbSets (to be created)
    // Will be added when Manufacturing module is created
    #endregion

    #region Repair Module DbSets (to be created)
    // Will be added when Repair module is created
    #endregion

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure all modules
        ConfigureCoreModule(builder);
        ConfigureLookupModule(builder);
        ConfigureProductModule(builder);
        
        // Configure audit and soft delete filters
        ConfigureAuditAndSoftDelete(builder);
        
        // Configure unique constraints with soft delete support
        ConfigureUniqueConstraints(builder);
    }

    #region Core Module Configuration
    private static void ConfigureCoreModule(ModelBuilder builder)
    {
        // Configure Branch
        builder.Entity<Branch>(entity =>
        {
            entity.ToTable("Branches", "Core");
            
            // Unique constraint excluding inactive records
            entity.HasIndex(b => b.Code)
                  .IsUnique()
                  .HasFilter("[IsActive] = 1")
                  .HasDatabaseName("IX_Branches_Code_Active");

            entity.Property(b => b.Code).IsRequired().HasMaxLength(20);
            entity.Property(b => b.Name).IsRequired().HasMaxLength(100);
            entity.Property(b => b.Address).HasMaxLength(500);
            entity.Property(b => b.Phone).HasMaxLength(20);
            entity.Property(b => b.ManagerName).HasMaxLength(100);
            entity.Property(b => b.OperatingHours).HasMaxLength(1000);
            entity.Property(b => b.MaxCashLimit).HasColumnType("decimal(18,2)");

            // Configure row version for optimistic concurrency
            entity.Property(b => b.RowVersion).IsRowVersion();
        });

        // Configure AuditLog
        builder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs", "Audit");
            
            entity.HasKey(al => al.Id);
            entity.Property(al => al.Timestamp).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(al => al.UserId).HasMaxLength(450);
            entity.Property(al => al.UserName).HasMaxLength(256);
            entity.Property(al => al.Action).IsRequired().HasMaxLength(50);
            entity.Property(al => al.EntityType).HasMaxLength(100);
            entity.Property(al => al.EntityId).HasMaxLength(100);
            entity.Property(al => al.Description).IsRequired().HasMaxLength(500);
            entity.Property(al => al.OldValues).HasColumnType("nvarchar(max)");
            entity.Property(al => al.NewValues).HasColumnType("nvarchar(max)");
            entity.Property(al => al.IpAddress).HasMaxLength(50);
            entity.Property(al => al.UserAgent).HasMaxLength(500);
            entity.Property(al => al.SessionId).HasMaxLength(100);
            entity.Property(al => al.ErrorMessage).HasMaxLength(500);
            entity.Property(al => al.Details).HasMaxLength(1000);
            entity.Property(al => al.BranchName).HasMaxLength(100);
            entity.Property(al => al.CorrelationId).HasMaxLength(100);

            // Indexes for performance
            entity.HasIndex(al => al.Timestamp).HasDatabaseName("IX_AuditLogs_Timestamp");
            entity.HasIndex(al => new { al.EntityType, al.EntityId }).HasDatabaseName("IX_AuditLogs_Entity");
            entity.HasIndex(al => al.UserId).HasDatabaseName("IX_AuditLogs_UserId");
            entity.HasIndex(al => al.CorrelationId).HasDatabaseName("IX_AuditLogs_CorrelationId");
        });

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("AspNetUsers", "Identity");
            
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(200);
            entity.Property(u => u.EmployeeCode).HasMaxLength(50);
            entity.Property(u => u.CreatedBy).HasMaxLength(450);
            entity.Property(u => u.ModifiedBy).HasMaxLength(450);

            // Unique constraint on employee code excluding inactive users
            entity.HasIndex(u => u.EmployeeCode)
                  .IsUnique()
                  .HasFilter("[EmployeeCode] IS NOT NULL AND [IsActive] = 1")
                  .HasDatabaseName("IX_AspNetUsers_EmployeeCode_Active");

            // Navigation properties
            entity.HasOne(u => u.Branch)
                  .WithMany(b => b.Users)
                  .HasForeignKey(u => u.BranchId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
    #endregion

    #region Lookup Module Configuration
    private static void ConfigureLookupModule(ModelBuilder builder)
    {
        // Configure all lookup entities with consistent pattern
        var lookupEntities = new[]
        {
            typeof(KaratTypeLookup),
            typeof(ProductCategoryTypeLookup),
            typeof(SubCategoryLookup),
            typeof(ChargeTypeLookup),
            typeof(BusinessEntityTypeLookup),
            typeof(PaymentMethodLookup),
            typeof(OrderTypeLookup),
            typeof(OrderStatusLookup),
            typeof(FinancialTransactionTypeLookup),
            typeof(FinancialTransactionStatusLookup),
            typeof(TransactionTypeLookup),
            typeof(TransactionStatusLookup),
            typeof(RepairStatusLookup),
            typeof(RepairPriorityLookup)
        };

        foreach (var entityType in lookupEntities)
        {
            builder.Entity(entityType, entity =>
            {
                // Set schema to Lookup
                var tableName = entityType.Name;
                entity.ToTable(tableName, "Lookup");

                // Common properties
                entity.Property("Name").IsRequired().HasMaxLength(100);
                entity.Property("Description").HasMaxLength(500);
                entity.Property("DisplayOrder").IsRequired(false);
                entity.Property("IsActive").HasDefaultValue(true);
                entity.Property("IsSystemManaged").HasDefaultValue(false);

                // Unique constraint on Name excluding inactive records
                entity.HasIndex("Name")
                      .IsUnique()
                      .HasFilter("[IsActive] = 1")
                      .HasDatabaseName($"IX_{tableName}_Name_Active");

                // Row version for optimistic concurrency
                entity.Property("RowVersion").IsRowVersion();
            });
        }

        // Configure specific lookup entities with additional properties
        builder.Entity<KaratTypeLookup>(entity =>
        {
            entity.Property(kt => kt.KaratValue).IsRequired();
            entity.Property(kt => kt.PurityPercentage).HasColumnType("decimal(5,4)").IsRequired();
            entity.Property(kt => kt.Abbreviation).IsRequired().HasMaxLength(10);
            entity.Property(kt => kt.IsCommon).HasDefaultValue(true);
        });

        builder.Entity<ProductCategoryTypeLookup>(entity =>
        {
            entity.Property(pct => pct.Code).IsRequired().HasMaxLength(20);
            entity.Property(pct => pct.HasMakingCharges).HasDefaultValue(true);
            entity.Property(pct => pct.DefaultTaxRate).HasColumnType("decimal(5,4)");
            entity.Property(pct => pct.RequiresWeight).HasDefaultValue(true);

            entity.HasIndex(pct => pct.Code)
                  .IsUnique()
                  .HasFilter("[IsActive] = 1")
                  .HasDatabaseName("IX_ProductCategoryTypes_Code_Active");
        });

        builder.Entity<SubCategoryLookup>(entity =>
        {
            entity.Property(sc => sc.Code).IsRequired().HasMaxLength(20);
            entity.Property(sc => sc.DefaultMakingChargeRate).HasColumnType("decimal(10,4)");

            entity.HasOne(sc => sc.CategoryType)
                  .WithMany()
                  .HasForeignKey(sc => sc.CategoryTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(sc => sc.Code)
                  .IsUnique()
                  .HasFilter("[IsActive] = 1")
                  .HasDatabaseName("IX_SubCategories_Code_Active");
        });

        // Configure other specific lookup entities...
        ConfigureBusinessLookups(builder);
        ConfigureTransactionLookups(builder);
        ConfigureRepairLookups(builder);
    }

    private static void ConfigureBusinessLookups(ModelBuilder builder)
    {
        builder.Entity<BusinessEntityTypeLookup>(entity =>
        {
            entity.Property(bet => bet.Code).IsRequired().HasMaxLength(20);
            entity.Property(bet => bet.CanPurchase).HasDefaultValue(false);
            entity.Property(bet => bet.CanSell).HasDefaultValue(false);
            entity.Property(bet => bet.HasCreditLimit).HasDefaultValue(false);

            entity.HasIndex(bet => bet.Code)
                  .IsUnique()
                  .HasFilter("[IsActive] = 1")
                  .HasDatabaseName("IX_BusinessEntityTypes_Code_Active");
        });

        builder.Entity<PaymentMethodLookup>(entity =>
        {
            entity.Property(pm => pm.Code).IsRequired().HasMaxLength(20);
            entity.Property(pm => pm.RequiresImmediateSettlement).HasDefaultValue(true);
            entity.Property(pm => pm.SupportsPartialPayments).HasDefaultValue(false);
            entity.Property(pm => pm.MaxTransactionAmount).HasColumnType("decimal(18,2)");
            entity.Property(pm => pm.ProcessingFeePercentage).HasColumnType("decimal(5,4)");
            entity.Property(pm => pm.IsElectronic).HasDefaultValue(false);

            entity.HasIndex(pm => pm.Code)
                  .IsUnique()
                  .HasFilter("[IsActive] = 1")
                  .HasDatabaseName("IX_PaymentMethods_Code_Active");
        });
    }

    private static void ConfigureTransactionLookups(ModelBuilder builder)
    {
        builder.Entity<OrderTypeLookup>(entity =>
        {
            entity.Property(ot => ot.Code).IsRequired().HasMaxLength(20);
            entity.Property(ot => ot.IncreasesInventory).HasDefaultValue(false);
            entity.Property(ot => ot.DecreasesInventory).HasDefaultValue(true);
            entity.Property(ot => ot.RequiresApproval).HasDefaultValue(false);
            entity.Property(ot => ot.CanBeVoided).HasDefaultValue(true);
        });

        builder.Entity<OrderStatusLookup>(entity =>
        {
            entity.Property(os => os.Code).IsRequired().HasMaxLength(20);
            entity.Property(os => os.IsFinal).HasDefaultValue(false);
            entity.Property(os => os.IsSuccess).HasDefaultValue(false);
            entity.Property(os => os.IsCancellation).HasDefaultValue(false);
            entity.Property(os => os.ColorClass).HasMaxLength(50);
        });

        // Configure other transaction lookups...
    }

    private static void ConfigureRepairLookups(ModelBuilder builder)
    {
        builder.Entity<RepairStatusLookup>(entity =>
        {
            entity.Property(rs => rs.Code).IsRequired().HasMaxLength(20);
            entity.Property(rs => rs.IsInProgress).HasDefaultValue(false);
            entity.Property(rs => rs.IsFinal).HasDefaultValue(false);
            entity.Property(rs => rs.IsCompleted).HasDefaultValue(false);
            entity.Property(rs => rs.IsCancelled).HasDefaultValue(false);
            entity.Property(rs => rs.RequiresCustomerNotification).HasDefaultValue(false);
            entity.Property(rs => rs.ColorClass).HasMaxLength(50);
        });

        builder.Entity<RepairPriorityLookup>(entity =>
        {
            entity.Property(rp => rp.Code).IsRequired().HasMaxLength(20);
            entity.Property(rp => rp.PriorityLevel).HasDefaultValue(1);
            entity.Property(rp => rp.ExpectedCompletionDays).IsRequired(false);
            entity.Property(rp => rp.AdditionalCostPercentage).HasColumnType("decimal(5,2)");
            entity.Property(rp => rp.ColorClass).HasMaxLength(50);
        });
    }
    #endregion

    #region Product Module Configuration
    private static void ConfigureProductModule(ModelBuilder builder)
    {
        // Configure Product
        builder.Entity<Product>(entity =>
        {
            entity.ToTable("Products", "Product");

            // Unique constraint on product code excluding inactive records
            entity.HasIndex(p => p.ProductCode)
                  .IsUnique()
                  .HasFilter("[IsActive] = 1")
                  .HasDatabaseName("IX_Products_ProductCode_Active");

            entity.Property(p => p.ProductCode).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Weight).HasColumnType("decimal(10,3)");
            entity.Property(p => p.Brand).HasMaxLength(100);
            entity.Property(p => p.DesignStyle).HasMaxLength(100);
            entity.Property(p => p.Shape).HasMaxLength(50);
            entity.Property(p => p.PurityCertificateNumber).HasMaxLength(100);
            entity.Property(p => p.CountryOfOrigin).HasMaxLength(100);
            entity.Property(p => p.MinimumStockLevel).HasColumnType("decimal(10,3)");
            entity.Property(p => p.MaximumStockLevel).HasColumnType("decimal(10,3)");
            entity.Property(p => p.ReorderPoint).HasColumnType("decimal(10,3)");
            entity.Property(p => p.StandardCost).HasColumnType("decimal(18,2)");
            entity.Property(p => p.CurrentPrice).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Images).HasColumnType("nvarchar(max)");
            entity.Property(p => p.Specifications).HasColumnType("nvarchar(max)");
            entity.Property(p => p.Notes).HasMaxLength(1000);
            entity.Property(p => p.Barcode).HasMaxLength(100);
            entity.Property(p => p.IsAvailableForSale).HasDefaultValue(true);
            entity.Property(p => p.IsFeatured).HasDefaultValue(false);

            // Row version for optimistic concurrency
            entity.Property(p => p.RowVersion).IsRowVersion();

            // Navigation properties
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

        // Configure GoldRate
        builder.Entity<GoldRate>(entity =>
        {
            entity.ToTable("GoldRates", "Product");

            entity.HasIndex(gr => new { gr.KaratTypeId, gr.EffectiveFrom })
                  .IsUnique()
                  .HasDatabaseName("IX_GoldRates_KaratType_EffectiveFrom");

            entity.Property(gr => gr.RatePerGram).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(gr => gr.Source).IsRequired().HasMaxLength(50);
            entity.Property(gr => gr.SourceReference).HasMaxLength(100);
            entity.Property(gr => gr.Notes).HasMaxLength(1000);
            entity.Property(gr => gr.SetByUserId).HasMaxLength(450);
            entity.Property(gr => gr.ApprovedByUserId).HasMaxLength(450);

            // Row version for optimistic concurrency
            entity.Property(gr => gr.RowVersion).IsRowVersion();

            entity.HasOne(gr => gr.KaratType)
                  .WithMany()
                  .HasForeignKey(gr => gr.KaratTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure MakingCharges
        builder.Entity<MakingCharges>(entity =>
        {
            entity.ToTable("MakingCharges", "Product");

            entity.HasIndex(mc => new { mc.ProductCategoryId, mc.SubCategoryId, mc.EffectiveFrom })
                  .HasDatabaseName("IX_MakingCharges_Category_EffectiveFrom");

            entity.Property(mc => mc.ChargeValue).HasColumnType("decimal(10,4)").IsRequired();
            entity.Property(mc => mc.MinimumCharge).HasColumnType("decimal(18,2)");
            entity.Property(mc => mc.MaximumCharge).HasColumnType("decimal(18,2)");
            entity.Property(mc => mc.MinimumWeight).HasColumnType("decimal(10,3)");
            entity.Property(mc => mc.MaximumWeight).HasColumnType("decimal(10,3)");
            entity.Property(mc => mc.Description).HasMaxLength(500);
            entity.Property(mc => mc.Notes).HasMaxLength(1000);

            // Row version for optimistic concurrency
            entity.Property(mc => mc.RowVersion).IsRowVersion();

            entity.HasOne(mc => mc.ProductCategory)
                  .WithMany()
                  .HasForeignKey(mc => mc.ProductCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(mc => mc.ChargeType)
                  .WithMany()
                  .HasForeignKey(mc => mc.ChargeTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure TaxConfiguration
        builder.Entity<TaxConfiguration>(entity =>
        {
            entity.ToTable("TaxConfigurations", "Product");

            // Unique constraint on tax code excluding inactive records
            entity.HasIndex(tc => tc.TaxCode)
                  .IsUnique()
                  .HasFilter("[IsActive] = 1")
                  .HasDatabaseName("IX_TaxConfigurations_TaxCode_Active");

            entity.Property(tc => tc.TaxCode).IsRequired().HasMaxLength(20);
            entity.Property(tc => tc.TaxName).IsRequired().HasMaxLength(100);
            entity.Property(tc => tc.TaxRate).HasColumnType("decimal(10,4)").IsRequired();
            entity.Property(tc => tc.MinimumTransactionAmount).HasColumnType("decimal(18,2)");
            entity.Property(tc => tc.MaximumTransactionAmount).HasColumnType("decimal(18,2)");
            entity.Property(tc => tc.TaxAuthority).HasMaxLength(200);
            entity.Property(tc => tc.TaxRegistrationNumber).HasMaxLength(100);
            entity.Property(tc => tc.Description).HasMaxLength(500);
            entity.Property(tc => tc.Notes).HasMaxLength(1000);

            // Row version for optimistic concurrency
            entity.Property(tc => tc.RowVersion).IsRowVersion();

            entity.HasOne(tc => tc.TaxType)
                  .WithMany()
                  .HasForeignKey(tc => tc.TaxTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
    #endregion

    #region Audit and Soft Delete Configuration
    private void ConfigureAuditAndSoftDelete(ModelBuilder builder)
    {
        // Configure soft delete filter for all BaseEntity derived entities
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsActive));
                var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(true)), parameter);
                entityType.SetQueryFilter(filter);
            }
        }
    }
    #endregion

    #region Unique Constraints Configuration
    private static void ConfigureUniqueConstraints(ModelBuilder builder)
    {
        // All unique constraints are configured with filters to exclude inactive records
        // This is done in individual entity configurations above
        
        // Additional composite unique constraints can be added here as needed
    }
    #endregion

    #region SaveChanges Override
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
        var currentTime = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = currentTime;
                    entry.Entity.CreatedBy = currentUserId;
                    entry.Entity.IsActive = true;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedAt = currentTime;
                    entry.Entity.ModifiedBy = currentUserId;
                    // Prevent modification of CreatedAt and CreatedBy
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Property(e => e.CreatedBy).IsModified = false;
                    break;
            }
        }
    }
    #endregion
}
