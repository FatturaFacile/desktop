// <copyright file="GetDataRetentionStatusQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Queries;

/// <summary>
/// Reads the retention settings of the active company.
/// </summary>
public sealed record GetDataRetentionStatusQuery : IRequest<DataRetentionStatusDto>, IAuthorizedRequest
{
    /// <inheritdoc />
    public Permission RequiredPermission => Permission.TimeRead;
}
