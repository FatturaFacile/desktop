// <copyright file="RoutingCostInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing.Costing;

/// <summary>
/// Routing data of one operation used for standard costing.
/// </summary>
/// <param name="SetupMinutes">The setup minutes per order.</param>
/// <param name="RunMinutesPerUnit">The machine run minutes per unit.</param>
/// <param name="LaborMinutesPerUnit">The labour minutes per unit.</param>
/// <param name="HourlyLaborRate">The labour rate per hour.</param>
/// <param name="HourlyOverheadRate">The overhead rate per machine hour.</param>
public sealed record RoutingCostInput(decimal SetupMinutes, decimal RunMinutesPerUnit, decimal LaborMinutesPerUnit, decimal HourlyLaborRate, decimal HourlyOverheadRate);
