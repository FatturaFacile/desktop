// <copyright file="IQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common;

/// <summary>
/// Defines a contract for processing a specific query type and returning data of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TQuery">The query type being evaluated.</typeparam>
/// <typeparam name="TResult">The resulting data type produced by executing the query.</typeparam>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Executes the read-only projection or query operation.
    /// </summary>
    /// <param name="query">The query parameters to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>A task representing the asynchronous query operation, returning <typeparamref name="TResult"/> upon completion.</returns>
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
