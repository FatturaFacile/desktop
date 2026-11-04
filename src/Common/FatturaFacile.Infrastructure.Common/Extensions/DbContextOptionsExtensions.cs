using Microsoft.EntityFrameworkCore;
using FatturaFacile.Infrastructure.Common.Configuration;

namespace FatturaFacile.Infrastructure.Common.Extensions;

public static class DbContextOptionsExtensions
{
    public static DbContextOptionsBuilder ConfigureDatabaseProvider(
        this DbContextOptionsBuilder options,
        DatabaseProvider provider,
        string connectionString)
    {
        return provider switch
        {
            DatabaseProvider.Sqlite => options.UseSqlite(connectionString, b =>
            {
                b.MigrationsAssembly("FatturaFacile.Infrastructure.Common");
            }),
            DatabaseProvider.PostgreSQL => options.UseNpgsql(connectionString, b =>
            {
                b.MigrationsAssembly("FatturaFacile.Infrastructure.Common");
            }),
            DatabaseProvider.SqlServer => options.UseSqlServer(connectionString, b =>
            {
                b.MigrationsAssembly("FatturaFacile.Infrastructure.Common");
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(provider), $"Unsupported database provider: {provider}")
        };
    }
}
