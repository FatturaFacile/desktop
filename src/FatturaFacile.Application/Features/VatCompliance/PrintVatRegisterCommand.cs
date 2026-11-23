// <copyright file="PrintVatRegisterCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Prints a VAT register definitively: seals the lines, numbers the pages progressively in the year and records the print for audit.
/// </summary>
/// <param name="Kind">The register.</param>
/// <param name="Section">The sezionale.</param>
/// <param name="Year">The register year.</param>
/// <param name="From">The first day.</param>
/// <param name="To">The last day.</param>
/// <param name="VirtualStampReference">The reference of the virtual stamp authorisation.</param>
public sealed record PrintVatRegisterCommand(VatRegisterKind Kind, VatRegisterSection Section, int Year, DateTime From, DateTime To, string? VirtualStampReference) : IRequest<PrintVatRegisterResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
