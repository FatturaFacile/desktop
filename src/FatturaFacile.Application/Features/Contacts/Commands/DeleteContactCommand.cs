// <copyright file="DeleteContactCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Contacts.Commands;

/// <summary>
/// Command to delete a contact.
/// </summary>
/// <param name="ContactId">The contact identifier.</param>
public sealed record DeleteContactCommand(Guid ContactId) : IRequest<bool>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
