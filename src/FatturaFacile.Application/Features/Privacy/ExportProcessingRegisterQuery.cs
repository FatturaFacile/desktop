// <copyright file="ExportProcessingRegisterQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Privacy;

/// <summary>
/// Exports the register of processing activities (art. 12 nLPD, art. 30 GDPR).
/// </summary>
public sealed record ExportProcessingRegisterQuery : IRequest<GeneratedFile>, IAuthorizedRequest
{
    /// <inheritdoc />
    public Permission RequiredPermission => Permission.CompanySettingsManage;
}
