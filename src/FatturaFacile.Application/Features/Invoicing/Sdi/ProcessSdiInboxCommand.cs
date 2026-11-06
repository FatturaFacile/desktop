// <copyright file="ProcessSdiInboxCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Invoicing.Sdi;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Invoicing.Sdi;

/// <summary>
/// Processes the files received from the SDI: updates the state of the sent invoices and imports the supplier invoices.
/// </summary>
public sealed record ProcessSdiInboxCommand : IRequest<SdiInboxResult>, IAuthorizedRequest
{
    /// <inheritdoc />
    public Permission RequiredPermission => Permission.InvoicesIssue;
}
