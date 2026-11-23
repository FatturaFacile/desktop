// <copyright file="ReceivableItemPage.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Receivables;

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// A page of receivable items.
/// </summary>
/// <param name="Items">The items of the page.</param>
/// <param name="CurrentPage">The page number, from 1.</param>
/// <param name="LastPage">The last page number.</param>
/// <param name="Total">The items over all the pages.</param>
public sealed record ReceivableItemPage(
    IReadOnlyList<ReceivableItem> Items,
    int CurrentPage,
    int LastPage,
    int Total);
