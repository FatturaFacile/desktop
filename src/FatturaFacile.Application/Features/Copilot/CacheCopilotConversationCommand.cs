// <copyright file="CacheCopilotConversationCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Copilot;
using MediatR;

namespace FatturaFacile.Application.Features.Copilot;

/// <summary>
/// Stores (inserts or updates) a conversation and the messages it carries in the local cache.
/// </summary>
/// <param name="Conversation">The conversation as returned by the server.</param>
public sealed record CacheCopilotConversationCommand(CopilotConversation Conversation) : IRequest<bool>;
