// <copyright file="DeleteRecordCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Records;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Records;

/// <summary>
/// Deletes a master data record (logically when the entity supports it).
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <param name="Id">The record identifier.</param>
public sealed record DeleteRecordCommand<TEntity>(Guid Id) : IRequest<bool>, IAuthorizedRequest
    where TEntity : class, IBaseEntity, new()
{
    /// <inheritdoc/>
    public Permission RequiredPermission => RecordPermissions.Write(typeof(TEntity));
}
