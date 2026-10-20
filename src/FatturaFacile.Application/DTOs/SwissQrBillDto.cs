namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Represents swiss qr bill dto.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="TenantId">The tenant id.</param>
/// <param name="BillNumber">The bill number.</param>
/// <param name="BillDate">The bill date.</param>
/// <param name="ClientId">The client id.</param>
/// <param name="CreditorName">The creditor name.</param>
/// <param name="CreditorAddressFormatted">The creditor address formatted.</param>
/// <param name="CreditorQrIban">The creditor qr iban.</param>
/// <param name="DebtorName">The debtor name.</param>
/// <param name="DebtorAddressFormatted">The debtor address formatted.</param>
/// <param name="Amount">The amount.</param>
/// <param name="Currency">The currency.</param>
/// <param name="ReferenceNumber">The reference number.</param>
/// <param name="AdditionalMessage">The additional message.</param>
/// <param name="DueDate">The due date.</param>
/// <param name="IsPaid">The is paid.</param>
public record SwissQrBillDto(
    Guid Id,
    Guid TenantId,
    string BillNumber,
    DateTime BillDate,
    Guid ClientId,
    string CreditorName,
    string CreditorAddressFormatted,
    string CreditorQrIban,
    string DebtorName,
    string DebtorAddressFormatted,
    decimal Amount,
    string Currency,
    string ReferenceNumber,
    string? AdditionalMessage,
    DateTime DueDate,
    bool IsPaid);
