// <copyright file="DeleteIntentLetterCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Deletes a declaration of intent that no invoice used.
/// </summary>
/// <param name="LetterId">The letter.</param>
public sealed record DeleteIntentLetterCommand(Guid LetterId) : IRequest<bool>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
