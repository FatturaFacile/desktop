// <copyright file="ICopilotContextSource.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Copilot;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Implemented by a module view model to tell the Copilot what the user is looking at.
/// </summary>
public interface ICopilotContextSource
{
    /// <summary>
    /// Gets the context (route, selected entity type and identifier) of the module.
    /// </summary>
    /// <returns>The context.</returns>
    public CopilotContext GetCopilotContext();
}
