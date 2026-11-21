// <copyright file="MesConflictItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A command the server refused, as the operator sees it.
/// </summary>
/// <param name="CommandId">The refused command.</param>
/// <param name="ActionText">The localized action.</param>
/// <param name="TimeText">When the operator acted, local time.</param>
/// <param name="Error">What the server answered.</param>
internal sealed record MesConflictItem(Guid CommandId, string ActionText, string TimeText, string Error);
