// <copyright file="DeleteCachedCopilotConversationCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using MediatR;

namespace FatturaFacile.Application.Features.Copilot;

/// <summary>
/// Removes a conversation and its messages from the local cache.
/// </summary>
/// <param name="RemoteId">The server identifier of the conversation.</param>
public sealed record DeleteCachedCopilotConversationCommand(string RemoteId) : IRequest<bool>;
