namespace DijaGoldPOS.API.Shared;

/// <summary>
/// Base exception for business rule violations
/// </summary>
public class BusinessRuleException : Exception
{
    /// <summary>
    /// Business rule code for identification
    /// </summary>
    public string? RuleCode { get; }

    /// <summary>
    /// Category of the business rule
    /// </summary>
    public string? Category { get; }

    /// <summary>
    /// User-friendly message for display
    /// </summary>
    public string? UserFriendlyMessage { get; }

    public BusinessRuleException(string message) : base(message)
    {
    }

    public BusinessRuleException(string message, string ruleCode) : base(message)
    {
        RuleCode = ruleCode;
    }

    public BusinessRuleException(string message, string ruleCode, string category) : base(message)
    {
        RuleCode = ruleCode;
        Category = category;
    }

    public BusinessRuleException(string message, string ruleCode, string category, string userFriendlyMessage) : base(message)
    {
        RuleCode = ruleCode;
        Category = category;
        UserFriendlyMessage = userFriendlyMessage;
    }

    public BusinessRuleException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Base exception for domain-related errors
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Domain error code for identification
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Entity type that caused the error
    /// </summary>
    public string? EntityType { get; }

    /// <summary>
    /// User-friendly message for display
    /// </summary>
    public string? UserFriendlyMessage { get; }

    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string message, string errorCode, string entityType) : base(message)
    {
        ErrorCode = errorCode;
        EntityType = entityType;
    }

    public DomainException(string message, string errorCode, string entityType, string userFriendlyMessage) : base(message)
    {
        ErrorCode = errorCode;
        EntityType = entityType;
        UserFriendlyMessage = userFriendlyMessage;
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception for entity not found scenarios
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityType, object id) 
        : base($"{entityType} with ID '{id}' was not found.", "ENTITY_NOT_FOUND", entityType, $"The requested {entityType.ToLowerInvariant()} could not be found.")
    {
    }

    public EntityNotFoundException(string entityType, string property, object value) 
        : base($"{entityType} with {property} '{value}' was not found.", "ENTITY_NOT_FOUND", entityType, $"The requested {entityType.ToLowerInvariant()} could not be found.")
    {
    }
}

/// <summary>
/// Exception for duplicate entity scenarios
/// </summary>
public class DuplicateEntityException : DomainException
{
    public DuplicateEntityException(string entityType, string property, object value) 
        : base($"{entityType} with {property} '{value}' already exists.", "DUPLICATE_ENTITY", entityType, $"A {entityType.ToLowerInvariant()} with this {property.ToLowerInvariant()} already exists.")
    {
    }
}

/// <summary>
/// Exception for invalid entity state scenarios
/// </summary>
public class InvalidEntityStateException : DomainException
{
    public InvalidEntityStateException(string entityType, string currentState, string requiredState) 
        : base($"{entityType} is in '{currentState}' state but '{requiredState}' state is required.", "INVALID_ENTITY_STATE", entityType, $"This operation cannot be performed on a {entityType.ToLowerInvariant()} in its current state.")
    {
    }
}

/// <summary>
/// Exception for insufficient permissions
/// </summary>
public class InsufficientPermissionsException : DomainException
{
    public InsufficientPermissionsException(string operation, string resource) 
        : base($"Insufficient permissions to {operation} {resource}.", "INSUFFICIENT_PERMISSIONS", resource, "You don't have permission to perform this action.")
    {
    }
}

/// <summary>
/// Exception for inventory-related errors
/// </summary>
public class InventoryException : DomainException
{
    public InventoryException(string message) : base(message, "INVENTORY_ERROR", "Inventory", message)
    {
    }

    public InventoryException(string message, string userFriendlyMessage) : base(message, "INVENTORY_ERROR", "Inventory", userFriendlyMessage)
    {
    }
}

/// <summary>
/// Exception for insufficient stock scenarios
/// </summary>
public class InsufficientStockException : InventoryException
{
    public InsufficientStockException(string productName, decimal available, decimal requested) 
        : base($"Insufficient stock for '{productName}'. Available: {available}, Requested: {requested}.", 
               $"Not enough {productName} in stock. Only {available} available.")
    {
    }
}

/// <summary>
/// Exception for financial transaction errors
/// </summary>
public class FinancialTransactionException : DomainException
{
    public FinancialTransactionException(string message) : base(message, "FINANCIAL_ERROR", "Transaction", message)
    {
    }

    public FinancialTransactionException(string message, string userFriendlyMessage) : base(message, "FINANCIAL_ERROR", "Transaction", userFriendlyMessage)
    {
    }
}

/// <summary>
/// Exception for payment processing errors
/// </summary>
public class PaymentException : FinancialTransactionException
{
    public PaymentException(string message) : base(message, "Payment processing failed. Please try again.")
    {
    }

    public PaymentException(string message, string userFriendlyMessage) : base(message, userFriendlyMessage)
    {
    }
}
