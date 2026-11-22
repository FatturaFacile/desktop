// <copyright file="PullAccountingTemplatesCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Accounting;
using FatturaFacile.Domain.Common;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Commands;

/// <summary>
/// Replaces the cached templates with the ones the server sends (matched by identifier).
/// </summary>
/// <param name="api">The ledger gateway.</param>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentTenantService">The active tenant.</param>
/// <param name="time">The clock.</param>
public sealed class PullAccountingTemplatesCommandHandler(ILedgerApiClient api, IUnitOfWork unitOfWork, ICurrentTenantService currentTenantService, TimeProvider time) : IRequestHandler<PullAccountingTemplatesCommand, int>
{
    /// <inheritdoc/>
    public Task<int> Handle(PullAccountingTemplatesCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(cancellationToken);
    }

    private async Task<int> HandleInternalAsync(CancellationToken cancellationToken)
    {
        var json = await api.GetTemplatesAsync(cancellationToken).ConfigureAwait(false);
        var pulled = LedgerApiMapper.ParseTemplates(json, currentTenantService.TenantId, time.GetUtcNow().UtcDateTime);
        var repository = unitOfWork.Repository<AccountingTemplate>();
        var cached = (await repository.FindAsync(static _ => true, cancellationToken).ConfigureAwait(false)).ToDictionary(static t => t.Id);

        foreach (var template in pulled)
        {
            if (cached.TryGetValue(template.Id, out var existing))
            {
                existing.Code = template.Code;
                existing.Category = template.Category;
                existing.LedgerType = template.LedgerType;
                existing.NamesJson = template.NamesJson;
                existing.ParametersJson = template.ParametersJson;
                existing.LinesJson = template.LinesJson;
                existing.IsSystem = template.IsSystem;
                existing.SyncedAt = template.SyncedAt;
            }
            else
            {
                await repository.AddAsync(template, cancellationToken).ConfigureAwait(false);
            }
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return pulled.Count;
    }
}
