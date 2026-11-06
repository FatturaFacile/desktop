// <copyright file="ImportWithholdingTaxTariffsCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Payroll.Swiss;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Handler for <see cref="ImportWithholdingTaxTariffsCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ImportWithholdingTaxTariffsCommandHandler"/> class.
/// </remarks>
/// <param name="store">The tariff store.</param>
public sealed class ImportWithholdingTaxTariffsCommandHandler(IWithholdingTaxTariffStore store)
    : IRequestHandler<ImportWithholdingTaxTariffsCommand, int>
{
    /// <inheritdoc/>
    public Task<int> Handle(ImportWithholdingTaxTariffsCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var rows = WithholdingTaxTariffParser.Parse(request.Content);
        return store.ReplaceAsync(rows, cancellationToken);
    }
}
