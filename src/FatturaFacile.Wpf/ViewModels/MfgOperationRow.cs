// <copyright file="MfgOperationRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// An operation step row of the manufacturing view, with the offline lead time figures.
/// </summary>
/// <param name="Sequence">The sequence.</param>
/// <param name="Code">The operation code.</param>
/// <param name="Name">The name.</param>
/// <param name="WorkCenter">The work centre.</param>
/// <param name="Machine">The machine.</param>
/// <param name="Type">The localized operation type.</param>
/// <param name="Setup">The setup minutes text.</param>
/// <param name="Run">The run minutes per unit text.</param>
/// <param name="Duration">The step duration text for the quantity.</param>
/// <param name="Start">The start offset text.</param>
/// <param name="End">The end offset text.</param>
/// <param name="Links">The linked BOM components, quality control point and work instruction.</param>
internal sealed record MfgOperationRow(int Sequence, string Code, string Name, string WorkCenter, string Machine, string Type, string Setup, string Run, string Duration, string Start, string End, string Links);
