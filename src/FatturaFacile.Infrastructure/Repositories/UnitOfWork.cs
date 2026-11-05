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
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly FatturaFacileDbContext _context;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UnitOfWork(FatturaFacileDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public IRepository<T> Repository<T>()
        where T : class, IBaseEntity => (IRepository<T>)_repositories.GetOrAdd(typeof(T), static (_, ctx) => new Repository<T>(ctx), _context);

    /// <inheritdoc/>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _context.Dispose();
            _disposed = true;
        }
    }
}
