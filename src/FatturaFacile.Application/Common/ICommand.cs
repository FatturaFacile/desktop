// <copyright file="ICommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common;

/// <summary>
/// Defines a CQRS command request that does not return a value beyond execution confirmation.
/// </summary>
public interface ICommand : ICommand<Unit>
{
}
