using DijaGoldPOS.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Unit of Work implementation for coordinating repository operations and managing database transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private bool _disposed = false;

    // Repository instances (lazy initialization)
    private IProductRepository? _products;
    private IInventoryRepository? _inventory;
    private IInventoryMovementRepository? _inventoryMovements;
    private ITransactionRepository? _transactions;
    private IGoldRateRepository? _goldRates;
    private ICustomerRepository? _customers;
    private IBranchRepository? _branches;
    private ISupplierRepository? _suppliers;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // Repository properties with lazy initialization
    public IProductRepository Products =>
        _products ??= new ProductRepository(_context);

    public IInventoryRepository Inventory =>
        _inventory ??= new InventoryRepository(_context);

    public IInventoryMovementRepository InventoryMovements =>
        _inventoryMovements ??= new InventoryMovementRepository(_context);

    public ITransactionRepository Transactions =>
        _transactions ??= new TransactionRepository(_context);

    public IGoldRateRepository GoldRates =>
        _goldRates ??= new GoldRateRepository(_context);

    public ICustomerRepository Customers =>
        _customers ??= new CustomerRepository(_context);

    public IBranchRepository Branches =>
        _branches ??= new BranchRepository(_context);

    public ISupplierRepository Suppliers =>
        _suppliers ??= new SupplierRepository(_context);

    /// <summary>
    /// Check if the unit of work has been disposed
    /// </summary>
    public bool IsDisposed => _disposed;

    /// <summary>
    /// Save all changes to the database
    /// </summary>
    public async Task<int> SaveChangesAsync()
    {
        ThrowIfDisposed();
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Save all changes to the database with cancellation token
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Begin a database transaction
    /// </summary>
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        ThrowIfDisposed();
        return await _context.Database.BeginTransactionAsync();
    }

    /// <summary>
    /// Begin a database transaction with cancellation token
    /// </summary>
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Execute a function within a database transaction
    /// </summary>
    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
    {
        ThrowIfDisposed();
        
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        using var transaction = await BeginTransactionAsync();
        try
        {
            var result = await operation();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Execute an action within a database transaction
    /// </summary>
    public async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        ThrowIfDisposed();
        
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        using var transaction = await BeginTransactionAsync();
        try
        {
            await operation();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose resources
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context?.Dispose();
            _disposed = true;
        }
    }

    /// <summary>
    /// Throw exception if disposed
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(UnitOfWork));
        }
    }
}