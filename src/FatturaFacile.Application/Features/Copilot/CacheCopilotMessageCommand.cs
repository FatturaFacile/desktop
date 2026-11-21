// <copyright file="CacheCopilotMessageCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Copilot;
using MediatR;

namespace FatturaFacile.Application.Features.Copilot;

/// <summary>
/// Stores (inserts or updates) one message, e.g. an answer that just finished streaming or an action whose status changed.
/// </summary>
/// <param name="Message">The message.</param>
public sealed record CacheCopilotMessageCommand(CopilotMessage Message) : IRequest<bool>;
