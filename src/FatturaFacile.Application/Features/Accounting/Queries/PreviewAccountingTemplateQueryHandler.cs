// <copyright file="PreviewAccountingTemplateQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Queries;

/// <summary>
/// Hydrates a template against the local chart of accounts.
/// </summary>
/// <param name="accounts">The chart of accounts repository.</param>
public sealed class PreviewAccountingTemplateQueryHandler(IRepository<ChartOfAccount> accounts) : IRequestHandler<PreviewAccountingTemplateQuery, TemplatePreview>
{
    /// <inheritdoc/>
    public Task<TemplatePreview> Handle(PreviewAccountingTemplateQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return Task.FromResult(TemplateEngine.Preview(request.Template, request.Input, LedgerAccounts.Load(accounts)));
    }
}
