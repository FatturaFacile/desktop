// <copyright file="UpdateProvisionRatesCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Changes the bad-debt provision rates per aging bucket.
/// </summary>
/// <param name="Request">The rates.</param>
public sealed record UpdateProvisionRatesCommand(ProvisionRatesRequest Request) : IRequest<ReceivablesActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
