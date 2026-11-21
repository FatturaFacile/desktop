// <copyright file="MesSuspensionReason.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Reason code of a suspension (causale di sospensione). The wire name is the snake_case of the member.
/// </summary>
public enum MesSuspensionReason
{
    /// <summary>
    /// The machine broke down.
    /// </summary>
    MachineBreakdown = 0,

    /// <summary>
    /// Material is missing.
    /// </summary>
    MaterialShortage = 1,

    /// <summary>
    /// A quality hold.
    /// </summary>
    QualityHold = 2,

    /// <summary>
    /// A break.
    /// </summary>
    Break = 3,

    /// <summary>
    /// A shift handover.
    /// </summary>
    ShiftHandover = 4,

    /// <summary>
    /// A tool change.
    /// </summary>
    ToolChange = 5,

    /// <summary>
    /// Any other reason.
    /// </summary>
    Other = 6,
}
