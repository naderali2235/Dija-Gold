namespace DijaGoldPOS.API.Models.Enums;

/// <summary>
/// Represents the different types of transactions in the POS system
/// </summary>
public enum TransactionType
{
    /// <summary>Sales transaction</summary>
    Sale = 1,
    
    /// <summary>Return transaction (requires manager approval)</summary>
    Return = 2,
    
    /// <summary>Repair service transaction</summary>
    Repair = 3
}

/// <summary>
/// Represents the payment methods supported
/// </summary>
public enum PaymentMethod
{
    /// <summary>Cash payment (Egyptian Pounds)</summary>
    Cash = 1
    // Future: Card, Bank Transfer, etc.
}

/// <summary>
/// Represents the status of a transaction
/// </summary>
public enum TransactionStatus
{
    /// <summary>Transaction is pending</summary>
    Pending = 1,
    
    /// <summary>Transaction is completed</summary>
    Completed = 2,
    
    /// <summary>Transaction is cancelled</summary>
    Cancelled = 3,
    
    /// <summary>Transaction is refunded</summary>
    Refunded = 4,
    
    /// <summary>Transaction is voided</summary>
    Voided = 5
}

/// <summary>
/// Represents the charge type for making charges and taxes
/// </summary>
public enum ChargeType
{
    /// <summary>Percentage-based charge</summary>
    Percentage = 1,
    
    /// <summary>Fixed amount charge</summary>
    FixedAmount = 2
}