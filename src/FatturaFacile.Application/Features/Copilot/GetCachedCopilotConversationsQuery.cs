// <copyright file="GetCachedCopilotConversationsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Copilot;
using MediatR;

namespace FatturaFacile.Application.Features.Copilot;

/// <summary>
/// Reads the cached conversations of the active tenant, newest first, without messages. Works offline.
/// </summary>
public sealed record GetCachedCopilotConversationsQuery : IRequest<IReadOnlyList<CopilotConversation>>;
