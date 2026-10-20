using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

public record ClientDto(
    Guid Id,
    Guid TenantId,
    string BusinessName,
    string? TradeName,
    Jurisdiction Jurisdiction,
    string? FiscalCode,
    string? VatNumber,
    string? SdiCode,
    string? PecEmail,
    string? SwissUid,
    string? QrIban,
    string? CantonalCode,
    string AddressFormatted,
    string Email,
    string? PhoneNumber,
    int PaymentTermDays,
    decimal CreditLimitAmount,
    string CreditLimitCurrency,
    bool IsActive);

public record ItalianInvoiceDto(
    Guid Id,
    Guid TenantId,
    string DocumentNumber,
    DateTime DocumentDate,
    string FormatoTrasmissione,
    string ProgressivoInvio,
    Guid ClientId,
    string CedenteDenominazione,
    string CedentePartitaIva,
    string CessionarioDenominazione,
    string? CessionarioPartitaIva,
    string CessionarioCodiceDestinatario,
    decimal TotalNet,
    decimal TotalVat,
    decimal TotalGross,
    string Currency,
    DateTime PaymentDueDate,
    SdiTransmissionStatus SdiStatus,
    string? SdiIdentifier,
    List<InvoiceLineDto> Lines);

public record InvoiceLineDto(
    int LineNumber,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal VatRate,
    string? NaturaExemptionCode,
    decimal LineTotal);

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

public record CustomerVehicleDto(
    Guid Id,
    Guid TenantId,
    Guid ClientId,
    string LicensePlate,
    string VinChassisNumber,
    string Brand,
    string Model,
    int RegistrationYear,
    Jurisdiction Jurisdiction,
    int CurrentKilometers);

public record TireHotelDto(
    Guid Id,
    Guid TenantId,
    Guid VehicleId,
    string VehiclePlate,
    TireSeason Season,
    string Brand,
    string Dimension,
    string DotCode,
    decimal TreadDepthFrontLeftMm,
    decimal TreadDepthFrontRightMm,
    decimal TreadDepthRearLeftMm,
    decimal TreadDepthRearRightMm,
    string WarehouseRackCoordinate,
    string Status,
    DateTime StoredDateUtc);

public record TimeEntryDto(
    Guid Id,
    Guid TenantId,
    Guid EmployeeId,
    string EmployeeName,
    DateTime ClockInUtc,
    DateTime? ClockOutUtc,
    int BreakDurationMinutes,
    decimal TotalNetHours,
    RecordingType RecordingType,
    double? GpsLatitude,
    double? GpsLongitude);

public record AbsenceDto(
    Guid Id,
    Guid TenantId,
    Guid EmployeeId,
    AbsenceType AbsenceType,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalWorkingDays,
    bool IsApproved,
    string? CertificateNumber);

public record SupplierDto(
    Guid Id,
    Guid TenantId,
    string Name,
    Jurisdiction Jurisdiction,
    string? VatNumber,
    string? SwissUid,
    string? SdiCode,
    string AddressFormatted,
    string? Iban,
    int PaymentTermsDays,
    bool IsActive);

public record PurchaseOrderDto(
    Guid Id,
    Guid TenantId,
    string OrderNumber,
    Guid SupplierId,
    string SupplierName,
    DateTime OrderDate,
    decimal TotalAmount,
    string Currency,
    string Status);

public record TransportDocumentDto(
    Guid Id,
    Guid TenantId,
    string DocumentNumber,
    DateTime DocumentDate,
    Guid ClientId,
    string ClientName,
    string DestinationAddressFormatted,
    string TransportationReason,
    string CarrierName,
    int NumberOfPackages,
    decimal TotalWeightKg,
    string? CustomsCmrNotes);

public record TenantDto(
    Guid Id,
    string Code,
    string BusinessName,
    string CountryCode,
    Currency BaseCurrency,
    string? VatNumber,
    string? FiscalCode,
    string? SdiCode,
    string? SwissUid,
    string? QrIban,
    string ContactEmail,
    bool IsActive);

public record UserDto(
    Guid Id,
    Guid TenantId,
    string Username,
    string Email,
    string FullName,
    UserRole Role,
    bool IsActive,
    DateTime? LastLoginUtc);

public record AppTranslationDto(
    Guid Id,
    Guid? TenantId,
    string Locale,
    string Key,
    string DefaultValue,
    string? CustomValue,
    string EffectiveValue,
    DateTime UpdatedAtUtc);

public record DashboardKpiDto(
    decimal TotalRevenueEur,
    decimal TotalRevenueChf,
    decimal PendingReceivablesEur,
    decimal PendingReceivablesChf,
    decimal PendingVatLiabilityEur,
    decimal PendingVatLiabilityChf,
    DateTime? NextTaxDeadline,
    int ActiveClientsCount,
    int PendingInvoicesCount,
    int StoredTireSetsCount);
