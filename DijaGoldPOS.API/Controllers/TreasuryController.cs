using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.AspNetCore;

namespace DijaGoldPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TreasuryController : ControllerBase
{
    private readonly ITreasuryService _treasuryService;
    private readonly IValidator<FeedFromCashDrawerRequest> _feedValidator;
    private readonly IValidator<AdjustRequest> _adjustValidator;
    private readonly IValidator<PaySupplierRequest> _paySupplierValidator;

    public TreasuryController(
        ITreasuryService treasuryService,
        IValidator<FeedFromCashDrawerRequest> feedValidator,
        IValidator<AdjustRequest> adjustValidator,
        IValidator<PaySupplierRequest> paySupplierValidator)
    {
        _treasuryService = treasuryService;
        _feedValidator = feedValidator;
        _adjustValidator = adjustValidator;
        _paySupplierValidator = paySupplierValidator;
    }

    [HttpGet("branches/{branchId}/balance")]
    public async Task<ActionResult<decimal>> GetBalance(int branchId)
    {
        var balance = await _treasuryService.GetBalanceAsync(branchId);
        return Ok(balance);
    }

    public class AdjustRequest
    {
        public decimal Amount { get; set; }
        public TreasuryTransactionDirection Direction { get; set; }
        public string? Reason { get; set; }
    }

    [Authorize(Policy = "ManagerOnly")]
    [HttpPost("branches/{branchId}/adjust")]
    public async Task<ActionResult<TreasuryTransaction>> Adjust(int branchId, [FromBody, CustomizeValidator(Skip = true)] AdjustRequest request)
    {
        var adjustCtx = new ValidationContext<AdjustRequest>(request);
        adjustCtx.RootContextData["branchId"] = branchId;
        var adjustResult = await _adjustValidator.ValidateAsync(adjustCtx);
        if (!adjustResult.IsValid)
        {
            foreach (var error in adjustResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return ValidationProblem(ModelState);
        }

        var userId = User?.Identity?.Name ?? "system";
        var result = await _treasuryService.AdjustAsync(branchId, request.Amount, request.Direction, request.Reason, userId!);
        return Ok(result);
    }

    public class FeedFromCashDrawerRequest
    {
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
    }

    [HttpPost("branches/{branchId}/feed-from-cashdrawer")]
    public async Task<ActionResult<TreasuryTransaction>> FeedFromCashDrawer(int branchId, [FromBody, CustomizeValidator(Skip = true)] FeedFromCashDrawerRequest request)
    {
        // Validate with branch context so validator can check drawer state
        var context = new ValidationContext<FeedFromCashDrawerRequest>(request);
        context.RootContextData["branchId"] = branchId;
        ValidationResult vr = await _feedValidator.ValidateAsync(context);
        if (!vr.IsValid)
        {
            foreach (var error in vr.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return ValidationProblem(ModelState);
        }

        var userId = User?.Identity?.Name ?? "system";
        var result = await _treasuryService.FeedFromCashDrawerAsync(branchId, request.Date, userId!, request.Notes);
        return Ok(result);
    }

    [HttpGet("branches/{branchId}/transactions")]
    public async Task<ActionResult<IEnumerable<TreasuryTransaction>>> GetTransactions(int branchId, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] TreasuryTransactionType? type = null)
    {
        var items = await _treasuryService.GetTransactionsAsync(branchId, from, to, type);
        return Ok(items);
    }

    public class PaySupplierRequest
    {
        public int SupplierId { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }

    [HttpPost("branches/{branchId}/pay-supplier")]
    public async Task<ActionResult> PaySupplier(int branchId, [FromBody, CustomizeValidator(Skip = true)] PaySupplierRequest request)
    {
        var payCtx = new ValidationContext<PaySupplierRequest>(request);
        payCtx.RootContextData["branchId"] = branchId;
        var payResult = await _paySupplierValidator.ValidateAsync(payCtx);
        if (!payResult.IsValid)
        {
            foreach (var error in payResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return ValidationProblem(ModelState);
        }

        var userId = User?.Identity?.Name ?? "system";
        var (treasuryTxn, supplierTxn) = await _treasuryService.PaySupplierAsync(branchId, request.SupplierId, request.Amount, userId!, request.Notes);
        return Ok(new { treasuryTxn, supplierTxn });
    }
}
