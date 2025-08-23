using DijaGoldPOS.API.Models.LookupTables;

namespace DijaGoldPOS.API.Shared;

/// <summary>
/// Constants for lookup table IDs to replace enum usage
/// These IDs correspond to the seeded data in LookupTableSeeder
/// </summary>
public static class LookupTableConstants
{
    // Order Types
    public const int OrderTypeSale = 1;
    public const int OrderTypeReturn = 2;
    public const int OrderTypeExchange = 3;
    public const int OrderTypeLayaway = 4;
    public const int OrderTypeReservation = 5;
    public const int OrderTypeRepair = 6;

    // Order Statuses
    public const int OrderStatusPending = 1;
    public const int OrderStatusCompleted = 2;
    public const int OrderStatusCancelled = 3;
    public const int OrderStatusRefunded = 4;

    // Financial Transaction Types
    public const int FinancialTransactionTypeSale = 1;
    public const int FinancialTransactionTypeReturn = 2;
    public const int FinancialTransactionTypeRepair = 3;
    public const int FinancialTransactionTypeExchange = 4;
    public const int FinancialTransactionTypeRefund = 5;
    public const int FinancialTransactionTypeAdjustment = 6;
    public const int FinancialTransactionTypeVoid = 7;

    // Financial Transaction Statuses
    public const int FinancialTransactionStatusPending = 1;
    public const int FinancialTransactionStatusCompleted = 2;
    public const int FinancialTransactionStatusCancelled = 3;
    public const int FinancialTransactionStatusRefunded = 4;
    public const int FinancialTransactionStatusVoided = 5;
    public const int FinancialTransactionStatusReversed = 6;

    // Business Entity Types
    public const int BusinessEntityTypeCustomer = 1;
    public const int BusinessEntityTypeSupplier = 2;
    public const int BusinessEntityTypeBranch = 3;
    public const int BusinessEntityTypeOrder = 4;

    // Repair Statuses
    public const int RepairStatusPending = 1;
    public const int RepairStatusInProgress = 2;
    public const int RepairStatusCompleted = 3;
    public const int RepairStatusReadyForPickup = 4;
    public const int RepairStatusDelivered = 5;
    public const int RepairStatusCancelled = 6;

    // Repair Priorities
    public const int RepairPriorityLow = 1;
    public const int RepairPriorityMedium = 2;
    public const int RepairPriorityHigh = 3;
    public const int RepairPriorityUrgent = 4;

    // Karat Types
    public const int KaratType18K = 1;
    public const int KaratType21K = 2;
    public const int KaratType22K = 3;
    public const int KaratType24K = 4;

    // Product Category Types
    public const int ProductCategoryTypeGoldJewelry = 1;
    public const int ProductCategoryTypeBullion = 2;
    public const int ProductCategoryTypeGoldCoins = 3;

    // Transaction Types
    public const int TransactionTypeSale = 1;
    public const int TransactionTypeReturn = 2;
    public const int TransactionTypeRepair = 3;

    // Payment Methods
    public const int PaymentMethodCash = 1;

    // Transaction Statuses
    public const int TransactionStatusPending = 1;
    public const int TransactionStatusCompleted = 2;
    public const int TransactionStatusCancelled = 3;
    public const int TransactionStatusRefunded = 4;
    public const int TransactionStatusVoided = 5;

    // Charge Types
    public const int ChargeTypePercentage = 1;
    public const int ChargeTypeFixedAmount = 2;
}
