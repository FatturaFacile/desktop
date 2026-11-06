// <copyright file="DesignTimeDbContextFactory.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
/// Creates the context for the EF Core tools (dotnet ef migrations add ...).
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FatturaFacileDbContext>
{
    /// <inheritdoc/>
    public FatturaFacileDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseSqlite("Data Source=design-time.sqlite")
            .Options;

        return new FatturaFacileDbContext(options, new CurrentTenantService(), new AesGcmFieldEncryptor(new byte[32]));
    }
}
