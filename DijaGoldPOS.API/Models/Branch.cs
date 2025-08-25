namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents a branch/store location
/// </summary>
public class Branch : BaseEntity
{
    /// <summary>
    /// Branch name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Branch code for identification
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Branch address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Branch phone number
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Manager name
    /// </summary>
    public string? ManagerName { get; set; }
    
    /// <summary>
    /// Whether this is the main branch/headquarters
    /// </summary>
    public bool IsHeadquarters { get; set; } = false;
    
    /// <summary>
    /// Navigation property to users assigned to this branch
    /// </summary>
    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    
    /// <summary>
    /// Navigation property to inventory items at this branch
    /// </summary>
    public virtual ICollection<Inventory> InventoryItems { get; set; } = new List<Inventory>();
    
    /// <summary>
    /// Navigation property to financial transactions at this branch
    /// </summary>
    public virtual ICollection<FinancialTransaction> FinancialTransactions { get; set; } = new List<FinancialTransaction>();
    
    /// <summary>
    /// Navigation property to orders at this branch
    /// </summary>
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();



    /// <summary>
    /// Navigation property to cash drawer balances for this branch
    /// </summary>
    public virtual ICollection<CashDrawerBalance> CashDrawerBalances { get; set; } = new List<CashDrawerBalance>();

    /// <summary>
    /// Navigation property to product ownerships at this branch
    /// </summary>
    public virtual ICollection<ProductOwnership> ProductOwnerships { get; set; } = new List<ProductOwnership>();

    /// <summary>
    /// Navigation property to customer purchases at this branch
    /// </summary>
    public virtual ICollection<CustomerPurchase> CustomerPurchases { get; set; } = new List<CustomerPurchase>();
}
