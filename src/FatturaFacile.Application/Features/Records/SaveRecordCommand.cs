// <copyright file="SaveRecordCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Records;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Records;

/// <summary>
/// Creates or updates a master data record (contacts, products, accounts, bank accounts, fixed assets).
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <param name="Record">The record values (an empty identifier creates the record).</param>
public sealed record SaveRecordCommand<TEntity>(TEntity Record) : IRequest<Guid>, IAuthorizedRequest
    where TEntity : class, IBaseEntity, new()
{
    /// <inheritdoc/>
    public Permission RequiredPermission => RecordPermissions.Write(typeof(TEntity));
}
