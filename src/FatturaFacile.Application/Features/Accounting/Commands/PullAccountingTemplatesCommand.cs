// <copyright file="PullAccountingTemplatesCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Commands;

/// <summary>
/// Pulls the accounting templates (causali contabili) of the tenant from the web-api into the local cache.
/// </summary>
public sealed record PullAccountingTemplatesCommand : IRequest<int>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
