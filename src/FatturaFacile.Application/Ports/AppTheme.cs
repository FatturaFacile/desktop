// <copyright file="AppTheme.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Ports;

/// <summary>
/// Specifies the user interface visual theme preference for the desktop application.
/// </summary>
public enum AppTheme
{
    /// <summary>
    /// Follows the host Windows operating system light/dark theme preference automatically.
    /// </summary>
    System = 0,

    /// <summary>
    /// Enforces high-contrast light background palettes and dark typography.
    /// </summary>
    Light = 1,

    /// <summary>
    /// Enforces dark slate palettes and light typography to reduce eye strain.
    /// </summary>
    Dark = 2,
}
