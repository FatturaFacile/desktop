// <copyright file="RecordWithholdingPaymentCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="RecordWithholdingPaymentCommand"/>: the payment is kept offline and sent to the web-api by the next sync.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class RecordWithholdingPaymentCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RecordWithholdingPaymentCommand, bool>
{
    /// <inheritdoc/>
    public Task<bool> Handle(RecordWithholdingPaymentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(RecordWithholdingPaymentCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<WithholdingF24Payment>();
        var payment = await repository.GetByIdAsync(request.PaymentId, cancellationToken).ConfigureAwait(false);
        if (payment is null)
        {
            return false;
        }

        payment.PaidDate = request.PaidDate.Date;
        payment.PaymentReference = string.IsNullOrWhiteSpace(request.PaymentReference) ? null : request.PaymentReference.Trim();
        payment.SyncError = null;
        payment.SyncState = payment.SettlementServerId is null ? VatComplianceSyncState.Synced : VatComplianceSyncState.PendingUpdate;
        repository.Update(payment);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}
