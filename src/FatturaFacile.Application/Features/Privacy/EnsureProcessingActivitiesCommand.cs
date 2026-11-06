// <copyright file="EnsureProcessingActivitiesCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Privacy;

/// <summary>
/// Creates the default entries of the register of processing activities when the register is empty.
/// </summary>
public sealed record EnsureProcessingActivitiesCommand : IRequest<int>, IAuthorizedRequest
{
    /// <inheritdoc />
    public Permission RequiredPermission => Permission.CompanySettingsManage;
}
