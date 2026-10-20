namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies movement type.
/// </summary>
public enum MovementType
{
    /// <summary>
    /// Receipt.
    /// </summary>
    Receipt = 1,

/// <summary>
                    /// Issue.
                    /// </summary>
    Issue = 2,

/// <summary>
                  /// Transfer.
                  /// </summary>
    Transfer = 3,

/// <summary>
                     /// Adjustment.
                     /// </summary>
    Adjustment = 4,
}
