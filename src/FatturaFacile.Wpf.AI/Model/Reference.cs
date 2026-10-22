// <copyright file="Reference.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.AI;

/// <summary>
/// Represents a cited document reference, including source filename, page designation, and verbatim quoted text.
/// </summary>
/// <param name="File">The filename or URI of the referenced source document.</param>
/// <param name="Page">The page number or section locator within the source document.</param>
/// <param name="Quote">The verbatim excerpt or passage retrieved from the source document.</param>
public record Reference(string File, string Page, string Quote);
