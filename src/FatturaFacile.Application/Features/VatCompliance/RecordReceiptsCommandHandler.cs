// <copyright file="RecordReceiptsCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="RecordReceiptsCommand"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class RecordReceiptsCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RecordReceiptsCommand, int>
{
    private const string ReceiptsDescription = "Corrispettivi";

    /// <inheritdoc/>
    public Task<int> Handle(RecordReceiptsCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private static IReadOnlyList<(decimal Rate, decimal Taxable, decimal Vat)> SplitLines(RecordReceiptsCommand request, IReadOnlyList<VatRegisterEntry> yearEntries) =>
        request.Allocations.Count > 0
            ? [.. request.Allocations.Select(static a => SplitOf(a))]
            : ReceiptsVentilation.Distribute(request.GrossAmount, PurchasesByRate(yearEntries));

    private static (decimal Rate, decimal Taxable, decimal Vat) SplitOf(ReceiptAllocation allocation)
    {
        var (taxable, vat) = ReceiptsVentilation.Scorporo(allocation.Gross, allocation.VatRate);

        return (allocation.VatRate, taxable, vat);
    }

    private static IReadOnlyList<(decimal Rate, decimal Taxable)> PurchasesByRate(IReadOnlyList<VatRegisterEntry> entries) =>
        [.. entries.Where(static e => e.Kind == VatRegisterKind.Purchases && !e.IsReverseCharge && e.TaxableAmount > 0m).Select(static e => (e.VatRate, e.TaxableAmount))];

    private static VatRegisterEntry NewEntry(DateTime date, (decimal Rate, decimal Taxable, decimal Vat) line, Guid document) => new()
    {
        Kind = VatRegisterKind.Receipts,
        Section = VatRegisterSection.Ordinary,
        Year = date.Year,
        RegistrationDate = date,
        DocumentDate = date,
        DocumentNumber = date.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
        Description = ReceiptsDescription,
        TaxableAmount = line.Taxable,
        VatRate = line.Rate,
        VatAmount = line.Vat,
        SourceType = VatRegisterBuilder.ManualSource,
        SourceId = document,
    };

    private async Task<int> HandleInternalAsync(RecordReceiptsCommand request, CancellationToken cancellationToken)
    {
        var date = request.Date.Date;
        var registry = unitOfWork.Repository<VatRegisterEntry>();
        var yearEntries = await registry.FindAsync(e => e.Year == date.Year, cancellationToken).ConfigureAwait(false);

        var split = SplitLines(request, yearEntries);

        var document = Guid.NewGuid();
        var drafts = split.Select(line => NewEntry(date, line, document)).ToList();

        VatRegisterNumbering.Assign(drafts, (kind, section, year) => yearEntries.Where(e => e.Kind == kind && e.Section == section && e.Year == year).Select(static e => e.ProtocolNumber).DefaultIfEmpty(0).Max());

        foreach (var draft in drafts)
        {
            await registry.AddAsync(draft, cancellationToken).ConfigureAwait(false);
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return drafts.Count;
    }
}
