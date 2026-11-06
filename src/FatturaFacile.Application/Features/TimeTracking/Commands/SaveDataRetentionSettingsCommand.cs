// <copyright file="SaveDataRetentionSettingsCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Changes how many years the working time records of the active company are kept.
/// </summary>
/// <param name="Years">The number of years (at least five).</param>
public sealed record SaveDataRetentionSettingsCommand(int Years) : IRequest<bool>, IAuthorizedRequest
{
    /// <inheritdoc />
    public Permission RequiredPermission => Permission.TimeManage;
}
