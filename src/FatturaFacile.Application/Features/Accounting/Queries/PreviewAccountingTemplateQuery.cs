// <copyright file="PreviewAccountingTemplateQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Queries;

/// <summary>
/// Hydrates a template with the inputs of the user into journal lines; nothing is written.
/// </summary>
/// <param name="Template">The template.</param>
/// <param name="Input">The inputs.</param>
public sealed record PreviewAccountingTemplateQuery(TemplateDefinition Template, TemplateInput Input) : IRequest<TemplatePreview>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
