// <copyright file="ManufacturingPage.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;

namespace FatturaFacile.Application.Manufacturing.Sync;

/// <summary>
/// One page of a list resource.
/// </summary>
/// <param name="Items">The rows.</param>
/// <param name="HasMore">A value indicating whether more pages follow.</param>
/// <param name="ServerTime">The server time of the response.</param>
public sealed record ManufacturingPage(IReadOnlyList<JsonElement> Items, bool HasMore, DateTime ServerTime);
