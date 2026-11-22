// <copyright file="GetAccountingTemplatesQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Queries;

/// <summary>
/// The journal templates (causali contabili) usable offline: the synced ones, plus the built-in ones for codes not synced.
/// </summary>
public sealed record GetAccountingTemplatesQuery : IRequest<IReadOnlyList<TemplateDefinition>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
