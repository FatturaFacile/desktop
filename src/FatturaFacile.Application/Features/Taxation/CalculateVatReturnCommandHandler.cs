// <copyright file="CalculateVatReturnCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Taxation;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Taxation;

/// <summary>
/// Handler for <see cref="CalculateVatReturnCommand"/>: issued customer invoices and posted supplier invoices of the
/// period; the credit of the previous closed return is carried over.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CalculateVatReturnCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentTenantService">The current company.</param>
public sealed class CalculateVatReturnCommandHandler(IUnitOfWork unitOfWork, ICurrentTenantService currentTenantService)
    : IRequestHandler<CalculateVatReturnCommand, Guid>
{
    /// <inheritdoc/>
    public Task<Guid> Handle(CalculateVatReturnCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.To < request.From
            ? throw new ArgumentException("Il periodo non è valido.", nameof(request))
            : HandleInternalAsync(request.From.Date, request.To.Date, cancellationToken);
    }

    private async Task<Guid> HandleInternalAsync(DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var returns = await unitOfWork.Repository<VatReturn>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);

        if (returns.Any(r => string.Equals(r.Status, "Final", StringComparison.Ordinal) && r.PeriodFrom <= to && r.PeriodTo >= from))
        {
            throw new InvalidOperationException("Esiste già una liquidazione chiusa che copre questo periodo.");
        }

        var end = to.AddDays(1);
        var invoices = await unitOfWork.Repository<Invoice>().FindAsync(i => i.Date >= from && i.Date < end, cancellationToken).ConfigureAwait(false);
        var issued = invoices.Where(static i => !string.Equals(i.Status, "draft", StringComparison.OrdinalIgnoreCase) && i.DeletedAt is null).ToList();
        var ids = issued.Select(static i => i.Id).ToHashSet();
        var items = await unitOfWork.Repository<InvoiceItem>().FindAsync(i => ids.Contains(i.InvoiceId), cancellationToken).ConfigureAwait(false);
        List<(Invoice Invoice, IReadOnlyList<InvoiceItem> Items)> sales = [];

        foreach (var invoice in issued)
        {
            sales.Add((invoice, items.Where(it => it.InvoiceId == invoice.Id).ToList()));
        }

        var purchases = await unitOfWork.Repository<PurchaseInvoice>().FindAsync(p => p.RegistrationDate >= from && p.RegistrationDate < end, cancellationToken).ConfigureAwait(false);
        var posted = purchases.Where(static p => !string.Equals(p.Status, "Draft", StringComparison.Ordinal)).ToList();

        var previous = returns
            .Where(r => string.Equals(r.Status, "Final", StringComparison.Ordinal) && r.PeriodTo < from)
            .OrderByDescending(static r => r.PeriodTo)
            .FirstOrDefault();
        var previousCredit = previous is { Payable: < 0m } ? -previous.Payable : 0m;

        var computed = VatReturnCalculator.Compute(currentTenantService.CountryCode, from, to, sales, posted, previousCredit);

        foreach (var draft in returns.Where(r => string.Equals(r.Status, "Draft", StringComparison.Ordinal) && r.PeriodFrom == from && r.PeriodTo == to))
        {
            unitOfWork.Repository<VatReturn>().Delete(draft);
        }

        await unitOfWork.Repository<VatReturn>().AddAsync(computed, cancellationToken).ConfigureAwait(false);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return computed.Id;
    }
}
