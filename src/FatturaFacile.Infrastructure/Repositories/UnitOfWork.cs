// <copyright file="UnitOfWork.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Common;
using FatturaFacile.Infrastructure.Persistence;

namespace FatturaFacile.Infrastructure.Repositories;

/// <summary>
/// Unit of work implementation coordinating EF Core transactions and caching repositories.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UnitOfWork"/> class.
/// </remarks>
/// <param name="context">The database context.</param>
public sealed class UnitOfWork(FatturaFacileDbContext context) : IUnitOfWork
{
    private readonly ConcurrentDictionary<Type, object> _repositories = new();
    // private bool _disposed;

    /// <inheritdoc/>
    public IRepository<T> Repository<T>()
        where T : class, IBaseEntity => (IRepository<T>)_repositories.GetOrAdd(typeof(T), static (_, ctx) => new Repository<T>(ctx), context);

    /// <inheritdoc/>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);

    /// <inheritdoc/>
    public void Dispose()
    {
        // if (!_disposed)
        // {
        // context.Dispose();
        // _disposed = true;
        // }
    }
}
