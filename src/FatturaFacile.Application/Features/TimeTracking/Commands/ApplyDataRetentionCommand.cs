// <copyright file="ApplyDataRetentionCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Applies the retention rules to the active company now.
/// </summary>
public sealed record ApplyDataRetentionCommand : IRequest<DataRetentionResult>, IAuthorizedRequest
{
    /// <inheritdoc />
    public Permission RequiredPermission => Permission.TimeManage;
}
