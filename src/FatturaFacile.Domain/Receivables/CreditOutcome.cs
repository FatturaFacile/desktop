// <copyright file="CreditOutcome.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// The result of a credit check.
/// </summary>
public enum CreditOutcome
{
    /// <summary>
    /// The operation may go on.
    /// </summary>
    Ok = 0,

    /// <summary>
    /// The operation may go on, the desk is alerted.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// The operation is blocked.
    /// </summary>
    Block = 2,
}
