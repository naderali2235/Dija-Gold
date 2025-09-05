using DijaGoldPOS.API.Models.Shared;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models.ManfacturingModels;

/// <summary>
/// Represents the workflow history for manufacturing processes
/// </summary>
public class ManufacturingWorkflowHistory : BaseEntity
{
    /// <summary>
    /// The manufacturing record this history entry belongs to
    /// </summary>

    public int ProductManufactureId { get; set; }

    /// <summary>
    /// Previous status
    /// </summary>


    public string FromStatus { get; set; } = string.Empty;

    /// <summary>
    /// New status
    /// </summary>


    public string ToStatus { get; set; } = string.Empty;

    /// <summary>
    /// Action performed
    /// </summary>


    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// User who performed the action
    /// </summary>


    public string ActionByUserId { get; set; } = string.Empty;

    /// <summary>
    /// User name who performed the action
    /// </summary>


    public string ActionByUserName { get; set; } = string.Empty;

    /// <summary>
    /// Notes about the transition
    /// </summary>

    public string? Notes { get; set; }

    /// <summary>
    /// Navigation property to the manufacturing record
    /// </summary>
    [JsonIgnore]
    public virtual ProductManufacture ProductManufacture { get; set; } = null!;
}
