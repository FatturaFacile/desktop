// <copyright file="GetRecordsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Linq.Expressions;
using FatturaFacile.Application.Common.Records;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Records;

/// <summary>
/// Returns detached copies of master data records visible to the current company.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <param name="Filter">The optional filter.</param>
public sealed record GetRecordsQuery<TEntity>(Expression<Func<TEntity, bool>>? Filter = null) : IRequest<IReadOnlyList<TEntity>>, IAuthorizedRequest
    where TEntity : class, IBaseEntity, new()
{
    /// <inheritdoc/>
    public Permission RequiredPermission => RecordPermissions.Read(typeof(TEntity));
}
