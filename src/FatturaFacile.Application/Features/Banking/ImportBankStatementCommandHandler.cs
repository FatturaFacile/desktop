// <copyright file="ImportBankStatementCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Banking;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Banking;

/// <summary>
/// Handler for <see cref="ImportBankStatementCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ImportBankStatementCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="reconciler">The reconciler.</param>
public sealed class ImportBankStatementCommandHandler(IUnitOfWork unitOfWork, BankReconciler reconciler) : IRequestHandler<ImportBankStatementCommand, BankImportResultDto>
{
    /// <inheritdoc/>
    public Task<BankImportResultDto> Handle(ImportBankStatementCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<BankImportResultDto> HandleInternalAsync(ImportBankStatementCommand request, CancellationToken cancellationToken)
    {
        var account = await unitOfWork.Repository<BankAccount>().GetByIdAsync(request.BankAccountId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Conto bancario non trovato.");
        var (iban, transactions) = CamtStatementParser.Parse(request.Xml);

        if (iban is not null && !string.Equals(PaymentReferenceNormalizer.Normalize(iban), PaymentReferenceNormalizer.Normalize(account.Iban), StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"L'estratto conto è del conto {iban}, non di {account.Iban}.");
        }

        var accountId = account.Id;
        var existing = await unitOfWork.Repository<BankTransaction>().FindAsync(t => t.BankAccountId == accountId, cancellationToken).ConfigureAwait(false);
        var known = existing.Where(static t => t.ExternalId is not null).Select(static t => t.ExternalId).ToHashSet(StringComparer.Ordinal);
        var imported = 0;
        var duplicates = 0;
        var matched = 0;

        foreach (var transaction in transactions)
        {
            if (transaction.ExternalId is not null && !known.Add(transaction.ExternalId))
            {
                duplicates++;
                continue;
            }

            transaction.BankAccountId = accountId;
            await unitOfWork.Repository<BankTransaction>().AddAsync(transaction, cancellationToken).ConfigureAwait(false);
            imported++;

            if (await reconciler.TryAutoMatchAsync(transaction, cancellationToken).ConfigureAwait(false))
            {
                matched++;
            }
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new BankImportResultDto(imported, duplicates, matched);
    }
}
