// <copyright file="ICopilotEntityTarget.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;

namespace FatturaFacile.Wpf.ViewModels.Copilot;

/// <summary>
/// A module view model that tells the Copilot what is selected and can select an entity cited by the assistant.
/// </summary>
internal interface ICopilotEntityTarget : ICopilotContextSource
{
    /// <summary>
    /// Selects the cited entity in the module's list.
    /// </summary>
    /// <param name="entityType">The entity type of the citation.</param>
    /// <param name="entityId">The entity identifier of the citation.</param>
    /// <returns><see langword="true"/> when the entity was found and selected.</returns>
    public Task<bool> SelectEntityAsync(string? entityType, string? entityId);
}
