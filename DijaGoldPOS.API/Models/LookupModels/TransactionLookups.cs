using DijaGoldPOS.API.Models.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models.LookupModels;

/// <summary>
/// Lookup table for order types (Sale, Return, Exchange)
/// </summary>
[Table("OrderTypes", Schema = "Lookup")]
public class OrderTypeLookup : BaseEntity
{
    /// <summary>
    /// Order type code (SALE, RETURN, EXCHANGE)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Whether this order type affects inventory positively
    /// </summary>
    public bool IncreasesInventory { get; set; } = false;

    /// <summary>
    /// Whether this order type affects inventory negatively
    /// </summary>
    public bool DecreasesInventory { get; set; } = true;

    /// <summary>
    /// Whether this order type requires approval
    /// </summary>
    public bool RequiresApproval { get; set; } = false;

    /// <summary>
    /// Whether this order type can be voided
    /// </summary>
    public bool CanBeVoided { get; set; } = true;

    public string Description { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// Lookup table for order statuses (Pending, Completed, Cancelled)
/// </summary>
[Table("OrderStatuses", Schema = "Lookup")]
public class OrderStatusLookup : BaseEntity
{
    /// <summary>
    /// Status code (PENDING, COMPLETED, CANCELLED)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a final status (no further changes allowed)
    /// </summary>
    public bool IsFinal { get; set; } = false;

    /// <summary>
    /// Whether this status indicates success
    /// </summary>
    public bool IsSuccess { get; set; } = false;

    /// <summary>
    /// Whether this status indicates cancellation
    /// </summary>
    public bool IsCancellation { get; set; } = false;

    /// <summary>
    /// CSS color class for UI display
    /// </summary>
    public string? ColorClass { get; set; }
    public string Description { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// Lookup table for financial transaction types
/// </summary>
[Table("FinancialTransactionTypes", Schema = "Lookup")]
public class FinancialTransactionTypeLookup : BaseEntity
{
    /// <summary>
    /// Transaction type code (SALE, PURCHASE, PAYMENT, REFUND)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Whether this transaction type increases cash balance
    /// </summary>
    public bool IncreasesCash { get; set; } = false;

    /// <summary>
    /// Whether this transaction type decreases cash balance
    /// </summary>
    public bool DecreasesCash { get; set; } = false;

    /// <summary>
    /// Whether this transaction type affects inventory
    /// </summary>
    public bool AffectsInventory { get; set; } = false;

    /// <summary>
    /// Whether this transaction type requires approval
    /// </summary>
    public bool RequiresApproval { get; set; } = false;
    public string Description { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// Lookup table for financial transaction statuses
/// </summary>
[Table("FinancialTransactionStatuses", Schema = "Lookup")]
public class FinancialTransactionStatusLookup : BaseEntity
{
    /// <summary>
    /// Status code (PENDING, COMPLETED, FAILED, REVERSED)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a final status
    /// </summary>
    public bool IsFinal { get; set; } = false;

    /// <summary>
    /// Whether this status indicates success
    /// </summary>
    public bool IsSuccess { get; set; } = false;

    /// <summary>
    /// Whether this status indicates failure
    /// </summary>
    public bool IsFailure { get; set; } = false;

    /// <summary>
    /// Whether this status allows reversal
    /// </summary>
    public bool AllowsReversal { get; set; } = false;
    public string Description { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// Lookup table for general transaction types
/// </summary>
[Table("TransactionTypes", Schema = "Lookup")]
public class TransactionTypeLookup : BaseEntity
{
    /// <summary>
    /// Transaction type code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Transaction category (CASH, INVENTORY, FINANCIAL)
    /// </summary>
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// Lookup table for general transaction statuses
/// </summary>
[Table("TransactionStatuses", Schema = "Lookup")]
public class TransactionStatusLookup : BaseEntity
{
    /// <summary>
    /// Status code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Status category (PROCESSING, COMPLETED, FAILED)
    /// </summary>
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; } = 0;
}
