// <copyright file="ImportPurchaseInvoiceFileCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Purchasing;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Purchasing;

/// <summary>
/// Imports a received FatturaPA file (XML or P7M) as draft supplier invoices.
/// </summary>
/// <param name="FileName">The file name.</param>
/// <param name="Content">The file content.</param>
public sealed record ImportPurchaseInvoiceFileCommand(string FileName, ReadOnlyMemory<byte> Content) : IRequest<PurchaseImportResult>, IAuthorizedRequest
{
    /// <inheritdoc />
    public Permission RequiredPermission => Permission.AccountingWrite;
}
