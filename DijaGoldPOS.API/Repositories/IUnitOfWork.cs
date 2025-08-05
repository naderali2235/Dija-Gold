namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Unit of Work interface for coordinating repository operations and managing database transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repository properties
    IProductRepository Products { get; }
    IInventoryRepository Inventory { get; }
    IInventoryMovementRepository InventoryMovements { get; }
    ITransactionRepository Transactions { get; }
    IGoldRateRepository GoldRates { get; }
    ICustomerRepository Customers { get; }
    IBranchRepository Branches { get; }
    ISupplierRepository Suppliers { get; }

    /// <summary>
    /// Save all changes to the database
    /// </summary>
    /// <returns>Number of entities saved</returns>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Save all changes to the database with cancellation token
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities saved</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Begin a database transaction
    /// </summary>
    /// <returns>Database transaction</returns>
    Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync();

    /// <summary>
    /// Begin a database transaction with cancellation token
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Database transaction</returns>
    Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Execute a function within a database transaction
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <returns>Result of the operation</returns>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);

    /// <summary>
    /// Execute an action within a database transaction
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    Task ExecuteInTransactionAsync(Func<Task> operation);

    /// <summary>
    /// Check if the unit of work has been disposed
    /// </summary>
    bool IsDisposed { get; }
}