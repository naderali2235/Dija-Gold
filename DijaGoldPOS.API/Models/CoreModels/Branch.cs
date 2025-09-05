using DijaGoldPOS.API.Models.CustomerModels;
using DijaGoldPOS.API.Models.FinancialModels;
using DijaGoldPOS.API.Models.InventoryModels;
using DijaGoldPOS.API.Models.ProductModels;
using DijaGoldPOS.API.Models.SalesModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models.CoreModels;

/// <summary>
/// Represents a branch/store location in the system
/// </summary>
[Table("Branches", Schema = "Core")]
public class Branch : BaseEntity
{
    /// <summary>
    /// Unique branch code for identification
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Branch display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Physical address of the branch
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Branch phone number
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Name of the branch manager
    /// </summary>
    public string? ManagerName { get; set; }

    /// <summary>
    /// Whether this branch is currently operational
    /// </summary>
    public bool IsOperational { get; set; } = true;

    /// <summary>
    /// Branch opening hours (JSON format)
    /// </summary>
    public string? OperatingHours { get; set; }

    /// <summary>
    /// Maximum cash limit for this branch
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxCashLimit { get; set; }

    // Navigation Properties
    /// <summary>
    /// Users assigned to this branch
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    /// <summary>
    /// Financial transactions processed at this branch
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<FinancialTransaction> FinancialTransactions { get; set; } = new List<FinancialTransaction>();

    /// <summary>
    /// Orders created at this branch
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    /// <summary>
    /// Cash drawer balances for this branch
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<CashDrawerBalance> CashDrawerBalances { get; set; } = new List<CashDrawerBalance>();

    /// <summary>
    /// Product ownerships at this branch
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<ProductOwnership> ProductOwnerships { get; set; } = new List<ProductOwnership>();

    /// <summary>
    /// Inventory items at this branch
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<Inventory> InventoryItems { get; set; } = new List<Inventory>();

    /// <summary>
    /// Customer purchases at this branch
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<CustomerPurchase> CustomerPurchases { get; set; } = new List<CustomerPurchase>();
}
