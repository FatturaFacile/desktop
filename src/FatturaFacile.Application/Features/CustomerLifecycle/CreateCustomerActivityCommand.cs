// <copyright file="CreateCustomerActivityCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.CustomerLifecycle;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.CustomerLifecycle;

/// <summary>
/// Command of the customer quick actions: logs a visit or creates a follow-up as a CRM activity of the account. When the server
/// is unreachable the activity is saved in the local outbox instead.
/// </summary>
/// <param name="AccountId">The server identifier of the CRM account.</param>
/// <param name="Type">The activity type (see <see cref="CustomerActivityTypes"/>).</param>
/// <param name="Subject">The subject.</param>
/// <param name="DueDate">The optional due date.</param>
public sealed record CreateCustomerActivityCommand(Guid AccountId, string Type, string Subject, DateTime? DueDate) : IRequest<CustomerActivityOutcome>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
