// <copyright file="SkillMatrixCellState.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// Whether a skill of an employee counts for staffing.
/// </summary>
public enum SkillMatrixCellState
{
    /// <summary>
    /// The proficiency is valid and any required certificate is valid.
    /// </summary>
    Valid,

    /// <summary>
    /// The proficiency has expired.
    /// </summary>
    Expired,

    /// <summary>
    /// The skill needs a certificate that is missing.
    /// </summary>
    CertificateMissing,

    /// <summary>
    /// The skill needs a certificate that has expired.
    /// </summary>
    CertificateExpired,
}
