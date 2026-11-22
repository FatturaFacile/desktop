// <copyright file="PostAccountingTemplateCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Commands;

/// <summary>
/// Hydrates a template and creates the entry in the ledger book of the template.
/// </summary>
/// <param name="Template">The template.</param>
/// <param name="Input">The inputs of the user.</param>
/// <param name="PostingDate">The posting date.</param>
/// <param name="Description">The narrative; the template name when empty.</param>
/// <param name="PostImmediately">Whether to post the entry or keep it a draft.</param>
public sealed record PostAccountingTemplateCommand(TemplateDefinition Template, TemplateInput Input, DateTime PostingDate, string? Description, bool PostImmediately = true) : IRequest<GeneralLedgerEntryDto>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
