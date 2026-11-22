// <copyright file="ProjectEvmEacPoint.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectEvm;

/// <summary>
/// A point of the EAC projection that runs from the last actual date to the planned end of the project.
/// </summary>
/// <param name="Date">The date.</param>
/// <param name="Eac">The projected cost.</param>
public sealed record ProjectEvmEacPoint(DateTime Date, double Eac);
