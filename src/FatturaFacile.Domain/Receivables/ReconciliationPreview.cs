// <copyright file="ReconciliationPreview.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// What an item looks like after an allocation.
/// </summary>
/// <param name="OpenAfter">The open amount after the allocation.</param>
/// <param name="StatusAfter">The status after the allocation.</param>
public sealed record ReconciliationPreview(decimal OpenAfter, ReceivableStatus StatusAfter);
