// <copyright file="MesOpenInterval.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Mes;

/// <summary>
/// The interval of a phase that has not ended yet.
/// </summary>
/// <param name="Category">What it measures.</param>
/// <param name="StartedAtUtc">When it started (UTC).</param>
/// <param name="MachineAttended">Whether the machine is attended.</param>
/// <param name="Reason">The suspension reason of a suspension.</param>
public sealed record MesOpenInterval(MesTimeCategory Category, DateTime StartedAtUtc, bool MachineAttended, MesSuspensionReason? Reason);
