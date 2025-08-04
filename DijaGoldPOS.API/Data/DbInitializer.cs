using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Data;

/// <summary>
/// Database initializer for seeding initial data
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Initialize database with seed data
    /// </summary>
    public static async Task InitializeAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Run any pending migrations
        if (context.Database.GetPendingMigrations().Any())
        {
            await context.Database.MigrateAsync();
        }

        // Seed roles
        await SeedRolesAsync(roleManager);

        // Seed branches
        await SeedBranchesAsync(context);

        // Seed users
        await SeedUsersAsync(userManager, context);

        // Seed tax configurations
        await SeedTaxConfigurationsAsync(context);

        // Seed gold rates
        await SeedGoldRatesAsync(context);

        // Seed making charges
        await SeedMakingChargesAsync(context);

        // Seed sample products
        await SeedProductsAsync(context);

        await context.SaveChanges();
    }

    /// <summary>
    /// Seed application roles
    /// </summary>
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Manager", "Cashier" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    /// <summary>
    /// Seed branches
    /// </summary>
    private static async Task SeedBranchesAsync(ApplicationDbContext context)
    {
        if (!await context.Branches.AnyAsync())
        {
            var branches = new List<Branch>
            {
                new Branch
                {
                    Name = "Main Branch",
                    Code = "MAIN",
                    Address = "123 Gold Street, Cairo, Egypt",
                    Phone = "+20 2 1234567",
                    ManagerName = "Ahmed Hassan",
                    IsHeadquarters = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new Branch
                {
                    Name = "Alexandria Branch",
                    Code = "ALEX",
                    Address = "456 Mediterranean Ave, Alexandria, Egypt",
                    Phone = "+20 3 7654321",
                    ManagerName = "Fatima Ali",
                    IsHeadquarters = false,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Branches.AddRangeAsync(branches);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seed default users
    /// </summary>
    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        var mainBranch = await context.Branches.FirstAsync(b => b.Code == "MAIN");
        var alexBranch = await context.Branches.FirstAsync(b => b.Code == "ALEX");

        // Seed Manager
        var managerEmail = "manager@dijagold.com";
        if (await userManager.FindByEmailAsync(managerEmail) == null)
        {
            var manager = new ApplicationUser
            {
                UserName = managerEmail,
                Email = managerEmail,
                FullName = "System Manager",
                EmployeeCode = "MGR001",
                BranchId = mainBranch.Id,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(manager, "Manager123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(manager, "Manager");
            }
        }

        // Seed Cashier
        var cashierEmail = "cashier@dijagold.com";
        if (await userManager.FindByEmailAsync(cashierEmail) == null)
        {
            var cashier = new ApplicationUser
            {
                UserName = cashierEmail,
                Email = cashierEmail,
                FullName = "System Cashier",
                EmployeeCode = "CSH001",
                BranchId = mainBranch.Id,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(cashier, "Cashier123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(cashier, "Cashier");
            }
        }

        // Seed Alexandria Branch Cashier
        var alexCashierEmail = "alex.cashier@dijagold.com";
        if (await userManager.FindByEmailAsync(alexCashierEmail) == null)
        {
            var alexCashier = new ApplicationUser
            {
                UserName = alexCashierEmail,
                Email = alexCashierEmail,
                FullName = "Alexandria Cashier",
                EmployeeCode = "CSH002",
                BranchId = alexBranch.Id,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(alexCashier, "Cashier123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(alexCashier, "Cashier");
            }
        }
    }

    /// <summary>
    /// Seed tax configurations
    /// </summary>
    private static async Task SeedTaxConfigurationsAsync(ApplicationDbContext context)
    {
        if (!await context.TaxConfigurations.AnyAsync())
        {
            var taxes = new List<TaxConfiguration>
            {
                new TaxConfiguration
                {
                    TaxName = "Value Added Tax",
                    TaxCode = "VAT",
                    TaxType = ChargeType.Percentage,
                    TaxRate = 14.00m, // 14% VAT in Egypt
                    IsMandatory = true,
                    EffectiveFrom = DateTime.UtcNow.Date,
                    IsCurrent = true,
                    DisplayOrder = 1,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.TaxConfigurations.AddRangeAsync(taxes);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seed initial gold rates
    /// </summary>
    private static async Task SeedGoldRatesAsync(ApplicationDbContext context)
    {
        if (!await context.GoldRates.AnyAsync())
        {
            var goldRates = new List<GoldRate>
            {
                new GoldRate
                {
                    KaratType = KaratType.K18,
                    RatePerGram = 1450.00m, // Sample rate in EGP per gram
                    EffectiveFrom = DateTime.UtcNow.Date,
                    IsCurrent = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new GoldRate
                {
                    KaratType = KaratType.K21,
                    RatePerGram = 1650.00m, // Sample rate in EGP per gram
                    EffectiveFrom = DateTime.UtcNow.Date,
                    IsCurrent = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new GoldRate
                {
                    KaratType = KaratType.K22,
                    RatePerGram = 1750.00m, // Sample rate in EGP per gram
                    EffectiveFrom = DateTime.UtcNow.Date,
                    IsCurrent = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new GoldRate
                {
                    KaratType = KaratType.K24,
                    RatePerGram = 1900.00m, // Sample rate in EGP per gram
                    EffectiveFrom = DateTime.UtcNow.Date,
                    IsCurrent = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.GoldRates.AddRangeAsync(goldRates);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seed making charges configurations
    /// </summary>
    private static async Task SeedMakingChargesAsync(ApplicationDbContext context)
    {
        if (!await context.MakingCharges.AnyAsync())
        {
            var makingCharges = new List<MakingCharges>
            {
                new MakingCharges
                {
                    Name = "Jewelry Making Charges",
                    ProductCategory = ProductCategoryType.GoldJewelry,
                    ChargeType = ChargeType.Percentage,
                    ChargeValue = 12.00m, // 12% of gold value
                    EffectiveFrom = DateTime.UtcNow.Date,
                    IsCurrent = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new MakingCharges
                {
                    Name = "Ring Making Charges",
                    ProductCategory = ProductCategoryType.GoldJewelry,
                    SubCategory = "Rings",
                    ChargeType = ChargeType.Percentage,
                    ChargeValue = 15.00m, // 15% of gold value for rings
                    EffectiveFrom = DateTime.UtcNow.Date,
                    IsCurrent = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new MakingCharges
                {
                    Name = "Bullion Processing Charges",
                    ProductCategory = ProductCategoryType.Bullion,
                    ChargeType = ChargeType.FixedAmount,
                    ChargeValue = 50.00m, // 50 EGP fixed charge
                    EffectiveFrom = DateTime.UtcNow.Date,
                    IsCurrent = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new MakingCharges
                {
                    Name = "Coin Processing Charges",
                    ProductCategory = ProductCategoryType.Coins,
                    ChargeType = ChargeType.FixedAmount,
                    ChargeValue = 25.00m, // 25 EGP fixed charge
                    EffectiveFrom = DateTime.UtcNow.Date,
                    IsCurrent = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.MakingCharges.AddRangeAsync(makingCharges);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seed sample products
    /// </summary>
    private static async Task SeedProductsAsync(ApplicationDbContext context)
    {
        if (!await context.Products.AnyAsync())
        {
            var products = new List<Product>
            {
                new Product
                {
                    ProductCode = "JW001",
                    Name = "Gold Wedding Ring",
                    CategoryType = ProductCategoryType.GoldJewelry,
                    KaratType = KaratType.K18,
                    Weight = 3.500m,
                    Brand = "Dija Gold",
                    DesignStyle = "Classic",
                    SubCategory = "Rings",
                    MakingChargesApplicable = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    ProductCode = "JW002",
                    Name = "Gold Necklace Chain",
                    CategoryType = ProductCategoryType.GoldJewelry,
                    KaratType = KaratType.K21,
                    Weight = 15.750m,
                    Brand = "Dija Gold",
                    DesignStyle = "Rope Chain",
                    SubCategory = "Necklaces",
                    MakingChargesApplicable = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    ProductCode = "BL001",
                    Name = "Gold Bar 10g",
                    CategoryType = ProductCategoryType.Bullion,
                    KaratType = KaratType.K24,
                    Weight = 10.000m,
                    Brand = "Dija Gold",
                    Shape = "Bar",
                    PurityCertificateNumber = "DG-2024-001",
                    MakingChargesApplicable = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    ProductCode = "CN001",
                    Name = "Egyptian Gold Coin 1g",
                    CategoryType = ProductCategoryType.Coins,
                    KaratType = KaratType.K22,
                    Weight = 1.000m,
                    CountryOfOrigin = "Egypt",
                    YearOfMinting = 2024,
                    FaceValue = 100.00m,
                    HasNumismaticValue = false,
                    MakingChargesApplicable = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            // Add inventory for these products
            var branches = await context.Branches.ToListAsync();
            var inventoryItems = new List<Inventory>();

            foreach (var product in products)
            {
                foreach (var branch in branches)
                {
                    inventoryItems.Add(new Inventory
                    {
                        ProductId = product.Id,
                        BranchId = branch.Id,
                        QuantityOnHand = 10, // Sample quantity
                        WeightOnHand = product.Weight * 10, // Total weight
                        MinimumStockLevel = 2,
                        MaximumStockLevel = 50,
                        ReorderPoint = 5,
                        LastCountDate = DateTime.UtcNow,
                        CreatedBy = "system",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await context.Inventories.AddRangeAsync(inventoryItems);
            await context.SaveChangesAsync();
        }
    }
}