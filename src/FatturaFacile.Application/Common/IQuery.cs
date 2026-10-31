// <copyright file="IQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common;

/// <summary>
/// Defines a CQRS query request that retrieves state and returns data of type <typeparamref name="TResult"/> without modifying state.
/// </summary>
/// <typeparam name="TResult">The type of data returned by the query.</typeparam>
public interface IQuery<out TResult>
{
}
