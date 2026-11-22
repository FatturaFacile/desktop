// <copyright file="FieldServiceWarning.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// A reason a work order cannot be dispatched to a technician, found locally on the board or reported by the server.
/// </summary>
/// <param name="Kind">What is wrong.</param>
/// <param name="Detail">The detail (skills, server message), possibly empty.</param>
public sealed record FieldServiceWarning(FieldServiceWarningKind Kind, string Detail);
