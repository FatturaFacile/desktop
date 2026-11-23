// <copyright file="RebateAgreementAction.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// A status change of a rebate agreement.
/// </summary>
public enum RebateAgreementAction
{
    /// <summary>
    /// Activates a draft agreement.
    /// </summary>
    Activate = 0,

    /// <summary>
    /// Closes an active agreement.
    /// </summary>
    Close = 1,

    /// <summary>
    /// Deletes a draft agreement.
    /// </summary>
    Delete = 2,
}
