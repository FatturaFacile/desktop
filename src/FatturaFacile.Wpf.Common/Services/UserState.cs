// <copyright file="UserState.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.Common.Services;

/// <summary>
/// Immutable record capturing the serialized visual docking layout and logical navigation state of the application.
/// </summary>
/// <param name="LogicalState">The serialized logical state of active views and navigation paths.</param>
/// <param name="VisualState">The serialized XML layout of docking panels and grids.</param>
/// <param name="LayoutVersion">The schema version string of the saved layout.</param>
public sealed record UserState(
    string LogicalState = "",
    string VisualState = "",
    string LayoutVersion = "1.0");
