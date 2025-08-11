using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Reporting service implementation
/// </summary>
public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReportService> _logger;
    private readonly IPricingService _pricingService;

    public ReportService(
        ApplicationDbContext context,
        ILogger<ReportService> logger,
        IPricingService pricingService)
    {
        _context = context;
        _logger = logger;
        _pricingService = pricingService;
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

            var transactions = await _context.Transactions
                .Include(t => t.TransactionItems)
                .ThenInclude(ti => ti.Product)
                .Where(t => t.BranchId == branchId && 
                           t.TransactionDate >= startDate && 
                           t.TransactionDate < endDate &&
                           t.Status == TransactionStatus.Completed)
                .ToListAsync();

            var salesTransactions = transactions.Where(t => t.TransactionType == TransactionType.Sale);
            var returnTransactions = transactions.Where(t => t.TransactionType == TransactionType.Return);

            var totalSales = salesTransactions.Sum(t => t.TotalAmount);
            var totalReturns = returnTransactions.Sum(t => t.TotalAmount);

            // Category breakdown
            var categoryBreakdown = salesTransactions
                .SelectMany(t => t.TransactionItems)
                .GroupBy(ti => ti.Product!.CategoryType)
                .Select(g => new CategorySalesBreakdown
                {
                    Category = g.Key,
                    TotalSales = g.Sum(ti => ti.LineTotal + ti.MakingChargesAmount),
                    TotalWeight = g.Sum(ti => ti.TotalWeight),
                    TransactionCount = g.Select(ti => ti.TransactionId).Distinct().Count()
                })
                .ToList();

            // Payment method breakdown
            var paymentBreakdown = salesTransactions
                .GroupBy(t => t.PaymentMethod)
                .Select(g => new PaymentMethodBreakdown
                {
                    PaymentMethod = g.Key,
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

            var cashTransactions = await _context.Transactions
                .Where(t => t.BranchId == branchId && 
                           t.TransactionDate >= startDate && 
                           t.TransactionDate < endDate &&
                           t.PaymentMethod == PaymentMethod.Cash &&
                           t.Status == TransactionStatus.Completed)
                .ToListAsync();

            var cashSales = cashTransactions
                .Where(t => t.TransactionType == TransactionType.Sale)
                .Sum(t => t.AmountPaid);

            var cashReturns = cashTransactions
                .Where(t => t.TransactionType == TransactionType.Return)
                .Sum(t => Math.Abs(t.ChangeGiven)); // Returns have negative change (refunds)

            // In a real implementation, opening balance would come from previous day's closing
            var openingBalance = 1000.00m; // This should be retrieved from system

            var expectedClosingBalance = openingBalance + cashSales - cashReturns;

            // Actual closing balance would be entered by staff during end-of-day process
            var actualClosingBalance = expectedClosingBalance; // Placeholder

            return new CashReconciliationReport
            {
                BranchId = branchId,
                BranchName = branch.Name,
                ReportDate = date,
                OpeningBalance = openingBalance,
                CashSales = cashSales,
                CashReturns = cashReturns,
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
                    Category = g.First().Inventory.Product.CategoryType,
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
                    .Where(i => i.ProductId == movement.ProductId && i.BranchId == branchId)
                    .FirstOrDefaultAsync();

                movement.ClosingQuantity = currentInventory?.QuantityOnHand ?? 0;
                movement.ClosingWeight = currentInventory?.WeightOnHand ?? 0;
                
                // Opening = Closing - Net Movement
                var netMovement = movement.Purchases - movement.Sales + movement.Returns + movement.Adjustments + movement.Transfers;
                movement.OpeningQuantity = movement.ClosingQuantity - netMovement;
                movement.OpeningWeight = movement.ClosingWeight - (netMovement * (currentInventory?.Product.Weight ?? 0m));
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
    public async Task<ProfitAnalysisReport> GetProfitAnalysisReportAsync(int? branchId, DateTime fromDate, DateTime toDate, ProductCategoryType? categoryType = null)
    {
        try
        {
            var query = _context.Transactions
                .Include(t => t.Branch)
                .Include(t => t.TransactionItems)
                .ThenInclude(ti => ti.Product)
                .Where(t => t.TransactionDate >= fromDate &&
                           t.TransactionDate <= toDate &&
                           t.TransactionType == TransactionType.Sale &&
                           t.Status == TransactionStatus.Completed);

            if (branchId.HasValue)
                query = query.Where(t => t.BranchId == branchId.Value);

            if (categoryType.HasValue)
                query = query.Where(t => t.TransactionItems.Any(ti => ti.Product!.CategoryType == categoryType.Value));

            var transactions = await query.ToListAsync();

            var totalRevenue = transactions.Sum(t => t.TotalAmount);

            // Simplified cost calculation - in reality, would use actual purchase costs
            var totalCostOfGoodsSold = transactions
                .SelectMany(t => t.TransactionItems)
                .Sum(ti => ti.TotalWeight * (ti.GoldRatePerGram * 0.85m)); // Assuming 85% of gold rate as cost

            var grossProfit = totalRevenue - totalCostOfGoodsSold;
            var grossProfitMargin = totalRevenue > 0 ? (grossProfit / totalRevenue) * 100 : 0;

            // Product analysis
            var productAnalysis = transactions
                .SelectMany(t => t.TransactionItems)
                .GroupBy(ti => ti.ProductId)
                .Select(g => new ProductProfitAnalysis
                {
                    ProductId = g.Key,
                    ProductName = g.First().Product!.Name,
                    Category = g.First().Product!.CategoryType,
                    Revenue = g.Sum(ti => ti.LineTotal + ti.MakingChargesAmount),
                    CostOfGoodsSold = g.Sum(ti => ti.TotalWeight * (ti.GoldRatePerGram * 0.85m)),
                    QuantitySold = g.Sum(ti => ti.Quantity)
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
            var query = _context.Transactions
                .Include(t => t.Customer)
                .Include(t => t.Branch)
                .Where(t => t.TransactionDate >= fromDate &&
                           t.TransactionDate <= toDate &&
                           t.TransactionType == TransactionType.Sale &&
                           t.Status == TransactionStatus.Completed &&
                           t.CustomerId.HasValue);

            if (branchId.HasValue)
                query = query.Where(t => t.BranchId == branchId.Value);

            var customerTransactions = await query.ToListAsync();

            var topCustomers = customerTransactions
                .GroupBy(t => t.CustomerId!.Value)
                .Select(g => new CustomerAnalysisSummary
                {
                    CustomerId = g.Key,
                    CustomerName = g.First().Customer!.FullName,
                    TotalPurchases = g.Sum(t => t.TotalAmount),
                    TransactionCount = g.Count(),
                    LastPurchaseDate = g.Max(t => t.TransactionDate)
                })
                .OrderByDescending(c => c.TotalPurchases)
                .Take(topCustomersCount)
                .ToList();

            foreach (var customer in topCustomers)
            {
                customer.AverageTransactionValue = customer.TransactionCount > 0 ? 
                    customer.TotalPurchases / customer.TransactionCount : 0;
            }

            var totalCustomerSales = customerTransactions.Sum(t => t.TotalAmount);
            var uniqueCustomers = customerTransactions.Select(t => t.CustomerId).Distinct().Count();

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
                var currentGoldRate = await _pricingService.GetCurrentGoldRateAsync(item.Product.KaratType);
                var estimatedValue = currentGoldRate != null ? 
                    item.WeightOnHand * currentGoldRate.RatePerGram : 0;

                valuationItems.Add(new InventoryValuationSummary
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    Category = item.Product.CategoryType,
                    KaratType = item.Product.KaratType,
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
            var query = _context.TransactionTaxes
                .Include(tt => tt.Transaction)
                .ThenInclude(t => t.Branch)
                .Include(tt => tt.TaxConfiguration)
                .Where(tt => tt.Transaction.TransactionDate >= fromDate &&
                            tt.Transaction.TransactionDate <= toDate &&
                            tt.Transaction.Status == TransactionStatus.Completed);

            if (branchId.HasValue)
                query = query.Where(tt => tt.Transaction.BranchId == branchId.Value);

            var transactionTaxes = await query.ToListAsync();

            var taxSummaries = transactionTaxes
                .GroupBy(tt => tt.TaxConfigurationId)
                .Select(g => new TaxSummary
                {
                    TaxName = g.First().TaxConfiguration!.TaxName,
                    TaxCode = g.First().TaxConfiguration!.TaxCode,
                    TaxRate = g.First().TaxConfiguration!.TaxRate,
                    TaxableAmount = g.Sum(tt => tt.TaxableAmount),
                    TaxAmount = g.Sum(tt => tt.TaxAmount)
                })
                .ToList();

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

            var transactions = await _context.Transactions
                .Include(t => t.Customer)
                .Include(t => t.Cashier)
                .Where(t => t.BranchId == branchId &&
                           t.TransactionDate >= startDate &&
                           t.TransactionDate < endDate)
                .OrderBy(t => t.TransactionDate)
                .ToListAsync();

            var transactionEntries = transactions.Select(t => new TransactionLogEntry
            {
                TransactionNumber = t.TransactionNumber,
                TransactionDate = t.TransactionDate,
                TransactionType = t.TransactionType,
                CustomerName = t.Customer?.FullName,
                CashierName = t.Cashier?.FullName ?? "",
                TotalAmount = t.TotalAmount,
                Status = t.Status
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

    /// <summary>
    /// Export report to Excel
    /// </summary>
    public Task<byte[]> ExportToExcelAsync(object reportData, string reportName)
    {
        try
        {
            // In a full implementation, would use a library like EPPlus or ClosedXML
            // For now, return CSV-like content as bytes
            var csvContent = ConvertToCsv(reportData, reportName);
            return Task.FromResult(Encoding.UTF8.GetBytes(csvContent));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report {ReportName} to Excel", reportName);
            throw;
        }
    }

    /// <summary>
    /// Export report to PDF
    /// </summary>
    public Task<byte[]> ExportToPdfAsync(object reportData, string reportName)
    {
        try
        {
            // In a full implementation, would use a library like iTextSharp or QuestPDF
            // For now, return formatted text as bytes
            var pdfContent = $"PDF Report: {reportName}\n\nGenerated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}\n\n{System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}";
            return Task.FromResult(Encoding.UTF8.GetBytes(pdfContent));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report {ReportName} to PDF", reportName);
            throw;
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Convert report data to CSV format
    /// </summary>
    private string ConvertToCsv(object reportData, string reportName)
    {
        var csv = new StringBuilder();
        csv.AppendLine($"Report: {reportName}");
        csv.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine();

        // This is a simplified CSV conversion
        // In reality, would need proper reflection-based conversion for each report type
        var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        csv.AppendLine(json);

        return csv.ToString();
    }

    #endregion
}