// <copyright file="IDatabaseSeeder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Ports;

/// <summary>
/// Initializes and populates local database storage with baseline default configurations,
/// standard Italian VAT rates, mock contacts, and initial tenant records.
/// </summary>
public interface IDatabaseSeeder
{
    /// <summary>
    /// Executes the initial database data seeding routines.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to observe during seeding.</param>
    /// <returns>A task representing the asynchronous database seeding operation.</returns>
    Task SeedAsync(CancellationToken cancellationToken = default);
}
