// <copyright file="DbContextOptionsExtensions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Infrastructure.Common.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Common.Extensions;

/// <summary>
/// Extension methods for configuring EF Core DbContext options based on selected database engine provider.
/// </summary>
public static class DbContextOptionsExtensions
{
    /// <summary>
    /// Configures the appropriate database engine provider (Sqlite, PostgreSQL, or SQL Server) on the DbContext builder.
    /// </summary>
    /// <param name="options">The DbContext options builder.</param>
    /// <param name="provider">The database engine provider enum.</param>
    /// <param name="connectionString">The ADO.NET connection string.</param>
    /// <returns>The configured <see cref="DbContextOptionsBuilder"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported database provider is passed.</exception>
    public static DbContextOptionsBuilder ConfigureDatabaseProvider(
        this DbContextOptionsBuilder options,
        DatabaseProvider provider,
        string connectionString)
    {
        return provider switch
        {
            DatabaseProvider.Sqlite => options.UseSqlite(connectionString, b =>
            {
                _ = b.MigrationsAssembly("FatturaFacile.Infrastructure.Common");
            }),
            DatabaseProvider.PostgreSQL => options.UseNpgsql(connectionString, b =>
            {
                b.MigrationsAssembly("FatturaFacile.Infrastructure.Common");
            }),
            DatabaseProvider.SqlServer => options.UseSqlServer(connectionString, b =>
            {
                _ = b.MigrationsAssembly("FatturaFacile.Infrastructure.Common");
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(provider), $"Unsupported database provider: {provider}"),
        };
    }
}
