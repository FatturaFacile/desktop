namespace FatturaFacile.Application.DTOs;

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