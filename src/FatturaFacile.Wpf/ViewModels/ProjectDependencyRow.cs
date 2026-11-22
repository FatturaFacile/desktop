// <copyright file="ProjectDependencyRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One dependency of the project, formatted as "1.1 -> 1.2 (FS +2)".
/// </summary>
/// <param name="Id">The dependency identifier.</param>
/// <param name="Text">The formatted dependency.</param>
internal sealed record ProjectDependencyRow(Guid Id, string Text);
