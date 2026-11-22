// <copyright file="LedgerAccountInfo.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// The attributes of an account a report needs.
/// </summary>
/// <param name="Id">The account identifier.</param>
/// <param name="Code">The account code.</param>
/// <param name="Name">The account name.</param>
/// <param name="AccountType">The account type (asset, liability, equity, revenue, expense).</param>
/// <param name="NormalBalance">The normal balance side, debit or credit.</param>
public sealed record LedgerAccountInfo(Guid Id, string Code, string Name, string AccountType, string NormalBalance)
{
    /// <summary>
    /// Gets the sign turning debit minus credit into a balance on the normal side: +1 for debit-normal accounts, -1 for credit-normal ones.
    /// </summary>
    public int Sign => string.Equals(NormalBalance, "credit", StringComparison.OrdinalIgnoreCase) ? -1 : 1;
}
