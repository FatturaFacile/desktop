// <copyright file="RebateChoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One entry of a picker of the supplier rebates views (supplier, blanket agreement, account, kind, mode).
/// </summary>
/// <param name="Id">The identifier or wire code, empty for the "none" entry.</param>
/// <param name="Label">The text shown in the picker.</param>
internal sealed record RebateChoice(string Id, string Label);
