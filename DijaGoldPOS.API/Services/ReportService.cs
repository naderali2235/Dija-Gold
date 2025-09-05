using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Models;

using DijaGoldPOS.API.Shared;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Reporting service implementation
/// </summary>
public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReportService> _logger;
    private readonly IPricingService _pricingService;
    private readonly ICashDrawerService _cashDrawerService;

    public ReportService(
        ApplicationDbContext context,
        ILogger<ReportService> logger,
        IPricingService pricingService,
        ICashDrawerService cashDrawerService)
    {
        _context = context;
        _logger = logger;
        _pricingService = pricingService;
        _cashDrawerService = cashDrawerService;
    }

    /// <summary>
    /// Generate daily sales summary report
    /// </summary>
    public async Task<DailySalesSummaryReport> GetDailySalesSummaryAsync(int branchId, DateTime date)
    {
        try
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            var branch = await _context.Branches.FindAsync(branchId);
            if (branch == null)
                throw new ArgumentException($"Branch {branchId} not found");

            var financialTransactions = await _context.FinancialTransactions
                .Where(t => t.BranchId == branchId && 
                           t.TransactionDate >= startDate && 
                           t.TransactionDate < endDate &&
                           (t.StatusId == LookupTableConstants.FinancialTransactionStatusCompleted || t.StatusId == LookupTableConstants.FinancialTransactionStatusPending))
                .ToListAsync();

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.BranchId == branchId && 
                           o.OrderDate >= startDate && 
                           o.OrderDate < endDate &&
                           (o.StatusId == LookupTableConstants.OrderStatusCompleted || o.StatusId == LookupTableConstants.OrderStatusPending))
                .ToListAsync();

            var salesTransactions = financialTransactions.Where(t => t.TransactionTypeId == LookupTableConstants.FinancialTransactionTypeSale);
            var returnTransactions = financialTransactions.Where(t => t.TransactionTypeId == LookupTableConstants.FinancialTransactionTypeReturn);

            var totalSales = salesTransactions.Sum(t => t.TotalAmount);
            var totalReturns = returnTransactions.Sum(t => t.TotalAmount);

            // Category breakdown
            var categoryBreakdown = orders
                .Where(o => o.OrderTypeId == LookupTableConstants.OrderTypeSale)
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.Product!.CategoryTypeId)
                .Select(g => new CategorySalesBreakdown
                {
                    CategoryId = g.Key,
                    TotalSales = g.Sum(oi => oi.TotalAmount),
                    TotalWeight = g.Sum(oi => oi.Product.Weight * oi.Quantity),
                    TransactionCount = g.Select(oi => oi.OrderId).Distinct().Count()
                })
                .ToList();

            // Payment method breakdown
            var paymentBreakdown = salesTransactions
                .GroupBy(t => t.PaymentMethodId)
                .Select(g => new PaymentMethodBreakdown
                {
                    PaymentMethodId = g.Key,
                    Amount = g.Sum(t => t.TotalAmount),
                    TransactionCount = g.Count()
                })
                .ToList();

            return new DailySalesSummaryReport
            {
                BranchId = branchId,
                BranchName = branch.Name,
                ReportDate = date,
                TotalSales = totalSales,
                TotalReturns = totalReturns,
                NetSales = totalSales - totalReturns,
                TransactionCount = salesTransactions.Count(),
                CategoryBreakdown = categoryBreakdown,
                PaymentMethodBreakdown = paymentBreakdown
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating daily sales summary for branch {BranchId} on {Date}", branchId, date);
            throw;
        }
    }

    /// <summary>
    /// Generate cash reconciliation report
    /// </summary>
    public async Task<CashReconciliationReport> GetCashReconciliationAsync(int branchId, DateTime date)
    {
        try
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            var branch = await _context.Branches.FindAsync(branchId);
            if (branch == null)
                throw new ArgumentException($"Branch {branchId} not found");

            var cashTransactions = await _context.FinancialTransactions
                .Where(t => t.BranchId == branchId && 
                           t.TransactionDate >= startDate && 
                           t.TransactionDate < endDate &&
                           t.PaymentMethodId == LookupTableConstants.PaymentMethodCash &&
                           t.StatusId == LookupTableConstants.FinancialTransactionStatusCompleted)
                .ToListAsync();

            var cashSales = cashTransactions
                .Where(t => t.TransactionTypeId == LookupTableConstants.FinancialTransactionTypeSale)
                .Sum(t => t.AmountPaid);

            var cashReturns = cashTransactions
                .Where(t => t.TransactionTypeId == LookupTableConstants.FinancialTransactionTypeReturn)
                .Sum(t => Math.Abs(t.ChangeGiven)); // Returns have negative change (refunds)

            var cashRepairs = cashTransactions
                .Where(t => t.TransactionTypeId == LookupTableConstants.FinancialTransactionTypeRepair)
                .Sum(t => t.AmountPaid);

            // Get opening balance from cash drawer system
            var openingBalance = await _cashDrawerService.GetOpeningBalanceAsync(branchId, date);
            
            var expectedClosingBalance = openingBalance + cashSales + cashRepairs - cashReturns;
            
            // Get current cash drawer balance for actual closing balance
            var cashDrawerBalance = await _cashDrawerService.GetBalanceAsync(branchId, date);
            var actualClosingBalance = cashDrawerBalance?.ActualClosingBalance ?? expectedClosingBalance;

            return new CashReconciliationReport
            {
                BranchId = branchId,
                BranchName = branch.Name,
                ReportDate = date,
                OpeningBalance = openingBalance,
                CashSales = cashSales,
                CashReturns = cashReturns,
                CashRepairs = cashRepairs,
                ExpectedClosingBalance = expectedClosingBalance,
                ActualClosingBalance = actualClosingBalance,
                CashOverShort = actualClosingBalance - expectedClosingBalance
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cash reconciliation for branch {BranchId} on {Date}", branchId, date);
            throw;
        }
    }

    /// <summary>
    /// Generate inventory movement report
    /// </summary>
    public async Task<InventoryMovementReport> GetInventoryMovementReportAsync(int branchId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var branch = await _context.Branches.FindAsync(branchId);
            if (branch == null)
                throw new ArgumentException($"Branch {branchId} not found");

            var movements = await _context.InventoryMovements
                .Include(im => im.Inventory)
                .ThenInclude(i => i.Product)
                .Where(im => im.Inventory.BranchId == branchId &&
                            im.CreatedAt >= fromDate &&
                            im.CreatedAt <= toDate)
                .GroupBy(im => im.Inventory.ProductId)
                .Select(g => new InventoryMovementSummary
                {
                    ProductId = g.Key,
                    ProductName = g.First().Inventory.Product.Name,
                    CategoryId = g.First().Inventory.Product.CategoryTypeId,
                    Purchases = g.Where(im => im.MovementType == "Purchase").Sum(im => im.QuantityChange),
                    Sales = Math.Abs(g.Where(im => im.MovementType == "Sale").Sum(im => im.QuantityChange)),
                    Returns = g.Where(im => im.MovementType == "Return").Sum(im => im.QuantityChange),
                    Adjustments = g.Where(im => im.MovementType == "Adjustment").Sum(im => im.QuantityChange),
                    Transfers = g.Where(im => im.MovementType.Contains("Transfer")).Sum(im => im.QuantityChange)
                })
                .ToListAsync();

            // Calculate opening and closing quantities (simplified - in reality would need more complex logic)
            foreach (var movement in movements)
            {
                var currentInventory = await _context.Inventories
                    .Include(i => i.Product)
                    .Where(i => i.ProductId == movement.ProductId && i.BranchId == branchId)
                    .FirstOrDefaultAsync();

                movement.ClosingQuantity = currentInventory?.QuantityOnHand ?? 0;
                movement.ClosingWeight = currentInventory?.WeightOnHand ?? 0;
                
                // Opening = Closing - Net Movement
                var netMovement = movement.Purchases - movement.Sales + movement.Returns + movement.Adjustments + movement.Transfers;
                movement.OpeningQuantity = movement.ClosingQuantity - netMovement;
                
                // Calculate opening weight with null safety
                var productWeight = currentInventory?.Product?.Weight ?? 0;
                movement.OpeningWeight = movement.ClosingWeight - (netMovement * productWeight);
            }

            return new InventoryMovementReport
            {
                BranchId = branchId,
                BranchName = branch.Name,
                FromDate = fromDate,
                ToDate = toDate,
                Movements = movements
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating inventory movement report for branch {BranchId}", branchId);
            throw;
        }
    }

    /// <summary>
    /// Generate profit analysis report
    /// </summary>
    public async Task<ProfitAnalysisReport> GetProfitAnalysisReportAsync(int? branchId, DateTime fromDate, DateTime toDate, int? categoryTypeId = null)
    {
        try
        {
            var financialTransactionsQuery = _context.FinancialTransactions
                .Include(t => t.Branch)
                .Where(t => t.TransactionDate >= fromDate &&
                           t.TransactionDate <= toDate &&
                           t.TransactionTypeId == LookupTableConstants.FinancialTransactionTypeSale &&
                           t.StatusId == LookupTableConstants.FinancialTransactionStatusCompleted);

            if (branchId.HasValue)
                financialTransactionsQuery = financialTransactionsQuery.Where(t => t.BranchId == branchId.Value);

            var ordersQuery = _context.Orders
                .Include(o => o.Branch)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.OrderDate >= fromDate &&
                           o.OrderDate <= toDate &&
                           o.OrderTypeId == LookupTableConstants.OrderTypeSale &&
                           o.StatusId == LookupTableConstants.OrderStatusCompleted);

            if (branchId.HasValue)
                ordersQuery = ordersQuery.Where(o => o.BranchId == branchId.Value);

            if (categoryTypeId.HasValue)
                ordersQuery = ordersQuery.Where(o => o.OrderItems.Any(oi => oi.Product!.CategoryTypeId == categoryTypeId.Value));

            var financialTransactions = await financialTransactionsQuery.ToListAsync();
            var orders = await ordersQuery.ToListAsync();

            var totalRevenue = financialTransactions.Sum(t => t.TotalAmount);

            // Simplified cost calculation - in reality, would use actual purchase costs
            var totalCostOfGoodsSold = orders
                .SelectMany(o => o.OrderItems)
                .Sum(oi => oi.Product.Weight * oi.Quantity * 100 * 0.85m); // Assuming 85% of gold rate as cost

            var grossProfit = totalRevenue - totalCostOfGoodsSold;
            var grossProfitMargin = totalRevenue > 0 ? (grossProfit / totalRevenue) * 100 : 0;

            // Product analysis
            var productAnalysis = orders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new ProductProfitAnalysis
                {
                    ProductId = g.Key,
                    ProductName = g.First().Product!.Name,
                    CategoryId = g.First().Product!.CategoryTypeId,
                    Revenue = g.Sum(oi => oi.TotalAmount),
                    CostOfGoodsSold = g.Sum(oi => oi.Product.Weight * oi.Quantity * 100 * 0.85m),
                    QuantitySold = g.Sum(oi => oi.Quantity)
                })
                .ToList();

            foreach (var product in productAnalysis)
            {
                product.GrossProfit = product.Revenue - product.CostOfGoodsSold;
                product.GrossProfitMargin = product.Revenue > 0 ? (product.GrossProfit / product.Revenue) * 100 : 0;
            }

            var branch = branchId.HasValue ? await _context.Branches.FindAsync(branchId.Value) : null;

            return new ProfitAnalysisReport
            {
                BranchId = branchId,
                BranchName = branch?.Name,
                FromDate = fromDate,
                ToDate = toDate,
                TotalRevenue = totalRevenue,
                TotalCostOfGoodsSold = totalCostOfGoodsSold,
                GrossProfit = grossProfit,
                GrossProfitMargin = grossProfitMargin,
                ProductAnalysis = productAnalysis.OrderByDescending(p => p.Revenue).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating profit analysis report");
            throw;
        }
    }

    /// <summary>
    /// Generate customer analysis report
    /// </summary>
    public async Task<CustomerAnalysisReport> GetCustomerAnalysisReportAsync(int? branchId, DateTime fromDate, DateTime toDate, int topCustomersCount = 20)
    {
        try
        {
            var ordersQuery = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Branch)
                .Where(o => o.OrderDate >= fromDate &&
                           o.OrderDate <= toDate &&
                           o.OrderTypeId == LookupTableConstants.OrderTypeSale &&
                           o.StatusId == LookupTableConstants.OrderStatusCompleted &&
                           o.CustomerId.HasValue);

            if (branchId.HasValue)
                ordersQuery = ordersQuery.Where(o => o.BranchId == branchId.Value);

            var customerOrders = await ordersQuery.ToListAsync();

            var topCustomers = customerOrders
                .GroupBy(o => o.CustomerId!.Value)
                .Select(g => new CustomerAnalysisSummary
                {
                    CustomerId = g.Key,
                    CustomerName = g.First().Customer!.FullName,
                    TotalPurchases = g.Sum(o => o.OrderItems.Sum(oi => oi.TotalAmount)),
                    TransactionCount = g.Count(),
                    LastPurchaseDate = g.Max(o => o.OrderDate)
                })
                .OrderByDescending(c => c.TotalPurchases)
                .Take(topCustomersCount)
                .ToList();

            foreach (var customer in topCustomers)
            {
                customer.AverageTransactionValue = customer.TransactionCount > 0 ? 
                    customer.TotalPurchases / customer.TransactionCount : 0;
            }

            var totalCustomerSales = customerOrders.Sum(o => o.OrderItems.Sum(oi => oi.TotalAmount));
            var uniqueCustomers = customerOrders.Select(o => o.CustomerId).Distinct().Count();

            var branch = branchId.HasValue ? await _context.Branches.FindAsync(branchId.Value) : null;

            return new CustomerAnalysisReport
            {
                BranchId = branchId,
                BranchName = branch?.Name,
                FromDate = fromDate,
                ToDate = toDate,
                TopCustomers = topCustomers,
                TotalCustomerSales = totalCustomerSales,
                TotalUniqueCustomers = uniqueCustomers
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating customer analysis report");
            throw;
        }
    }

    /// <summary>
    /// Generate supplier balance report
    /// </summary>
    public async Task<SupplierBalanceReport> GetSupplierBalanceReportAsync(DateTime? asOfDate = null)
    {
        try
        {
            var cutoffDate = asOfDate ?? DateTime.UtcNow;

            var suppliers = await _context.Suppliers
                .Include(s => s.PurchaseOrders.Where(po => po.OrderDate <= cutoffDate))
                .Where(s => s.IsActive)
                .ToListAsync();

            var supplierBalances = suppliers.Select(s => new SupplierBalanceSummary
            {
                SupplierId = s.Id,
                SupplierName = s.CompanyName,
                CurrentBalance = s.CurrentBalance,
                OverdueAmount = s.PurchaseOrders
                    .Where(po => po.PaymentStatus != "Paid" && 
                                po.OrderDate.AddDays(s.PaymentTermsDays) < cutoffDate)
                    .Sum(po => po.OutstandingBalance),
                DaysOverdue = s.PurchaseOrders.Any(po => 
                    po.PaymentStatus != "Paid" && 
                    po.OrderDate.AddDays(s.PaymentTermsDays) < cutoffDate) ?
                    (int)(cutoffDate - s.PurchaseOrders
                        .Where(po => po.PaymentStatus != "Paid")
                        .Min(po => po.OrderDate.AddDays(s.PaymentTermsDays))).TotalDays : 0,
                LastPaymentDate = s.PurchaseOrders
                    .Where(po => po.AmountPaid > 0)
                    .DefaultIfEmpty()
                    .Max(po => po?.ModifiedAt ?? DateTime.MinValue)
            }).ToList();

            return new SupplierBalanceReport
            {
                AsOfDate = cutoffDate,
                SupplierBalances = supplierBalances.OrderByDescending(s => s.CurrentBalance).ToList(),
                TotalPayables = supplierBalances.Sum(s => s.CurrentBalance)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating supplier balance report");
            throw;
        }
    }

    /// <summary>
    /// Generate inventory valuation report
    /// </summary>
    public async Task<InventoryValuationReport> GetInventoryValuationReportAsync(int? branchId = null, DateTime? asOfDate = null)
    {
        try
        {
            var cutoffDate = asOfDate ?? DateTime.UtcNow;

            var query = _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Branch)
                .Where(i => i.WeightOnHand > 0);

            if (branchId.HasValue)
                query = query.Where(i => i.BranchId == branchId.Value);

            var inventoryItems = await query.ToListAsync();

            var valuationItems = new List<InventoryValuationSummary>();

            foreach (var item in inventoryItems)
            {
                var currentGoldRate = await _pricingService.GetCurrentGoldRateAsync(item.Product.KaratTypeId);
                var estimatedValue = currentGoldRate != null ? 
                    item.WeightOnHand * currentGoldRate.RatePerGram : 0;

                valuationItems.Add(new InventoryValuationSummary
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    CategoryId = item.Product.CategoryTypeId,
                    KaratTypeId = item.Product.KaratTypeId,
                    QuantityOnHand = item.QuantityOnHand,
                    WeightOnHand = item.WeightOnHand,
                    CurrentGoldRate = currentGoldRate?.RatePerGram ?? 0,
                    EstimatedValue = estimatedValue
                });
            }

            var branch = branchId.HasValue ? await _context.Branches.FindAsync(branchId.Value) : null;

            return new InventoryValuationReport
            {
                BranchId = branchId,
                BranchName = branch?.Name,
                AsOfDate = cutoffDate,
                Items = valuationItems.OrderByDescending(i => i.EstimatedValue).ToList(),
                TotalInventoryValue = valuationItems.Sum(i => i.EstimatedValue)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating inventory valuation report");
            throw;
        }
    }

    /// <summary>
    /// Generate tax report
    /// </summary>
    public async Task<TaxReport> GetTaxReportAsync(int? branchId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            // Note: Tax information is now part of FinancialTransactions
            // This is a simplified implementation - in a real scenario, you might need to join with tax configurations
            var financialTransactions = await _context.FinancialTransactions
                .Include(t => t.Branch)
                .Where(t => t.TransactionDate >= fromDate &&
                           t.TransactionDate <= toDate &&
                           t.StatusId == LookupTableConstants.FinancialTransactionStatusCompleted)
                .ToListAsync();

            if (branchId.HasValue)
                financialTransactions = financialTransactions.Where(t => t.BranchId == branchId.Value).ToList();

            // Simplified tax summary - in reality, you'd need to join with tax configurations
            var taxSummaries = new List<TaxSummary>
            {
                new TaxSummary
                {
                    TaxName = "General Tax",
                    TaxCode = "GST",
                    TaxRate = 5.0m, // This should come from tax configuration
                    TaxableAmount = financialTransactions.Sum(t => t.Subtotal),
                    TaxAmount = financialTransactions.Sum(t => t.TotalTaxAmount)
                }
            };

            var branch = branchId.HasValue ? await _context.Branches.FindAsync(branchId.Value) : null;

            return new TaxReport
            {
                BranchId = branchId,
                BranchName = branch?.Name,
                FromDate = fromDate,
                ToDate = toDate,
                TaxSummaries = taxSummaries,
                TotalTaxCollected = taxSummaries.Sum(ts => ts.TaxAmount)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tax report");
            throw;
        }
    }

    /// <summary>
    /// Generate transaction log report
    /// </summary>
    public async Task<TransactionLogReport> GetTransactionLogReportAsync(int branchId, DateTime date)
    {
        try
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            var branch = await _context.Branches.FindAsync(branchId);
            if (branch == null)
                throw new ArgumentException($"Branch {branchId} not found");

            var financialTransactions = await _context.FinancialTransactions
                .Include(t => t.Branch)
                .Where(t => t.BranchId == branchId &&
                           t.TransactionDate >= startDate &&
                           t.TransactionDate < endDate)
                .OrderBy(t => t.TransactionDate)
                .ToListAsync();

            var transactionEntries = financialTransactions.Select(t => new TransactionLogEntry
            {
                TransactionNumber = t.TransactionNumber,
                TransactionDate = t.TransactionDate,
                TransactionTypeId = t.TransactionTypeId,
                CustomerName = null, // Customer info is in Orders, not FinancialTransactions
                CashierName = t.ProcessedByUserId, // This should be resolved to user name
                TotalAmount = t.TotalAmount,
                StatusId = t.StatusId
            }).ToList();

            return new TransactionLogReport
            {
                BranchId = branchId,
                BranchName = branch.Name,
                ReportDate = date,
                Transactions = transactionEntries
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating transaction log report for branch {BranchId} on {Date}", branchId, date);
            throw;
        }
    }




}
