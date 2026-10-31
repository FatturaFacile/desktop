// <copyright file="ICommand.Generic.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common;

/// <summary>
/// Defines a CQRS command request that yields a structured response of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of result returned after the command executes.</typeparam>
public interface ICommand<out TResult>
{
}
