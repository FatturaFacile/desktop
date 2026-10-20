namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies qr bill type.
/// </summary>
public enum QrBillType
{
    /// <summary>
    /// Qr Iban With Reference.
    /// </summary>
    QrIbanWithReference = 1,

/// <summary>
                                /// Iban With Scor.
                                /// </summary>
    IbanWithScor = 2,

/// <summary>
                         /// Iban Without Reference.
                         /// </summary>
    IbanWithoutReference = 3,
}
