// <copyright file="ExportCuTelematicQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Exports the working file of the Certificazioni Uniche of a year for the telematic channel.
/// </summary>
/// <param name="Year">The year of the payments.</param>
public sealed record ExportCuTelematicQuery(int Year) : IRequest<GeneratedFile>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
