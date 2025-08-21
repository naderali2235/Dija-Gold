using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Models.Enums;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Interface for reporting service
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Generate daily sales summary report
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="date">Report date</param>
    /// <returns>Daily sales summary</returns>
    Task<DailySalesSummaryReport> GetDailySalesSummaryAsync(int branchId, DateTime date);

    /// <summary>
    /// Generate cash reconciliation report
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="date">Report date</param>
    /// <returns>Cash reconciliation report</returns>
    Task<CashReconciliationReport> GetCashReconciliationAsync(int branchId, DateTime date);

    /// <summary>
    /// Generate inventory movement report
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>Inventory movement report</returns>
    Task<InventoryMovementReport> GetInventoryMovementReportAsync(int branchId, DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Generate profit analysis report
    /// </summary>
    /// <param name="branchId">Branch ID (null for all branches)</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <param name="categoryType">Product category filter</param>
    /// <returns>Profit analysis report</returns>
    Task<ProfitAnalysisReport> GetProfitAnalysisReportAsync(int? branchId, DateTime fromDate, DateTime toDate, ProductCategoryType? categoryType = null);

    /// <summary>
    /// Generate customer analysis report
    /// </summary>
    /// <param name="branchId">Branch ID (null for all branches)</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <param name="topCustomersCount">Number of top customers to show</param>
    /// <returns>Customer analysis report</returns>
    Task<CustomerAnalysisReport> GetCustomerAnalysisReportAsync(int? branchId, DateTime fromDate, DateTime toDate, int topCustomersCount = 20);

    /// <summary>
    /// Generate supplier balance report
    /// </summary>
    /// <param name="asOfDate">As of date</param>
    /// <returns>Supplier balance report</returns>
    Task<SupplierBalanceReport> GetSupplierBalanceReportAsync(DateTime? asOfDate = null);

    /// <summary>
    /// Generate inventory valuation report
    /// </summary>
    /// <param name="branchId">Branch ID (null for all branches)</param>
    /// <param name="asOfDate">As of date</param>
    /// <returns>Inventory valuation report</returns>
    Task<InventoryValuationReport> GetInventoryValuationReportAsync(int? branchId = null, DateTime? asOfDate = null);

    /// <summary>
    /// Generate tax report
    /// </summary>
    /// <param name="branchId">Branch ID (null for all branches)</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>Tax report</returns>
    Task<TaxReport> GetTaxReportAsync(int? branchId, DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Generate transaction log report
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="date">Report date</param>
    /// <returns>Transaction log report</returns>
    Task<TransactionLogReport> GetTransactionLogReportAsync(int branchId, DateTime date);

    /// <summary>
    /// Export report to Excel
    /// </summary>
    /// <param name="reportData">Report data</param>
    /// <param name="reportName">Report name</param>
    /// <returns>Excel file bytes</returns>
    Task<byte[]> ExportToExcelAsync(object reportData, string reportName);

    /// <summary>
    /// Export report to PDF
    /// </summary>
    /// <param name="reportData">Report data</param>
    /// <param name="reportName">Report name</param>
    /// <returns>PDF file bytes</returns>
    Task<byte[]> ExportToPdfAsync(object reportData, string reportName);
}

// Report Models
public class DailySalesSummaryReport
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalReturns { get; set; }
    public decimal NetSales { get; set; }
    public int TransactionCount { get; set; }
    public List<CategorySalesBreakdown> CategoryBreakdown { get; set; } = new();
    public List<PaymentMethodBreakdown> PaymentMethodBreakdown { get; set; } = new();
}

public class CategorySalesBreakdown
{
    public ProductCategoryType Category { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalWeight { get; set; }
    public int TransactionCount { get; set; }
}

public class PaymentMethodBreakdown
{
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public int TransactionCount { get; set; }
}

public class CashReconciliationReport
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal CashSales { get; set; }
    public decimal CashReturns { get; set; }
    public decimal CashRepairs { get; set; }
    public decimal ExpectedClosingBalance { get; set; }
    public decimal ActualClosingBalance { get; set; }
    public decimal CashOverShort { get; set; }
}

public class InventoryMovementReport
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<InventoryMovementSummary> Movements { get; set; } = new();
}

public class InventoryMovementSummary
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public ProductCategoryType Category { get; set; }
    public decimal OpeningQuantity { get; set; }
    public decimal OpeningWeight { get; set; }
    public decimal Purchases { get; set; }
    public decimal Sales { get; set; }
    public decimal Returns { get; set; }
    public decimal Adjustments { get; set; }
    public decimal Transfers { get; set; }
    public decimal ClosingQuantity { get; set; }
    public decimal ClosingWeight { get; set; }
}

public class ProfitAnalysisReport
{
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCostOfGoodsSold { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal GrossProfitMargin { get; set; }
    public List<ProductProfitAnalysis> ProductAnalysis { get; set; } = new();
}

public class ProductProfitAnalysis
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public ProductCategoryType Category { get; set; }
    public decimal Revenue { get; set; }
    public decimal CostOfGoodsSold { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal GrossProfitMargin { get; set; }
    public decimal QuantitySold { get; set; }
}

public class CustomerAnalysisReport
{
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<CustomerAnalysisSummary> TopCustomers { get; set; } = new();
    public decimal TotalCustomerSales { get; set; }
    public int TotalUniqueCustomers { get; set; }
}

public class CustomerAnalysisSummary
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalPurchases { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageTransactionValue { get; set; }
    public DateTime LastPurchaseDate { get; set; }
}

public class SupplierBalanceReport
{
    public DateTime AsOfDate { get; set; }
    public List<SupplierBalanceSummary> SupplierBalances { get; set; } = new();
    public decimal TotalPayables { get; set; }
}

public class SupplierBalanceSummary
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal OverdueAmount { get; set; }
    public int DaysOverdue { get; set; }
    public DateTime LastPaymentDate { get; set; }
}

public class InventoryValuationReport
{
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public DateTime AsOfDate { get; set; }
    public List<InventoryValuationSummary> Items { get; set; } = new();
    public decimal TotalInventoryValue { get; set; }
}

public class InventoryValuationSummary
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public ProductCategoryType Category { get; set; }
    public KaratType KaratType { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal WeightOnHand { get; set; }
    public decimal CurrentGoldRate { get; set; }
    public decimal EstimatedValue { get; set; }
}

public class TaxReport
{
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<TaxSummary> TaxSummaries { get; set; } = new();
    public decimal TotalTaxCollected { get; set; }
}

public class TaxSummary
{
    public string TaxName { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
}

public class TransactionLogReport
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public List<TransactionLogEntry> Transactions { get; set; } = new();
}

public class TransactionLogEntry
{
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public TransactionType TransactionType { get; set; }
    public string? CustomerName { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public TransactionStatus Status { get; set; }
}