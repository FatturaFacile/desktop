// <copyright file="PostAccountingTemplateCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Commands;

/// <summary>
/// Hydrates a template against the local chart of accounts and creates the entry through the ordinary create command, so the
/// same rules and numbering apply.
/// </summary>
/// <param name="sender">The MediatR sender.</param>
/// <param name="accounts">The chart of accounts repository.</param>
public sealed class PostAccountingTemplateCommandHandler(ISender sender, IRepository<ChartOfAccount> accounts) : IRequestHandler<PostAccountingTemplateCommand, GeneralLedgerEntryDto>
{
    /// <inheritdoc/>
    public Task<GeneralLedgerEntryDto> Handle(PostAccountingTemplateCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var preview = TemplateEngine.Preview(request.Template, request.Input, LedgerAccounts.Load(accounts));
        if (!preview.IsBalanced)
        {
            throw new InvalidOperationException("The template does not yield a balanced entry for the given inputs.");
        }

        var description = string.IsNullOrWhiteSpace(request.Description) ? request.Template.Name : request.Description;

        return sender.Send(
            new CreateJournalEntryCommand(request.PostingDate, description, request.Template.Ledger, preview.Lines, request.PostImmediately, "accounting_template"),
            cancellationToken);
    }
}
