using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Repositories;
using DijaGoldPOS.API.Services;
using DijaGoldPOS.API.Shared;
using Serilog.Context;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service implementation for financial transaction operations
/// </summary>
public class FinancialTransactionService : IFinancialTransactionService
{
    private readonly IFinancialTransactionRepository _financialTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly IStructuredLoggingService _structuredLogging;
    private readonly ILogger<FinancialTransactionService> _logger;

    public FinancialTransactionService(
        IFinancialTransactionRepository financialTransactionRepository,
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        IStructuredLoggingService structuredLogging,
        ILogger<FinancialTransactionService> logger)
    {
        _financialTransactionRepository = financialTransactionRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _structuredLogging = structuredLogging;
        _logger = logger;
    }

    public async Task<FinancialTransaction> CreateFinancialTransactionAsync(CreateFinancialTransactionRequest request, string userId)
    {
        using var performanceTimer = _structuredLogging.BeginPerformanceTimer(
            "CreateFinancialTransaction",
            new Dictionary<string, object>
            {
                ["BranchId"] = request.BranchId,
                ["TransactionTypeId"] = request.TransactionTypeId,
                ["TotalAmount"] = request.TotalAmount,
                ["UserId"] = userId
            });

        try
        {
            using (LogContext.PushProperty("Operation", "CreateFinancialTransaction"))
            using (LogContext.PushProperty("BranchId", request.BranchId))
            using (LogContext.PushProperty("TransactionTypeId", request.TransactionTypeId))
            using (LogContext.PushProperty("TotalAmount", request.TotalAmount))
            using (LogContext.PushProperty("UserId", userId))
            {
                // Validate request
                if (request.TotalAmount <= 0)
                {
                    await _structuredLogging.LogSecurityEventAsync(
                        "INVALID_TRANSACTION_AMOUNT",
                        $"Attempted to create transaction with invalid amount: {request.TotalAmount}",
                        new Dictionary<string, object>
                        {
                            ["TotalAmount"] = request.TotalAmount,
                            ["UserId"] = userId,
                            ["BranchId"] = request.BranchId
                        });
                    throw new ArgumentException("Total amount must be greater than zero");
                }

                if (request.AmountPaid < request.TotalAmount)
                {
                    await _structuredLogging.LogSecurityEventAsync(
                        "INSUFFICIENT_PAYMENT",
                        $"Payment amount {request.AmountPaid} less than total {request.TotalAmount}",
                        new Dictionary<string, object>
                        {
                            ["AmountPaid"] = request.AmountPaid,
                            ["TotalAmount"] = request.TotalAmount,
                            ["UserId"] = userId
                        });
                    throw new ArgumentException("Amount paid cannot be less than total amount");
                }

                // Generate transaction number
                var transactionNumber = await _financialTransactionRepository.GetNextTransactionNumberAsync(request.BranchId);

                // Create financial transaction
                var financialTransaction = new FinancialTransaction
                {
                    TransactionNumber = transactionNumber,
                    TransactionTypeId = request.TransactionTypeId,
                    BusinessEntityId = request.BusinessEntityId,
                    BusinessEntityTypeId = request.BusinessEntityTypeId,
                    TransactionDate = DateTime.UtcNow,
                    Subtotal = request.Subtotal,
                    TotalTaxAmount = request.TotalTaxAmount,
                    TotalDiscountAmount = request.TotalDiscountAmount,
                    TotalAmount = request.TotalAmount,
                    AmountPaid = request.AmountPaid,
                    ChangeGiven = request.ChangeGiven,
                    PaymentMethodId = request.PaymentMethodId,
                    StatusId = LookupTableConstants.FinancialTransactionStatusCompleted,
                    ProcessedByUserId = userId,
                    ApprovedByUserId = request.ApprovedByUserId,
                    Notes = request.Notes,
                    ReceiptPrinted = false,
                    GeneralLedgerPosted = false
                };

                // Add to repository
                await _financialTransactionRepository.AddAsync(financialTransaction);
                await _unitOfWork.SaveChangesAsync();

                // Structured audit log with full context
                await _structuredLogging.LogBusinessOperationAsync(
                    "CREATE_FINANCIAL_TRANSACTION",
                    "FinancialTransaction",
                    financialTransaction.Id.ToString(),
                    new
                    {
                        TransactionNumber = transactionNumber,
                        TransactionTypeId = request.TransactionTypeId,
                        BusinessEntityId = request.BusinessEntityId,
                        TotalAmount = request.TotalAmount,
                        AmountPaid = request.AmountPaid,
                        BranchId = request.BranchId
                    },
                    $"Created financial transaction {transactionNumber} with amount {request.TotalAmount:C}");

                _logger.LogInformation("Financial transaction {TransactionNumber} created successfully by user {UserId} with amount {TotalAmount}",
                    transactionNumber, userId, request.TotalAmount);

                return financialTransaction;
            }
        }
        catch (Exception ex)
        {
            await _structuredLogging.LogErrorAsync(
                ex,
                "CreateFinancialTransaction",
                "FinancialTransaction",
                null,
                new Dictionary<string, object>
                {
                    ["BranchId"] = request.BranchId,
                    ["TransactionTypeId"] = request.TransactionTypeId,
                    ["TotalAmount"] = request.TotalAmount,
                    ["UserId"] = userId
                });

            throw;
        }
    }

    public async Task<FinancialTransaction?> GetFinancialTransactionAsync(int transactionId)
    {
        return await _financialTransactionRepository.GetByIdAsync(transactionId);
    }

    public async Task<FinancialTransaction?> GetFinancialTransactionByNumberAsync(string transactionNumber, int branchId)
    {
        return await _financialTransactionRepository.GetByTransactionNumberAsync(transactionNumber, branchId);
    }

    public async Task<(List<FinancialTransaction> Transactions, int TotalCount)> SearchFinancialTransactionsAsync(FinancialTransactionSearchRequest searchRequest)
    {
        return await _financialTransactionRepository.SearchAsync(
            searchRequest.BranchId,
            searchRequest.TransactionTypeId,
            searchRequest.StatusId,
            searchRequest.FromDate,
            searchRequest.ToDate,
            searchRequest.TransactionNumber,
            searchRequest.ProcessedByUserId,
            searchRequest.BusinessEntityId,
            searchRequest.BusinessEntityTypeId,
            searchRequest.Page,
            searchRequest.PageSize);
    }

    public async Task<FinancialTransaction?> UpdateFinancialTransactionAsync(int transactionId, UpdateFinancialTransactionRequest request, string userId)
    {
        try
        {
            var transaction = await _financialTransactionRepository.GetByIdAsync(transactionId);
            if (transaction == null)
                return null;

            // Store old values for audit
            var oldValues = new
            {
                transaction.Subtotal,
                transaction.TotalTaxAmount,
                transaction.TotalDiscountAmount,
                transaction.TotalAmount,
                transaction.AmountPaid,
                transaction.ChangeGiven,
                transaction.PaymentMethodId,
                transaction.StatusId,
                transaction.Notes
            };

            // Update fields if provided
            if (request.Subtotal.HasValue)
                transaction.Subtotal = request.Subtotal.Value;

            if (request.TotalTaxAmount.HasValue)
                transaction.TotalTaxAmount = request.TotalTaxAmount.Value;

            if (request.TotalDiscountAmount.HasValue)
                transaction.TotalDiscountAmount = request.TotalDiscountAmount.Value;

            if (request.TotalAmount.HasValue)
                transaction.TotalAmount = request.TotalAmount.Value;

            if (request.AmountPaid.HasValue)
                transaction.AmountPaid = request.AmountPaid.Value;

            if (request.ChangeGiven.HasValue)
                transaction.ChangeGiven = request.ChangeGiven.Value;

            if (request.PaymentMethodId.HasValue)
                transaction.PaymentMethodId = request.PaymentMethodId.Value;

            if (request.StatusId.HasValue)
                transaction.StatusId = request.StatusId.Value;

            if (request.Notes != null)
                transaction.Notes = request.Notes;

            // Update audit fields
            transaction.ModifiedAt = DateTime.UtcNow;
            transaction.ModifiedBy = userId;

            // Save changes
            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogActionAsync(
                userId,
                "Update",
                "FinancialTransaction",
                transactionId.ToString(),
                $"Updated financial transaction {transaction.TransactionNumber}");

            _logger.LogInformation("Financial transaction {TransactionNumber} updated successfully by user {UserId}", 
                transaction.TransactionNumber, userId);

            return transaction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating financial transaction {TransactionId}", transactionId);
            throw;
        }
    }

    public async Task<FinancialTransactionResult> VoidFinancialTransactionAsync(int transactionId, string reason, string userId)
    {
        using var performanceTimer = _structuredLogging.BeginPerformanceTimer(
            "VoidFinancialTransaction",
            new Dictionary<string, object>
            {
                ["TransactionId"] = transactionId,
                ["UserId"] = userId,
                ["Reason"] = reason
            });

        try
        {
            using (LogContext.PushProperty("Operation", "VoidFinancialTransaction"))
            using (LogContext.PushProperty("TransactionId", transactionId))
            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("Reason", reason))
            {
                var transaction = await _financialTransactionRepository.GetByIdAsync(transactionId);
                if (transaction == null)
                {
                    await _structuredLogging.LogSecurityEventAsync(
                        "VOID_TRANSACTION_NOT_FOUND",
                        $"Attempted to void non-existent transaction {transactionId}",
                        new Dictionary<string, object>
                        {
                            ["TransactionId"] = transactionId,
                            ["UserId"] = userId
                        });

                    return new FinancialTransactionResult { IsSuccess = false, ErrorMessage = "Transaction not found" };
                }

                // Validate if transaction can be voided
                var (canVoid, errorMessage) = await CanVoidFinancialTransactionAsync(transactionId);
                if (!canVoid)
                {
                    await _structuredLogging.LogSecurityEventAsync(
                        "VOID_TRANSACTION_VALIDATION_FAILED",
                        $"Void validation failed for transaction {transactionId}: {errorMessage}",
                        new Dictionary<string, object>
                        {
                            ["TransactionId"] = transactionId,
                            ["TransactionNumber"] = transaction.TransactionNumber,
                            ["UserId"] = userId,
                            ["ErrorMessage"] = errorMessage ?? "Unknown error"
                        });

                    return new FinancialTransactionResult { IsSuccess = false, ErrorMessage = errorMessage };
                }

                // Store old values for audit
                var oldValues = new
                {
                    transaction.StatusId,
                    transaction.Notes,
                    transaction.TotalAmount
                };

                // Update transaction status
                transaction.StatusId = LookupTableConstants.FinancialTransactionStatusCancelled;
                transaction.Notes = string.IsNullOrEmpty(transaction.Notes) ? reason : $"{transaction.Notes}; Voided: {reason}";
                transaction.ModifiedAt = DateTime.UtcNow;
                transaction.ModifiedBy = userId;

                // Save changes
                await _unitOfWork.SaveChangesAsync();

                // Structured audit log with full context
                await _structuredLogging.LogDataModificationAsync(
                    "VOID_FINANCIAL_TRANSACTION",
                    "FinancialTransaction",
                    transactionId.ToString(),
                    oldValues,
                    new
                    {
                        transaction.StatusId,
                        transaction.Notes,
                        transaction.TotalAmount,
                        VoidReason = reason
                    },
                    $"Voided financial transaction {transaction.TransactionNumber} for amount {transaction.TotalAmount:C}. Reason: {reason}");

                _logger.LogWarning("Financial transaction {TransactionNumber} voided by user {UserId} for amount {TotalAmount}. Reason: {Reason}",
                    transaction.TransactionNumber, userId, transaction.TotalAmount, reason);

                return new FinancialTransactionResult { IsSuccess = true, Transaction = transaction };
            }
        }
        catch (Exception ex)
        {
            await _structuredLogging.LogErrorAsync(
                ex,
                "VoidFinancialTransaction",
                "FinancialTransaction",
                transactionId.ToString(),
                new Dictionary<string, object>
                {
                    ["TransactionId"] = transactionId,
                    ["UserId"] = userId,
                    ["Reason"] = reason
                });

            return new FinancialTransactionResult { IsSuccess = false, ErrorMessage = "An error occurred while voiding the transaction" };
        }
    }

    public async Task<FinancialTransactionResult> CreateReversalTransactionAsync(int originalTransactionId, string reason, string userId, string managerId)
    {
        try
        {
            var originalTransaction = await _financialTransactionRepository.GetByIdAsync(originalTransactionId);
            if (originalTransaction == null)
                return new FinancialTransactionResult { IsSuccess = false, ErrorMessage = "Original transaction not found" };

            // Validate if transaction can be reversed
            var (canReverse, errorMessage) = await CanReverseFinancialTransactionAsync(originalTransactionId);
            if (!canReverse)
                return new FinancialTransactionResult { IsSuccess = false, ErrorMessage = errorMessage };

            // Create reversal transaction
            var reversalRequest = new CreateFinancialTransactionRequest
            {
                BranchId = originalTransaction.BranchId,
                TransactionTypeId = LookupTableConstants.FinancialTransactionTypeRefund,
                BusinessEntityId = originalTransaction.BusinessEntityId,
                BusinessEntityTypeId = originalTransaction.BusinessEntityTypeId,
                Subtotal = -originalTransaction.Subtotal,
                TotalTaxAmount = -originalTransaction.TotalTaxAmount,
                TotalDiscountAmount = -originalTransaction.TotalDiscountAmount,
                TotalAmount = -originalTransaction.TotalAmount,
                AmountPaid = originalTransaction.AmountPaid, // Refund the amount paid
                ChangeGiven = 0,
                PaymentMethodId = originalTransaction.PaymentMethodId,
                Notes = $"Reversal of {originalTransaction.TransactionNumber}. Reason: {reason}",
                ApprovedByUserId = managerId
            };

            var reversalTransaction = await CreateFinancialTransactionAsync(reversalRequest, userId);

            // Link to original transaction
            reversalTransaction.OriginalTransactionId = originalTransactionId;
            await _unitOfWork.SaveChangesAsync();

            // Update original transaction status
            originalTransaction.StatusId = LookupTableConstants.FinancialTransactionStatusRefunded;
            originalTransaction.ModifiedAt = DateTime.UtcNow;
            originalTransaction.ModifiedBy = userId;
            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogActionAsync(
                userId,
                "Reverse",
                "FinancialTransaction",
                originalTransactionId.ToString(),
                $"Created reversal transaction {reversalTransaction.TransactionNumber} for {originalTransaction.TransactionNumber}. Reason: {reason}");

            _logger.LogInformation("Reversal transaction {ReversalNumber} created for {OriginalNumber} by user {UserId}", 
                reversalTransaction.TransactionNumber, originalTransaction.TransactionNumber, userId);

            return new FinancialTransactionResult { IsSuccess = true, Transaction = reversalTransaction };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reversal transaction for {OriginalTransactionId}", originalTransactionId);
            return new FinancialTransactionResult { IsSuccess = false, ErrorMessage = "An error occurred while creating the reversal transaction" };
        }
    }

    public async Task<(bool CanVoid, string? ErrorMessage)> CanVoidFinancialTransactionAsync(int transactionId)
    {
        var transaction = await _financialTransactionRepository.GetByIdAsync(transactionId);
        if (transaction == null)
            return (false, "Transaction not found");

        if (transaction.StatusId != LookupTableConstants.FinancialTransactionStatusCompleted)
            return (false, "Only completed transactions can be voided");

        // Add additional business rules as needed
        // For example, check if transaction is within voidable time window
        var timeSinceTransaction = DateTime.UtcNow - transaction.TransactionDate;
        if (timeSinceTransaction.TotalHours > 24)
            return (false, "Transactions can only be voided within 24 hours");

        return (true, null);
    }

    public async Task<(bool CanReverse, string? ErrorMessage)> CanReverseFinancialTransactionAsync(int transactionId)
    {
        var transaction = await _financialTransactionRepository.GetByIdAsync(transactionId);
        if (transaction == null)
            return (false, "Transaction not found");

        if (transaction.StatusId != LookupTableConstants.FinancialTransactionStatusCompleted)
            return (false, "Only completed transactions can be reversed");

        // Check if already has reversal transactions
        var reversals = await _financialTransactionRepository.GetReversalTransactionsAsync(transactionId);
        if (reversals.Any())
            return (false, "Transaction already has reversal transactions");

        return (true, null);
    }

    public async Task<FinancialTransactionSummary> GetFinancialTransactionSummaryAsync(int? branchId, DateTime? fromDate, DateTime? toDate)
    {
        var repoSummary = await _financialTransactionRepository.GetSummaryAsync(branchId, fromDate, toDate);
        
        return new FinancialTransactionSummary
        {
            TotalTransactions = repoSummary.TotalTransactions,
            TotalAmount = repoSummary.TotalAmount,
            TransactionTypeCounts = repoSummary.TransactionTypeCounts,
            StatusCounts = repoSummary.StatusCounts
        };
    }

    public async Task<string> GenerateNextTransactionNumberAsync(int branchId)
    {
        return await _financialTransactionRepository.GetNextTransactionNumberAsync(branchId);
    }

    public async Task<bool> MarkReceiptPrintedAsync(int transactionId, string userId)
    {
        try
        {
            var transaction = await _financialTransactionRepository.GetByIdAsync(transactionId);
            if (transaction == null)
                return false;

            transaction.ReceiptPrinted = true;
            transaction.ModifiedAt = DateTime.UtcNow;
            transaction.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogActionAsync(
                userId,
                "MarkReceiptPrinted",
                "FinancialTransaction",
                transactionId.ToString(),
                $"Marked receipt as printed for transaction {transaction.TransactionNumber}");

            _logger.LogInformation("Receipt marked as printed for transaction {TransactionNumber} by user {UserId}", 
                transaction.TransactionNumber, userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking receipt as printed for transaction {TransactionId}", transactionId);
            return false;
        }
    }

    public async Task<bool> MarkGeneralLedgerPostedAsync(int transactionId, string userId)
    {
        try
        {
            var transaction = await _financialTransactionRepository.GetByIdAsync(transactionId);
            if (transaction == null)
                return false;

            transaction.GeneralLedgerPosted = true;
            transaction.ModifiedAt = DateTime.UtcNow;
            transaction.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogActionAsync(
                userId,
                "MarkGLPosted",
                "FinancialTransaction",
                transactionId.ToString(),
                $"Marked as GL posted for transaction {transaction.TransactionNumber}");

            _logger.LogInformation("GL posted marked for transaction {TransactionNumber} by user {UserId}", 
                transaction.TransactionNumber, userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking GL posted for transaction {TransactionId}", transactionId);
            return false;
        }
    }


}
