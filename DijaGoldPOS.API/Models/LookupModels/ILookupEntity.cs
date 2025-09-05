using DijaGoldPOS.API.Models.Shared;

namespace DijaGoldPOS.API.Models.LookupModels;

/// <summary>
/// Interface for all lookup entities providing consistent structure
/// </summary>
public interface ILookupEntity
{
    /// <summary>
    /// Unique identifier for the lookup entry
    /// </summary>
    int Id { get; set; }

    /// <summary>
    /// Display name of the lookup entry
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Detailed description of the lookup entry
    /// </summary>
    string? Description { get; set; }

    /// <summary>
    /// Display order for UI sorting (optional)
    /// </summary>
    int? DisplayOrder { get; set; }

    /// <summary>
    /// Whether this lookup entry is active
    /// </summary>
    bool IsActive { get; set; }

    /// <summary>
    /// Whether this is a system-managed lookup that cannot be deleted
    /// </summary>
    bool IsSystemManaged { get; set; }

    /// <summary>
    /// Additional metadata in JSON format
    /// </summary>
    string? Metadata { get; set; }
}

/// <summary>
/// Base class for all lookup entities
/// </summary>
public abstract class BaseLookupEntity : BaseEntity
{
    /// <summary>
    /// Display name of the lookup entry
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the lookup entry
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display order for UI sorting (optional)
    /// </summary>
    public int? DisplayOrder { get; set; }

    /// <summary>
    /// Whether this is a system-managed lookup that cannot be deleted
    /// </summary>
    public bool IsSystemManaged { get; set; } = false;
}
