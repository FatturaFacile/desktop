// <copyright file="RegisterVatDocumentsCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="RegisterVatDocumentsCommand"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class RegisterVatDocumentsCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RegisterVatDocumentsCommand, RegisterVatDocumentsResult>
{
    /// <inheritdoc/>
    public Task<RegisterVatDocumentsResult> Handle(RegisterVatDocumentsCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<RegisterVatDocumentsResult> HandleInternalAsync(RegisterVatDocumentsCommand request, CancellationToken cancellationToken)
    {
        var from = request.From.Date;
        var to = request.To.Date;
        var year = from.Year;
        var registry = unitOfWork.Repository<VatRegisterEntry>();

        var existing = await registry.FindAsync(e => e.Year == year || e.Year == year - 1, cancellationToken).ConfigureAwait(false);
        var proRata = request.ProRataPercent ?? (existing.Any(e => e.Year == year - 1 && e.Kind == VatRegisterKind.Sales)
            ? ProRataCalculator.FromSales(existing.Where(e => e.Year == year - 1))
            : ProRataCalculator.FullDeduction);

        var invoices = await unitOfWork.Repository<Invoice>()
            .FindAsync(i => i.Date >= from && i.Date <= to && i.Jurisdiction == "IT" && i.DeletedAt == null, cancellationToken)
            .ConfigureAwait(false);
        var invoiceIds = invoices.Select(static i => i.Id).ToList();
        var items = (await unitOfWork.Repository<InvoiceItem>().FindAsync(i => invoiceIds.Contains(i.InvoiceId), cancellationToken).ConfigureAwait(false)).ToLookup(static i => i.InvoiceId);

        var purchases = await unitOfWork.Repository<PurchaseInvoice>()
            .FindAsync(p => p.RegistrationDate >= from && p.RegistrationDate <= to, cancellationToken)
            .ConfigureAwait(false);
        var purchaseIds = purchases.Select(static p => p.Id).ToList();
        var lines = (await unitOfWork.Repository<PurchaseInvoiceLine>().FindAsync(l => purchaseIds.Contains(l.PurchaseInvoiceId), cancellationToken).ConfigureAwait(false)).ToLookup(static l => l.PurchaseInvoiceId);

        var contacts = await IntentLetterLookup.ContactsAsync(unitOfWork, invoices.Select(static i => i.ContactId).Concat(purchases.Select(static p => p.SupplierId)), cancellationToken).ConfigureAwait(false);

        var registered = existing.Where(static e => e.SourceId is not null).Select(static e => (e.SourceType, e.SourceId!.Value)).ToHashSet();
        var drafts = VatRegisterBuilder.BuildSales(invoices, items, contacts)
            .Concat(VatRegisterBuilder.BuildPurchases(purchases, lines, contacts, proRata))
            .Where(d => !registered.Contains((d.SourceType, d.SourceId!.Value)))
            .ToList();

        VatRegisterNumbering.Assign(drafts, (kind, section, draftYear) => existing.Where(e => e.Kind == kind && e.Section == section && e.Year == draftYear).Select(static e => e.ProtocolNumber).DefaultIfEmpty(0).Max());

        foreach (var draft in drafts)
        {
            await registry.AddAsync(draft, cancellationToken).ConfigureAwait(false);
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new RegisterVatDocumentsResult(
            drafts.Select(static d => (d.SourceType, d.SourceId)).Distinct().Count(static d => string.Equals(d.SourceType, VatRegisterBuilder.InvoiceSource, StringComparison.Ordinal) || string.Equals(d.SourceType, VatRegisterBuilder.ReverseChargeSource, StringComparison.Ordinal)),
            drafts.Select(static d => (d.SourceType, d.SourceId)).Distinct().Count(static d => string.Equals(d.SourceType, VatRegisterBuilder.PurchaseSource, StringComparison.Ordinal)),
            proRata);
    }
}
