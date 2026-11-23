// <copyright file="SyncVatComplianceCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Syncs the VAT compliance data of a year with the web-api.
/// </summary>
/// <param name="Year">The year.</param>
public sealed record SyncVatComplianceCommand(int Year) : IRequest<VatComplianceSyncResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
