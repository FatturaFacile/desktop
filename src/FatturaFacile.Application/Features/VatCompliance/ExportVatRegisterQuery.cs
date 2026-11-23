// <copyright file="ExportVatRegisterQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Exports a VAT register of a period as a PDF or CSV file; the print is a preview, not the definitive print.
/// </summary>
/// <param name="Kind">The register.</param>
/// <param name="Section">The sezionale, or null for all.</param>
/// <param name="From">The first day.</param>
/// <param name="To">The last day.</param>
/// <param name="Pdf">True for a PDF, false for a CSV.</param>
public sealed record ExportVatRegisterQuery(VatRegisterKind Kind, VatRegisterSection? Section, DateTime From, DateTime To, bool Pdf) : IRequest<GeneratedFile>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
