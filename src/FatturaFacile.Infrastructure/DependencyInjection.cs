using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Repositories;
using FatturaFacile.Infrastructure.Localization;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Persistence.Repositories;
using FatturaFacile.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // 1. Core Services (Singletons for WPF desktop runtime session lifetime)
        _ = services.AddSingleton<ITenantProvider, TenantProvider>();
        _ = services.AddSingleton<ICurrentUserService, CurrentUserService>();
        _ = services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        _ = services.AddSingleton<ILocalizationService, SqliteLocalizationService>();

        // 2. EF Core SQLite Context
        _ = services.AddDbContext<FatturaFacileDbContext>((sp, options) => _ = options.UseSqlite(connectionString));

        _ = services.AddScoped<IFatturaFacileDbContext>(sp => sp.GetRequiredService<FatturaFacileDbContext>());
        _ = services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FatturaFacileDbContext>());

        // 3. Repositories
        _ = services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        _ = services.AddScoped<IClientRepository, ClientRepository>();
        _ = services.AddScoped<IItalianInvoiceRepository, ItalianInvoiceRepository>();
        _ = services.AddScoped<ISwissQrBillRepository, SwissQrBillRepository>();
        _ = services.AddScoped<ITireRecordRepository, TireRecordRepository>();
        _ = services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
        _ = services.AddScoped<IUserRepository, UserRepository>();
        _ = services.AddScoped<ITenantRepository, TenantRepository>();
        _ = services.AddScoped<ITranslationRepository, TranslationRepository>();

        return services;
    }
}
