using FatturaFacile.Application.Common.Behaviors;

using FluentValidation;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // 1. Register AutoMapper
        _ = services.AddAutoMapper(cfg => cfg.AddMaps(typeof(AssemblyReference).Assembly));

        // 2. Register FluentValidation
        _ = services.AddValidatorsFromAssembly(typeof(AssemblyReference).Assembly);

        // 3. Register MediatR with pipeline behaviors in strict order:
        // Logging -> TenantContext -> Validation -> Transaction
        _ = services.AddMediatR(cfg =>
        {
            _ = cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly);
            _ = cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            _ = cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TenantBehavior<,>));
            _ = cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            _ = cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        });

        return services;
    }
}
