// <copyright file="MesTestRows.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Mes;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Builds phase rows for the MES tests.
/// </summary>
internal static class MesTestRows
{
    /// <summary>
    /// Creates a phase row in a state.
    /// </summary>
    /// <param name="status">The state.</param>
    /// <param name="phaseId">The server phase, or null.</param>
    /// <param name="operationId">The operation, or a new one.</param>
    /// <param name="open">The open interval.</param>
    /// <param name="components">The components.</param>
    /// <returns>The row.</returns>
    public static MesPhaseRow Row(MesPhaseStatus status, Guid? phaseId = null, Guid? operationId = null, MesOpenInterval? open = null, IReadOnlyList<MesComponent>? components = null) =>
        new(
            phaseId,
            Version: 1,
            status,
            ResumeTo: null,
            MesMachineMode.Attended,
            "PO-100",
            "ITM-1",
            "Bracket",
            operationId ?? Guid.NewGuid(),
            Sequence: 10,
            "Milling",
            TargetQuantity: 100m,
            GoodQuantity: 0m,
            ScrapQuantity: 0m,
            MesTimeSummary.Empty,
            open,
            components ?? []);
}
