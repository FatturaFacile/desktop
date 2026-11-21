// <copyright file="ManufacturingDesignTimeFactory.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FatturaFacile.Infrastructure.Manufacturing;

/// <summary>
/// Design-time factory used by "dotnet ef migrations" for the manufacturing context.
/// </summary>
public sealed class ManufacturingDesignTimeFactory : IDesignTimeDbContextFactory<ManufacturingDbContext>
{
    /// <inheritdoc/>
    public ManufacturingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ManufacturingDbContext>()
            .UseSqlite("Data Source=design-time-manufacturing.sqlite")
            .Options;

        return new ManufacturingDbContext(options, new CurrentTenantService());
    }
}
