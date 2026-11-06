// <copyright file="ImportWithholdingTaxTariffsCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Imports an official ESTV withholding tax tariff file (tarCCYY.txt), replacing the rows with the same canton and validity date.
/// </summary>
/// <param name="Content">The file content.</param>
public sealed record ImportWithholdingTaxTariffsCommand(string Content) : IRequest<int>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.PayrollWrite;
}
