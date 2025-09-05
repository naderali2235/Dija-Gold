using DijaGoldPOS.API.Models.ManfacturingModels;

namespace DijaGoldPOS.API.IServices;

/// <summary>
/// Interface for manufacturing workflow management
/// </summary>
public interface IManufacturingWorkflowService
{
    /// <summary>
    /// Transitions a manufacturing record to the next workflow step
    /// </summary>
    Task<bool> TransitionWorkflowAsync(int productManufactureId, string targetStatus, string? notes = null);

    /// <summary>
    /// Performs quality check on a manufacturing record
    /// </summary>
    Task<bool> PerformQualityCheckAsync(int productManufactureId, bool passed, string? notes = null);

    /// <summary>
    /// Performs final approval on a manufacturing record
    /// </summary>
    Task<bool> PerformFinalApprovalAsync(int productManufactureId, bool approved, string? notes = null);

    /// <summary>
    /// Gets workflow history for a manufacturing record
    /// </summary>
    Task<IEnumerable<ManufacturingWorkflowHistory>> GetWorkflowHistoryAsync(int productManufactureId);

    /// <summary>
    /// Gets available transitions for current status
    /// </summary>
    IEnumerable<string> GetAvailableTransitions(string currentStatus);
}
