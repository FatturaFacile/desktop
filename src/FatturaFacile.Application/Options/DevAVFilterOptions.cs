// <copyright file="DevAVFilterOptions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Configuration;

/// <summary>
/// Configuration options for DevAV filters.
/// </summary>
public sealed class DevAVFilterOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "DevAVFilters";

    /// <summary>
    /// Gets the custom filters for tasks.
    /// </summary>
    public IReadOnlyList<FilterInfoOption> TasksCustomFilters { get; init; } = [];

    /// <summary>
    /// Gets the custom filters for employees.
    /// </summary>
    public IReadOnlyList<FilterInfoOption> EmployeesCustomFilters { get; init; } = [];

    /// <summary>
    /// Gets the custom filters for customers.
    /// </summary>
    public IReadOnlyList<FilterInfoOption> CustomersCustomFilters { get; init; } = [];

    /// <summary>
    /// Gets the static filters for customers.
    /// </summary>
    public IReadOnlyList<FilterInfoOption> CustomersStaticFilters { get; init; } = [];

    /// <summary>
    /// Gets the custom filters for products.
    /// </summary>
    public IReadOnlyList<FilterInfoOption> ProductsCustomFilters { get; init; } = [];

    /// <summary>
    /// Gets the static filters for products.
    /// </summary>
    public IReadOnlyList<FilterInfoOption> ProductsStaticFilters { get; init; } = [];

    /// <summary>
    /// Gets the custom filters for orders.
    /// </summary>
    public IReadOnlyList<FilterInfoOption> OrdersCustomFilters { get; init; } = [];

    /// <summary>
    /// Gets the static filters for orders.
    /// </summary>
    public IReadOnlyList<FilterInfoOption> OrdersStaticFilters { get; init; } = [];

    /// <summary>
    /// Gets the static filters for quotes.
    /// </summary>
    public IReadOnlyList<FilterInfoOption> QuotesStaticFilters { get; init; } = [];

    /// <summary>
    /// Gets the static filters for employees.
    /// </summary>
    public IReadOnlyList<FilterInfoOption> EmployeesStaticFilters { get; init; } = [];

    /// <summary>
    /// Gets the static filters for tasks.
    /// </summary>
    public IReadOnlyList<FilterInfoOption> TasksStaticFilters { get; init; } = [];
}
