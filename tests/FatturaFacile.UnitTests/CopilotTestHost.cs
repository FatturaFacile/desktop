// <copyright file="CopilotTestHost.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Copilot;
using FatturaFacile.Domain.Common;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Repositories;
using FatturaFacile.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.UnitTests;

/// <summary>
/// An in-memory host running the real Copilot cache handlers (MediatR + unit of work + tenant filters).
/// </summary>
internal sealed class CopilotTestHost : IDisposable
{
    private readonly ServiceProvider _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CopilotTestHost"/> class.
    /// </summary>
    public CopilotTestHost()
    {
        var databaseName = Guid.NewGuid().ToString();
        ServiceCollection services = new();
        _ = services.AddLogging();
        _ = services.AddSingleton<ICurrentTenantService, CurrentTenantService>();
        _ = services.AddSingleton<TenantSaveChangesInterceptor>();
        _ = services.AddDbContext<FatturaFacileDbContext>((sp, options) => options
            .UseInMemoryDatabase(databaseName)
            .AddInterceptors(sp.GetRequiredService<TenantSaveChangesInterceptor>()));
        _ = services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        _ = services.AddScoped<IUnitOfWork, UnitOfWork>();
        _ = services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CacheCopilotConversationCommand>());
        _provider = services.BuildServiceProvider();
        Tenant = _provider.GetRequiredService<ICurrentTenantService>();
        Tenant.SetCurrentTenant(Guid.NewGuid(), "ACME", "CH", "CHF");
    }

    /// <summary>
    /// Gets the active tenant.
    /// </summary>
    public ICurrentTenantService Tenant { get; }

    /// <summary>
    /// Gets a sender whose requests run in their own scope.
    /// </summary>
    public ISender Sender => new ScopedSender(_provider);

    /// <inheritdoc />
    public void Dispose() => _provider.Dispose();

    private sealed class ScopedSender(IServiceProvider provider) : ISender
    {
        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            await using var scope = provider.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<ISender>().Send(request, cancellationToken);
        }

        public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            await using var scope = provider.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<ISender>().Send(request, cancellationToken);
        }

        public async Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            await using var scope = provider.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<ISender>().Send(request, cancellationToken);
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
