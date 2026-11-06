// <copyright file="EraseContactDataCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Privacy;

/// <summary>
/// Erases the personal data of a contact (art. 32 nLPD, art. 17 GDPR): anonymises it when no document must still be kept, otherwise removes only the contact data not required by the retention obligations.
/// </summary>
/// <param name="ContactId">The contact.</param>
public sealed record EraseContactDataCommand(Guid ContactId) : IRequest<string>, IAuthorizedRequest
{
    /// <inheritdoc />
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
