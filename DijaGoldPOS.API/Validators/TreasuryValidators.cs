using System.Threading;
using System.Threading.Tasks;
using DijaGoldPOS.API.Controllers;
using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Models.FinancialModels;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Validators;

public class FeedFromCashDrawerRequestValidator : AbstractValidator<TreasuryController.FeedFromCashDrawerRequest>
{
    private readonly ApplicationDbContext _db;

    public FeedFromCashDrawerRequestValidator(ApplicationDbContext db)
    {
        _db = db;

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required")
            .MustAsync(CashDrawerExistsAndClosedWithSettlement)
            .WithMessage((request, date) =>
            {
                // Specific message set in predicate; this fallback shouldn't appear.
                return "Invalid cash drawer state for the provided date.";
            });
    }

public class AdjustRequestValidator : AbstractValidator<TreasuryController.AdjustRequest>
{
    private readonly ApplicationDbContext _db;

    public AdjustRequestValidator(ApplicationDbContext db)
    {
        _db = db;

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be > 0");

        RuleFor(x => x.Direction)
            .IsInEnum();

        RuleFor(x => x)
            .CustomAsync(async (request, context, ct) =>
            {
                if (request.Direction != TreasuryTransactionDirection.Debit) return;
                if (!context.RootContextData.TryGetValue("branchId", out var branchObj) || branchObj is not int branchId || branchId <= 0)
                {
                    context.AddFailure("branchId", "BranchId is required for validation");
                    return;
                }

                var account = await _db.TreasuryAccounts.AsNoTracking()
                    .FirstOrDefaultAsync(a => a.BranchId == branchId && a.IsActive, ct);

                var current = account?.CurrentBalance ?? 0m;
                if (current - request.Amount < 0m)
                {
                    context.AddFailure("Amount", "Insufficient treasury balance");
                }
            });
    }

    private async Task<bool> HaveSufficientBalanceIfDebit(
        TreasuryController.AdjustRequest request,
        ValidationContext<TreasuryController.AdjustRequest> context,
        CancellationToken ct)
    {
        if (!context.RootContextData.TryGetValue("branchId", out var branchObj) || branchObj is not int branchId || branchId <= 0)
        {
            context.AddFailure("branchId", "BranchId is required for validation");
            return false;
        }

        var account = await _db.TreasuryAccounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.BranchId == branchId && a.IsActive, ct);

        var current = account?.CurrentBalance ?? 0m;
        return current - request.Amount >= 0m;
    }
}

public class PaySupplierRequestValidator : AbstractValidator<TreasuryController.PaySupplierRequest>
{
    private readonly ApplicationDbContext _db;

    public PaySupplierRequestValidator(ApplicationDbContext db)
    {
        _db = db;

        RuleFor(x => x.SupplierId)
            .GreaterThan(0)
            .MustAsync((supplierId, ct) => SupplierExists(supplierId, ct))
            .WithMessage(x => $"Supplier {x.SupplierId} not found");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be > 0");

        RuleFor(x => x)
            .CustomAsync(async (request, context, ct) =>
            {
                if (!context.RootContextData.TryGetValue("branchId", out var branchObj) || branchObj is not int branchId || branchId <= 0)
                {
                    context.AddFailure("branchId", "BranchId is required for validation");
                    return;
                }

                var account = await _db.TreasuryAccounts.AsNoTracking()
                    .FirstOrDefaultAsync(a => a.BranchId == branchId && a.IsActive, ct);

                var current = account?.CurrentBalance ?? 0m;
                if (current < request.Amount)
                {
                    context.AddFailure("Amount", "Insufficient treasury balance");
                }
            });

        // Prevent overpaying a supplier (do not allow supplier outstanding to go negative)
        RuleFor(x => x)
            .MustAsync((request, ct) => NotExceedSupplierOutstanding(request, ct))
            .WithMessage("Payment exceeds supplier outstanding balance");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }

    private async Task<bool> SupplierExists(int supplierId, CancellationToken ct)
    {
        return await _db.Suppliers.AsNoTracking().AnyAsync(s => s.Id == supplierId && s.IsActive, ct);
    }

    private async Task<bool> HaveSufficientTreasuryBalance(
        TreasuryController.PaySupplierRequest request,
        ValidationContext<TreasuryController.PaySupplierRequest> context,
        CancellationToken ct)
    {
        if (!context.RootContextData.TryGetValue("branchId", out var branchObj) || branchObj is not int branchId || branchId <= 0)
        {
            context.AddFailure("branchId", "BranchId is required for validation");
            return false;
        }

        var account = await _db.TreasuryAccounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.BranchId == branchId && a.IsActive, ct);

        var current = account?.CurrentBalance ?? 0m;
        return current >= request.Amount;
    }

    private async Task<bool> NotExceedSupplierOutstanding(
        TreasuryController.PaySupplierRequest request,
        CancellationToken ct)
    {
        var supplier = await _db.Suppliers.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SupplierId && s.IsActive, ct);

        if (supplier == null)
        {
            // SupplierExists rule will surface a clearer message; fail here anyway
            return false;
        }

        // Supplier.CurrentBalance represents outstanding payable to supplier (>= 0)
        // Block if payment would drive balance below zero
        return supplier.CurrentBalance >= request.Amount;
    }
}

    private async Task<bool> CashDrawerExistsAndClosedWithSettlement(
        TreasuryController.FeedFromCashDrawerRequest request,
        DateTime date,
        ValidationContext<TreasuryController.FeedFromCashDrawerRequest> context,
        CancellationToken cancellationToken)
    {
        // branchId is provided via RootContextData by the controller
        if (!context.RootContextData.TryGetValue("branchId", out var branchObj) || branchObj is not int branchId || branchId <= 0)
        {
            context.AddFailure("branchId", "BranchId is required for validation");
            return false;
        }

        var balanceDate = date.Date;
        var cdb = await _db.CashDrawerBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BranchId == branchId && x.BalanceDate == balanceDate, cancellationToken);

        if (cdb == null)
        {
            context.AddFailure("date", $"No cash drawer for date {balanceDate:yyyy-MM-dd}");
            return false;
        }

        if (cdb.Status != CashDrawerStatus.Closed)
        {
            context.AddFailure("date", "Cash drawer must be closed before feeding treasury");
            return false;
        }

        var settled = cdb.SettledAmount ?? 0m;
        if (settled <= 0)
        {
            context.AddFailure("date", "No settled amount to feed");
            return false;
        }

        return true;
    }
}
