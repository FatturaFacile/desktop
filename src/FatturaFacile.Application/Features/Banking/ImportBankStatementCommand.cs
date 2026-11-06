// <copyright file="ImportBankStatementCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Banking;

/// <summary>
/// Imports an ISO 20022 camt.053/054 statement into a bank account, skipping movements already imported, and
/// reconciles them automatically.
/// </summary>
/// <param name="BankAccountId">The bank account.</param>
/// <param name="Xml">The statement XML.</param>
public sealed record ImportBankStatementCommand(Guid BankAccountId, string Xml) : IRequest<BankImportResultDto>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
