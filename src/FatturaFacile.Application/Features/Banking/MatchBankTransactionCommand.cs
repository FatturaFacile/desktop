// <copyright file="MatchBankTransactionCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Banking;

/// <summary>
/// Reconciles a bank movement manually with a customer or supplier invoice.
/// </summary>
/// <param name="TransactionId">The movement.</param>
/// <param name="DocumentType">Invoice or PurchaseInvoice.</param>
/// <param name="DocumentId">The document.</param>
public sealed record MatchBankTransactionCommand(Guid TransactionId, string DocumentType, Guid DocumentId) : IRequest<bool>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
