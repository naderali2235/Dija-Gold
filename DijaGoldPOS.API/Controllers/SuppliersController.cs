using AutoMapper;
using AutoMapper.QueryableExtensions;
using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Services;
using DijaGoldPOS.API.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Suppliers controller for supplier management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ISupplierRepository _supplierRepository;
    private readonly ISupplierService _supplierService;
    private readonly IAuditService _auditService;
    private readonly ILogger<SuppliersController> _logger;
    private readonly IMapper _mapper;

    public SuppliersController(
        ApplicationDbContext context,
        ISupplierRepository supplierRepository,
        ISupplierService supplierService,
        IAuditService auditService,
        ILogger<SuppliersController> logger,
        IMapper mapper)
    {
        _context = context;
        _supplierRepository = supplierRepository;
        _supplierService = supplierService;
        _auditService = auditService;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all suppliers with optional filtering and pagination
    /// </summary>
    /// <param name="searchRequest">Search parameters</param>
    /// <returns>List of suppliers</returns>
    [HttpGet]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SupplierDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuppliers([FromQuery] SupplierSearchRequestDto searchRequest)
    {
        try
        {
            var query = _context.Suppliers.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchRequest.SearchTerm))
            {
                var searchTerm = searchRequest.SearchTerm.ToLower();
                query = query.Where(s => 
                    s.CompanyName.ToLower().Contains(searchTerm) ||
                    (s.ContactPersonName != null && s.ContactPersonName.ToLower().Contains(searchTerm)) ||
                    (s.Phone != null && s.Phone.Contains(searchTerm)) ||
                    (s.Email != null && s.Email.ToLower().Contains(searchTerm))
                );
            }

            if (!string.IsNullOrWhiteSpace(searchRequest.TaxRegistrationNumber))
            {
                query = query.Where(s => s.TaxRegistrationNumber == searchRequest.TaxRegistrationNumber);
            }

            if (!string.IsNullOrWhiteSpace(searchRequest.CommercialRegistrationNumber))
            {
                query = query.Where(s => s.CommercialRegistrationNumber == searchRequest.CommercialRegistrationNumber);
            }

            if (searchRequest.CreditLimitEnforced.HasValue)
            {
                query = query.Where(s => s.CreditLimitEnforced == searchRequest.CreditLimitEnforced.Value);
            }

            if (searchRequest.IsActive.HasValue)
            {
                query = query.Where(s => s.IsActive == searchRequest.IsActive.Value);
            }

            // Apply sorting
            query = query.OrderByDescending(s => s.CreatedAt);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination and mapping
            var suppliers = await query
                .Skip((searchRequest.PageNumber - 1) * searchRequest.PageSize)
                .Take(searchRequest.PageSize)
                .ProjectTo<SupplierDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var result = new PagedResult<SupplierDto>
            {
                Items = suppliers,
                TotalCount = totalCount,
                PageNumber = searchRequest.PageNumber,
                PageSize = searchRequest.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / searchRequest.PageSize)
            };

            return Ok(ApiResponse<PagedResult<SupplierDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suppliers");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving suppliers"));
        }
    }

    /// <summary>
    /// Get supplier by ID
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <returns>Supplier details</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSupplier(int id)
    {
        try
        {
            var supplierDto = await _context.Suppliers
                .Where(s => s.Id == id)
                .ProjectTo<SupplierDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (supplierDto == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Supplier not found"));
            }

            return Ok(ApiResponse<SupplierDto>.SuccessResponse(supplierDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supplier with ID {SupplierId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the supplier"));
        }
    }

    /// <summary>
    /// Create a new supplier
    /// </summary>
    /// <param name="request">Supplier creation request</param>
    /// <returns>Created supplier</returns>
    [HttpPost]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequestDto request)
    {
        try
        {
            // Check for duplicate tax registration number
            if (!string.IsNullOrWhiteSpace(request.TaxRegistrationNumber))
            {
                var existingSupplier = await _context.Suppliers
                    .FirstOrDefaultAsync(s => s.TaxRegistrationNumber == request.TaxRegistrationNumber);
                if (existingSupplier != null)
                {
                    return BadRequest(ApiResponse.ErrorResponse("A supplier with this tax registration number already exists"));
                }
            }

            // Check for duplicate commercial registration number
            if (!string.IsNullOrWhiteSpace(request.CommercialRegistrationNumber))
            {
                var existingSupplier = await _context.Suppliers
                    .FirstOrDefaultAsync(s => s.CommercialRegistrationNumber == request.CommercialRegistrationNumber);
                if (existingSupplier != null)
                {
                    return BadRequest(ApiResponse.ErrorResponse("A supplier with this commercial registration number already exists"));
                }
            }

            var supplier = new Supplier
            {
                CompanyName = request.CompanyName,
                ContactPersonName = request.ContactPersonName,
                Phone = request.Phone,
                Email = request.Email,
                Address = request.Address,
                TaxRegistrationNumber = request.TaxRegistrationNumber,
                CommercialRegistrationNumber = request.CommercialRegistrationNumber,
                CreditLimit = request.CreditLimit,
                CurrentBalance = 0,
                PaymentTermsDays = request.PaymentTermsDays,
                CreditLimitEnforced = request.CreditLimitEnforced,
                PaymentTerms = request.PaymentTerms,
                Notes = request.Notes,
                LastTransactionDate = null,
                IsActive = true
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "CREATE",
                "Supplier",
                supplier.Id.ToString(),
                $"Created supplier: {supplier.CompanyName}"
            );

            var supplierDto = new SupplierDto
            {
                Id = supplier.Id,
                CompanyName = supplier.CompanyName,
                ContactPersonName = supplier.ContactPersonName,
                Phone = supplier.Phone,
                Email = supplier.Email,
                Address = supplier.Address,
                TaxRegistrationNumber = supplier.TaxRegistrationNumber,
                CommercialRegistrationNumber = supplier.CommercialRegistrationNumber,
                CreditLimit = supplier.CreditLimit,
                CurrentBalance = supplier.CurrentBalance,
                PaymentTermsDays = supplier.PaymentTermsDays,
                CreditLimitEnforced = supplier.CreditLimitEnforced,
                PaymentTerms = supplier.PaymentTerms,
                Notes = supplier.Notes,
                LastTransactionDate = supplier.LastTransactionDate,
                CreatedAt = supplier.CreatedAt,
                IsActive = supplier.IsActive,
                IsSystemSupplier = supplier.IsSystemSupplier
            };

            return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, 
                ApiResponse<SupplierDto>.SuccessResponse(supplierDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating supplier");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while creating the supplier"));
        }
    }

    /// <summary>
    /// Update an existing supplier
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <param name="request">Supplier update request</param>
    /// <returns>Updated supplier</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierRequestDto request)
    {
        try
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Supplier not found"));
            }

            // Check for duplicate tax registration number (excluding current supplier)
            if (!string.IsNullOrWhiteSpace(request.TaxRegistrationNumber) && request.TaxRegistrationNumber != supplier.TaxRegistrationNumber)
            {
                var existingSupplier = await _context.Suppliers
                    .FirstOrDefaultAsync(s => s.TaxRegistrationNumber == request.TaxRegistrationNumber && s.Id != id);
                if (existingSupplier != null)
                {
                    return BadRequest(ApiResponse.ErrorResponse("A supplier with this tax registration number already exists"));
                }
            }

            // Check for duplicate commercial registration number (excluding current supplier)
            if (!string.IsNullOrWhiteSpace(request.CommercialRegistrationNumber) && request.CommercialRegistrationNumber != supplier.CommercialRegistrationNumber)
            {
                var existingSupplier = await _context.Suppliers
                    .FirstOrDefaultAsync(s => s.CommercialRegistrationNumber == request.CommercialRegistrationNumber && s.Id != id);
                if (existingSupplier != null)
                {
                    return BadRequest(ApiResponse.ErrorResponse("A supplier with this commercial registration number already exists"));
                }
            }

            // Update supplier properties
            supplier.CompanyName = request.CompanyName;
            supplier.ContactPersonName = request.ContactPersonName;
            supplier.Phone = request.Phone;
            supplier.Email = request.Email;
            supplier.Address = request.Address;
            supplier.TaxRegistrationNumber = request.TaxRegistrationNumber;
            supplier.CommercialRegistrationNumber = request.CommercialRegistrationNumber;
            supplier.CreditLimit = request.CreditLimit;
            supplier.PaymentTermsDays = request.PaymentTermsDays;
            supplier.CreditLimitEnforced = request.CreditLimitEnforced;
            supplier.PaymentTerms = request.PaymentTerms;
            supplier.Notes = request.Notes;
            supplier.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "UPDATE",
                "Supplier",
                supplier.Id.ToString(),
                $"Updated supplier: {supplier.CompanyName}"
            );

            var supplierDto = new SupplierDto
            {
                Id = supplier.Id,
                CompanyName = supplier.CompanyName,
                ContactPersonName = supplier.ContactPersonName,
                Phone = supplier.Phone,
                Email = supplier.Email,
                Address = supplier.Address,
                TaxRegistrationNumber = supplier.TaxRegistrationNumber,
                CommercialRegistrationNumber = supplier.CommercialRegistrationNumber,
                CreditLimit = supplier.CreditLimit,
                CurrentBalance = supplier.CurrentBalance,
                PaymentTermsDays = supplier.PaymentTermsDays,
                CreditLimitEnforced = supplier.CreditLimitEnforced,
                PaymentTerms = supplier.PaymentTerms,
                Notes = supplier.Notes,
                LastTransactionDate = supplier.LastTransactionDate,
                CreatedAt = supplier.CreatedAt,
                IsActive = supplier.IsActive,
                IsSystemSupplier = supplier.IsSystemSupplier
            };

            return Ok(ApiResponse<SupplierDto>.SuccessResponse(supplierDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating supplier with ID {SupplierId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating the supplier"));
        }
    }

    /// <summary>
    /// Soft delete a supplier
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        try
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Supplier not found"));
            }

            // Check if this is a system supplier that cannot be deleted
            if (supplier.IsSystemSupplier)
            {
                return BadRequest(ApiResponse.ErrorResponse("Cannot delete system supplier. This supplier is protected and cannot be removed."));
            }

            // Check if supplier has products
            var hasProducts = await _context.Products.AnyAsync(p => p.SupplierId == id);
            if (hasProducts)
            {
                return BadRequest(ApiResponse.ErrorResponse("Cannot delete supplier with associated products"));
            }

            supplier.IsActive = false;
            supplier.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "DELETE",
                "Supplier",
                supplier.Id.ToString(),
                $"Soft deleted supplier: {supplier.CompanyName}"
            );

            return Ok(ApiResponse.SuccessResponse("Supplier deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier with ID {SupplierId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting the supplier"));
        }
    }

    /// <summary>
    /// Get supplier products
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <returns>Supplier products</returns>
    [HttpGet("{id}/products")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<SupplierProductsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSupplierProducts(int id)
    {
        try
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Supplier not found"));
            }

            var products = await _context.Products
                .Where(p => p.SupplierId == id)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new SupplierProductDto
                {
                    ProductId = p.Id,
                    ProductCode = p.ProductCode,
                    ProductName = p.Name,
                    CategoryType = p.CategoryType != null ? p.CategoryType.Name : "Unknown",
                    Weight = p.Weight,
                    CreatedAt = p.CreatedAt,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            var result = new SupplierProductsDto
            {
                SupplierId = supplier.Id,
                SupplierName = supplier.CompanyName,
                Products = products,
                TotalProductCount = products.Count
            };

            return Ok(ApiResponse<SupplierProductsDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products for supplier {SupplierId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving supplier products"));
        }
    }

    /// <summary>
    /// Get supplier balance information
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <returns>Supplier balance details</returns>
    [HttpGet("{id}/balance")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<SupplierBalanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSupplierBalance(int id)
    {
        try
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Supplier not found"));
            }

            // Calculate available credit
            var availableCredit = supplier.CreditLimit - supplier.CurrentBalance;

            // Get recent transactions (you would implement this based on your transaction log system)
            var recentTransactions = new List<SupplierTransactionDto>(); // Placeholder

            var balanceDto = new SupplierBalanceDto
            {
                SupplierId = supplier.Id,
                SupplierName = supplier.CompanyName,
                CreditLimit = supplier.CreditLimit,
                CurrentBalance = supplier.CurrentBalance,
                AvailableCredit = availableCredit,
                PaymentTermsDays = supplier.PaymentTermsDays,
                CreditLimitEnforced = supplier.CreditLimitEnforced,
#pragma warning disable CS8601 // Possible null reference assignment
                LastTransactionDate = supplier.LastTransactionDate,
#pragma warning restore CS8601 // Possible null reference assignment
                RecentTransactions = recentTransactions
            };

            return Ok(ApiResponse<SupplierBalanceDto>.SuccessResponse(balanceDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving balance for supplier {SupplierId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving supplier balance"));
        }
    }

    /// <summary>
    /// Update supplier balance
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <param name="request">Balance update request</param>
    /// <returns>Updated balance information</returns>
    [HttpPut("{id}/balance")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<SupplierBalanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplierBalance(int id, [FromBody] UpdateSupplierBalanceRequestDto request)
    {
        try
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Supplier not found"));
            }

            var oldBalance = supplier.CurrentBalance;

            // Update balance based on transaction type
            switch (request.TransactionType.ToLower())
            {
                case "payment":
                    supplier.CurrentBalance -= request.Amount; // Payment reduces balance
                    break;
                case "credit":
                    supplier.CurrentBalance += request.Amount; // Credit increases balance
                    break;
                case "adjustment":
                    supplier.CurrentBalance = request.Amount; // Direct adjustment
                    break;
                default:
                    return BadRequest(ApiResponse.ErrorResponse("Invalid transaction type. Use 'payment', 'credit', or 'adjustment'"));
            }

            supplier.LastTransactionDate = DateTime.UtcNow;
            supplier.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Create supplier transaction record
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var branchId = User.FindFirst("BranchId")?.Value;
            
            var supplierTransaction = new SupplierTransaction
            {
                SupplierId = supplier.Id,
                TransactionDate = DateTime.UtcNow,
                TransactionType = request.TransactionType.ToLower(),
                Amount = request.Amount,
                BalanceAfterTransaction = supplier.CurrentBalance,
                ReferenceNumber = request.ReferenceNumber,
                Notes = request.Notes,
                CreatedByUserId = userId,
                BranchId = !string.IsNullOrEmpty(branchId) ? int.Parse(branchId) : 1 // Default to branch 1 if not specified
            };

            await _supplierRepository.CreateTransactionAsync(supplierTransaction);

            await _auditService.LogAsync(
                userId,
                "UPDATE",
                "SupplierBalance",
                supplier.Id.ToString(),
                $"Updated balance for supplier: {supplier.CompanyName} - {request.TransactionType}: {request.Amount:C}, Old Balance: {oldBalance:C}, New Balance: {supplier.CurrentBalance:C}"
            );

            // Get recent transactions for the balance DTO
            var recentTransactions = await _supplierRepository.GetRecentTransactionsAsync(supplier.Id, 5);
            var recentTransactionDtos = _mapper.Map<List<SupplierTransactionDto>>(recentTransactions);

            // Calculate available credit
            var availableCredit = supplier.CreditLimit - supplier.CurrentBalance;

            var balanceDto = new SupplierBalanceDto
            {
                SupplierId = supplier.Id,
                SupplierName = supplier.CompanyName,
                CreditLimit = supplier.CreditLimit,
                CurrentBalance = supplier.CurrentBalance,
                AvailableCredit = availableCredit,
                PaymentTermsDays = supplier.PaymentTermsDays,
                CreditLimitEnforced = supplier.CreditLimitEnforced,
                LastTransactionDate = supplier.LastTransactionDate,
                RecentTransactions = recentTransactionDtos
            };

            return Ok(ApiResponse<SupplierBalanceDto>.SuccessResponse(balanceDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating balance for supplier {SupplierId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating supplier balance"));
        }
    }

    /// <summary>
    /// Get supplier transactions
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <param name="fromDate">From date (optional)</param>
    /// <param name="toDate">To date (optional)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>Supplier transaction history</returns>
    [HttpGet("{id}/transactions")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SupplierTransactionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSupplierTransactions(
        int id,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Supplier not found"));
            }

            // Get transactions from repository
            var (transactions, totalCount) = await _supplierRepository.GetTransactionsAsync(
                id, fromDate, toDate, pageNumber, pageSize);

            // Map to DTOs
            var transactionDtos = _mapper.Map<List<SupplierTransactionDto>>(transactions);

            // Create paged result
            var result = new PagedResult<SupplierTransactionDto>
            {
                Items = transactionDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(ApiResponse<PagedResult<SupplierTransactionDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for supplier {SupplierId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving supplier transactions"));
        }
    }

    /// <summary>
    /// Get suppliers with credit alerts
    /// </summary>
    /// <param name="warningPercentage">Warning threshold percentage (default: 80%)</param>
    /// <returns>List of suppliers with credit alerts</returns>
    [HttpGet("credit-alerts")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<List<SupplierCreditAlertDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupplierCreditAlerts([FromQuery] decimal warningPercentage = 0.8m)
    {
        try
        {
            var alerts = await _supplierService.GetAllSupplierCreditAlertsAsync(warningPercentage);
            return Ok(ApiResponse<List<SupplierCreditAlertDto>>.SuccessResponse(alerts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supplier credit alerts");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving supplier credit alerts"));
        }
    }

    /// <summary>
    /// Get suppliers near credit limit
    /// </summary>
    /// <param name="warningPercentage">Warning threshold percentage (default: 80%)</param>
    /// <returns>List of suppliers near credit limit</returns>
    [HttpGet("near-credit-limit")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<List<SupplierCreditAlertDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuppliersNearCreditLimit([FromQuery] decimal warningPercentage = 0.8m)
    {
        try
        {
            var alerts = await _supplierService.GetSuppliersNearCreditLimitAsync(warningPercentage);
            return Ok(ApiResponse<List<SupplierCreditAlertDto>>.SuccessResponse(alerts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suppliers near credit limit");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving suppliers near credit limit"));
        }
    }

    /// <summary>
    /// Get suppliers over credit limit
    /// </summary>
    /// <returns>List of suppliers over credit limit</returns>
    [HttpGet("over-credit-limit")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<List<SupplierCreditAlertDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuppliersOverCreditLimit()
    {
        try
        {
            var alerts = await _supplierService.GetSuppliersOverCreditLimitAsync();
            return Ok(ApiResponse<List<SupplierCreditAlertDto>>.SuccessResponse(alerts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suppliers over credit limit");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving suppliers over credit limit"));
        }
    }

    /// <summary>
    /// Validate supplier credit before purchase
    /// </summary>
    /// <param name="supplierId">Supplier ID</param>
    /// <param name="amount">Purchase amount</param>
    /// <returns>Credit validation result</returns>
    [HttpGet("{supplierId}/validate-credit")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<SupplierCreditValidationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidateSupplierCredit(int supplierId, [FromQuery] decimal amount)
    {
        try
        {
            var result = await _supplierService.ValidateSupplierCreditAsync(supplierId, amount);
            return Ok(ApiResponse<SupplierCreditValidationResult>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating supplier credit. SupplierId: {SupplierId}", supplierId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while validating supplier credit"));
        }
    }
}
