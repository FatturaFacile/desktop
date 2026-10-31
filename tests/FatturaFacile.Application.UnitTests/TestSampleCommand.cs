// <copyright file="TestSampleCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using MediatR;

namespace FatturaFacile.Application.UnitTests;

/// <summary>
/// Represents a test command used to verify MediatR pipeline behaviors.
/// </summary>
/// <param name="Name">The command test payload name.</param>
public sealed record TestSampleCommand(string Name) : IRequest<string>;
