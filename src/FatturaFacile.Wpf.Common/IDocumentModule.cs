// <copyright file="IDocumentModule.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace Common;

/// <summary>
/// Contract representing an active or background document module hosted within a document region.
/// </summary>
public interface IDocumentModule
{
    /// <summary>
    /// Gets the user-facing caption displayed in the document tab or header.
    /// </summary>
    string Caption { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this document module is currently the active document.
    /// </summary>
    bool IsActive { get; set; }
}
