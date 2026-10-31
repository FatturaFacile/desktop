// <copyright file="TestSampleQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using MediatR;

namespace FatturaFacile.Application.UnitTests;

/// <summary>
/// Represents a test query used to verify MediatR pipeline behaviors.
/// </summary>
/// <param name="Id">The query test identifier.</param>
public sealed record TestSampleQuery(int Id) : IRequest<string>;
