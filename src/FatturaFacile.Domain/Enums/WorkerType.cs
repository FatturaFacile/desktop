// <copyright file="WorkerType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies the contractual and regulatory employment classification of a worker within Italian and Swiss labor frameworks.
/// </summary>
public enum WorkerType
{
    /// <summary>
    /// Regular direct payroll employee under a standard employment contract subject to local social contributions and statutory withholding.
    /// </summary>
    Employee = 1,

    /// <summary>
    /// Cross-border commuter worker (<c>lavoratore frontaliere</c> / G-Permit holder) residing in one nation and commuting for employment into another, governed by bilateral cross-border taxation agreements.
    /// </summary>
    CrossBorderFrontaliere = 2,

    /// <summary>
    /// External subcontractor or independent contractor providing external services or trade works under a commercial contract for services.
    /// </summary>
    Subcontractor = 3,

    /// <summary>
    /// External professional consultant or technical specialist delivering strategic, corporate, or advisory services.
    /// </summary>
    Adviser = 4,
}
