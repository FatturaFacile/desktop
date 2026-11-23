// <copyright file="GetVatRegisterQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="GetVatRegisterQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class GetVatRegisterQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetVatRegisterQuery, VatRegisterReport>
{
    /// <inheritdoc/>
    public Task<VatRegisterReport> Handle(GetVatRegisterQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<VatRegisterReport> HandleInternalAsync(GetVatRegisterQuery request, CancellationToken cancellationToken)
    {
        var kind = request.Kind;
        var from = request.From.Date;
        var to = request.To.Date;
        var entries = await unitOfWork.Repository<VatRegisterEntry>()
            .FindAsync(e => e.Kind == kind && e.RegistrationDate >= from && e.RegistrationDate <= to, cancellationToken)
            .ConfigureAwait(false);

        return VatRegisterReportBuilder.Build(kind, request.Section, from, to, request.Section is { } section ? entries.Where(e => e.Section == section) : entries);
    }
}
