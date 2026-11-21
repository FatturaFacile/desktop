// <copyright file="ICopilotHost.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Copilot;

namespace FatturaFacile.Wpf.ViewModels.Copilot;

/// <summary>
/// The operations a message or action card asks of the Copilot view model that owns it.
/// </summary>
internal interface ICopilotHost
{
    /// <summary>
    /// Confirms and executes the proposed action of a card.
    /// </summary>
    /// <param name="action">The card.</param>
    /// <returns>A task representing the call.</returns>
    public Task ConfirmAsync(CopilotActionViewModel action);

    /// <summary>
    /// Rejects the proposed action of a card.
    /// </summary>
    /// <param name="action">The card.</param>
    /// <returns>A task representing the call.</returns>
    public Task RejectAsync(CopilotActionViewModel action);

    /// <summary>
    /// Opens a citation (entity citations navigate to the native view).
    /// </summary>
    /// <param name="citation">The citation.</param>
    /// <returns>A task representing the navigation.</returns>
    public Task OpenCitationAsync(CopilotCitation citation);

    /// <summary>
    /// Opens an external web link.
    /// </summary>
    /// <param name="url">The address.</param>
    public void OpenExternal(string url);

    /// <summary>
    /// Gets a localized string.
    /// </summary>
    /// <param name="key">The resource key.</param>
    /// <returns>The text.</returns>
    public string Localize(string key);
}
