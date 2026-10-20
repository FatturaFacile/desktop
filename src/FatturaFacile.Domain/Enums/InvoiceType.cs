namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies invoice type.
/// </summary>
public enum InvoiceType
{
    /// <summary>
    /// Sales.
    /// </summary>
    Sales = 1,

/// <summary>
                  /// Purchase.
                  /// </summary>
    Purchase = 2,

/// <summary>
                     /// Credit Note.
                     /// </summary>
    CreditNote = 3,

/// <summary>
                       /// Debit Note.
                       /// </summary>
    DebitNote = 4,

/// <summary>
                      /// Fee Note.
                      /// </summary>
    FeeNote = 5,
}
