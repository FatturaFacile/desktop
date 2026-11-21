// <copyright file="MesDefectReason.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Reason code of scrapped pieces (causale di difetto). The wire name is the snake_case of the member.
/// </summary>
public enum MesDefectReason
{
    /// <summary>
    /// Out of dimension.
    /// </summary>
    Dimensional = 0,

    /// <summary>
    /// Surface defect.
    /// </summary>
    Surface = 1,

    /// <summary>
    /// Defect of the material.
    /// </summary>
    MaterialDefect = 2,

    /// <summary>
    /// Worn tool.
    /// </summary>
    ToolWear = 3,

    /// <summary>
    /// Setup error.
    /// </summary>
    SetupError = 4,

    /// <summary>
    /// Operator error.
    /// </summary>
    OperatorError = 5,

    /// <summary>
    /// Machine fault.
    /// </summary>
    MachineFault = 6,

    /// <summary>
    /// Any other reason.
    /// </summary>
    Other = 7,
}
