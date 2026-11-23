// <copyright file="BlanketContract.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// A blanket purchase agreement of the picker.
/// </summary>
/// <param name="Id">The agreement identifier.</param>
/// <param name="ContractNumber">The agreement number.</param>
/// <param name="Title">The title.</param>
/// <param name="SupplierId">The supplier identifier.</param>
/// <param name="Status">The status.</param>
public sealed record BlanketContract(string Id, string ContractNumber, string Title, string? SupplierId, string Status)
{
    /// <summary>
    /// Gets the label of the picker: the number and the title.
    /// </summary>
    public string Label => string.IsNullOrWhiteSpace(Title) ? ContractNumber : ContractNumber + " - " + Title;
}
