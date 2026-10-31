// <copyright file="RepositoryBase.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Linq.Expressions;
using FatturaFacile.Application.Ports;
using FatturaFacile.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
/// Generic Entity Framework Core repository implementation providing foundational data access operations.
/// </summary>
/// <typeparam name="T">The entity type deriving from <see cref="EntityBase"/>.</typeparam>
public class RepositoryBase<T> : IRepository<T> where T : EntityBase
{
    /// <summary>
    /// The Entity Framework database context instance.
    /// </summary>
    protected readonly AppDbContext Context;

    /// <summary>
    /// The EF Core DbSet representing the entity table.
    /// </summary>
    protected readonly DbSet<T> DbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase{T}"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public RepositoryBase(AppDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = DbSet.AsNoTracking();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        _ = DbSet.Add(entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _ = DbSet.Update(entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _ = DbSet.Remove(entity);
        return Task.CompletedTask;
    }
}
