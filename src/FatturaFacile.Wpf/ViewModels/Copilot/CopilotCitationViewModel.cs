// <copyright file="CopilotCitationViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows.Input;
using FatturaFacile.Application.Copilot;

namespace FatturaFacile.Wpf.ViewModels.Copilot;

/// <summary>
/// A citation chip below a message: regulatory snippets show their source in a tooltip, entity citations navigate
/// to the native view.
/// </summary>
internal sealed class CopilotCitationViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CopilotCitationViewModel"/> class.
    /// </summary>
    /// <param name="citation">The citation.</param>
    /// <param name="openCommand">The command that opens the citation; its parameter is the citation.</param>
    public CopilotCitationViewModel(CopilotCitation citation, ICommand openCommand)
    {
        ArgumentNullException.ThrowIfNull(citation);

        Citation = citation;
        OpenCommand = openCommand;
        ToolTip = string.Join(" · ", new[] { citation.Title, citation.SectionRef, citation.Jurisdiction }.Where(static s => !string.IsNullOrWhiteSpace(s)));
    }

    /// <summary>
    /// Gets the citation.
    /// </summary>
    public CopilotCitation Citation { get; }

    /// <summary>
    /// Gets the command that opens the citation.
    /// </summary>
    public ICommand OpenCommand { get; }

    /// <summary>
    /// Gets the label.
    /// </summary>
    public string Label => Citation.Label;

    /// <summary>
    /// Gets a value indicating whether the citation links to an ERP entity.
    /// </summary>
    public bool IsEntity => Citation.IsEntity;

    /// <summary>
    /// Gets the tooltip (title, section and jurisdiction of a regulatory source).
    /// </summary>
    public string ToolTip { get; }
}
