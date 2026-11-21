// <copyright file="MesInterval.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Mes;

/// <summary>
/// A timestamped interval of a phase, as the server records it.
/// </summary>
/// <param name="Category">What the interval measures.</param>
/// <param name="OperatorId">The operator, if known.</param>
/// <param name="MachineAttended">Whether the machine was attended.</param>
/// <param name="StartedAtUtc">When it started (UTC).</param>
/// <param name="EndedAtUtc">When it ended, null while it is open.</param>
public sealed record MesInterval(MesTimeCategory Category, string? OperatorId, bool MachineAttended, DateTime StartedAtUtc, DateTime? EndedAtUtc);
