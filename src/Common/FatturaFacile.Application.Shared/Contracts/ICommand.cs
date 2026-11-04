// <copyright file="ICommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Shared.Results;
using MediatR;

namespace FatturaFacile.Application.Shared.Contracts;

/// <summary>
/// Contract for a CQRS command that mutates state and returns a typed <see cref="Result{TResponse}"/>.
/// </summary>
/// <typeparam name="TResponse">The payload type returned on command success.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;

/// <summary>
/// Contract for a CQRS command that returns a non-generic <see cref="Result"/>.
/// Commands mutate state and are dispatched via MediatR.
/// </summary>
public interface ICommand : IRequest<Result>;
