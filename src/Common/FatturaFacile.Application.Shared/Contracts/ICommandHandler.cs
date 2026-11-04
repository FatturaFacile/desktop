// <copyright file="ICommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Shared.Results;
using MediatR;

namespace FatturaFacile.Application.Shared.Contracts;

/// <summary>
/// Defines a handler for processing a state-mutating <see cref="ICommand{TResponse}"/>.
/// </summary>
/// <typeparam name="TCommand">The command type being handled.</typeparam>
/// <typeparam name="TResponse">The payload type returned on command success.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;

/// <summary>
/// Defines a handler for processing a state-mutating <see cref="ICommand"/>.
/// </summary>
/// <typeparam name="TCommand">The command type being handled.</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;
