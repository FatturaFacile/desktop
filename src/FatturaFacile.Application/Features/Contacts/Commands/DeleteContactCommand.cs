// <copyright file="DeleteContactCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using MediatR;

namespace FatturaFacile.Application.Features.Contacts.Commands;

/// <summary>
/// Command to delete a contact.
/// </summary>
/// <param name="ContactId">The contact identifier.</param>
public sealed record DeleteContactCommand(Guid ContactId) : IRequest<bool>;
