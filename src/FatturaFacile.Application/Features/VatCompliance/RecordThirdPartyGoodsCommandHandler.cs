// <copyright file="RecordThirdPartyGoodsCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="RecordThirdPartyGoodsCommand"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class RecordThirdPartyGoodsCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RecordThirdPartyGoodsCommand, Guid>
{
    /// <inheritdoc/>
    public Task<Guid> Handle(RecordThirdPartyGoodsCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<Guid> HandleInternalAsync(RecordThirdPartyGoodsCommand request, CancellationToken cancellationToken)
    {
        var date = request.Date.Date;
        var registry = unitOfWork.Repository<VatRegisterEntry>();
        var yearEntries = await registry.FindAsync(e => e.Year == date.Year && e.Kind == VatRegisterKind.ThirdPartyGoods, cancellationToken).ConfigureAwait(false);

        // Goods held for third parties are outside the VAT of the company: the value is registered, never a VAT amount.
        VatRegisterEntry entry = new()
        {
            Kind = VatRegisterKind.ThirdPartyGoods,
            Section = VatRegisterSection.Ordinary,
            Year = date.Year,
            RegistrationDate = date,
            DocumentDate = date,
            DocumentNumber = request.DocumentNumber.Trim(),
            PartnerName = request.PartnerName.Trim(),
            Description = request.Description.Trim(),
            TaxableAmount = request.Value,
            SourceType = VatRegisterBuilder.ManualSource,
            SourceId = Guid.NewGuid(),
        };

        VatRegisterNumbering.Assign([entry], (_, _, _) => yearEntries.Select(static e => e.ProtocolNumber).DefaultIfEmpty(0).Max());
        await registry.AddAsync(entry, cancellationToken).ConfigureAwait(false);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return entry.Id;
    }
}
