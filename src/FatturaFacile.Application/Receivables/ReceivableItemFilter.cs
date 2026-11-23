// <copyright file="ReceivableItemFilter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Receivables;

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// The filters of the open items list; the drill-down of an aging figure sets the bucket and the customer.
/// </summary>
/// <param name="CustomerId">The customer.</param>
/// <param name="Status">The status.</param>
/// <param name="DocumentType">The document type.</param>
/// <param name="Currency">The ISO currency.</param>
/// <param name="DueFrom">The first due date.</param>
/// <param name="DueTo">The last due date.</param>
/// <param name="IssuedFrom">The first issue date.</param>
/// <param name="IssuedTo">The last issue date.</param>
/// <param name="ValueFrom">The lowest amount.</param>
/// <param name="ValueTo">The highest amount.</param>
/// <param name="Bucket">The aging bucket key.</param>
/// <param name="OpenOnly">Whether settled items are left out.</param>
/// <param name="Page">The page number, from 1.</param>
/// <param name="PerPage">The items per page.</param>
public sealed record ReceivableItemFilter(
    Guid? CustomerId,
    ReceivableStatus? Status,
    string? DocumentType,
    string? Currency,
    DateOnly? DueFrom,
    DateOnly? DueTo,
    DateOnly? IssuedFrom,
    DateOnly? IssuedTo,
    decimal? ValueFrom,
    decimal? ValueTo,
    string? Bucket,
    bool OpenOnly,
    int Page,
    int PerPage)
{
    /// <summary>
    /// Gets the filter of the open items of everybody, first page.
    /// </summary>
    public static ReceivableItemFilter Default { get; } = new(CustomerId: null, Status: null, DocumentType: null, Currency: null, DueFrom: null, DueTo: null, IssuedFrom: null, IssuedTo: null, ValueFrom: null, ValueTo: null, Bucket: null, OpenOnly: true, Page: 1, PerPage: 50);
}
