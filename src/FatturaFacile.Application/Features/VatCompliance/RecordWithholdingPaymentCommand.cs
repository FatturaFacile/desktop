// <copyright file="RecordWithholdingPaymentCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Records the payment of the withholdings of a month with the Modello F24.
/// </summary>
/// <param name="PaymentId">The F24 payment.</param>
/// <param name="PaidDate">The date of the payment.</param>
/// <param name="PaymentReference">The reference of the payment.</param>
public sealed record RecordWithholdingPaymentCommand(Guid PaymentId, DateTime PaidDate, string? PaymentReference) : IRequest<bool>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
