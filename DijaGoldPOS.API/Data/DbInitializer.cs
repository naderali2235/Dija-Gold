using DijaGoldPOS.API.Models;
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
        //// Check if database was just created
        //var wasDatabaseCreated = await context.Database.EnsureCreatedAsync();
        
        //// Run any pending migrations
        //if (context.Database.GetPendingMigrations().Any())
        //{
        //    await context.Database.MigrateAsync();
        //}

        // Only seed data if database was just created (fresh database)
            // Seed lookup tables first
            await LookupTableSeeder.SeedLookupTablesAsync(context);

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

            // Seed customers
            await SeedCustomersAsync(context);

            // Seed suppliers
            await SeedSuppliersAsync(context);

            // Seed technicians
            await SeedTechniciansAsync(context);

            // Seed purchase orders
            await SeedPurchaseOrdersAsync(context);

            // Seed orders and financial transactions
            await SeedOrdersAndTransactionsAsync(context);

            // Seed repair jobs
            await SeedRepairJobsAsync(context);

            await context.SaveChangesAsync();
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
            var percentageChargeType = await context.ChargeTypeLookups.FirstAsync(ct => ct.Name == "Percentage");
            
            var taxes = new List<TaxConfiguration>
            {
                new TaxConfiguration
                {
                    TaxName = "Value Added Tax",
                    TaxCode = "VAT",
                    TaxTypeId = percentageChargeType.Id,
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
            var k18Karat = await context.KaratTypeLookups.FirstAsync(kt => kt.Name == "18K Gold");
            var k21Karat = await context.KaratTypeLookups.FirstAsync(kt => kt.Name == "21K Gold");
            var k22Karat = await context.KaratTypeLookups.FirstAsync(kt => kt.Name == "22K Gold");
            var k24Karat = await context.KaratTypeLookups.FirstAsync(kt => kt.Name == "24K Gold");
            
            var goldRates = new List<GoldRate>
            {
                new GoldRate
                {
                    KaratTypeId = k18Karat.Id,
                    RatePerGram = 1450.00m, // Sample rate in EGP per gram
                    EffectiveFrom = DateTime.UtcNow.Date,
                    IsCurrent = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new GoldRate
                {
                    KaratTypeId = k21Karat.Id,
                    RatePerGram = 1650.00m, // Sample rate in EGP per gram
                    EffectiveFrom = DateTime.UtcNow.Date,
                    IsCurrent = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new GoldRate
                {
                    KaratTypeId = k22Karat.Id,
                    RatePerGram = 1750.00m, // Sample rate in EGP per gram
                    EffectiveFrom = DateTime.UtcNow.Date,
                    IsCurrent = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new GoldRate
                {
                    KaratTypeId = k24Karat.Id,
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
            var goldJewelryCategory = await context.ProductCategoryTypeLookups.FirstAsync(pct => pct.Name == "Gold Jewelry");
            var bullionCategory = await context.ProductCategoryTypeLookups.FirstAsync(pct => pct.Name == "Bullion");
            var coinsCategory = await context.ProductCategoryTypeLookups.FirstAsync(pct => pct.Name == "Gold Coins");
            var percentageChargeType = await context.ChargeTypeLookups.FirstAsync(ct => ct.Name == "Percentage");
            var fixedAmountChargeType = await context.ChargeTypeLookups.FirstAsync(ct => ct.Name == "Fixed Amount");
            
            var makingCharges = new List<MakingCharges>
            {
                new MakingCharges
                {
                    Name = "Jewelry Making Charges",
                    ProductCategoryId = goldJewelryCategory.Id,
                    ChargeTypeId = percentageChargeType.Id,
                    ChargeValue = 12.00m, // 12% of gold value
                    EffectiveFrom = DateTime.UtcNow.Date,
                    IsCurrent = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new MakingCharges
                {
                    Name = "Ring Making Charges",
                    ProductCategoryId = goldJewelryCategory.Id,
                    SubCategory = "Rings",
                    ChargeTypeId = percentageChargeType.Id,
                    ChargeValue = 15.00m, // 15% of gold value for rings
                    EffectiveFrom = DateTime.UtcNow.Date,
                    IsCurrent = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new MakingCharges
                {
                    Name = "Bullion Processing Charges",
                    ProductCategoryId = bullionCategory.Id,
                    ChargeTypeId = fixedAmountChargeType.Id,
                    ChargeValue = 50.00m, // 50 EGP fixed charge
                    EffectiveFrom = DateTime.UtcNow.Date,
                    IsCurrent = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new MakingCharges
                {
                    Name = "Coin Processing Charges",
                    ProductCategoryId = coinsCategory.Id,
                    ChargeTypeId = fixedAmountChargeType.Id,
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
            var goldJewelryCategory = await context.ProductCategoryTypeLookups.FirstAsync(pct => pct.Name == "Gold Jewelry");
            var bullionCategory = await context.ProductCategoryTypeLookups.FirstAsync(pct => pct.Name == "Bullion");
            var coinsCategory = await context.ProductCategoryTypeLookups.FirstAsync(pct => pct.Name == "Gold Coins");
            var k18Karat = await context.KaratTypeLookups.FirstAsync(kt => kt.Name == "18K Gold");
            var k21Karat = await context.KaratTypeLookups.FirstAsync(kt => kt.Name == "21K Gold");
            var k22Karat = await context.KaratTypeLookups.FirstAsync(kt => kt.Name == "22K Gold");
            var k24Karat = await context.KaratTypeLookups.FirstAsync(kt => kt.Name == "24K Gold");
            
            var products = new List<Product>
            {
                new Product
                {
                    ProductCode = "JW001",
                    Name = "Gold Wedding Ring",
                    CategoryTypeId = goldJewelryCategory.Id,
                    KaratTypeId = k18Karat.Id,
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
                    CategoryTypeId = goldJewelryCategory.Id,
                    KaratTypeId = k21Karat.Id,
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
                    CategoryTypeId = bullionCategory.Id,
                    KaratTypeId = k24Karat.Id,
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
                    CategoryTypeId = coinsCategory.Id,
                    KaratTypeId = k22Karat.Id,
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

    /// <summary>
    /// Seed sample customers
    /// </summary>
    private static async Task SeedCustomersAsync(ApplicationDbContext context)
    {
        if (!await context.Customers.AnyAsync())
        {
            var customers = new List<Customer>
            {
                new Customer
                {
                    FullName = "Ahmed Mohamed Hassan",
                    NationalId = "12345678901234",
                    MobileNumber = "+201234567890",
                    Email = "ahmed.hassan@email.com",
                    Address = "15 El-Tahrir Street, Downtown Cairo, Egypt",
                    RegistrationDate = DateTime.UtcNow.AddDays(-30),
                    LoyaltyTier = 3,
                    LoyaltyPoints = 1250,
                    TotalPurchaseAmount = 45000.00m,
                    DefaultDiscountPercentage = 5.00m,
                    MakingChargesWaived = false,
                    Notes = "Regular customer, prefers 21K gold jewelry",
                    LastPurchaseDate = DateTime.UtcNow.AddDays(-5),
                    TotalOrders = 8,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new Customer
                {
                    FullName = "Fatima Ali Mahmoud",
                    NationalId = "98765432109876",
                    MobileNumber = "+201098765432",
                    Email = "fatima.mahmoud@email.com",
                    Address = "45 Alexandria Road, Heliopolis, Cairo, Egypt",
                    RegistrationDate = DateTime.UtcNow.AddDays(-45),
                    LoyaltyTier = 2,
                    LoyaltyPoints = 750,
                    TotalPurchaseAmount = 28000.00m,
                    DefaultDiscountPercentage = 3.00m,
                    MakingChargesWaived = false,
                    Notes = "Prefers rings and necklaces, 18K gold",
                    LastPurchaseDate = DateTime.UtcNow.AddDays(-12),
                    TotalOrders = 5,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new Customer
                {
                    FullName = "Omar Khalil Ibrahim",
                    NationalId = "11223344556677",
                    MobileNumber = "+201122334455",
                    Email = "omar.ibrahim@email.com",
                    Address = "78 Zamalek Street, Zamalek, Cairo, Egypt",
                    RegistrationDate = DateTime.UtcNow.AddDays(-15),
                    LoyaltyTier = 1,
                    LoyaltyPoints = 200,
                    TotalPurchaseAmount = 8500.00m,
                    DefaultDiscountPercentage = 0.00m,
                    MakingChargesWaived = false,
                    Notes = "New customer, interested in bullion",
                    LastPurchaseDate = DateTime.UtcNow.AddDays(-2),
                    TotalOrders = 2,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new Customer
                {
                    FullName = "Nour El-Din Salah",
                    NationalId = "55667788990011",
                    MobileNumber = "+205566778899",
                    Email = "nour.salah@email.com",
                    Address = "23 Maadi Corniche, Maadi, Cairo, Egypt",
                    RegistrationDate = DateTime.UtcNow.AddDays(-60),
                    LoyaltyTier = 4,
                    LoyaltyPoints = 2100,
                    TotalPurchaseAmount = 75000.00m,
                    DefaultDiscountPercentage = 8.00m,
                    MakingChargesWaived = true,
                    Notes = "VIP customer, makes charges waived, prefers 22K gold",
                    LastPurchaseDate = DateTime.UtcNow.AddDays(-1),
                    TotalOrders = 15,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new Customer
                {
                    FullName = "Layla Ahmed Mostafa",
                    NationalId = "99887766554433",
                    MobileNumber = "+209988776655",
                    Email = "layla.mostafa@email.com",
                    Address = "67 Garden City Street, Garden City, Cairo, Egypt",
                    RegistrationDate = DateTime.UtcNow.AddDays(-90),
                    LoyaltyTier = 5,
                    LoyaltyPoints = 3500,
                    TotalPurchaseAmount = 120000.00m,
                    DefaultDiscountPercentage = 10.00m,
                    MakingChargesWaived = true,
                    Notes = "Premium customer, highest loyalty tier, 24K gold collector",
                    LastPurchaseDate = DateTime.UtcNow.AddDays(-3),
                    TotalOrders = 25,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seed sample suppliers
    /// </summary>
    private static async Task SeedSuppliersAsync(ApplicationDbContext context)
    {
        if (!await context.Suppliers.AnyAsync())
        {
            var suppliers = new List<Supplier>
            {
                new Supplier
                {
                    CompanyName = "Egyptian Gold Refinery Co.",
                    ContactPersonName = "Mahmoud Abdel Rahman",
                    Phone = "+20223456789",
                    Email = "contact@egrefinery.com",
                    Address = "15 Industrial Zone, 6th October City, Giza, Egypt",
                    TaxRegistrationNumber = "123456789",
                    CommercialRegistrationNumber = "CR-2020-001",
                    CreditLimit = 500000.00m,
                    CurrentBalance = 125000.00m,
                    PaymentTermsDays = 30,
                    CreditLimitEnforced = true,
                    PaymentTerms = "Net 30 days, 2% discount if paid within 10 days",
                    Notes = "Primary gold supplier, high quality 24K gold",
                    LastTransactionDate = DateTime.UtcNow.AddDays(-5),
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new Supplier
                {
                    CompanyName = "Cairo Jewelry Manufacturing",
                    ContactPersonName = "Hassan El-Sayed",
                    Phone = "+20234567890",
                    Email = "sales@cairojewelry.com",
                    Address = "45 El-Haram Street, Giza, Egypt",
                    TaxRegistrationNumber = "987654321",
                    CommercialRegistrationNumber = "CR-2019-002",
                    CreditLimit = 300000.00m,
                    CurrentBalance = 75000.00m,
                    PaymentTermsDays = 45,
                    CreditLimitEnforced = true,
                    PaymentTerms = "Net 45 days, specializes in custom jewelry",
                    Notes = "Custom jewelry manufacturer, 18K and 21K gold",
                    LastTransactionDate = DateTime.UtcNow.AddDays(-12),
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new Supplier
                {
                    CompanyName = "Alexandria Gold Trading",
                    ContactPersonName = "Amira Mohamed",
                    Phone = "+20345678901",
                    Email = "info@alexgold.com",
                    Address = "78 El-Mansheya Square, Alexandria, Egypt",
                    TaxRegistrationNumber = "456789123",
                    CommercialRegistrationNumber = "CR-2021-003",
                    CreditLimit = 200000.00m,
                    CurrentBalance = 45000.00m,
                    PaymentTermsDays = 30,
                    CreditLimitEnforced = true,
                    PaymentTerms = "Net 30 days, bulk gold supplier",
                    Notes = "Regional supplier, good for bulk orders",
                    LastTransactionDate = DateTime.UtcNow.AddDays(-8),
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new Supplier
                {
                    CompanyName = "Luxor Coin Mint",
                    ContactPersonName = "Tarek El-Naggar",
                    Phone = "+20976543210",
                    Email = "mint@luxorcoins.com",
                    Address = "12 Karnak Street, Luxor, Egypt",
                    TaxRegistrationNumber = "789123456",
                    CommercialRegistrationNumber = "CR-2018-004",
                    CreditLimit = 150000.00m,
                    CurrentBalance = 25000.00m,
                    PaymentTermsDays = 60,
                    CreditLimitEnforced = false,
                    PaymentTerms = "Net 60 days, commemorative coins specialist",
                    Notes = "Specializes in commemorative and collectible coins",
                    LastTransactionDate = DateTime.UtcNow.AddDays(-20),
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Suppliers.AddRangeAsync(suppliers);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seed sample technicians
    /// </summary>
    private static async Task SeedTechniciansAsync(ApplicationDbContext context)
    {
        if (!await context.Technicians.AnyAsync())
        {
            var mainBranch = await context.Branches.FirstAsync(b => b.Code == "MAIN");
            var alexBranch = await context.Branches.FirstAsync(b => b.Code == "ALEX");

            var technicians = new List<Technician>
            {
                new Technician
                {
                    FullName = "Mohamed El-Sayed Hassan",
                    PhoneNumber = "+201112223334",
                    Email = "mohamed.hassan@dijagold.com",
                    Specialization = "Gold jewelry repair, ring resizing, chain repair",
                    BranchId = mainBranch.Id,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new Technician
                {
                    FullName = "Ahmed Ali Mahmoud",
                    PhoneNumber = "+201223334445",
                    Email = "ahmed.mahmoud@dijagold.com",
                    Specialization = "Engraving, stone setting, custom jewelry",
                    BranchId = mainBranch.Id,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new Technician
                {
                    FullName = "Fatima Omar Ibrahim",
                    PhoneNumber = "+201334445556",
                    Email = "fatima.ibrahim@dijagold.com",
                    Specialization = "Necklace repair, bracelet repair, quality control",
                    BranchId = alexBranch.Id,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new Technician
                {
                    FullName = "Khalil Mostafa El-Sayed",
                    PhoneNumber = "+201445556667",
                    Email = "khalil.elsayed@dijagold.com",
                    Specialization = "Watch repair, antique jewelry restoration",
                    BranchId = mainBranch.Id,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Technicians.AddRangeAsync(technicians);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seed purchase orders
    /// </summary>
    private static async Task SeedPurchaseOrdersAsync(ApplicationDbContext context)
    {
        if (!await context.PurchaseOrders.AnyAsync())
        {
            var mainBranch = await context.Branches.FirstAsync(b => b.Code == "MAIN");
            var alexBranch = await context.Branches.FirstAsync(b => b.Code == "ALEX");
            var supplier1 = await context.Suppliers.FirstAsync(s => s.CompanyName == "Egyptian Gold Refinery Co.");
            var supplier2 = await context.Suppliers.FirstAsync(s => s.CompanyName == "Cairo Jewelry Manufacturing");
            var supplier3 = await context.Suppliers.FirstAsync(s => s.CompanyName == "Alexandria Gold Trading");
            var supplier4 = await context.Suppliers.FirstAsync(s => s.CompanyName == "Luxor Coin Mint");

            var purchaseOrders = new List<PurchaseOrder>
            {
                new PurchaseOrder
                {
                    PurchaseOrderNumber = "PO001",
                    SupplierId = supplier1.Id,
                    BranchId = mainBranch.Id,
                    OrderDate = DateTime.UtcNow.AddDays(-10),
                    ExpectedDeliveryDate = DateTime.UtcNow.AddDays(10),
                    Status = "Pending",
                    TotalAmount = 150000.00m,
                    AmountPaid = 0,
                    OutstandingBalance = 150000.00m,
                    PaymentStatus = "Unpaid",
                    Notes = "Initial purchase for inventory",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new PurchaseOrder
                {
                    PurchaseOrderNumber = "PO002",
                    SupplierId = supplier2.Id,
                    BranchId = alexBranch.Id,
                    OrderDate = DateTime.UtcNow.AddDays(-5),
                    ExpectedDeliveryDate = DateTime.UtcNow.AddDays(5),
                    Status = "Received",
                    TotalAmount = 50000.00m,
                    AmountPaid = 50000.00m,
                    OutstandingBalance = 0,
                    PaymentStatus = "Paid",
                    Notes = "Completed order for inventory",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new PurchaseOrder
                {
                    PurchaseOrderNumber = "PO003",
                    SupplierId = supplier3.Id,
                    BranchId = mainBranch.Id,
                    OrderDate = DateTime.UtcNow.AddDays(-3),
                    ExpectedDeliveryDate = DateTime.UtcNow.AddDays(3),
                    Status = "Pending",
                    TotalAmount = 200000.00m,
                    AmountPaid = 0,
                    OutstandingBalance = 200000.00m,
                    PaymentStatus = "Unpaid",
                    Notes = "Pending order for inventory",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new PurchaseOrder
                {
                    PurchaseOrderNumber = "PO004",
                    SupplierId = supplier4.Id,
                    BranchId = alexBranch.Id,
                    OrderDate = DateTime.UtcNow.AddDays(-15),
                    ExpectedDeliveryDate = DateTime.UtcNow.AddDays(15),
                    Status = "Received",
                    TotalAmount = 100000.00m,
                    AmountPaid = 100000.00m,
                    OutstandingBalance = 0,
                    PaymentStatus = "Paid",
                    Notes = "Completed order for inventory",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.PurchaseOrders.AddRangeAsync(purchaseOrders);
            await context.SaveChangesAsync();

            // Add purchase order items
            var purchaseOrderItems = new List<PurchaseOrderItem>
            {
                new PurchaseOrderItem
                {
                    PurchaseOrderId = purchaseOrders[0].Id,
                    ProductId = (await context.Products.FirstAsync(p => p.ProductCode == "BL001")).Id,
                    QuantityOrdered = 10,
                    QuantityReceived = 0,
                    WeightOrdered = 100.000m,
                    WeightReceived = 0,
                    UnitCost = 1450.00m,
                    LineTotal = 14500.00m,
                    Status = "Pending",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new PurchaseOrderItem
                {
                    PurchaseOrderId = purchaseOrders[0].Id,
                    ProductId = (await context.Products.FirstAsync(p => p.ProductCode == "CN001")).Id,
                    QuantityOrdered = 50,
                    QuantityReceived = 0,
                    WeightOrdered = 50.000m,
                    WeightReceived = 0,
                    UnitCost = 1750.00m,
                    LineTotal = 87500.00m,
                    Status = "Pending",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new PurchaseOrderItem
                {
                    PurchaseOrderId = purchaseOrders[1].Id,
                    ProductId = (await context.Products.FirstAsync(p => p.ProductCode == "JW001")).Id,
                    QuantityOrdered = 5,
                    QuantityReceived = 5,
                    WeightOrdered = 17.500m,
                    WeightReceived = 17.500m,
                    UnitCost = 1450.00m,
                    LineTotal = 7250.00m,
                    Status = "Received",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new PurchaseOrderItem
                {
                    PurchaseOrderId = purchaseOrders[2].Id,
                    ProductId = (await context.Products.FirstAsync(p => p.ProductCode == "BL001")).Id,
                    QuantityOrdered = 20,
                    QuantityReceived = 0,
                    WeightOrdered = 200.000m,
                    WeightReceived = 0,
                    UnitCost = 1450.00m,
                    LineTotal = 29000.00m,
                    Status = "Pending",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new PurchaseOrderItem
                {
                    PurchaseOrderId = purchaseOrders[3].Id,
                    ProductId = (await context.Products.FirstAsync(p => p.ProductCode == "CN001")).Id,
                    QuantityOrdered = 100,
                    QuantityReceived = 100,
                    WeightOrdered = 100.000m,
                    WeightReceived = 100.000m,
                    UnitCost = 1750.00m,
                    LineTotal = 175000.00m,
                    Status = "Received",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.PurchaseOrderItems.AddRangeAsync(purchaseOrderItems);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seed orders and financial transactions
    /// </summary>
    private static async Task SeedOrdersAndTransactionsAsync(ApplicationDbContext context)
    {
        if (!await context.Orders.AnyAsync())
        {
            var mainBranch = await context.Branches.FirstAsync(b => b.Code == "MAIN");
            var alexBranch = await context.Branches.FirstAsync(b => b.Code == "ALEX");
            var saleOrderType = await context.OrderTypeLookups.FirstAsync(ot => ot.Name == "Sale");
            var completedStatus = await context.OrderStatusLookups.FirstAsync(os => os.Name == "Completed");
            var pendingStatus = await context.OrderStatusLookups.FirstAsync(os => os.Name == "Pending");
            
            // Get users from the AspNetUsers table
            var cashier = await context.Set<ApplicationUser>().FirstAsync(u => u.Email == "cashier@dijagold.com");
            var manager = await context.Set<ApplicationUser>().FirstAsync(u => u.Email == "manager@dijagold.com");
            var customer1 = await context.Customers.FirstAsync(c => c.FullName == "Ahmed Mohamed Hassan");
            var customer2 = await context.Customers.FirstAsync(c => c.FullName == "Fatima Ali Mahmoud");
            var customer3 = await context.Customers.FirstAsync(c => c.FullName == "Omar Khalil Ibrahim");
            var customer4 = await context.Customers.FirstAsync(c => c.FullName == "Nour El-Din Salah");
            var customer5 = await context.Customers.FirstAsync(c => c.FullName == "Layla Ahmed Mostafa");

            var orders = new List<Order>
            {
                new Order
                {
                    OrderNumber = "ORD001",
                    OrderTypeId = saleOrderType.Id,
                    OrderDate = DateTime.UtcNow.AddDays(-10),
                    BranchId = mainBranch.Id,
                    CustomerId = customer1.Id,
                    CashierId = cashier.Id,
                    StatusId = completedStatus.Id,
                    Notes = "Completed order for customer",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new Order
                {
                    OrderNumber = "ORD002",
                    OrderTypeId = saleOrderType.Id,
                    OrderDate = DateTime.UtcNow.AddDays(-5),
                    BranchId = alexBranch.Id,
                    CustomerId = customer2.Id,
                    CashierId = cashier.Id,
                    StatusId = pendingStatus.Id,
                    Notes = "Pending order for customer",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new Order
                {
                    OrderNumber = "ORD003",
                    OrderTypeId = saleOrderType.Id,
                    OrderDate = DateTime.UtcNow.AddDays(-3),
                    BranchId = mainBranch.Id,
                    CustomerId = customer3.Id,
                    CashierId = cashier.Id,
                    StatusId = completedStatus.Id,
                    Notes = "Completed order for customer",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new Order
                {
                    OrderNumber = "ORD004",
                    OrderTypeId = saleOrderType.Id,
                    OrderDate = DateTime.UtcNow.AddDays(-15),
                    BranchId = alexBranch.Id,
                    CustomerId = customer4.Id,
                    CashierId = cashier.Id,
                    StatusId = pendingStatus.Id,
                    Notes = "Pending order for customer",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new Order
                {
                    OrderNumber = "ORD005",
                    OrderTypeId = saleOrderType.Id,
                    OrderDate = DateTime.UtcNow.AddDays(-90),
                    BranchId = mainBranch.Id,
                    CustomerId = customer5.Id,
                    CashierId = cashier.Id,
                    StatusId = completedStatus.Id,
                    Notes = "Completed order for customer",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Orders.AddRangeAsync(orders);
            await context.SaveChangesAsync();

            // Add order items
            var orderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    OrderId = orders[0].Id,
                    ProductId = (await context.Products.FirstAsync(p => p.ProductCode == "JW001")).Id,
                    Quantity = 1,
                    UnitPrice = 12000.00m,
                    TotalPrice = 12000.00m,
                    DiscountPercentage = 5.00m,
                    DiscountAmount = 600.00m,
                    FinalPrice = 11400.00m,
                    MakingCharges = 1200.00m,
                    TaxAmount = 1680.00m,
                    TotalAmount = 13680.00m,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new OrderItem
                {
                    OrderId = orders[1].Id,
                    ProductId = (await context.Products.FirstAsync(p => p.ProductCode == "JW002")).Id,
                    Quantity = 2,
                    UnitPrice = 2500.00m,
                    TotalPrice = 5000.00m,
                    DiscountPercentage = 3.00m,
                    DiscountAmount = 150.00m,
                    FinalPrice = 4850.00m,
                    MakingCharges = 500.00m,
                    TaxAmount = 700.00m,
                    TotalAmount = 5700.00m,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new OrderItem
                {
                    OrderId = orders[2].Id,
                    ProductId = (await context.Products.FirstAsync(p => p.ProductCode == "BL001")).Id,
                    Quantity = 1,
                    UnitPrice = 10000.00m,
                    TotalPrice = 10000.00m,
                    DiscountPercentage = 0.00m,
                    DiscountAmount = 0.00m,
                    FinalPrice = 10000.00m,
                    MakingCharges = 0.00m,
                    TaxAmount = 1400.00m,
                    TotalAmount = 11400.00m,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new OrderItem
                {
                    OrderId = orders[3].Id,
                    ProductId = (await context.Products.FirstAsync(p => p.ProductCode == "CN001")).Id,
                    Quantity = 5,
                    UnitPrice = 20000.00m,
                    TotalPrice = 100000.00m,
                    DiscountPercentage = 8.00m,
                    DiscountAmount = 8000.00m,
                    FinalPrice = 92000.00m,
                    MakingCharges = 0.00m,
                    TaxAmount = 14000.00m,
                    TotalAmount = 114000.00m,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new OrderItem
                {
                    OrderId = orders[4].Id,
                    ProductId = (await context.Products.FirstAsync(p => p.ProductCode == "BL001")).Id,
                    Quantity = 10,
                    UnitPrice = 150000.00m,
                    TotalPrice = 1500000.00m,
                    DiscountPercentage = 10.00m,
                    DiscountAmount = 150000.00m,
                    FinalPrice = 1350000.00m,
                    MakingCharges = 0.00m,
                    TaxAmount = 210000.00m,
                    TotalAmount = 1710000.00m,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.OrderItems.AddRangeAsync(orderItems);
            await context.SaveChangesAsync();

            // Add financial transactions (Sales)
            var saleTransactionType = await context.FinancialTransactionTypeLookups.FirstAsync(ftt => ftt.Name == "Sale");
            var completedTransactionStatus = await context.FinancialTransactionStatusLookups.FirstAsync(fts => fts.Name == "Completed");
            var orderBusinessEntityType = await context.BusinessEntityTypeLookups.FirstAsync(bet => bet.Name == "Order");
            var cashPaymentMethod = await context.PaymentMethodLookups.FirstAsync(pm => pm.Name == "Cash");

            var financialTransactions = new List<FinancialTransaction>
            {
                new FinancialTransaction
                {
                    TransactionNumber = "TRX001",
                    TransactionDate = DateTime.UtcNow.AddDays(-10),
                    BranchId = mainBranch.Id,
                    TransactionTypeId = saleTransactionType.Id,
                    BusinessEntityId = orders[0].Id,
                    BusinessEntityTypeId = orderBusinessEntityType.Id,
                    ProcessedByUserId = cashier.Id,
                    Subtotal = 11400.00m,
                    TotalTaxAmount = 1680.00m,
                    TotalDiscountAmount = 600.00m,
                    TotalAmount = 13680.00m,
                    AmountPaid = 13680.00m,
                    ChangeGiven = 0,
                    PaymentMethodId = cashPaymentMethod.Id,
                    StatusId = completedTransactionStatus.Id,
                    Notes = "Sale transaction for order ORD001",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new FinancialTransaction
                {
                    TransactionNumber = "TRX002",
                    TransactionDate = DateTime.UtcNow.AddDays(-5),
                    BranchId = alexBranch.Id,
                    TransactionTypeId = saleTransactionType.Id,
                    BusinessEntityId = orders[1].Id,
                    BusinessEntityTypeId = orderBusinessEntityType.Id,
                    ProcessedByUserId = cashier.Id,
                    Subtotal = 4850.00m,
                    TotalTaxAmount = 700.00m,
                    TotalDiscountAmount = 150.00m,
                    TotalAmount = 5700.00m,
                    AmountPaid = 5700.00m,
                    ChangeGiven = 0,
                    PaymentMethodId = cashPaymentMethod.Id,
                    StatusId = completedTransactionStatus.Id,
                    Notes = "Sale transaction for order ORD002",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new FinancialTransaction
                {
                    TransactionNumber = "TRX003",
                    TransactionDate = DateTime.UtcNow.AddDays(-3),
                    BranchId = mainBranch.Id,
                    TransactionTypeId = saleTransactionType.Id,
                    BusinessEntityId = orders[2].Id,
                    BusinessEntityTypeId = orderBusinessEntityType.Id,
                    ProcessedByUserId = cashier.Id,
                    Subtotal = 10000.00m,
                    TotalTaxAmount = 1400.00m,
                    TotalDiscountAmount = 0,
                    TotalAmount = 11400.00m,
                    AmountPaid = 11400.00m,
                    ChangeGiven = 0,
                    PaymentMethodId = cashPaymentMethod.Id,
                    StatusId = completedTransactionStatus.Id,
                    Notes = "Sale transaction for order ORD003",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new FinancialTransaction
                {
                    TransactionNumber = "TRX004",
                    TransactionDate = DateTime.UtcNow.AddDays(-15),
                    BranchId = alexBranch.Id,
                    TransactionTypeId = saleTransactionType.Id,
                    BusinessEntityId = orders[3].Id,
                    BusinessEntityTypeId = orderBusinessEntityType.Id,
                    ProcessedByUserId = cashier.Id,
                    Subtotal = 92000.00m,
                    TotalTaxAmount = 14000.00m,
                    TotalDiscountAmount = 8000.00m,
                    TotalAmount = 114000.00m,
                    AmountPaid = 114000.00m,
                    ChangeGiven = 0,
                    PaymentMethodId = cashPaymentMethod.Id,
                    StatusId = completedTransactionStatus.Id,
                    Notes = "Sale transaction for order ORD004",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new FinancialTransaction
                {
                    TransactionNumber = "TRX005",
                    TransactionDate = DateTime.UtcNow.AddDays(-90),
                    BranchId = mainBranch.Id,
                    TransactionTypeId = saleTransactionType.Id,
                    BusinessEntityId = orders[4].Id,
                    BusinessEntityTypeId = orderBusinessEntityType.Id,
                    ProcessedByUserId = cashier.Id,
                    Subtotal = 1350000.00m,
                    TotalTaxAmount = 210000.00m,
                    TotalDiscountAmount = 150000.00m,
                    TotalAmount = 1710000.00m,
                    AmountPaid = 1710000.00m,
                    ChangeGiven = 0,
                    PaymentMethodId = cashPaymentMethod.Id,
                    StatusId = completedTransactionStatus.Id,
                    Notes = "Sale transaction for order ORD005",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.FinancialTransactions.AddRangeAsync(financialTransactions);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seed repair jobs
    /// </summary>
    private static async Task SeedRepairJobsAsync(ApplicationDbContext context)
    {
        if (!await context.RepairJobs.AnyAsync())
        {
            var completedRepairStatus = await context.RepairStatusLookups.FirstAsync(rs => rs.Name == "Completed");
            var pendingRepairStatus = await context.RepairStatusLookups.FirstAsync(rs => rs.Name == "Pending");
            var highPriority = await context.RepairPriorityLookups.FirstAsync(rp => rp.Name == "High");
            var mediumPriority = await context.RepairPriorityLookups.FirstAsync(rp => rp.Name == "Medium");
            var lowPriority = await context.RepairPriorityLookups.FirstAsync(rp => rp.Name == "Low");
            var technician1 = await context.Technicians.FirstAsync(t => t.FullName == "Mohamed El-Sayed Hassan");
            var technician2 = await context.Technicians.FirstAsync(t => t.FullName == "Ahmed Ali Mahmoud");
            var technician3 = await context.Technicians.FirstAsync(t => t.FullName == "Fatima Omar Ibrahim");
            var technician4 = await context.Technicians.FirstAsync(t => t.FullName == "Khalil Mostafa El-Sayed");

            var repairJobs = new List<RepairJob>
            {
                new RepairJob
                {
                    StatusId = completedRepairStatus.Id,
                    PriorityId = highPriority.Id,
                    AssignedTechnicianId = technician1.Id,
                    StartedDate = DateTime.UtcNow.AddDays(-12),
                    CompletedDate = DateTime.UtcNow.AddDays(-10),
                    ReadyForPickupDate = DateTime.UtcNow.AddDays(-10),
                    DeliveredDate = DateTime.UtcNow.AddDays(-9),
                    TechnicianNotes = "Ring resizing completed successfully. Customer satisfied with the work.",
                    ActualCost = 1500.00m,
                    MaterialsUsed = "Gold solder, polishing compound",
                    HoursSpent = 2.50m,
                    QualityCheckedBy = technician2.Id,
                    QualityCheckDate = DateTime.UtcNow.AddDays(-10),
                    CustomerNotified = true,
                    CustomerNotificationDate = DateTime.UtcNow.AddDays(-10),
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new RepairJob
                {
                    StatusId = pendingRepairStatus.Id,
                    PriorityId = mediumPriority.Id,
                    AssignedTechnicianId = technician2.Id,
                    StartedDate = DateTime.UtcNow.AddDays(-5),
                    TechnicianNotes = "Chain repair in progress. Need to replace broken links.",
                    ActualCost = 800.00m,
                    MaterialsUsed = "Gold chain links, solder",
                    HoursSpent = 1.00m,
                    CustomerNotified = false,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new RepairJob
                {
                    StatusId = completedRepairStatus.Id,
                    PriorityId = lowPriority.Id,
                    AssignedTechnicianId = technician3.Id,
                    StartedDate = DateTime.UtcNow.AddDays(-5),
                    CompletedDate = DateTime.UtcNow.AddDays(-3),
                    ReadyForPickupDate = DateTime.UtcNow.AddDays(-3),
                    DeliveredDate = DateTime.UtcNow.AddDays(-2),
                    TechnicianNotes = "Bracelet repair completed. Added missing clasp.",
                    ActualCost = 2000.00m,
                    MaterialsUsed = "Gold clasp, solder, polishing materials",
                    HoursSpent = 3.00m,
                    QualityCheckedBy = technician1.Id,
                    QualityCheckDate = DateTime.UtcNow.AddDays(-3),
                    CustomerNotified = true,
                    CustomerNotificationDate = DateTime.UtcNow.AddDays(-3),
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new RepairJob
                {
                    StatusId = pendingRepairStatus.Id,
                    PriorityId = highPriority.Id,
                    AssignedTechnicianId = technician4.Id,
                    StartedDate = DateTime.UtcNow.AddDays(-15),
                    TechnicianNotes = "Complex necklace repair. Multiple broken links need replacement.",
                    ActualCost = 1500.00m,
                    MaterialsUsed = "Gold links, solder, specialized tools",
                    HoursSpent = 4.50m,
                    CustomerNotified = false,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new RepairJob
                {
                    StatusId = completedRepairStatus.Id,
                    PriorityId = highPriority.Id,
                    AssignedTechnicianId = technician1.Id,
                    StartedDate = DateTime.UtcNow.AddDays(-95),
                    CompletedDate = DateTime.UtcNow.AddDays(-90),
                    ReadyForPickupDate = DateTime.UtcNow.AddDays(-90),
                    DeliveredDate = DateTime.UtcNow.AddDays(-89),
                    TechnicianNotes = "Major restoration work completed. Antique piece fully restored to original condition.",
                    ActualCost = 18000.00m,
                    MaterialsUsed = "Specialized restoration materials, gold plating, antique tools",
                    HoursSpent = 25.00m,
                    QualityCheckedBy = technician2.Id,
                    QualityCheckDate = DateTime.UtcNow.AddDays(-90),
                    CustomerNotified = true,
                    CustomerNotificationDate = DateTime.UtcNow.AddDays(-90),
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.RepairJobs.AddRangeAsync(repairJobs);
            await context.SaveChangesAsync();

            // Add financial transactions for repair jobs
            var repairTransactionType = await context.FinancialTransactionTypeLookups.FirstAsync(ftt => ftt.Name == "Repair");
            var completedTransactionStatus = await context.FinancialTransactionStatusLookups.FirstAsync(fts => fts.Name == "Completed");
            var repairJobBusinessEntityType = await context.BusinessEntityTypeLookups.FirstAsync(bet => bet.Name == "RepairJob");
            var cashPaymentMethod = await context.PaymentMethodLookups.FirstAsync(pm => pm.Name == "Cash");
            var cashier = await context.Set<ApplicationUser>().FirstAsync(u => u.Email == "cashier@dijagold.com");
            var mainBranch = await context.Branches.FirstAsync(b => b.Code == "MAIN");
            var alexBranch = await context.Branches.FirstAsync(b => b.Code == "ALEX");

            var financialTransactions = new List<FinancialTransaction>
            {
                new FinancialTransaction
                {
                    TransactionNumber = "TRX006",
                    TransactionDate = DateTime.UtcNow.AddDays(-10),
                    BranchId = mainBranch.Id,
                    TransactionTypeId = repairTransactionType.Id,
                    BusinessEntityId = repairJobs[0].Id,
                    BusinessEntityTypeId = repairJobBusinessEntityType.Id,
                    ProcessedByUserId = cashier.Id,
                    Subtotal = 1500.00m,
                    TotalTaxAmount = 210.00m,
                    TotalDiscountAmount = 0,
                    TotalAmount = 1710.00m,
                    AmountPaid = 1710.00m,
                    ChangeGiven = 0,
                    PaymentMethodId = cashPaymentMethod.Id,
                    StatusId = completedTransactionStatus.Id,
                    Notes = "Repair transaction for RJ001",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new FinancialTransaction
                {
                    TransactionNumber = "TRX007",
                    TransactionDate = DateTime.UtcNow.AddDays(-5),
                    BranchId = alexBranch.Id,
                    TransactionTypeId = repairTransactionType.Id,
                    BusinessEntityId = repairJobs[1].Id,
                    BusinessEntityTypeId = repairJobBusinessEntityType.Id,
                    ProcessedByUserId = cashier.Id,
                    Subtotal = 800.00m,
                    TotalTaxAmount = 112.00m,
                    TotalDiscountAmount = 0,
                    TotalAmount = 912.00m,
                    AmountPaid = 0,
                    ChangeGiven = 0,
                    PaymentMethodId = cashPaymentMethod.Id,
                    StatusId = completedTransactionStatus.Id,
                    Notes = "Repair transaction for RJ002",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new FinancialTransaction
                {
                    TransactionNumber = "TRX008",
                    TransactionDate = DateTime.UtcNow.AddDays(-3),
                    BranchId = mainBranch.Id,
                    TransactionTypeId = repairTransactionType.Id,
                    BusinessEntityId = repairJobs[2].Id,
                    BusinessEntityTypeId = repairJobBusinessEntityType.Id,
                    ProcessedByUserId = cashier.Id,
                    Subtotal = 2000.00m,
                    TotalTaxAmount = 280.00m,
                    TotalDiscountAmount = 0,
                    TotalAmount = 2280.00m,
                    AmountPaid = 2280.00m,
                    ChangeGiven = 0,
                    PaymentMethodId = cashPaymentMethod.Id,
                    StatusId = completedTransactionStatus.Id,
                    Notes = "Repair transaction for RJ003",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new FinancialTransaction
                {
                    TransactionNumber = "TRX009",
                    TransactionDate = DateTime.UtcNow.AddDays(-15),
                    BranchId = alexBranch.Id,
                    TransactionTypeId = repairTransactionType.Id,
                    BusinessEntityId = repairJobs[3].Id,
                    BusinessEntityTypeId = repairJobBusinessEntityType.Id,
                    ProcessedByUserId = cashier.Id,
                    Subtotal = 1500.00m,
                    TotalTaxAmount = 210.00m,
                    TotalDiscountAmount = 0,
                    TotalAmount = 1710.00m,
                    AmountPaid = 0,
                    ChangeGiven = 0,
                    PaymentMethodId = cashPaymentMethod.Id,
                    StatusId = completedTransactionStatus.Id,
                    Notes = "Repair transaction for RJ004",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new FinancialTransaction
                {
                    TransactionNumber = "TRX010",
                    TransactionDate = DateTime.UtcNow.AddDays(-90),
                    BranchId = mainBranch.Id,
                    TransactionTypeId = repairTransactionType.Id,
                    BusinessEntityId = repairJobs[4].Id,
                    BusinessEntityTypeId = repairJobBusinessEntityType.Id,
                    ProcessedByUserId = cashier.Id,
                    Subtotal = 18000.00m,
                    TotalTaxAmount = 2520.00m,
                    TotalDiscountAmount = 0,
                    TotalAmount = 20520.00m,
                    AmountPaid = 20520.00m,
                    ChangeGiven = 0,
                    PaymentMethodId = cashPaymentMethod.Id,
                    StatusId = completedTransactionStatus.Id,
                    Notes = "Repair transaction for RJ005",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.FinancialTransactions.AddRangeAsync(financialTransactions);
            await context.SaveChangesAsync();
        }
    }
}