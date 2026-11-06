// <copyright file="ExportLipeQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.Taxation;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Taxation;

/// <summary>
/// Handles <see cref="ExportLipeQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentTenantService">The active company.</param>
public sealed class ExportLipeQueryHandler(IUnitOfWork unitOfWork, ICurrentTenantService currentTenantService) : IRequestHandler<ExportLipeQuery, GeneratedFile>
{
    /// <inheritdoc />
    public Task<GeneratedFile> Handle(ExportLipeQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.VatReturnId, cancellationToken);
    }

    private static string Digits(string? value) => string.Concat((value ?? string.Empty).Where(char.IsAsciiLetterOrDigit)).ToUpperInvariant();

    private async Task<GeneratedFile> HandleInternalAsync(Guid vatReturnId, CancellationToken cancellationToken)
    {
        var vatReturn = await unitOfWork.Repository<VatReturn>().GetByIdAsync(vatReturnId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Liquidazione non trovata.");

        if (!string.Equals(vatReturn.Country, "IT", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("La comunicazione LIPE riguarda solo le liquidazioni IVA italiane.");
        }

        var tenant = await unitOfWork.Repository<Tenant>().GetByIdAsync(currentTenantService.TenantId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Azienda non trovata.");
        var vatNumber = Digits(tenant.VatNumber);

        if (vatNumber.StartsWith("IT", StringComparison.Ordinal))
        {
            vatNumber = vatNumber[2..];
        }

        if (vatNumber.Length != 11)
        {
            throw new InvalidOperationException("Indicare la partita IVA italiana (11 cifre) nei dati dell'azienda.");
        }

        var taxCode = string.IsNullOrWhiteSpace(tenant.FiscalCode) ? vatNumber : Digits(tenant.FiscalCode);
        var from = vatReturn.PeriodFrom.Date;
        var to = vatReturn.PeriodTo.Date;
        var end = to.AddDays(1);
        var purchases = await unitOfWork.Repository<PurchaseInvoice>()
            .FindAsync(p => p.RegistrationDate >= from && p.RegistrationDate < end, cancellationToken)
            .ConfigureAwait(false);
        var monthly = ItalianVatPeriod.IsMonthly(from, to);

        LipeData data = new()
        {
            TaxCode = taxCode,
            VatNumber = vatNumber,
            Year = from.Year,
            Month = monthly ? from.Month : null,
            Quarter = monthly ? null : ItalianVatPeriod.Quarter(from),
            DeclarantTaxCode = string.IsNullOrWhiteSpace(tenant.DeclarantFiscalCode) || string.Equals(Digits(tenant.DeclarantFiscalCode), taxCode, StringComparison.Ordinal)
                ? null
                : Digits(tenant.DeclarantFiscalCode),
            DeclarantRoleCode = string.IsNullOrWhiteSpace(tenant.DeclarantRoleCode) ? "1" : tenant.DeclarantRoleCode,
            ActiveOperations = vatReturn.TaxableTurnover,
            PassiveOperations = purchases.Where(static p => string.Equals(p.Status, "Posted", StringComparison.Ordinal)).Sum(static p => p.TotalNet),
            OutputVat = vatReturn.OutputVat,
            InputVat = vatReturn.InputVat,
            PreviousCredit = vatReturn.PreviousCredit,
            Interest = vatReturn.Interest,
            Payable = vatReturn.Payable,
        };

        var summary = vatReturn.Payable > 0m
            ? string.Create(
                CultureInfo.InvariantCulture,
                $"F24 sezione Erario: codice tributo {ItalianVatPeriod.F24TaxCode(from, to)}, anno {from.Year}, importo {LipeXmlBuilder.FormatAmount(vatReturn.Payable)}.")
            : $"Nessun versamento: credito {LipeXmlBuilder.FormatAmount(-vatReturn.Payable)} riportato al periodo successivo.";

        return new GeneratedFile(LipeXmlBuilder.FileName(data), LipeXmlBuilder.Build(data), summary);
    }
}
