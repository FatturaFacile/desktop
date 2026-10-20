namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies production status.
/// </summary>
public enum ProductionStatus
{
    /// <summary>
    /// Planned.
    /// </summary>
    Planned = 1,

/// <summary>
                    /// In Production.
                    /// </summary>
    InProduction = 2,

/// <summary>
                         /// Completed.
                         /// </summary>
    Completed = 3,

/// <summary>
                      /// Cancelled.
                      /// </summary>
    Cancelled = 4,
}
