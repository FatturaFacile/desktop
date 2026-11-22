// <copyright file="FieldServiceLaneRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FatturaFacile.Application.FieldService;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The lane of a technician on the dispatcher timeline: the scheduled bars, the skills and the van, and the warnings the
/// work order selected in the queue would raise if it were dispatched to this technician.
/// </summary>
internal sealed partial class FieldServiceLaneRow : ObservableObject
{
    [ObservableProperty]
    private string _warningText = string.Empty;

    [ObservableProperty]
    private bool _hasWarning;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldServiceLaneRow"/> class.
    /// </summary>
    /// <param name="lane">The lane of the board.</param>
    /// <param name="name">The technician name, with the availability.</param>
    /// <param name="skillsText">The localized skills line.</param>
    /// <param name="vanText">The localized van line.</param>
    /// <param name="bars">The scheduled bars.</param>
    public FieldServiceLaneRow(FieldServiceLane lane, string name, string skillsText, string vanText, IEnumerable<FieldServiceBarRow> bars)
    {
        ArgumentNullException.ThrowIfNull(lane);
        ArgumentNullException.ThrowIfNull(bars);

        Lane = lane;
        Name = name;
        SkillsText = skillsText;
        VanText = vanText;
        Bars = new ObservableCollection<FieldServiceBarRow>(bars);
    }

    /// <summary>
    /// Gets the lane of the board.
    /// </summary>
    public FieldServiceLane Lane { get; }

    /// <summary>
    /// Gets the technician name, with the availability.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the localized skills line.
    /// </summary>
    public string SkillsText { get; }

    /// <summary>
    /// Gets the localized van line.
    /// </summary>
    public string VanText { get; }

    /// <summary>
    /// Gets the scheduled bars of the lane.
    /// </summary>
    public ObservableCollection<FieldServiceBarRow> Bars { get; }

    /// <summary>
    /// Shows the warnings of a dispatch preview, or clears them.
    /// </summary>
    /// <param name="text">The localized warnings, or empty.</param>
    public void ShowWarnings(string text)
    {
        WarningText = text;
        HasWarning = text.Length > 0;
    }
}
