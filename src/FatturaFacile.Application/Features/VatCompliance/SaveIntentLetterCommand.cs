// <copyright file="SaveIntentLetterCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Creates or changes a declaration of intent offline; it is sent to the web-api by the next sync.
/// </summary>
/// <param name="Input">The letter.</param>
public sealed record SaveIntentLetterCommand(IntentLetterInput Input) : IRequest<Guid>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
