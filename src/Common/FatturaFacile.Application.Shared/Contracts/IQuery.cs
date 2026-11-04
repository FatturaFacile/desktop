// <copyright file="IQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Shared.Results;
using MediatR;

namespace FatturaFacile.Application.Shared.Contracts;

/// <summary>
/// Contract for a CQRS query that reads persistent state without side effects, returning a <see cref="Result{TResponse}"/>.
/// </summary>
/// <typeparam name="TResponse">The query projection return type.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
