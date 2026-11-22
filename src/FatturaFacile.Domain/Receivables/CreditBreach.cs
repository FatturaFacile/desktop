// <copyright file="CreditBreach.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// The breach names of a credit decision, shared with the web-api.
/// </summary>
public static class CreditBreach
{
    /// <summary>
    /// The projected exposure is over the granted limit.
    /// </summary>
    public const string OverLimit = "over_limit";

    /// <summary>
    /// The oldest overdue item is over the tolerated days and the overdue amount over the tolerance.
    /// </summary>
    public const string OverdueDays = "overdue_days";

    /// <summary>
    /// The customer is on credit hold.
    /// </summary>
    public const string CreditHold = "credit_hold";
}
