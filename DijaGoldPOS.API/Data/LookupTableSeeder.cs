using DijaGoldPOS.API.Models.LookupTables;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Data;

/// <summary>
/// Seeds lookup tables with initial data
/// </summary>
public static class LookupTableSeeder
{
    /// <summary>
    /// Seeds all lookup tables with initial data
    /// </summary>
    public static async Task SeedLookupTablesAsync(ApplicationDbContext context)
    {
        await SeedOrderTypesAsync(context);
        await SeedOrderStatusesAsync(context);
        await SeedFinancialTransactionTypesAsync(context);
        await SeedFinancialTransactionStatusesAsync(context);
        await SeedBusinessEntityTypesAsync(context);
        await SeedRepairStatusesAsync(context);
        await SeedRepairPrioritiesAsync(context);
        await SeedKaratTypesAsync(context);
        await SeedProductCategoryTypesAsync(context);
        await SeedTransactionTypesAsync(context);
        await SeedPaymentMethodsAsync(context);
        await SeedTransactionStatusesAsync(context);
        await SeedChargeTypesAsync(context);
        await SeedSubCategoriesAsync(context);
        
        await context.SaveChangesAsync();
    }

    private static async Task SeedOrderTypesAsync(ApplicationDbContext context)
    {
        if (await context.OrderTypeLookups.AnyAsync()) return;

        var orderTypes = new[]
        {
            new OrderTypeLookup { Name = "Sale", Description = "Sale order", SortOrder = 1 },
            new OrderTypeLookup { Name = "Return", Description = "Return order", SortOrder = 2 },
            new OrderTypeLookup { Name = "Exchange", Description = "Exchange order", SortOrder = 3 },
            new OrderTypeLookup { Name = "Layaway", Description = "Layaway order", SortOrder = 4 },
            new OrderTypeLookup { Name = "Reservation", Description = "Reservation order", SortOrder = 5 }
        };

        await context.OrderTypeLookups.AddRangeAsync(orderTypes);
    }

    private static async Task SeedOrderStatusesAsync(ApplicationDbContext context)
    {
        if (await context.OrderStatusLookups.AnyAsync()) return;

        var orderStatuses = new[]
        {
            new OrderStatusLookup { Name = "Pending", Description = "Order is pending", SortOrder = 1 },
            new OrderStatusLookup { Name = "Confirmed", Description = "Order is confirmed", SortOrder = 2 },
            new OrderStatusLookup { Name = "In Progress", Description = "Order is in progress", SortOrder = 3 },
            new OrderStatusLookup { Name = "Completed", Description = "Order is completed", SortOrder = 4 },
            new OrderStatusLookup { Name = "Cancelled", Description = "Order is cancelled", SortOrder = 5 },
            new OrderStatusLookup { Name = "On Hold", Description = "Order is on hold", SortOrder = 6 },
            new OrderStatusLookup { Name = "Ready for Pickup", Description = "Order is ready for pickup", SortOrder = 7 },
            new OrderStatusLookup { Name = "Delivered", Description = "Order is delivered", SortOrder = 8 },
            new OrderStatusLookup { Name = "Returned", Description = "Order is returned", SortOrder = 9 },
            new OrderStatusLookup { Name = "Exchanged", Description = "Order is exchanged", SortOrder = 10 }
        };

        await context.OrderStatusLookups.AddRangeAsync(orderStatuses);
    }

    private static async Task SeedFinancialTransactionTypesAsync(ApplicationDbContext context)
    {
        if (await context.FinancialTransactionTypeLookups.AnyAsync()) return;

        var transactionTypes = new[]
        {
            new FinancialTransactionTypeLookup { Name = "Sale", Description = "Sale transaction", SortOrder = 1 },
            new FinancialTransactionTypeLookup { Name = "Return", Description = "Return transaction", SortOrder = 2 },
            new FinancialTransactionTypeLookup { Name = "Repair", Description = "Repair service transaction", SortOrder = 3 },
            new FinancialTransactionTypeLookup { Name = "Exchange", Description = "Exchange transaction", SortOrder = 4 },
            new FinancialTransactionTypeLookup { Name = "Refund", Description = "Refund transaction", SortOrder = 5 },
            new FinancialTransactionTypeLookup { Name = "Adjustment", Description = "Adjustment transaction", SortOrder = 6 },
            new FinancialTransactionTypeLookup { Name = "Void", Description = "Void transaction", SortOrder = 7 }
        };

        await context.FinancialTransactionTypeLookups.AddRangeAsync(transactionTypes);
    }

    private static async Task SeedFinancialTransactionStatusesAsync(ApplicationDbContext context)
    {
        if (await context.FinancialTransactionStatusLookups.AnyAsync()) return;

        var transactionStatuses = new[]
        {
            new FinancialTransactionStatusLookup { Name = "Pending", Description = "Transaction is pending", SortOrder = 1 },
            new FinancialTransactionStatusLookup { Name = "Completed", Description = "Transaction is completed", SortOrder = 2 },
            new FinancialTransactionStatusLookup { Name = "Cancelled", Description = "Transaction is cancelled", SortOrder = 3 },
            new FinancialTransactionStatusLookup { Name = "Refunded", Description = "Transaction is refunded", SortOrder = 4 },
            new FinancialTransactionStatusLookup { Name = "Voided", Description = "Transaction is voided", SortOrder = 5 },
            new FinancialTransactionStatusLookup { Name = "Reversed", Description = "Transaction is reversed", SortOrder = 6 }
        };

        await context.FinancialTransactionStatusLookups.AddRangeAsync(transactionStatuses);
    }

    private static async Task SeedBusinessEntityTypesAsync(ApplicationDbContext context)
    {
        if (await context.BusinessEntityTypeLookups.AnyAsync()) return;

        var entityTypes = new[]
        {
            new BusinessEntityTypeLookup { Name = "Order", Description = "Order entity", SortOrder = 1 },
            new BusinessEntityTypeLookup { Name = "RepairJob", Description = "Repair job entity", SortOrder = 2 },
            new BusinessEntityTypeLookup { Name = "PurchaseOrder", Description = "Purchase order entity", SortOrder = 3 },
            new BusinessEntityTypeLookup { Name = "SupplierTransaction", Description = "Supplier transaction entity", SortOrder = 4 }
        };

        await context.BusinessEntityTypeLookups.AddRangeAsync(entityTypes);
    }

    private static async Task SeedRepairStatusesAsync(ApplicationDbContext context)
    {
        if (await context.RepairStatusLookups.AnyAsync()) return;

        var repairStatuses = new[]
        {
            new RepairStatusLookup { Name = "Pending", Description = "Repair job is pending and waiting to be started", SortOrder = 1 },
            new RepairStatusLookup { Name = "In Progress", Description = "Repair work is in progress", SortOrder = 2 },
            new RepairStatusLookup { Name = "Completed", Description = "Repair work is completed", SortOrder = 3 },
            new RepairStatusLookup { Name = "Ready for Pickup", Description = "Repair is ready for customer pickup", SortOrder = 4 },
            new RepairStatusLookup { Name = "Delivered", Description = "Repair has been delivered to customer", SortOrder = 5 },
            new RepairStatusLookup { Name = "Cancelled", Description = "Repair job has been cancelled", SortOrder = 6 }
        };

        await context.RepairStatusLookups.AddRangeAsync(repairStatuses);
    }

    private static async Task SeedRepairPrioritiesAsync(ApplicationDbContext context)
    {
        if (await context.RepairPriorityLookups.AnyAsync()) return;

        var repairPriorities = new[]
        {
            new RepairPriorityLookup { Name = "Low", Description = "Low priority repair", SortOrder = 1 },
            new RepairPriorityLookup { Name = "Medium", Description = "Medium priority repair", SortOrder = 2 },
            new RepairPriorityLookup { Name = "High", Description = "High priority repair", SortOrder = 3 },
            new RepairPriorityLookup { Name = "Urgent", Description = "Urgent priority repair", SortOrder = 4 }
        };

        await context.RepairPriorityLookups.AddRangeAsync(repairPriorities);
    }

    private static async Task SeedKaratTypesAsync(ApplicationDbContext context)
    {
        if (await context.KaratTypeLookups.AnyAsync()) return;

        var karatTypes = new[]
        {
            new KaratTypeLookup { Name = "18K Gold", Description = "18 Karat Gold", KaratValue = 18, SortOrder = 1 },
            new KaratTypeLookup { Name = "21K Gold", Description = "21 Karat Gold (Popular in Egypt)", KaratValue = 21, SortOrder = 2 },
            new KaratTypeLookup { Name = "22K Gold", Description = "22 Karat Gold", KaratValue = 22, SortOrder = 3 },
            new KaratTypeLookup { Name = "24K Gold", Description = "24 Karat Gold (Pure Gold)", KaratValue = 24, SortOrder = 4 }
        };

        await context.KaratTypeLookups.AddRangeAsync(karatTypes);
    }

    private static async Task SeedProductCategoryTypesAsync(ApplicationDbContext context)
    {
        if (await context.ProductCategoryTypeLookups.AnyAsync()) return;

        var productCategories = new[]
        {
            new ProductCategoryTypeLookup { Name = "Gold Jewelry", Description = "Gold Jewelry - rings, necklaces, bracelets, earrings, etc.", SortOrder = 1 },
            new ProductCategoryTypeLookup { Name = "Bullion", Description = "Bullion - bars, ingots (0.25g to 100g)", SortOrder = 2 },
            new ProductCategoryTypeLookup { Name = "Gold Coins", Description = "Coins - gold coins (0.25g to 1g)", SortOrder = 3 }
        };

        await context.ProductCategoryTypeLookups.AddRangeAsync(productCategories);
    }

    private static async Task SeedTransactionTypesAsync(ApplicationDbContext context)
    {
        if (await context.TransactionTypeLookups.AnyAsync()) return;

        var transactionTypes = new[]
        {
            new TransactionTypeLookup { Name = "Sale", Description = "Sales transaction", SortOrder = 1 },
            new TransactionTypeLookup { Name = "Return", Description = "Return transaction (requires manager approval)", SortOrder = 2 },
            new TransactionTypeLookup { Name = "Repair", Description = "Repair service transaction", SortOrder = 3 }
        };

        await context.TransactionTypeLookups.AddRangeAsync(transactionTypes);
    }

    private static async Task SeedPaymentMethodsAsync(ApplicationDbContext context)
    {
        if (await context.PaymentMethodLookups.AnyAsync()) return;

        var paymentMethods = new[]
        {
            new PaymentMethodLookup { Name = "Cash", Description = "Cash payment (Egyptian Pounds)", SortOrder = 1 }
        };

        await context.PaymentMethodLookups.AddRangeAsync(paymentMethods);
    }

    private static async Task SeedTransactionStatusesAsync(ApplicationDbContext context)
    {
        if (await context.TransactionStatusLookups.AnyAsync()) return;

        var transactionStatuses = new[]
        {
            new TransactionStatusLookup { Name = "Pending", Description = "Transaction is pending", SortOrder = 1 },
            new TransactionStatusLookup { Name = "Completed", Description = "Transaction is completed", SortOrder = 2 },
            new TransactionStatusLookup { Name = "Cancelled", Description = "Transaction is cancelled", SortOrder = 3 },
            new TransactionStatusLookup { Name = "Refunded", Description = "Transaction is refunded", SortOrder = 4 },
            new TransactionStatusLookup { Name = "Voided", Description = "Transaction is voided", SortOrder = 5 }
        };

        await context.TransactionStatusLookups.AddRangeAsync(transactionStatuses);
    }

    private static async Task SeedChargeTypesAsync(ApplicationDbContext context)
    {
        if (await context.ChargeTypeLookups.AnyAsync()) return;

        var chargeTypes = new[]
        {
            new ChargeTypeLookup { Name = "Percentage", Description = "Percentage-based charge", SortOrder = 1 },
            new ChargeTypeLookup { Name = "Fixed Amount", Description = "Fixed amount charge", SortOrder = 2 }
        };

        await context.ChargeTypeLookups.AddRangeAsync(chargeTypes);
    }

    private static async Task SeedSubCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.SubCategoryLookups.AnyAsync()) return;

        var subCategories = new[]
        {
            new SubCategoryLookup { Name = "Rings", Description = "Wedding rings, engagement rings, fashion rings", SortOrder = 1 },
            new SubCategoryLookup { Name = "Necklaces", Description = "Chains, pendants, chokers", SortOrder = 2 },
            new SubCategoryLookup { Name = "Bracelets", Description = "Bangles, chain bracelets, charm bracelets", SortOrder = 3 },
            new SubCategoryLookup { Name = "Earrings", Description = "Studs, hoops, drop earrings, chandelier earrings", SortOrder = 4 },
            new SubCategoryLookup { Name = "Pendants", Description = "Necklace pendants, lockets", SortOrder = 5 },
            new SubCategoryLookup { Name = "Chains", Description = "Necklace chains, bracelet chains", SortOrder = 6 },
            new SubCategoryLookup { Name = "Bangles", Description = "Traditional bangles, modern bangles", SortOrder = 7 },
            new SubCategoryLookup { Name = "Anklets", Description = "Ankle bracelets, foot chains", SortOrder = 8 },
            new SubCategoryLookup { Name = "Watches", Description = "Gold watches, luxury timepieces", SortOrder = 9 },
            new SubCategoryLookup { Name = "Brooches", Description = "Decorative brooches, pins", SortOrder = 10 },
            new SubCategoryLookup { Name = "Bars", Description = "Gold bars, ingots", SortOrder = 11 },
            new SubCategoryLookup { Name = "Coins", Description = "Gold coins, commemorative coins", SortOrder = 12 },
            new SubCategoryLookup { Name = "Sets", Description = "Jewelry sets, matching pieces", SortOrder = 13 },
            new SubCategoryLookup { Name = "Other", Description = "Other jewelry items", SortOrder = 14 }
        };

        await context.SubCategoryLookups.AddRangeAsync(subCategories);
    }
}
