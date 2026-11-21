// <copyright file="MesBoardSnapshot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Mes;

/// <summary>
/// The operator board of a work centre as the web-api answered.
/// </summary>
/// <param name="ServerTimeUtc">The server instant the times are computed for (UTC).</param>
/// <param name="WorkCenterId">The work centre identifier.</param>
/// <param name="WorkCenterName">The work centre name.</param>
/// <param name="Rows">The phases first, then the operations nobody took over.</param>
public sealed record MesBoardSnapshot(DateTime ServerTimeUtc, Guid WorkCenterId, string WorkCenterName, IReadOnlyList<MesPhaseRow> Rows);
