// <copyright file="GetCachedCopilotConversationQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Copilot;
using MediatR;

namespace FatturaFacile.Application.Features.Copilot;

/// <summary>
/// Reads one cached conversation with its messages, citations and regulatory snippets. Works offline.
/// </summary>
/// <param name="RemoteId">The server identifier of the conversation.</param>
public sealed record GetCachedCopilotConversationQuery(string RemoteId) : IRequest<CopilotConversation?>;
